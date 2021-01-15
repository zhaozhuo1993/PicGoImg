using DBHelper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utils;
using WeChatCoreHandle;

namespace Controller
{
    /// <summary>
    /// 查询订单、报告
    /// </summary>
    public static class ConQuery
    {
        /// <summary>
        /// 获取订单主信息
        /// </summary>
        /// <param name="qs">录入起时</param>
        /// <param name="zs">录入止时</param>
        /// <param name="openid">患者id</param>
        /// <returns></returns>
        public static DataTable GetOrderMain(string qs,string zs,string openid) {
            string sql = @" SELECT
	dj.REGISTER_SN lsh,
	dj.REGISTER_NAME xm,
	dj.ID_NUMBER zjh,
	date_format(dj.INPUT_TIME,
		'%Y-%m-%d %H:%i:%s'
	)  lrsj,
	cj.BAR_NO tmh
FROM register_record dj
LEFT JOIN sample_record cj ON dj.REGISTER_SN = cj.REGISTER_SN
where dj.openid =?openid order by dj.INPUT_TIME desc ";
            MySqlParameter[] mySqlParameter = new MySqlParameter[]
           {
                new MySqlParameter("?openid",openid)
           };
            DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);
            return dt;
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="REGISTER_SN">登记流水号</param>
        /// <returns></returns>
        public static DataTable GetOrderNext(string REGISTER_SN)
        {
            string sql = @"SELECT
	dj.REGISTER_SN lsh,
	dj.HOSPITAL_CODE yydm,
	zf.CHARGE_ORDER_NO wxdh,
	zf.CHARGE_SN shdh,
	zf.COST_MONEY je,
	cj.bar_no tmh,
	tf.register_sn tfsn,
	cc.`NAME` cjdname,
	cc.POSITION address,
	cc.WE_VALUE jdz,
	cc.NS_VALUE wdz
FROM
	register_record dj
LEFT JOIN sample_record cj ON dj.register_sn = cj.REGISTER_SN
LEFT JOIN refund_record tf ON dj.register_sn = tf.register_sn
LEFT JOIN charge_record zf ON dj.register_sn = zf.REGISTER_SN
JOIN code_collectionplace cc ON dj.COLLECTIONPLACE_CODE = cc.`CODE`
WHERE  dj.REGISTER_SN = ?REGISTER_SN "; 
            MySqlParameter[] mySqlParameter = new MySqlParameter[]
           {
                new MySqlParameter("?REGISTER_SN",REGISTER_SN)
           };
            DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);
            return dt;
        }

        /// <summary>
        /// 获取报告列表
        /// </summary>
        /// <returns></returns>
        public static DataTable GetReportMain(string qs, string zs, string openid) {
            string sql = @"SELECT
	dj.register_name xm,
	dj.register_sn lsh,
	date_format(
		bg.TEST_TIME,
		'%Y-%m-%d %H:%i:%s'
	) jcsj,
	ch. NAME yljg,
	bg.REPORT_RESULT jg,
	dj.ID_NUMBER zjh,
	djn.ITEM_NAME xmmc
FROM
	report_record bg
JOIN sample_record cj ON bg.bar_no = cj.bar_no
JOIN register_record dj ON cj.REGISTER_SN = dj.REGISTER_SN
JOIN register_record_detail djn ON dj.register_sn = djn.REGISTER_SN
JOIN code_hospital ch ON cj.hospital_code = ch. CODE
WHERE  
	dj.openid = ?openid  order by bg.TEST_TIME desc ";
            MySqlParameter[] mySqlParameter = new MySqlParameter[]
          {
                new MySqlParameter("?openid",openid)
          };
            DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);
            return dt;
        }

        /// <summary>
        /// 获取报告详情
        /// </summary>
        /// <param name="REGISTER_SN">登记流水号</param>
        /// <returns></returns>
        public static DataTable GetReportNext(string REGISTER_SN)
        {
            DataTable dt = new DataTable();
            string sql = "";
            return dt;
        }


        /// <summary>
        /// 微信退款
        /// </summary>
        /// <param name="transaction_id">微信订单号</param>
        /// <param name="out_trade_no">商户订单号</param>
        /// <param name="total_fee"></param>
        /// <returns>退款金额 单位元</returns>
        public static WeChatPayCore Refund(string transaction_id, string out_trade_no, string total_fee) 
        {
            WeChatPayCore data = new WeChatPayCore(); 
            total_fee = (Convert.ToDouble(total_fee) * 100).ToString();
            if (!string.IsNullOrEmpty(transaction_id))//微信订单号存在的条件下，则已微信订单号为准
            {
                data.SetValue("transaction_id", transaction_id);
            }
            else//微信订单号不存在，才根据商户订单号去退款
            {
                data.SetValue("out_trade_no", out_trade_no);
            }
            data.SetValue("total_fee", int.Parse(total_fee));//订单总金额
            data.SetValue("refund_fee", int.Parse(total_fee));//退款金额
            data.SetValue("out_refund_no", WeChatPayApi.GenerateOutTradeNo());//随机生成商户退款单号
            data.SetValue("op_user_id", CoreConfig.mchid);//操作员，默认为商户号
            Log.LogWrite("诊查费退费3", JsonConvert.SerializeObject(data));
            WeChatPayCore result = WeChatPayApi.Refund(data);//提交退款申请给API，接收返回数据
            Log.LogWrite("诊查费退费4 ", JsonConvert.SerializeObject(result));
            return result;
        }

        private static void testPay() {
            //若传递了相关参数，则调统一下单接口，获得后续相关接口的入口参数
            JSApiPay jsApiPay = new JSApiPay();
            jsApiPay.openid = "";
            jsApiPay.total_fee = (double.Parse("") * 100).ToString("f0");
            jsApiPay.payDesc = "医师诊查费";


            //扩展数据 排班序号、病人ID、预约号、支付类型：0挂号费 1充值
            //jsApiPay.attach = "{\"XH\":\"123456\",\"BRID\":\"1244\",\"PAY_TYPE\":\"0\"}";
            jsApiPay.attach = "";

            WeChatPayCore unifiedOrderResult = jsApiPay.GetUnifiedOrderResult();
            string WeChatJsApiParam = jsApiPay.GetJsApiParameters();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="j"></param>
        /// <returns></returns>
        public static string SaveRefund(JObject j) {
            string sql = @"
INSERT INTO refund_record (
	REGISTER_SN,
	CHARGE_ORDER_NO,
	CHARGE_SN,
	COST_MONEY,
	ITEM_CODE,
	ITEM_NAME,
	REFUND_MONEY,
	REFUND_TIME
) SELECT
	REGISTER_SN,
	?charge_order_no CHARGE_ORDER_NO,
	 ?charge_sn CHARGE_SN,
	a.ITEM_MONEY COST_MONEY,
	a.ITEM_CODE ITEM_CODE,
	a.ITEM_NAME ITEM_NAME,
	a.ITEM_MONEY REFUND_MONEY,
	now() REFUND_TIME
FROM
	register_record_detail a
WHERE
	a.REGISTER_SN = ?register_sn  ";
            MySqlParameter[] mySqlParameter = new MySqlParameter[]
            {
                new MySqlParameter("?register_sn",j["lsh"].ToString()) ,
                new MySqlParameter("?charge_order_no",j["wxdh"].ToString()) ,
                new MySqlParameter("?charge_sn",j["shdh"].ToString()) 
            };
            try
            {
                var rel = MySQLHelper.ExecuteNonQuery(sql, CommandType.Text, mySqlParameter) > 0; 
                if (rel)
                {
                    return "1";
                }
                else
                {
                    return "微信退款成功，保存数据失败！";
                }
            }
            catch (Exception x)
            {
                Log.LogWrite("SaveRefund", "微信退款成功，保存数据失败 Exception：" + x.Message+"\r\t"+sql);
                return "微信退款成功，保存数据失败 Exception："+x.Message;
            }
            
        }
    }
}
