using System;
using System.Collections.Generic;
using System.IO; 
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;  
using System.Configuration;
using Utils;
using System.Runtime.Serialization.Json;
using System.Data;
using MySql.Data.MySqlClient;

namespace WeChatCoreHandle
{
    /// <summary>
    /// 支付结果通知回调处理类
    /// 负责接收微信支付后台发送的支付结果并对订单有效性进行验证，将验证结果反馈给微信支付后台
    /// </summary>
    public class ResultNotify : Notify
    {
        public ResultNotify(Page page)
            : base(page)
        {

        }

        public override void ProcessNotify()
        {
            WeChatPayCore notifyData = GetNotifyData();

            //检查支付结果中transaction_id是否存在
            if (!notifyData.IsSet("transaction_id"))
            {
                //若transaction_id不存在，则立即返回结果给微信支付后台
                WeChatPayCore res = new WeChatPayCore();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "支付结果中微信订单号不存在");
                page.Response.Write(res.ToXml());
                page.Response.End();
            }

            string transaction_id = notifyData.GetValue("transaction_id").ToString();
            Log.LogWrite("支付transaction_id", transaction_id);
            //查询订单，判断订单真实性
            if (!QueryOrder(transaction_id))
            {
                //若订单查询失败，则立即返回结果给微信支付后台
                WeChatPayCore res = new WeChatPayCore();
                res.SetValue("return_code", "FAIL");
                res.SetValue("return_msg", "订单查询失败");
                page.Response.Write(res.ToXml());
                page.Response.End();
            }
            //查询订单成功
            else
            {
                WeChatPayCore res = new WeChatPayCore();
                res.SetValue("return_code", "SUCCESS");
                res.SetValue("return_msg", "OK");
                if (notifyData.IsSet("attach"))
                {
                    string attch = notifyData.GetValue("attach").ToString().Replace("+", " ");
                    Log.LogWrite("查询订单成功attch", attch);
                    if (!string.IsNullOrEmpty(attch))
                    {
                        PayInfo payInfo = new PayInfo();
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(payInfo.GetType());
                        MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(attch));
                        payInfo = (PayInfo)serializer.ReadObject(mStream);
                        try
                        {
                            string sql = @"INSERT INTO charge_record (REGISTER_SN,CHARGE_ORDER_NO,CHARGE_SN,COST_MONEY,CHARGE_TYPE,CHARGE_TIME)
                                           VALUES(?REGISTER_SN,?CHARGE_ORDER_NO,?CHARGE_SN,?COST_MONEY,'01',NOW())";

                            MySqlParameter[] mySqlParameter = new MySqlParameter[]
                            {
                            new MySqlParameter("?REGISTER_SN",payInfo.LSH),
                            new MySqlParameter("?CHARGE_ORDER_NO",notifyData.GetValue("transaction_id").ToString()),
                            new MySqlParameter("?CHARGE_SN",notifyData.GetValue("out_trade_no").ToString()),
                            new MySqlParameter("?COST_MONEY",(double.Parse(notifyData.GetValue("total_fee").ToString()) / 100).ToString())
                            };
                            MSHelper.ExecuteNonQuery(sql, CommandType.Text, mySqlParameter);
                        }
                        catch (Exception ex)
                        {
                            Log.LogWrite("插入交易记录表", ex.Message);
                            page.Response.Write(res.ToXml());
                            page.Response.End();
                        }
                    }
                }


                //#region 支付业务处理

                //if (Pay.IsExitPayNumb(transaction_id))
                //{
                //    page.Response.Write(res.ToXml());
                //    page.Response.End();
                //}

                //if (notifyData.IsSet("attach"))
                //{
                //    /*XH字段中有空格，有时这个空格会变成+号 导致his业务失败 替换+号为空格 2020年4月2日*/
                //    string attch = notifyData.GetValue("attach").ToString().Replace("+"," ");
                //    Log.LogWrite("查询订单成功attch", attch);
                //    if (!string.IsNullOrEmpty(attch))
                //    {

                //        PayInfo payInfo = new PayInfo();
                //        DataContractJsonSerializer serializer = new DataContractJsonSerializer(payInfo.GetType());
                //        MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(attch));
                //        payInfo = (PayInfo)serializer.ReadObject(mStream);
                //        PayInfo payInfos = new PayInfo();

                //        payInfos.XH = payInfo.XH;
                //        if (payInfo.PAY_TYPE == "2")
                //        {
                //            payInfos.ZYH = payInfo.ZYH;
                //        }
                //        else
                //        {
                //            payInfos.BRID = payInfo.BRID;
                //        }
                //        payInfos.PAY_TYPE = payInfo.PAY_TYPE;
                //        payInfos.SD = payInfo.SD;
                //        payInfos.ID = notifyData.GetValue("out_trade_no").ToString();
                //        payInfos.WX_ID = notifyData.GetValue("openid").ToString();
                //        payInfos.PAY_TIME = notifyData.GetValue("time_end").ToString();
                //        payInfos.PAY_NUMBER = notifyData.GetValue("transaction_id").ToString();
                //        payInfos.PAY_MONEY = (double.Parse(notifyData.GetValue("total_fee").ToString()) / 100).ToString();
                //        Log.LogWrite("查询订单成功xh", payInfo.XH);
                //        Log.LogWrite("查询订单成功BRID", payInfo.BRID);
                //        Log.LogWrite("查询订单成功PAY_TYPE", payInfo.PAY_TYPE);
                //        Log.LogWrite("查询订单成功SD", payInfo.SD);
                //        Log.LogWrite("查询订单成功ID", notifyData.GetValue("out_trade_no").ToString());
                //        Log.LogWrite("查询订单成功WX_ID", notifyData.GetValue("openid").ToString());
                //        Log.LogWrite("查询订单成功PAY_TIME", notifyData.GetValue("time_end").ToString());
                //        Log.LogWrite("查询订单成功PAY_NUMBER", notifyData.GetValue("transaction_id").ToString());
                //        Log.LogWrite("查询订单成功PAY_MONEY", (double.Parse(notifyData.GetValue("total_fee").ToString()) / 100).ToString());
                //        try
                //        {
                //            WeChatDAL.Pay.SavePayInfo(payInfos);
                //            WebChartCoreHandle.HIS_WeChat.WeChatWS_JGYY yy = new WebChartCoreHandle.HIS_WeChat.WeChatWS_JGYY();
                //            yy.Url = ConfigurationManager.AppSettings["HIS_WS"].ToString();
                //            yy.Timeout = 20000;

                //            if (payInfos.PAY_TYPE == "0")//诊查费支付
                //            {

                //                #region 诊查费支付及回传HIS处理

                //                //string json = "{\"BRID\":\"" + payInfo.BRID + "\",\"SD\":\"" + payInfo.SD + "\", \"XH\":\"" + payInfo.XH + "\", \"DH\":\"" + payInfos.PAY_NUMBER + "\"}";
                //                string json = "{\"BRID\":\"" + payInfo.BRID + "\",\"SD\":\"" + payInfo.SD + "\", \"XH\":\"" + payInfo.XH + "\", \"DH\":\"" + payInfos.ID + "\"}";
                //                Log.LogWrite("诊查费支付及回传HIS处理入参", json);
                //                string rsult = yy.Register(json);
                //                Log.LogWrite("诊查费支付及回传HIS处理出参", rsult);
                //                //解析his结果json
                //                JZK jzk = new JZK();
                //                DataContractJsonSerializer serializerResult = new DataContractJsonSerializer(jzk.GetType());
                //                MemoryStream mStreamResult = new MemoryStream(Encoding.UTF8.GetBytes(rsult));
                //                jzk = (JZK)serializerResult.ReadObject(mStreamResult);
                //                if (jzk.CODE == "1")
                //                {
                //                    List<item_jzk> list = jzk.MESSAGE;
                //                    payInfos.YYH = list[0].YYH;
                //                    WeChatDAL.AppointmentRegistration.SaveGH(payInfos);
                //                    string messageContent = list[0].RQ + " " + list[0].AMPM + " " + list[0].KSMC + " " + list[0].LXMC + " " + list[0].YSMC;
                //                    string yyh = "";
                //                    if (payInfo.JZK == "-")
                //                    {
                //                        yyh = payInfos.YYH + "(就诊编号:" + payInfo.BRID + ")";
                //                        Send.SendMessageWk(MessageCore.GetMobByopenid(payInfos.WX_ID), messageContent, yyh);
                //                        Log.LogWrite("短信通知brid", yyh);
                //                    }
                //                    else
                //                    {
                //                        Send.SendMessage(MessageCore.GetMobByopenid(payInfos.WX_ID), messageContent, payInfos.YYH);
                //                    }
                //                    Log.LogWrite("ProcessNotify", "ProcessNotify: 病人ID为" + payInfo.BRID + "的患者，预约序号为" + payInfos.XH + "。 预约信息回传HIS成功！");
                //                }
                //                else
                //                {
                //                    Log.LogWrite("ProcessNotify", rsult);
                //                    Log.LogWrite("ProcessNotify", "ProcessNotify: 病人ID为" + payInfo.BRID + "的患者，预约序号为" + payInfos.XH + "。 预约信息回传HIS失败！");
                //                }

                //                #endregion
                //            }
                //            else if (payInfos.PAY_TYPE == "2")//住院押金充值
                //            {

                //                //web服务 住院充值
                //                #region 就诊卡充值及回传HIS处理

                //                //string json = "{\"ZYH\":\"" + payInfo.ZYH + "\", \"DH\":\"" + payInfos.ID + "\",\"CZJE\":\"" + payInfos.PAY_MONEY + "\"}";
                //                string json = "{\"ZYH\":\"" + payInfo.ZYH + "\", \"DH\":\"" + payInfos.ID + "\",\"CZJE\":\"" + payInfos.PAY_MONEY + "\"}";
                //                string rsult = yy.ChargeZY(json);

                //                //解析his结果json
                //                JZK jzk = new JZK();
                //                DataContractJsonSerializer serializerResult = new DataContractJsonSerializer(jzk.GetType());
                //                MemoryStream mStreamResult = new MemoryStream(Encoding.UTF8.GetBytes(rsult));
                //                jzk = (JZK)serializerResult.ReadObject(mStreamResult);
                //                if (jzk.CODE == "1")
                //                {
                //                    Log.LogWrite("ProcessNotify", "ProcessNotify: 住院号为" + payInfo.ZYH + "的患者，充值单号为" + payInfos.PAY_NUMBER + "。 住院充值回传HIS成功！");
                //                }
                //                else
                //                {
                //                    Log.LogWrite("ProcessNotify", rsult);
                //                    Log.LogWrite("ProcessNotify", "ProcessNotify: 住院号为" + payInfo.ZYH + "的患者，充值单号为" + payInfos.PAY_NUMBER + "。 住院充值回传HIS失败！");
                //                }

                //                #endregion
                //            }
                //            else
                //            {


                //                #region 就诊卡充值及回传HIS处理 
                //                //string json = "{\"BRID\":\"" + payInfo.BRID + "\", \"DH\":\"" + payInfos.PAY_NUMBER + "\",\"CZJE\":\"" + payInfos.PAY_MONEY + "\"}";
                //                string json = "{\"BRID\":\"" + payInfo.BRID + "\", \"DH\":\"" + payInfos.ID + "\",\"CZJE\":\"" + payInfos.PAY_MONEY + "\"}";
                //                string rsult = yy.Charge(json);

                //                //解析his结果json
                //                JZK jzk = new JZK();
                //                DataContractJsonSerializer serializerResult = new DataContractJsonSerializer(jzk.GetType());
                //                MemoryStream mStreamResult = new MemoryStream(Encoding.UTF8.GetBytes(rsult));
                //                jzk = (JZK)serializerResult.ReadObject(mStreamResult);
                //                if (jzk.CODE == "1")
                //                {
                //                    Log.LogWrite("ProcessNotify", "ProcessNotify: 病人ID为" + payInfo.BRID + "的患者，充值单号为" + payInfos.PAY_NUMBER + "。 就诊卡充值回传HIS成功！");
                //                }
                //                else
                //                {
                //                    Log.LogWrite("ProcessNotify", rsult);
                //                    Log.LogWrite("ProcessNotify", "ProcessNotify: 病人ID为" + payInfo.BRID + "的患者，充值单号为" + payInfos.PAY_NUMBER + "。 就诊卡充值回传HIS失败！");
                //                }

                //                #endregion
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            Log.LogWrite("ProcessNotify", ex.Message);
                //            page.Response.Write(res.ToXml());
                //            page.Response.End();
                //        }
                //    }
                //}

                //#endregion

                page.Response.Write(res.ToXml());
                page.Response.End();
            }
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="transaction_id"></param>
        /// <returns></returns>
        private bool QueryOrder(string transaction_id)
        {
            WeChatPayCore req = new WeChatPayCore();
            req.SetValue("transaction_id", transaction_id);
            WeChatPayCore res = WeChatPayApi.OrderQuery(req);
            if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                res.GetValue("result_code").ToString() == "SUCCESS")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        


    }
    public class PayInfo
    {
        public string LSH { get; set; }
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 微信openid
        /// </summary>
        public string WX_ID { get; set; }

        /// <summary>
        /// 微信支付订单号
        /// </summary>
        public string PAY_NUMBER { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public string PAY_TIME { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>
        public string PAY_MONEY { get; set; }

        /// <summary>
        /// 支付类型 
        /// </summary>
        public string PAY_TYPE { get; set; }
    }
}