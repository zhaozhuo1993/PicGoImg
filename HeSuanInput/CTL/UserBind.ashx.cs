using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Controller;
using Newtonsoft.Json;
using Utils;

namespace HeSuanInput.CTL
{
    /// <summary>
    /// UserBind 的摘要说明
    /// </summary>
    public class UserBind : IHttpHandler
    {
        // 定义 私有静态只读 对象  防止产生死锁
        private static readonly object objlock = new object();
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string sign = context.Request["sign"];
            string yyid = context.Request["yyid"];
            string json = "";

            switch (sign)
            {
                case "gethos":
                    json = Control.GetHos(yyid);
                    break;
                case "saveinfo":
                    string xm = context.Request["xm"]; 
                    string sfzh = context.Request["sfzh"]; 
                    string xb = context.Request["xb"]; 
                    string nl = context.Request["nl"]; 
                    string sjh = context.Request["sjh"]; 
                    string xzz = context.Request["xzz"]; 
                    string gfx = context.Request["gfx"];
                    string fr = context.Request["fr"];
                    string zh = context.Request["zh"].ToString();
                    string ryfl = context.Request["ryfl"];
                    string sfzf = context.Request["sfzf"];
                    lock (objlock)
                    {
                        try
                        {
                            if (context.Application[yyid + "-" + zh + "-xh"] == null)
                            {
                                context.Application[yyid + "-" + zh + "-xh"] = Control.GetMaxXh(yyid, zh);
                            }
                        }
                        catch (Exception)
                        {
                            context.Application[yyid + "-" + zh + "-xh"] = 0;
                        }
                        context.Application[yyid + "-" + zh + "-xh"] = (int)context.Application[yyid + "-" + zh + "-xh"] + 1;
                        string xh = context.Application[yyid + "-" + zh + "-xh"].ToString();
                        json = Control.AddInfo(xm, sfzh, xb, nl, sjh, xzz, gfx, fr, yyid, zh, xh, ryfl, sfzf);
                    }
                    break;
                case "GetHsryxxByYyid":
                    string btime = context.Request["btime"];
                    string etime = context.Request["etime"];
                    string para = context.Request["para"];
                    string hsjg = context.Request["hsjg"];
                    json = JsonConvert.SerializeObject(Control.GetHsryxxByYyid(yyid, btime, etime, para, hsjg));
                    break; 
                case "Login":
                    string code = context.Request["code"];
                    string pwd = context.Request["pwd"];
                    json = Control.Login(code, pwd);
                    break;
                case "UpdatePwd":
                    string ypwd = context.Request["ypwd"];
                    var rel = Control.GetYyInfo(yyid, ypwd);
                    if (rel)
                    {
                        string password = context.Request["pwd"];
                        json = Control.UpdatePwd(yyid, password);
                    }
                    else
                    {
                        json = "原密码错误";
                    }
                    break;
                case "export":
                    string betime = context.Request["btime"];
                    string entime = context.Request["etime"];
                    string paraa = context.Request["para"];
                    string hsjgg = context.Request["hsjg"];
                    var dt = Control.GetHsryxxExport(yyid, betime, entime, paraa, hsjgg);
                    var fileName = "核酸集采信息" + GetTimeStamp();
                    new ExcelHelper().ExportToExcel1(dt, fileName);
                    json = "../excel/" + fileName + ".xls";
                    break;
                case "GenerateResult":
                    string jsonstr = context.Request["list"];
                    json = Control.GenerateResult(jsonstr);
                    break;
                case "UpdateHsjg":
                    string upthsjg = context.Request["upthsjg"];
                    string bh = context.Request["bh"];
                    json = Control.UpdateHsjg(upthsjg, bh, yyid);
                    break;
                case "GetHsjg":
                    string xm1 = context.Request["xm"];
                    string sfzh1 = context.Request["sfzh"];
                    string sjh1 = context.Request["sjh"];
                    json = JsonConvert.SerializeObject(Control.GetHsjg(yyid, xm1, sfzh1, sjh1));
                    break;
            }
            context.Response.Write(json);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public string GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
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