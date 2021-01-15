using DBHelper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using WeChatCoreHandle;

namespace Controller
{
    /// <summary>
    /// 核酸采集-线下
    /// </summary>
    public static class Control_HSLive
    {
        /// <summary>
        /// 获取预约患者信息
        /// </summary>
        /// <param name="Type">0身份证号 1联系电话 2二维码</param>
        /// <param name="">Val</param>
        /// <returns></returns>
        public static string GetYYPatInfo(string type, string val, string yyid)
        {
            OutJsonHelper outJson = new OutJsonHelper();
            string sql = @"
select	
    t.register_sn,
	t.hospital_code,
	t.collectionplace_code,
	t.sn,
	t.channel_code,
	t.source,
	t.register_name,
	t.id_type,
	t.id_number,
	t.register_sex,
	t.register_age,
	t.register_address,
	t.register_phonenumber,
	t.register_type,
	t.is_high_risk_areas,
	t.is_fever,
	t.is_expense,
	date_format(t.input_time,'%Y-%m-%d %H:%i:%s') input_time,
  case when c.register_sn is not null then '已退费'
       when b.register_sn is not null then '已交费'
  else '未交费' end fyzt,
	a.item_code,
	a.item_money,
	a.item_name,
	b.charge_type,
	date_format(b.charge_time,'%Y-%m-%d %H:%i:%s') charge_time,
	date_format(c.refund_time,'%Y-%m-%d %H:%i:%s') refund_time  
  from 	register_record t
join register_record_detail a  on a.register_sn = t.register_sn
left join charge_record b on a.register_sn = b.register_sn
left join refund_record c on a.register_sn = c.register_sn   where t.hospital_code=?hospital_code and ";
            string whereStr = "";
            MySqlParameter[] mySqlParameter = new MySqlParameter[2];
            mySqlParameter[0] = new MySqlParameter("?hospital_code", yyid);
            switch (type)
            {
                case "0":
                    whereStr = " t.id_number=?id_number ";
                    mySqlParameter[1] = new MySqlParameter("?id_number", val);
                    break;
                case "1":
                    whereStr = " t.register_phonenumber=?register_phonenumber ";
                    mySqlParameter[1] = new MySqlParameter("?register_phonenumber", val);
                    break;
                case "2":
                    whereStr = " t.register_sn=?register_sn ";
                    mySqlParameter[1] = new MySqlParameter("?register_sn", val);
                    break;
            }

            DataTable dt = MySQLHelper.ExecuteDataTable(sql + whereStr, CommandType.Text, mySqlParameter);
            if (dt != null && dt.Rows.Count > 0)
            {
                outJson.message = MySQLHelper.ExecuteDataTable(sql + whereStr, CommandType.Text, mySqlParameter);
                outJson.message = dt;
            }
            else
            {
                outJson.errormsg = "未查询到预约核酸记录!";
            }
            return outJson.ToJsonStr();
        }
        /// <summary>
        /// 根据预约登记流水号和医院编码获取预约项目信息
        /// </summary>
        /// <param name="reg_sn">登记流水号</param>
        /// <param name="yyid">医疗机构代码</param>
        /// <returns></returns>
        public static string GetYYItemsInfoBySN(string reg_sn, string yyid)
        {
            OutJsonHelper outJson = new OutJsonHelper();
            string sql = @"
select	case when c.register_sn is not null then '已退费'
       when b.register_sn is not null then '已交费'
  else '未交费' end fyzt,
	a.item_code,
	a.item_money,
	a.item_name,
	b.charge_type,
	b.charge_time,
	b.charge_order_no,
	c.refund_time
from
	register_record_detail a
left join charge_record b on a.register_sn = b.register_sn
left join refund_record c on a.register_sn = c.register_sn
join register_record d on a.register_sn = d.register_sn 
where a.register_sn =?register_sn and d.hospital_code=?hospital_code ";
            MySqlParameter[] mySqlParameter = new MySqlParameter[2];
            mySqlParameter[0] = new MySqlParameter("?register_sn", reg_sn);
            mySqlParameter[1] = new MySqlParameter("?hospital_code", yyid);
            DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);
            if (dt != null && dt.Rows.Count > 0)
            {
                outJson.message = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);
                outJson.message = dt;
            }
            else
            {
                outJson.errormsg = "未查询到该医疗机构,该登记流水号的项目信息!";
            }
            return outJson.ToJsonStr();
        }
        /// <summary>
        /// 核酸线下登记
        /// </summary>
        /// <param name="reg_sn"></param>
        /// <param name="yyid"></param>
        /// <param name="bblx"></param>
        /// <param name="cjd"></param>
        /// <returns></returns>
        public static string HSLiveRegister(string reg_sn, string yyid, string bblx, string cjd)
        {
            OutJsonHelper outJson = new OutJsonHelper();
            string sql = @"
SELECT
      register_sn,
	  bar_no,
	  sample_type,
	  place_code,
	  hospital_code,
	  record_time
FROM sample_record WHERE register_sn =?register_sn and hospital_code=?hospital_code ";
            MySqlParameter[] mySqlParameter = new MySqlParameter[2];
            mySqlParameter[0] = new MySqlParameter("?register_sn", reg_sn);
            mySqlParameter[1] = new MySqlParameter("?hospital_code", yyid);
            DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);
            if (dt != null && dt.Rows.Count == 0)
            {
                string sqlUpdate = @"
insert into sample_record (register_sn,sample_type,place_code,hospital_code,record_time) 
 values(?register_sn,?sample_type,?place_code,?hospital_code,now())";
                MySqlParameter[] myUpdSqlParameter = new MySqlParameter[4];
                myUpdSqlParameter[0] = new MySqlParameter("?register_sn", reg_sn);
                myUpdSqlParameter[1] = new MySqlParameter("?sample_type", bblx);
                myUpdSqlParameter[2] = new MySqlParameter("?place_code", cjd);
                myUpdSqlParameter[3] = new MySqlParameter("?hospital_code", yyid);
                int i = MySQLHelper.ExecuteNonQuery(sqlUpdate, CommandType.Text, myUpdSqlParameter);
                if (i == 1)
                {
                    outJson.message = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);
                }
                else
                {
                    outJson.errormsg = "登记失败!请刷新页面重试!";
                }
            }
            else
            {
                outJson.errormsg = "患者已登记!,不允许再次登记!";
            }
            return outJson.ToJsonStr();
        }
        /// <summary>
        /// 生成新的条码
        /// </summary>
        /// <param name="seq_barno">条码序列 默认seq_barno</param>
        /// <param name="barno_length">条码长度位数 默认11位</param>
        /// <returns></returns>
        public static string GetNewBarNO(string seq_barno = "seq_barno", int barno_length = 11)
        {
            string sql = string.Format("SELECT nextval('{0}')", seq_barno);
            object ob = MySQLHelper.ExecuteScalar(sql, CommandType.Text, null);
            string barno = "1";
            if (ob != null)
            {
                barno = ob.ToString();
            }
            else
            {
                sql = string.Format("insert into sequence(SEQ_NAME,CURRENT_VAL,INCREMENT_VAL) values ('{0}',1,1)", seq_barno);
                int i = MySQLHelper.ExecuteNonQuery(sql, CommandType.Text, null);
                if (i > 0)
                {
                    barno = "1";
                }
            }
            return barno.PadLeft(barno_length, '0');
        }

        /// <summary>
        /// 打印条码
        /// </summary>
        /// <param name="bar_no"></param>
        /// <param name="reg_sns"></param>
        /// <returns></returns>
        public static string PrintNewBar(string reg_sns)
        {
            string bar_no = GetNewBarNO();
            OutJsonHelper outJson = new OutJsonHelper();
            string[] arr = reg_sns.Split(',');
            string reg_snsStr = "";
            for (int i = 0; i < arr.Length; i++)
            {
                reg_snsStr += "'" + arr[i] + "',";
            }
            string sql = "update sample_record set bar_no=?bar_no where register_sn in(" + reg_snsStr.Substring(0, reg_snsStr.Length - 1) + ")";
            MySqlParameter[] myUpdSqlParameter = new MySqlParameter[1];
            myUpdSqlParameter[0] = new MySqlParameter("?bar_no", bar_no);
            int t = MySQLHelper.ExecuteNonQuery(sql, CommandType.Text, myUpdSqlParameter);
            if (t >= 1)
            {
                string sql2 = @"
SELECT
      register_sn,
	  bar_no,
	  sample_type,
	  place_code,
	  hospital_code,
	  record_time
FROM sample_record WHERE register_sn in(" + reg_snsStr.Substring(0, reg_snsStr.Length - 1) + ")";
                outJson.message = MySQLHelper.ExecuteDataTable(sql2, CommandType.Text, null);

            }
            else
            {
                outJson.errormsg = "条码打印失败!!";
            }
            return outJson.ToJsonStr();
        }




        public static string WxMicroPay(string auth_code, string total_fee, string register_sn)
        {
            OutJsonHelper outJson = new OutJsonHelper();
            WeChatPayCore inputObj = new WeChatPayCore();
            int fee = int.Parse((double.Parse(total_fee) * 100).ToString());
            inputObj.SetValue("body", "核酸检测费"); //商品描述

            inputObj.SetValue("auth_code", auth_code);//用户授权码 
            inputObj.SetValue("total_fee", fee);//总金额
            inputObj.SetValue("out_trade_no", register_sn);//商户订单号

            //商户固定参数
            inputObj.SetValue("spbill_create_ip", CoreConfig.payIP);//终端ip
            inputObj.SetValue("appid", CoreConfig.AppID);//公众账号ID
            inputObj.SetValue("mch_id", CoreConfig.mchid);//商户号
            inputObj.SetValue("nonce_str", Guid.NewGuid().ToString().Replace("-", ""));//随机字符串
            inputObj.SetValue("sign", inputObj.MakeSign());//签名
            string xml = inputObj.ToXml();
            int timeout = 10;
            string url = "https://api.mch.weixin.qq.com/pay/micropay";
            string response = HttpService.Post(xml, url, false, timeout);
            WeChatPayCore result = new WeChatPayCore();
            result.FromXml(response);
            /*判断支付是否成功 3种状态*/
            if (result.GetValue("return_code").ToString() == "FAIL")
            {
                outJson.errormsg = "支付失败，请重试！";
            }
            else if (result.GetValue("result_code").ToString() == "SUCCESS")
            {

            }
            else
            {
                /*5秒重新查询一次，查询10次，查询订单是否成功*/
            }
            return outJson.ToJsonStr();
        }
    }
}
