using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;

namespace WeChatCoreHandle
{
    public class CoreConfig
    {
         

        /// <summary>
        /// 微信公众号开发者ID(AppID)  医院的
        /// </summary>
        public static string AppID
        {
            get { return ConfigurationManager.AppSettings["AppID"].ToString(); }
        }


        /// <summary>
        /// 开发者密码(AppSecret)
        /// </summary>
        public static string AppSecret
        {
            get { return ConfigurationManager.AppSettings["AppSecret"].ToString(); }
        }


        /// <summary>
        /// 商户号--（公众平台）
        /// </summary>
        public static readonly string mchid = ConfigurationManager.AppSettings["mchid"].ToString();

        /// <summary>
        /// 商户api秘钥
        /// </summary>
        public static readonly string mchsecret = ConfigurationManager.AppSettings["mchsecret"].ToString();


        /// <summary>
        /// 调用微信支付api的终端ip
        /// </summary>
        public static readonly string payIP = "182.92.195.138";


        /// <summary>
        /// 证书物理地址
        /// </summary>
        public static readonly string SSLCERT_PATH = ConfigurationManager.AppSettings["cert_path"].ToString();


        /// <summary>
        /// 证书密码
        /// </summary>
        public static readonly string SSLCERT_PASSWORD = ConfigurationManager.AppSettings["SSLCERT_PASSWORD"].ToString();

        ///<summary>
        ///微信支付结果异步通知的地址，不能携带参数 
        /// </summary>
        public static readonly string NOTIFY_URL = ConfigurationManager.AppSettings["NOTIFY_URL"].ToString();


        //=======【上报信息配置】===================================
        /* 测速上报等级，0.关闭上报; 1.仅错误时上报; 2.全量上报
        */
        public const int REPORT_LEVENL = 1;

        //=======【日志级别】===================================
        /* 日志等级，0.不输出日志；1.只输出错误信息; 2.输出错误和正常信息; 3.输出错误信息、正常信息和调试信息
        */
        public const int LOG_LEVENL = 0;


        /// <summary>
        /// 令牌(Token)
        /// </summary>
        public static string Token
        {
            get { return ConfigurationManager.AppSettings["Token"].ToString(); }
        }


        /// <summary>
        /// 消息加解密密钥(EncodingAESKey)
        /// </summary>
        public static string EncodingAESKey
        {
            get { return ConfigurationManager.AppSettings["EncodingAESKey"].ToString(); }
        }


        /// <summary>
        /// 加密签名
        /// </summary>
        public const string SIGNATURE = "signature";

        /// <summary>
        /// 时间戳
        /// </summary>
        public const string TIMESTAMP = "timestamp";
        /// <summary>
        /// 随机数
        /// </summary>
        public const string NONCE = "nonce";
        /// <summary>
        /// 随机字符串
        /// </summary>
        public const string ECHOSTR = "echostr";

        /// <summary>
        /// 发送人
        /// </summary>
        public const string FROM_USERNAME = "FromUserName";
        /// <summary>
        /// 开发者微信号
        /// </summary>
        public const string TO_USERNAME = "ToUserName";
        /// <summary>
        /// 消息内容
        /// </summary>
        public const string CONTENT = "Content";
        /// <summary>
        /// 消息创建时间 （整型）
        /// </summary>
        public const string CREATE_TIME = "CreateTime";
        /// <summary>
        /// 消息类型
        /// </summary>
        public const string MSG_TYPE = "MsgType";
        /// <summary>
        /// 消息id，64位整型
        /// </summary>
        public const string MSG_ID = "MsgId";

        /// <summary>
        /// 得到当前时间（整型）（考虑时区）
        /// </summary>
        /// <returns></returns>
        public static string GetNowTime()
        {
            DateTime timeStamp = new DateTime(1970, 1, 1);  //得到1970年的时间戳
            long a = (DateTime.UtcNow.Ticks - timeStamp.Ticks) / 10000000;
            return a.ToString();
        }

        /// <summary>
        /// 读取请求对象的内容 只能读一次
        /// </summary>
        /// <param name="request">HttpRequest对象</param>
        /// <returns>string</returns>
        public static string ReadRequest(HttpRequest request)
        {
            string reqStr = string.Empty;
            using (Stream s = request.InputStream)
            {
                using (StreamReader reader = new StreamReader(s, Encoding.UTF8))
                {
                    reqStr = reader.ReadToEnd();
                }
            }

            return reqStr;
        }






    }
}
