using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WeChatCoreHandle;
using Controller;
using Newtonsoft.Json.Linq;
using Utils;

namespace HeSuanInput.CTL
{
    /// <summary>
    /// 查询 我的订单、报告
    /// 和退费
    /// </summary>
    public class Query : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string sign = context.Request["sign"];
            jsonRes r = new jsonRes();
            switch (sign)
            {
                case "GetOpeinid":
                    GetOpeinid(context, r);
                    break;
                case "GetOrderMain":
                    GetOrderMain(context,r);
                    break;
                case "GetOrderNext":
                    GetOrderNext(context, r);
                    break;
                case "Refund":
                    Refund(context, r);
                    break;
                case "GetListReport":
                    GetListReport(context, r);break;
                case "GetReportImg":
                    GetReportImg(context, r);break;
                default:
                    r.code = "0";
                    r.msg = "方法名称参数sign错误！";
                    break;
            }
            context.Response.Write(JsonConvert.SerializeObject(r));
        }

        /// <summary>
        /// 获取报告图片
        /// </summary>
        /// <param name="context"></param>
        /// <param name="r"></param>
        private void GetReportImg(HttpContext context, jsonRes r)
        {
             
        }

        /// <summary>
        /// 获取报告列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="r"></param>
        private void GetListReport(HttpContext context, jsonRes r)
        {
            try
            {
                r.data = ConQuery.GetReportMain("", "", context.Request.Form["openid"]);
                if (r.data.Rows.Count==0)
                {
                    r.code = "0";
                    r.msg = "未查询到数据";
                }
            }
            catch (Exception ex)
            {
                r.code = "0";
                r.msg = "查询报错:" + ex.Message;
            }
        }

        void Refund(HttpContext context, jsonRes j) {
            /*
             微信退费
             保存退费到云数据库
             返回退费结果
             */
            string wxdh = context.Request.Form["wxdh"],
               shdh = context.Request.Form["shdh"],
               je = context.Request.Form["je"],
               lsh = context.Request.Form["lsh"];
            try
            {
                WeChatPayCore result = ConQuery.Refund(wxdh, shdh, je);
                if (result.GetValue("return_code").ToString() == "SUCCESS")
                {
                    JObject in_j = new JObject();
                    in_j["lsh"] = lsh;
                    in_j["wxdh"] = result.GetValue("transaction_id").ToString();//微信单号
                    in_j["shdh"] = result.GetValue("out_refund_no").ToString();//商户单号

                    string r = ConQuery.SaveRefund(in_j);
                    if (r == "1")
                    {
                        j.msg = "退费成功!";
                    }
                    else
                    {
                        j.code = "0";
                        j.msg = r;
                    }
                }
                else
                {
                    Utils.Log.LogWrite("微信退费失败，WeChatPayCore值：", JsonConvert.SerializeObject(result));
                    j.code = "0";
                    j.msg = "退费失败";
                }
            }
            catch (Exception ex)
            {
                j.code = "0";
                j.msg = "退费报错:"+ex.Message;
            }
        }


        /// <summary>
        /// 根据code 获取openid
        /// </summary>
        /// <param name="context"></param>
        /// <param name="j"></param>
        void GetOpeinid(HttpContext context, jsonRes j) {
            string code = context.Request.Form["code"];
            try
            {
                j.msg = CoreHandle.GetOpenid(code);//按code查询openid
                Utils.Log.LogWrite("根据code 获取openid", "code："+ code+ "----Opeinid：" + j.msg);
            }
            catch (Exception ex)
            {
                j.code = "0";
                j.msg = "获取id失败："+ex.Message;
                Log.LogWrite("根据code 获取openid Exception", "code：" + code + "----Opeinid：" + j.msg);
            }
        }


        /// <summary>
        /// 查询订单 主要信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="j">返回的对象</param>
        void GetOrderMain(HttpContext context,jsonRes j) {
            string openid = context.Request.Form["openid"];
            string qs = context.Request.Form["qs"] ?? "";
            string zs = context.Request.Form["zs"] ?? "";
            j.data =  ConQuery.GetOrderMain(qs,zs,openid);
            if (j.data.Rows.Count==0)
            {
                j.code = "0";
                j.msg = "未查询到数据！";
            }
        }



        /// <summary>
        /// 查询订单 详细信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="j">返回的对象</param>
        void GetOrderNext(HttpContext context, jsonRes j)
        {
            string lsh = context.Request.Form["lsh"]; 
            j.data = Controller.ConQuery.GetOrderNext(lsh);
            if (j.data.Rows.Count == 0)
            {
                j.code = "0";
                j.msg = "未查询到数据！";
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


    public class jsonRes {
        public jsonRes() {
            this.code = "1";
            this.msg = "操作成功";
        }
        /// <summary>
        /// 0 错误
        /// 1 正确
        /// </summary>
        public string code { get; set; }
        public string msg { get; set; }
        public DataTable data { get; set; }
    }
}