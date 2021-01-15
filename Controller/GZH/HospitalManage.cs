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
    public class HospitalManage
    {
        /// <summary>
        /// 获取医院信息
        /// </summary>
        /// <param name="hosid"></param>
        /// <returns></returns>
        public static DataTable GetHos()
        {
            try
            {
                string sql = @" SELECT a.`CODE`,a.`NAME`,a.`PHONE`,b.`CODE` AS CJDBM,b.NAME AS CJDMC,b.`POSITION`,b.WE_VALUE,b.NS_VALUE,
                                (SELECT GROUP_CONCAT(item_money SEPARATOR '-') FROM code_hospital_jcxm WHERE hospital_code = a.code) AS MONEYSTR,0 as jl
                                FROM code_hospital a JOIN code_collectionplace b ON a.`CODE` = b.`HOSPITAL_CODE` 
                                WHERE a.`STATE` = 1; ";

                return MySQLHelper.ExecuteDataTable(sql, CommandType.Text, null);
            }
            catch (Exception ex)
            {
                Log.LogWrite("获取医院信息-GetHos:", ex.Message);
                return new DataTable();
            }
        }
        /// <summary>
        /// 获取检查项目
        /// </summary>
        /// <returns></returns>
        public static DataTable GetJcxm(string yyid)
        {
            try
            {
                string sql = " SELECT * FROM code_hospital_jcxm WHERE HOSPITAL_CODE = ?yyid ";

                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?yyid",yyid)
                };

                return MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);
            }
            catch (Exception ex)
            {
                Log.LogWrite("获取检查项目-GetJcxm:", ex.Message);
                return new DataTable();
            }
        }
    }
}
