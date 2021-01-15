using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Utils
{
    public class OCR
    {
        public static string Discern(string imgBase64, string side)
        {
            String appcode = "128c41a1272a4551adaf1ce1ab46ea02";
            //face:表示正面；back：表示反面
            String config = "{\"side\": \"" + side + "\"}";

            String method = "POST";
            String url = "http://dm-51.data.aliyun.com/rest/160601/ocr/ocr_idcard.json";

            String bodys;
            //if (img_file.StartsWith("http"))
            //{
            //    bodys = "{\"image\":\"" + img_file + "\",\"configure\":" + config + "}";
            //}
            //else
            //{
                //FileStream fs = new FileStream(img_file, FileMode.Open);
                //BinaryReader br = new BinaryReader(fs);
                //byte[] contentBytes = br.ReadBytes(Convert.ToInt32(fs.Length));
                //String base64 = System.Convert.ToBase64String(contentBytes);
            bodys = "{\"image\":\"" + imgBase64 + "\",\"configure\":" + config + "}";
            //};

            HttpWebRequest httpRequest = null;
            HttpWebResponse httpResponse = null;
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            httpRequest = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            httpRequest.Method = method;
            httpRequest.Headers.Add("Authorization", "APPCODE " + appcode);
            //根据API的要求，定义相对应的Content-Type
            httpRequest.ContentType = "application/json; charset=UTF-8";

            if (0 < bodys.Length)
            {
                byte[] data = Encoding.UTF8.GetBytes(bodys);
                using (Stream stream = httpRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            try
            {
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            }
            catch (WebException ex)
            {
                Log.LogWrite("Discern-catch", ex.Message);
                httpResponse = (HttpWebResponse)ex.Response;
            }
            Stream st = httpResponse.GetResponseStream();
            StreamReader reader = new StreamReader(st, Encoding.GetEncoding("utf-8"));
            return reader.ReadToEnd();
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
