using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utils;

namespace HeSuanInput.CTL
{

    /// <summary>
    /// Manager 的摘要说明
    /// </summary>
    public class Manager : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string operate = context.Request["operate"];
            switch (operate)
            {
                case "GetLoginInfo":
                    GetLoginInfo(context);
                    break;
                case "GetReportInfo":
                    GetReportInfo(context);
                    break;
                case "GetSampleInfo":
                    GetSampleInfo(context);
                    break;
                case "GetNewBarNO":
                    GetNewBarNO(context);
                    break;
                case "ExpExcelSampleInfo":
                    ExpExcelSampleInfo(context);
                    break;
                case "UpdateReportResultInfo":
                    UpdateReportResultInfo(context);
                    break;
            };
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        void UpdateReportResultInfo(HttpContext context)
        {
            string json = context.Request["result"];
            context.Response.Write(Controller.Manager.UpdateReportResultInfo(json));
        }

        void ExpExcelSampleInfo(HttpContext context)
        {
            string start = context.Request["start"];
            string end = context.Request["end"];
            string hospital_code = context.Request["hospital_code"];
            string place_code = context.Request["place_code"];
            var dt = Controller.Manager.ExpExcelSampleInfo(start, end, hospital_code, place_code);
            var fileName = "核酸集采信息" + GetTimeStamp();
            new ExcelHelper().ExportToExcel1(dt, fileName);
            context.Response.Write("../excel/" + fileName + ".xls");
        }



        void GetNewBarNO(HttpContext context)
        {
            context.Response.Write(Controller.Manager.GetNewBarNO());
        }

        void GetReportInfo(HttpContext context)
        {
            string start = context.Request["start"];
            string end = context.Request["end"];
            string hospital_code = context.Request["hospital_code"];
            string place_code = context.Request["place_code"];

            context.Response.Write(Controller.Manager.GetReportInfo(start, end, hospital_code, place_code));

        }

        void GetSampleInfo(HttpContext context)
        {
            string start = context.Request["start"];
            string end = context.Request["end"];
            string hospital_code = context.Request["hospital_code"];
            string place_code = context.Request["place_code"];

            context.Response.Write(Controller.Manager.GetSampleInfo(start, end, hospital_code, place_code));

        }

        void GetLoginInfo(HttpContext context)
        {
            string code = context.Request["code"];
            string pwd = context.Request["pwd"];

            context.Response.Write(Controller.Manager.Login(code, pwd));

        }

        public string GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
    }
}