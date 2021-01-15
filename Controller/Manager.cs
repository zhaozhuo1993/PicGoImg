using DBHelper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Controller
{
    public class Manager
    {
        /// <summary>
        /// 管理端--登录
        /// </summary>
        /// <param name="code">用户编码</param>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        public static string Login(string code, string pwd)
        {
            try
            {
                string sql = " select a.code,a.name,a.role,a.hospital_code,b.`NAME` as hospital_name,a.PLACE_CODE,c.`NAME` as PLACE_NAME from code_user a inner join code_hospital b on a.hospital_code=b.`CODE` left join code_collectionplace c on a.HOSPITAL_CODE=c.HOSPITAL_CODE and a.PLACE_CODE=c.`CODE` WHERE a.code = ?code AND a.pwd = MD5(?pwd) AND a.state=1";

                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?code",code),
                new MySqlParameter("?pwd",pwd)
                };

                DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);

                if (dt == null)
                    return "";
                return JsonConvert.SerializeObject(dt);
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 得到结果信息
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="hospital_code">医疗机构代码</param>
        /// <param name="place_code">采集点代码</param>
        /// <returns></returns>
        public static string GetReportInfo(string start, string end, string hospital_code, string place_code)
        {
            try
            {
                StringBuilder str = new StringBuilder();
                str.Append(@"SELECT
	                            c.`NAME` AS yljg,
	                            d.`NAME` AS cjd,
	                            e.REPORT_RESULT hsjg,  
                                DATE_FORMAT(e.REPORT_TIME,'%Y-%m-%d %H:%i:%s') jcdate,
	                            a.REGISTER_NAME `NAME`,
	                            a.REGISTER_SEX xb,
	                            a.REGISTER_AGE age,
	                            a.ID_NUMBER idcard,	
	                            a.REGISTER_ADDRESS address,
	                            a.REGISTER_PHONENUMBER phone,
	                            a.INPUT_TIME intodate
                            FROM
	                            register_record a
                            INNER JOIN sample_record b ON a.REGISTER_SN = b.REGISTER_SN
                            INNER JOIN code_hospital c ON a.HOSPITAL_CODE = c.`CODE`
                            INNER JOIN code_collectionplace d ON b.PLACE_CODE = d.`CODE`
                            INNER JOIN report_record e on b.BAR_NO=e.BAR_NO
                            where REPORT_TIME>?start and REPORT_TIME<?end ");
                List<MySqlParameter> parList = new List<MySqlParameter>();
                parList.Add(new MySqlParameter("?start", start));
                parList.Add(new MySqlParameter("?end", end));
                if (string.IsNullOrEmpty(hospital_code) == false)
                {
                    str.Append(" and a.HOSPITAL_CODE=?HOSPITAL_CODE");
                    parList.Add(new MySqlParameter("?HOSPITAL_CODE", hospital_code));
                }
                if (string.IsNullOrEmpty(place_code) == false)
                {
                    str.Append(" and b.PLACE_CODE =?PLACE_CODE ");
                    parList.Add(new MySqlParameter("?PLACE_CODE", place_code));
                }

                DataTable dt = MySQLHelper.ExecuteDataTable(str.ToString(), CommandType.Text, parList.ToArray());

                if (dt == null)
                    return "";
                return JsonConvert.SerializeObject(dt);
            }
            catch (Exception ex)
            {
                return "";
            }
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
        /// 得到采集信息
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <param name="hospital_code">医疗机构代码</param>
        /// <param name="place_code">采集点代码</param>
        /// <returns></returns>
        public static string GetSampleInfo(string start, string end, string hospital_code, string place_code)
        {
            try
            {
                StringBuilder str = new StringBuilder();
                str.Append(@"SELECT 
                                d.`NAME` AS hospital_name,
	                            e.`NAME` AS place_name,
                                a.REGISTER_SN sn,
                                b.BAR_NO bar_no,
                                a.REGISTER_NAME `NAME`,
                                a.REGISTER_SEX sex,
                                a.REGISTER_AGE age,
                                a.ID_NUMBER idcard,
                                a.REGISTER_PHONENUMBER telphone,
                                a.REGISTER_ADDRESS address,
                                DATE_FORMAT(a.INPUT_TIME,'%Y-%m-%d %H:%i:%s') intodate,
                                DATE_FORMAT(b.RECORD_TIME,'%Y-%m-%d %H:%i:%s') sampledate,
                                c.ITEM_CODE item_code,
                                c.ITEM_NAME item_name,
                                case when b.SAMPLE_TYPE='01' then '咽拭子' when b.SAMPLE_TYPE='02' then '鼻咽拭子' when b.SAMPLE_TYPE='03' then '血' else '咽拭子' end sample_type
                                FROM register_record a
                                INNER JOIN sample_record b ON a.REGISTER_SN = b.REGISTER_SN
                                INNER JOIN register_record_detail c ON a.REGISTER_SN = c.REGISTER_SN
                                INNER JOIN code_hospital d ON a.HOSPITAL_CODE = d.`CODE`
                                INNER JOIN code_collectionplace e ON b.PLACE_CODE = e.`CODE`
                                where not exists(select 1 from refund_record t where t.REGISTER_SN = a.REGISTER_SN)
                                and not exists(select 1 from report_record t where t.BAR_NO = b.BAR_NO)
                                and RECORD_TIME>?start and RECORD_TIME <?end ");
                List<MySqlParameter> parList = new List<MySqlParameter>();
                parList.Add(new MySqlParameter("?start", start));
                parList.Add(new MySqlParameter("?end", end));
                if (string.IsNullOrEmpty(hospital_code) == false)
                {
                    str.Append(" and a.HOSPITAL_CODE=?HOSPITAL_CODE");
                    parList.Add(new MySqlParameter("?HOSPITAL_CODE", hospital_code));
                }
                if (string.IsNullOrEmpty(place_code) == false)
                {
                    str.Append(" and b.PLACE_CODE =?PLACE_CODE ");
                    parList.Add(new MySqlParameter("?PLACE_CODE", place_code));
                }

                DataTable dt = MySQLHelper.ExecuteDataTable(str.ToString(), CommandType.Text, parList.ToArray());

                if (dt == null)
                    return "";
                return JsonConvert.SerializeObject(dt);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        /// <summary>
        /// 导出采集标本信息
        /// </summary>
        /// <param name="start">开始时间</param>
        /// <param name="end">截止时间</param>
        /// <param name="hospital_code">医疗机构代码</param>
        /// <param name="place_code">采集点代码</param>
        /// <returns></returns>
        public static DataTable ExpExcelSampleInfo(string start, string end, string hospital_code, string place_code)
        {
            try
            {
                StringBuilder str = new StringBuilder();
                str.Append(@"SELECT 
                                d.`NAME` AS 医疗机构,
	                            e.`NAME` AS 采集点,
                                a.REGISTER_SN 序号,
                                b.BAR_NO 条码号,
                                a.REGISTER_NAME 姓名,
                                a.REGISTER_SEX 性别,
                                a.REGISTER_AGE 年龄,
                                a.ID_NUMBER 身份证号,
                                a.REGISTER_PHONENUMBER 联系电话,
                                a.REGISTER_ADDRESS 家庭住址,
                                DATE_FORMAT(a.INPUT_TIME,'%Y-%m-%d %H:%i:%s') 登记时间,
                                DATE_FORMAT(b.RECORD_TIME,'%Y-%m-%d %H:%i:%s') 采集时间,
                                c.ITEM_CODE 项目编码,
                                c.ITEM_NAME 项目名称,
                                case when b.SAMPLE_TYPE='01' then '咽拭子' when b.SAMPLE_TYPE='02' then '鼻咽拭子' when b.SAMPLE_TYPE='03' then '血' else '咽拭子' end 标本类型
                                FROM register_record a
                                INNER JOIN sample_record b ON a.REGISTER_SN = b.REGISTER_SN
                                INNER JOIN register_record_detail c ON a.REGISTER_SN = c.REGISTER_SN
                                INNER JOIN code_hospital d ON a.HOSPITAL_CODE = d.`CODE`
                                INNER JOIN code_collectionplace e ON b.PLACE_CODE = e.`CODE`
                                where not exists(select 1 from refund_record t where t.REGISTER_SN = a.REGISTER_SN)
                                and not exists(select 1 from report_record t where t.BAR_NO = b.BAR_NO)
                                and RECORD_TIME>?start and RECORD_TIME <?end ");
                List<MySqlParameter> parList = new List<MySqlParameter>();
                parList.Add(new MySqlParameter("?start", start));
                parList.Add(new MySqlParameter("?end", end));
                if (string.IsNullOrEmpty(hospital_code) == false)
                {
                    str.Append(" and a.HOSPITAL_CODE=?HOSPITAL_CODE");
                    parList.Add(new MySqlParameter("?HOSPITAL_CODE", hospital_code));
                }
                if (string.IsNullOrEmpty(place_code) == false)
                {
                    str.Append(" and b.PLACE_CODE =?PLACE_CODE ");
                    parList.Add(new MySqlParameter("?PLACE_CODE", place_code));
                }

                DataTable dt = MySQLHelper.ExecuteDataTable(str.ToString(), CommandType.Text, parList.ToArray());

                if (dt != null)
                {
                    return dt;
                }
                else
                {
                    return new DataTable();
                }
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
        }

        /// <summary>
        /// 更改核酸结果表
        /// </summary>
        /// <param name="json">[{"bar_no":"条码号","report_result":"结果"}]</param>
        /// <returns></returns>
        public static string UpdateReportResultInfo(string json)
        {
            JArray jary = JArray.Parse(json);
            DataTable dt = jary.ToObject<DataTable>();
            if (dt != null && dt.Rows.Count > 0)
            {
                List<string> sqlList = new List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    sqlList.Add(string.Format("update report_record set report_result='{0}' where bar_no='{1}'", row["report_result"].ToString(), row["bar_no"].ToString()));
                }
                if (sqlList.Count > 0)
                {
                    MySQLHelper.ExecutTransaction(sqlList);
                }
            }
            return "";
        }




        /// <summary>
        /// 登记患者 测试
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string Register(string json)
        {
            JArray jary = JArray.Parse(json);
            DataTable dt = jary.ToObject<DataTable>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    //插入register_record
                    string sql = "insert into register_record(REGISTER_SN,HOSPITAL_CODE,COLLECTIONPLACE_CODE,SN,CHANNEL_CODE,SOURCE,REGISTER_NAME,ID_TYPE,ID_NUMBER,REGISTER_SEX,REGISTER_AGE,REGISTER_ADDRESS,REGISTER_HH_ADDRESS,REGISTER_PHONENUMBER,REGISTER_TYPE,IS_HIGH_RISK_AREAS,IS_FEVER,IS_EXPENSE,INPUT_TIME,OPENID) values (REGISTER_SN,?HOSPITAL_CODE,?COLLECTIONPLACE_CODE,?SN,?CHANNEL_CODE,?SOURCE,?REGISTER_NAME,?ID_TYPE,?ID_NUMBER,?REGISTER_SEX,?REGISTER_AGE,?REGISTER_ADDRESS,?REGISTER_HH_ADDRESS,?REGISTER_PHONENUMBER,?REGISTER_TYPE,?IS_HIGH_RISK_AREAS,?IS_FEVER,?IS_EXPENSE,now(),?OPENID)";

                    MySqlParameter[] param = new MySqlParameter[] {new MySqlParameter("?REGISTER_SN",Guid.NewGuid().ToString()),
                new MySqlParameter("?HOSPITAL_CODE",row["HOSPITAL_CODE"].ToString()),
                new MySqlParameter("?COLLECTIONPLACE_CODE",row["COLLECTIONPLACE_CODE"].ToString())};
                }
            }
            return "";
        }

        /// <summary>
        /// 线下采集确认 测试
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string CollectionConfirm(string json)
        {
            JArray jary = JArray.Parse(json);
            DataTable dt = jary.ToObject<DataTable>();
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Dictionary<string, MySqlParameter[]> dirSql = new Dictionary<string, MySqlParameter[]>();
                    //更新登记表
                    string sno = row["REGISTER_SN"].ToString();
                    string hospital_code = row["HOSPITAL_CODE"].ToString();
                    string sql = string.Format("update register_record set COLLECTIONPLACE_CODE='{0}' where REGISTER_SN='{1}' and HOSPITAL_CODE='{2}'", row["PLACE_CODE"].ToString(), sno, hospital_code);
                    dirSql.Add(sql, null);
                    string bar_no = GetNewBarNO();
                    //插入采样表
                    string sql2 = string.Format("insert into sample_record (REGISTER_SN,BAR_NO,SAMPLE_TYPE,PLACE_CODE,HOSPITAL_CODE,RECORD_TIME) values ('{0}','{1}','{2}','{3}','{4}',now())", sno, bar_no, row["SAMPLE_TYPE"].ToString(), row["PLACE_CODE"].ToString(), hospital_code);
                    dirSql.Add(sql2, null);
                    MySQLHelper.ExecuteSqlTran(dirSql);
                }
            }
            return "";
        }
    }
}
