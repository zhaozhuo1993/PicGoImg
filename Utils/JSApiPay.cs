
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Utils;

namespace WeChatCoreHandle
{
    public class JSApiPay
    {

        /// <summary>
        /// 保存页面对象，因为要在类的方法中使用Page的Request对象
        /// </summary>
        private Page page { get; set; }

        /// <summary>
        /// openid用于调用统一下单接口
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// access_token用于获取收货地址js函数入口参数
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// 商品金额，用于统一下单
        /// </summary>
        public string total_fee { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string payDesc { get; set; }

        /// <summary>
        /// 商家数据包(预约号/就诊卡号/住院号等)
        /// </summary>
        public string attach { get; set; }

        /// <summary>
        /// 统一下单接口返回结果
        /// </summary>
        public WeChatPayCore unifiedOrderResult { get; set; }



        /**
        * 调用统一下单，获得下单结果
        * @return 统一下单结果
        * @失败时抛异常WxPayException
        */
        public WeChatPayCore GetUnifiedOrderResult()
        {
            //统一下单
            WeChatPayCore data = new WeChatPayCore();
            data.SetValue("body", payDesc);
            data.SetValue("attach", attach);
            data.SetValue("out_trade_no", WeChatPayApi.GenerateOutTradeNo());
            data.SetValue("total_fee", total_fee);
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
            data.SetValue("trade_type", "JSAPI");
            data.SetValue("openid", openid);

            WeChatPayCore result = WeChatPayApi.UnifiedOrder(data);
            if (!result.IsSet("appid") || !result.IsSet("prepay_id") || result.GetValue("prepay_id").ToString() == "")
            {
                Log.LogWrite("GetUnifiedOrderResult", "UnifiedOrder response error!");
            }
            unifiedOrderResult = result;
            return result;
        }

        /**
        *  
        * 从统一下单成功返回的数据中获取微信浏览器调起jsapi支付所需的参数，
        * 微信浏览器调起JSAPI时的输入参数格式如下：
        * {
        *   "appId" : "wx2421b1c4370ec43b",     //公众号名称，由商户传入     
        *   "timeStamp":" 1395712654",         //时间戳，自1970年以来的秒数     
        *   "nonceStr" : "e61463f8efa94090b1f366cccfbbb444", //随机串     
        *   "package" : "prepay_id=u802345jgfjsdfgsdg888",     
        *   "signType" : "MD5",         //微信签名方式:    
        *   "paySign" : "70EA570631E4BB79628FBCA90534C63FF7FADD89" //微信签名 
        * }
        * @return string 微信浏览器调起JSAPI时的输入参数，json格式可以直接做参数用
        * 更详细的说明请参考网页端调起支付API：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_7
        * 
        */
        public string GetJsApiParameters()
        {
            WeChatPayCore jsApiParam = new WeChatPayCore();
            jsApiParam.SetValue("appId", unifiedOrderResult.GetValue("appid"));
            jsApiParam.SetValue("timeStamp", WeChatPayApi.GenerateTimeStamp());
            jsApiParam.SetValue("nonceStr", WeChatPayApi.GenerateNonceStr());
            jsApiParam.SetValue("package", "prepay_id=" + unifiedOrderResult.GetValue("prepay_id"));
            jsApiParam.SetValue("signType", "MD5");
            jsApiParam.SetValue("paySign", jsApiParam.MakeSign());

            string parameters = jsApiParam.ToJson();
            return parameters;
        }
    }
}
