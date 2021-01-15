using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace WeChatCoreHandle
{

    /// <summary>
    /// 微信核心数据处理
    /// </summary>
    public class CoreHandle
    {

        private static DateTime GetAccessToken_Time;
        /// <summary>
        /// 过期时间为7200秒
        /// </summary>
        private static int Expires_Period = 7200;
        /// <summary>
        ///
        /// </summary>
        private static string mAccessToken;

        /// <summary>
        ///Access_token属性
        /// </summary>
        public static string AccessToken
        {
            get
            {
                //如果为空，或者过期，需要重新获取
                if (string.IsNullOrEmpty(mAccessToken) || HasExpired())
                {
                    //获取
                    mAccessToken = GetAccessToken(CoreConfig.AppID, CoreConfig.AppSecret);
                }
                return mAccessToken;
            }
        }

        /// <summary>
        /// 获取 Access_token属性
        /// </summary>
        /// <returns></returns>
        public static string GetAccessToken() {
            /*
             因为在别的类中调用 静态属性AccessToken报错 所以增加此方法
             */
            //如果为空，或者过期，需要重新获取
            if (string.IsNullOrEmpty(mAccessToken) || HasExpired())
            {
                //获取
                mAccessToken = GetAccessToken(CoreConfig.AppID, CoreConfig.AppSecret);
            }
            return mAccessToken;
        }



        /// <summary>
        ///获取Access_token
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="appSecret"></param>
        /// <returns></returns>
        private static string GetAccessToken(string appId, string appSecret)
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", appId, appSecret);
            string result = HttpUtility.GetData(url);
            XDocument doc = ParseJson(result, "root");
            XElement root = doc.Root;
            if (root != null)
            {
                XElement access_token = root.Element("access_token");
                if (access_token != null)
                {
                    GetAccessToken_Time = DateTime.Now;
                    if (root.Element("expires_in") != null)
                    {
                        Expires_Period = int.Parse(root.Element("expires_in").Value);
                    }
                    return access_token.Value;
                }
                else
                {
                    GetAccessToken_Time = DateTime.MinValue;
                }
            }
            return null;
        }

        /// <summary>
        /// 判断Access_token是否过期
        /// </summary>
        /// <returns>bool</returns>
        private static bool HasExpired()
        {
            if (GetAccessToken_Time != null)
            {
                //过期时间，允许有一定的误差，一分钟。获取时间消耗
                if (DateTime.Now > GetAccessToken_Time.AddSeconds(Expires_Period).AddSeconds(-60))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 网页授权获取加密微信号openid
        /// </summary>
        /// <param name="code">网页授权code</param>
        /// <returns></returns>
        public static string GetOpenid(string code)
        {
            string url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", CoreConfig.AppID, CoreConfig.AppSecret, code);
            string result = HttpUtility.GetData(url);
            //System.IO.File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "\\log.txt", "++++++++++++++" + result + "+++++++++++++++++++++++", System.Text.Encoding.Default);
            XDocument doc = ParseJson(result, "root");
            XElement root = doc.Root;
            if (root != null)
            {
                XElement open_id = root.Element("openid");
                if (open_id != null)
                {
                    return open_id.Value;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="json"></param>
        /// <param name="rootName"></param>
        /// <returns></returns>
        public static XDocument ParseJson(string json, string rootName)
        {
            return JsonConvert.DeserializeXNode(json, rootName);
        }

        /// <summary>
        /// 获取微信用户关注数量
        /// </summary>
        /// <returns></returns>
        public static string GetFollow()
        {
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/user/get?access_token={0}&next_openid={1 }", AccessToken, "");
            string result = HttpUtility.GetData(url);
            XDocument doc = ParseJson(result, "root");
            XElement root = doc.Root;
            if (root != null)
            {
                XElement access_token = root.Element("total");
                XElement next_openid = root.Element("next_openid");
                return access_token.Value + ";" + next_openid.Value;
            }
            return "[]";
        }

        /// <summary>
        /// 返回微信生成的带场景二维码地址字符串
        /// </summary>
        /// <param name="paraStr"></param>
        /// <param name="expire_seconds">二维码有效期 以秒为单位</param>
        /// <returns></returns>
        public static string QRcode(string paraStr, string expire_seconds)
        {
            if (string.IsNullOrEmpty(expire_seconds))
            {
                expire_seconds = "86400";
            }
            string url = string.Format("https://api.weixin.qq.com/cgi-bin/qrcode/create?access_token={0}", AccessToken);
            var data = "{\"expire_seconds\":\"" + expire_seconds + "\",\"action_name\": \"QR_LIMIT_STR_SCENE\", \"action_info\": {\"scene\": {\"scene_str\":\"" + paraStr + "\"}}}";
            string result = HttpUtility.SendHttpRequest(url, data);
            //Log.LogWrite("QRcode", result);
            ResultWX rw = JsonConvert.DeserializeObject<ResultWX>(result);
            if (rw.url != null)
            {
                return rw.url;
            }
            return null;
        }

    }

    public class ResultWX
    {
        public string ticket { get; set; }
        public string expire_seconds { get; set; }
        public string url { get; set; }
    }

}
