using Controller.GZH;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Utils;
using WeChatCoreHandle;

namespace HeSuanInput.UI.GZH.Action
{
    /// <summary>
    /// InfoCreate 的摘要说明
    /// </summary>
    public class InfoCreate : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string operate = context.Request["operate"];

            switch (operate)
            {
                case "GetOpenId":
                    GetOpenId(context);
                    break;
                case "GetHosInfo":
                    GetHosInfo(context);
                    break; 
                case "Register":
                    Register(context);
                    break; 
                case "GetJcxm":
                    GetJcxm(context);
                    break; 
                case "Pay":
                    Pay(context);
                    break; 
                case "GetIsPay":
                    GetIsPay(context);
                    break;
                case "InsertRegisterDetail":
                    InsertRegisterDetail(context);
                    break;
            }
        }
        /// <summary>
        /// 获取openid
        /// </summary>
        /// <param name="context"></param>
        public void GetOpenId(HttpContext context)
        {
            ResultModel rm = new ResultModel();
            string code = context.Request["code"];

            var rel = CoreHandle.GetOpenid(code);

            if (!string.IsNullOrEmpty(rel))
            {
                rm.IsSuccess = true;
                rm.Data = rel;
            }
            else
            {
                rm.IsSuccess = false;
                rm.Data = "获取openid失败";
            }

            context.Response.Write(JsonConvert.SerializeObject(rm));
            context.Response.End();
        }
        /// <summary>
        /// 获取医院信息
        /// </summary>
        /// <param name="context"></param>
        public void GetHosInfo(HttpContext context)
        {
            ResultModel rm = new ResultModel();

            DataTable dt = HospitalManage.GetHos();

            if(dt.Rows.Count > 0)
            {
                rm.IsSuccess = true;
                rm.Data = dt;
            }
            else
            {
                rm.IsSuccess = false;
                rm.Data = "获取医院信息失败";
            }

            context.Response.Write(JsonConvert.SerializeObject(rm));
            context.Response.End();
        }
        /// <summary>
        /// 获取医院信息
        /// </summary>
        /// <param name="context"></param>
        public void GetJcxm(HttpContext context)
        {
            ResultModel rm = new ResultModel();
            string yyid = context.Request["yyid"];

            DataTable dt = HospitalManage.GetJcxm(yyid);

            if (dt.Rows.Count > 0)
            {
                rm.IsSuccess = true;
                rm.Data = dt;
            }
            else
            {
                rm.IsSuccess = false;
                rm.Data = "获取医院检查项目失败";
            }

            context.Response.Write(JsonConvert.SerializeObject(rm));
            context.Response.End();
        }
        /// <summary>
        /// 信息录入
        /// </summary>
        /// <param name="context"></param>
        public void Register(HttpContext context)
        {
            ResultModel rm = new ResultModel();
            string xm = context.Request["xm"];
            string sfzh = context.Request["sfzh"];
            string xb = context.Request["xb"];
            string nl = context.Request["nl"];
            string sjh = context.Request["sjh"];
            string xzz = context.Request["xzz"];
            string hjdz = context.Request["hjdz"];
            string gfx = context.Request["gfx"];
            string ryfl = context.Request["ryfl"];
            string fr = context.Request["fr"];
            string zf = context.Request["zf"];
            string openid = context.Request["openid"];
            string ly = context.Request["ly"];
            string yyid = context.Request["yyid"];
            string cjdbm = context.Request["cjdbm"];
            string qd = context.Request["qd"];
            string zjlx = "0";

            string xmdm = context.Request["xmdm"];
            string xmmc = context.Request["xmmc"];
            string je = context.Request["je"];

            var rel = RegisterManage.Register(yyid, cjdbm, qd, ly, xm, zjlx, sfzh, xb, nl, xzz, hjdz, sjh, ryfl, gfx, fr, zf, openid, xmdm, xmmc, je);

            if (!string.IsNullOrEmpty(rel))
            {
                rm.IsSuccess = true;
                rm.Data = rel;
            }
            else
            {
                rm.IsSuccess = false;
                rm.Data = "录入患者信息失败，请联系管理人员";
            }
            context.Response.Write(JsonConvert.SerializeObject(rm));
            context.Response.End();
        }
        /// <summary>
        /// //H5调起JS API参数
        /// </summary>
        private static string WeChatJsApiParam { get; set; }
        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="context"></param>
        public void Pay(HttpContext context)
        {
            string openid = context.Request.Form["openid"];
            string total_fee = context.Request.Form["total_fee"];
            string attach = context.Request.Form["attach"];//扩展数据
            Log.LogWrite("支付openid", openid);
            Log.LogWrite("支付费total_fee", total_fee);
            Log.LogWrite("支付参数attach", attach);
            //检测是否给当前页面传递了相关参数
            if (string.IsNullOrEmpty(openid) || string.IsNullOrEmpty(total_fee))
            {
                Log.LogWrite("Pay", "openid,total_fee参数为空");
                context.Response.Write("[]");
                context.Response.End();
            }
            //若传递了相关参数，则调统一下单接口，获得后续相关接口的入口参数
            JSApiPay jsApiPay = new JSApiPay();
            jsApiPay.openid = openid;
            jsApiPay.total_fee = (double.Parse(total_fee) * 100).ToString("f0");
            jsApiPay.payDesc = "核酸检测";


            //扩展数据 排班序号、病人ID、预约号、支付类型：0挂号费 1充值
            //jsApiPay.attach = "{\"XH\":\"123456\",\"BRID\":\"1244\",\"PAY_TYPE\":\"0\"}";
            jsApiPay.attach = attach;
            WeChatPayCore unifiedOrderResult = jsApiPay.GetUnifiedOrderResult();
            WeChatJsApiParam = jsApiPay.GetJsApiParameters();
            Log.LogWrite("支付检查费用WeChatJsApiParam", WeChatJsApiParam);
            context.Response.Write(WeChatJsApiParam);
            context.Response.End();
        }
        /// <summary>
        /// 添加支付信息
        /// </summary>
        /// <param name="context"></param>
        public void InsertRegisterDetail(HttpContext context)
        {
            string lsh = context.Request.Form["lsh"];
            string xmbm = context.Request.Form["xmbm"];
            string xmmc = context.Request.Form["xmmc"];
            string je = context.Request.Form["je"];
            ResultModel rm = new ResultModel();

            var rel = RegisterManage.InsertRegisterDetail(lsh, xmbm, xmmc, je);

            if (rel)
            {
                rm.IsSuccess = true;
                rm.Data = "";
            }
            else
            {
                rm.IsSuccess = false;
                rm.Data = "添加支付信息失败";
            }

            context.Response.Write(JsonConvert.SerializeObject(rm));
            context.Response.End();
        }

        /// <summary>
        /// 获取支付信息
        /// </summary>
        /// <param name="context"></param>
        public void GetIsPay(HttpContext context)
        {
            string lsh = context.Request.Form["lsh"];
            ResultModel rm = new ResultModel();

            DataTable dt = RegisterManage.GetIsPay(lsh);

            if(dt != null)
            {
                rm.IsSuccess = true;
                rm.Data = dt;
            }
            else
            {
                rm.IsSuccess = false;
                rm.Data = "获取支付信息失败";
            }

            context.Response.Write(JsonConvert.SerializeObject(rm));
            context.Response.End();
        }
        public class ResultModel
        {
            public bool IsSuccess { get; set; }

            public object Data { get; set; }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}