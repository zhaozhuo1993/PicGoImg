using Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utils;

namespace HeSuanInput.CTL
{
    /// <summary>
    /// HSLive 的摘要说明
    /// </summary>
    public class HSLive : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {


            string operate = context.Request.QueryString["OPERATE"];
            switch (operate)
            {
                case "Login":
                case "GetYYPatInfo": GetYYPatInfo(context); break;
                case "GetYYItemsInfoBySN": GetYYItemsInfoBySN(context); break;
                case "HSLiveRegister": HSLiveRegister(context); break;
                case "GetNewBarNO": GetNewBarNO(context); break;
                case "PrintNewBar": PrintNewBar(context); break;
                case "WxMicroPay": WxMicroPay(context); break;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        void GetYYPatInfo(HttpContext context)
        {

            string type = context.Request["type"];
            string val = context.Request["val"];
            string yyid = context.Request["yyid"];
            Log.LogWrite("type:", type);
            Log.LogWrite("val:", val);
            Log.LogWrite("yyid:", yyid);
            context.Response.Write(Control_HSLive.GetYYPatInfo(type, val, yyid));

        }
        void GetYYItemsInfoBySN(HttpContext context)
        {
            string val = context.Request["reg_no"];
            string yyid = context.Request["yyid"];
            context.Response.Write(Control_HSLive.GetYYItemsInfoBySN(val, yyid));

        }
        void HSLiveRegister(HttpContext context)
        {
            string val = context.Request["reg_no"];
            string yyid = context.Request["yyid"];
            string bblx = context.Request["bblx"];
            string cjd = context.Request["cjd"];
            context.Response.Write(Control_HSLive.HSLiveRegister(val, yyid, bblx, cjd));

        }
        void GetNewBarNO(HttpContext context)
        {
            context.Response.Write(Control_HSLive.GetNewBarNO());
        }
        void PrintNewBar(HttpContext context)
        {
            string reg_sns = context.Request["reg_sns"];
            context.Response.Write(Control_HSLive.PrintNewBar(reg_sns));
        }
        void WxMicroPay(HttpContext context)
        {

        }
    }
}