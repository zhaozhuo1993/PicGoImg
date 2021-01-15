using DBHelper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Utils;

namespace Controller.GZH
{
    public class RegisterManage
    {
        // 定义 私有静态只读 对象  防止产生死锁
        private static readonly object objlock = new object();
        /// <summary>
        /// 信息录入
        /// </summary>
        /// <returns></returns>
        public static string Register(string yyid, string cjdbm, string qdbm, string ly, string xm, string zjlx, string sfzh, string xb, string nl, string xzz, string hjdz, string sjh, string rylb, string gfx, string fr, string zf, string openid, string xmdm, string xmmc, string je)
        {
            lock (objlock)
            {
                try
                {
                    string sql = @"INSERT INTO register_record    (REGISTER_SN,HOSPITAL_CODE,COLLECTIONPLACE_CODE,CHANNEL_CODE,SOURCE,REGISTER_NAME,ID_TYPE,ID_NUMBER,REGISTER_SEX,REGISTER_AGE,
                   REGISTER_ADDRESS,REGISTER_HH_ADDRESS,REGISTER_PHONENUMBER,REGISTER_TYPE,IS_HIGH_RISK_AREAS,IS_FEVER,IS_EXPENSE,INPUT_TIME,OPENID) VALUES (
                   nextval('seq_register'),?HOSPITAL_CODE,?COLLECTIONPLACE_CODE,?CHANNEL_CODE,?SOURCE,?REGISTER_NAME,?ID_TYPE,?ID_NUMBER,?REGISTER_SEX,?REGISTER_AGE,?REGISTER_ADDRESS,?REGISTER_HH_ADDRESS,?REGISTER_PHONENUMBER,?REGISTER_TYPE,?IS_HIGH_RISK_AREAS,?IS_FEVER,?IS_EXPENSE,NOW(),?OPENID);
                   INSERT INTO register_record_detail(REGISTER_SN,ITEM_CODE,ITEM_NAME,ITEM_MONEY) VALUES (currval('seq_register'),?xmdm,?xmmc,?je);
                   SELECT currval('seq_register');";

                    MySqlParameter[] mySqlParameter = new MySqlParameter[]
                    {
                    new MySqlParameter("?HOSPITAL_CODE",yyid),
                    new MySqlParameter("?COLLECTIONPLACE_CODE",cjdbm),
                    new MySqlParameter("?CHANNEL_CODE",qdbm),
                    new MySqlParameter("?SOURCE",ly),
                    new MySqlParameter("?REGISTER_NAME",xm),
                    new MySqlParameter("?ID_TYPE",zjlx),
                    new MySqlParameter("?ID_NUMBER",sfzh),
                    new MySqlParameter("?REGISTER_SEX",xb),
                    new MySqlParameter("?REGISTER_AGE",nl),
                    new MySqlParameter("?REGISTER_ADDRESS",xzz),
                    new MySqlParameter("?REGISTER_HH_ADDRESS",hjdz),
                    new MySqlParameter("?REGISTER_PHONENUMBER",sjh),
                    new MySqlParameter("?REGISTER_TYPE",rylb),
                    new MySqlParameter("?IS_HIGH_RISK_AREAS",gfx),
                    new MySqlParameter("?IS_FEVER",fr),
                    new MySqlParameter("?IS_EXPENSE",zf),
                    new MySqlParameter("?OPENID",openid),
                    new MySqlParameter("?xmdm",xmdm),
                    new MySqlParameter("?xmmc",xmmc),
                    new MySqlParameter("?je", double.Parse(je))
                    };
                    return MySQLHelper.ExecuteScalar(sql, CommandType.Text, mySqlParameter).ToString();
                }
                catch (Exception ex)
                {
                    Log.LogWrite("信息录入-Register", ex.Message);
                    return "";
                }
            }
        }

        /// <summary>
        /// 获取支付信息
        /// </summary>
        /// <returns></returns>
        public static DataTable GetIsPay(string lsh)
        {
            try
            {
                string sql = " SELECT * FROM charge_record WHERE REGISTER_SN = ?lsh ";

                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?lsh",lsh)
                };

                return MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);
            }
            catch (Exception ex)
            {
                Log.LogWrite("获取支付信息-GetIsPay", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 添加支付信息
        /// </summary>
        /// <returns></returns>
        public static bool InsertRegisterDetail(string lsh, string xmdm, string xmmc, string je)
        {
            try
            {
                string sql = @"INSERT INTO register_record_detail(REGISTER_SN,ITEM_CODE,ITEM_NAME,ITEM_MONEY) VALUES
                                (?lsh,?xmdm,?xmmc,?je)";

                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                    new MySqlParameter("?lsh",lsh),
                    new MySqlParameter("?xmdm",xmdm),
                    new MySqlParameter("?xmmc",xmmc),
                    new MySqlParameter("?je",je)
                };
                return MySQLHelper.ExecuteNonQuery(sql, CommandType.Text, mySqlParameter)> 0;
            }
            catch (Exception ex)
            {
                Log.LogWrite("添加支付信息-InsertRegisterDetail", ex.Message);
                return false;
            }
        }
    }
}
