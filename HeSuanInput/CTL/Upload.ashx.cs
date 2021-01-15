using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using Utils;

namespace HeSuanInput.CTL
{
    /// <summary>
    /// Upload 的摘要说明
    /// </summary>
    public class Upload : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            HttpPostedFile file = context.Request.Files["file"];
            Image imageFile = Image.FromStream(file.InputStream);
            string side = "face";
            var rel = OCR.Discern(ConvertImageToBase64(imageFile), side);

            context.Response.Write(rel);
        }

        public string ConvertImageToBase64(Image file)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                file.Save(memoryStream, file.RawFormat);
                byte[] imageBytes = memoryStream.ToArray();
                return Convert.ToBase64String(imageBytes);
            }
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