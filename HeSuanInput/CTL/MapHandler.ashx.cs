
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utils;

namespace MapAPI
{
    /// <summary>
    /// MapHandler 的摘要说明
    /// </summary>
    public class MapHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string sign = context.Request["sign"];
            Utils.Log.LogWrite("sign 0:", sign);
            switch (sign)
            {
                case "sign":
                    GetSign(context);
                    break;
                case "point":
                    GetPoint(context);
                    break;
                case "point2":
                    GetPointTX(context);
                    break;
            }
        }

        private void GetSign(HttpContext context)
        {
            Log.LogWrite("GetSign 1:", context.Request["url"]);
            string rtn = "";
            try
            {
                //生成tokcen
                string tocken = WeChat.GetAccessToken();/*用txt存储数据 获取token 有时会因访问被拒绝而报错 WeChatCoreHandle.CoreHandle.GetAccessToken();// */
                JObject TokenJO = (JObject)JsonConvert.DeserializeObject(tocken);
                //验证签名
                string Jsapi_Ticket = WeChat.GetWeiXinJsapi_Ticket(TokenJO["access_token"].ToString());
                JObject Jsapi_TicketJo = (JObject)JsonConvert.DeserializeObject(Jsapi_Ticket);
                #region
                string jsapi_ticket = Jsapi_TicketJo["ticket"].ToString();
                string noncestr = WeChat.CreatenNonce_str();
                long timestamp = WeChat.CreatenTimestamp();
                string outstring = "";
                string JS_SDK_Result = WeChat.GetSignature(jsapi_ticket, noncestr, timestamp, context.Request["url"], out outstring);
                //拼接json串返回前台
                rtn = "{\"appid\":\"" + WeChat.appID + "\",\"noncestr\":\"" + noncestr + "\",\"timestamp\":\"" + timestamp + "\",\"outstring\":\"" + outstring + "\",\"signature\":\"" + JS_SDK_Result.ToLower() + "\"}";
                #endregion
            }
            catch (Exception ex)
            {
                rtn = "GetSign Exception:" + ex.Message;
                Log.LogWrite("GetSign ", rtn);
            }
            context.Response.Write(rtn);
            context.Response.End();
        }

        private void GetPoint(HttpContext context)
        {
            string result = "";
            try
            {
                result = WeChat.GetPoint(context.Request["x"], context.Request["y"]);
            }
            catch (Exception x)
            {
                result = "GetPoint Exception: "+ x.Message;
                Log.LogWrite("GetPoint ", result);
            }
            context.Response.Write(result);
            context.Response.End();
        }

        private void GetPointTX(HttpContext context)
        {
            string result = "";
            try
            {
                result = WeChat.GetPointTX(context.Request["x"], context.Request["y"]);
            }
            catch (Exception x)
            {
                result = "GetPointTX Exception: " + x.Message;
                Log.LogWrite("GetPointTX ", result);
            }
            context.Response.Write(result);
            context.Response.End();
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    public class WeChat
    {

        private static string configSavePath = HttpContext.Current.Server.MapPath("AccessToken.txt");
        public static string appID = "wx22df7163508b4537";
        private static string appSecret = "19dd543accb6e872adf116fbf23a8f20";

        #region 获取AccessToken

        public static string GetAccessToken()
        {
            string strReturn = null;
            bool byUrl = true;
            string tokenTxt = null;
            Log.LogWrite("GetSign configSavePath", configSavePath);
            if (File.Exists(configSavePath))
            {
                tokenTxt = File.ReadAllText(configSavePath);
                tokenTxt = @"{""access_token"":""41_ -xn0ay_e6zvD1PGpZXvEt-LCq6rkV1Nj0e2IbPotsWeZmwZzv4TEB_vgBwW-40tq2fQ9G44p5wVhcyMGULxxJ0OMONcluRlxuiHXCFeQQSS7W2CnblixeBpI3Z-3cYn-KE65phvAKY4NA0zOBQDaAJAPER"",""expires_in"":44211.676349780093}";
                Log.LogWrite("GetSign tokenTxt", tokenTxt);
                AccessToken at = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessToken>(tokenTxt);
                //判断时间
                if (!string.IsNullOrWhiteSpace(tokenTxt) && at != null && !string.IsNullOrWhiteSpace(at.access_token))
                {
                    var before = DateTime.FromOADate(at.expires_in);
                    if ((DateTime.Now - before).TotalSeconds < 7200)
                    { //没过期
                        byUrl = false;

                        Log.LogWrite("GetSign byUrl", byUrl.ToString());
                    }
                    else
                    {
                        strReturn = GetAccesstokenByUrl();
                        File.WriteAllText(configSavePath, strReturn);
                        Log.LogWrite("GetSign strReturn", strReturn);
                    }
                }
            }
            if (byUrl)
            {
                strReturn = GetAccesstokenByUrl();
                Log.LogWrite("GetSign strReturn2", strReturn);
                File.WriteAllText(configSavePath, strReturn);
            }
            else
                strReturn = tokenTxt;
            return strReturn;
        }

        private static string GetAccesstokenByUrl()
        {
            string tokenUrl = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type={0}&appid={1}&secret={2}", "client_credential", appID, appSecret);
            var wc = new WebClient();
            var strReturn = wc.DownloadString(tokenUrl);

            AccessToken at = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessToken>(strReturn);

            if (at != null & !string.IsNullOrWhiteSpace(at.access_token))
            {
                at.expires_in = DateTime.Now.ToOADate();
                strReturn = Newtonsoft.Json.JsonConvert.SerializeObject(at);
            }
            return strReturn;
        }

        #endregion

        #region 获取Jsapi_Ticket
        public static string GetWeiXinJsapi_Ticket(string accessToken)
        {
            string tokenUrl = string.Format("https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type={1}", accessToken, "jsapi");
            var wc = new WebClient();
            var strReturn = wc.DownloadString(tokenUrl); //取得微信返回的json数据  
            return strReturn;
        }
        #endregion

        #region 基础字符
        private static string[] strs = new string[]
                                   {
                                  "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z",
                                  "A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T","U","V","W","X","Y","Z"
                                   };
        #endregion

        #region 创建随机字符串
        public static string CreatenNonce_str()
        {
            Random r = new Random();
            var sb = new StringBuilder();
            var length = strs.Length;
            for (int i = 0; i < 15; i++)
            {
                sb.Append(strs[r.Next(length - 1)]);
            }
            return sb.ToString();
        }
        #endregion

        #region  创建时间戳
        public static long CreatenTimestamp()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
        #endregion

        #region 签名算法
        /// <summary>
        /// 签名算法
        ///本代码来自开源微信SDK项目：https://github.com/night-king/weixinSDK
        /// </summary>
        /// <param name="jsapi_ticket">jsapi_ticket</param>
        /// <param name="noncestr">随机字符串(必须与wx.config中的nonceStr相同)</param>
        /// <param name="timestamp">时间戳(必须与wx.config中的timestamp相同)</param>
        /// <param name="url">当前网页的URL，不包含#及其后面部分(必须是调用JS接口页面的完整URL)</param>
        /// <returns></returns>
        public static string GetSignature(string jsapi_ticket, string noncestr, long timestamp, string url, out string string1)
        {
            var string1Builder = new StringBuilder();
            string1Builder.Append("jsapi_ticket=").Append(jsapi_ticket).Append("&")
                          .Append("noncestr=").Append(noncestr).Append("&")
                          .Append("timestamp=").Append(timestamp).Append("&")
                          .Append("url=").Append(url.IndexOf("#") >= 0 ? url.Substring(0, url.IndexOf("#")) : url);
            string1 = string1Builder.ToString();
            return FormsAuthentication.HashPasswordForStoringInConfigFile(string1, "SHA1");
        }
        #endregion

        #region 微信地理坐标转换成百度坐标
        public static string GetPoint(string x, string y)
        {
            string tokenUrl = string.Format("http://api.map.baidu.com/geoconv/v1/?coords={0},{1}&from=1&to=5&ak=C4cd2a8c0a40885f9ab67bcd56d7b7c9", x, y);
            var wc = new WebClient();
            var strReturn = wc.DownloadString(tokenUrl); //取得微信返回的json数据  
            return strReturn;
        }
        #endregion

        #region 百度地理坐标转换成微信腾讯地图坐标
        public static string GetPointTX(string x, string y)
        {
            string tokenUrl = string.Format("https://apis.map.qq.com/ws/coord/v1/translate?locations={0},{1}&type=3&key=JTJBZ-27ACS-GSNOK-6FDIV-Y72UE-QYBF3", x, y);
            var wc = new WebClient();
            var strReturn = wc.DownloadString(tokenUrl); //取得微信返回的json数据  
            return strReturn;
        }
        #endregion
    }

    public class AccessToken
    {
        /// <summary>
        /// access_token值
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// url返回的是7200，此处保存当前时间
        /// </summary>
        public double expires_in { get; set; }

    }
}