using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using DBHelper;
using Newtonsoft.Json;
using Utils;
using Newtonsoft.Json.Linq;

namespace Controller
{
    public static class Control
    {
        /// <summary>
        /// 获取医院信息
        /// </summary>
        /// <param name="hosid"></param>
        /// <returns></returns>
        public static string GetHos(string hosid)
        {
            string sql = @" SELECT a.YYID,a.YYMC,a.BZ,a.isuse,b.`lable`,b.`type`,b.`value`,b.`ismust` FROM hs_yydm a 
                            LEFT JOIN hs_yyzd b ON a.`yyid` = b.`yyid` WHERE a.yyid = ?yyid";

            MySqlParameter[] mySqlParameter = new MySqlParameter[]
            {
                new MySqlParameter("?yyid",hosid)
            };

            DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);

            if (dt == null)
                return "";
            return JsonConvert.SerializeObject(dt); 
        }
        /// <summary>
        /// 获取医院-组号里最大序号
        /// </summary>
        /// <param name="yyid"></param>
        /// <param name="zh"></param>
        /// <returns></returns>
        public static int GetMaxXh(string yyid, string zh)
        {
            string sql = " SELECT MAX(xh+0) FROM hs_ryxx WHERE yyid = ?yyid AND zh = ?zh";

            MySqlParameter[] mySqlParameter = new MySqlParameter[]
            {
                new MySqlParameter("?yyid",yyid),
                new MySqlParameter("?zh",zh)
            };

            DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);

            if (dt == null)
                return 0;
            return int.Parse(dt.Rows[0][0].ToString());
        }
        /// <summary>
        /// 添加信息
        /// </summary>
        /// <returns></returns>
        public static string AddInfo(string xm, string sfzh, string xb, string nl, string sjh, string xzz, string gfx, string fr, string yyid, string zh, string xh, string ryfl, string sfzf)
        {
            try
            {
                string sql = "INSERT INTO hs_ryxx(xm,sfzh,xb,nl,sjh,xzz,gfx,fr,yyid,lrsj,zh,xh,ryfl,sfzf) VALUES(?xm,?sfzh,?xb,?nl,?sjh,?xzz,?gfx,?fr,?yyid,NOW(),?zh,?xh,?ryfl,?sfzf)";

                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?yyid",yyid),
                new MySqlParameter("?xm",xm),
                new MySqlParameter("?sfzh",sfzh),
                new MySqlParameter("?xb",xb),
                new MySqlParameter("?nl",nl),
                new MySqlParameter("?sjh",sjh),
                new MySqlParameter("?xzz",xzz),
                new MySqlParameter("?gfx",gfx),
                new MySqlParameter("?fr",fr),
                new MySqlParameter("?zh",zh),
                new MySqlParameter("?xh",xh),
                new MySqlParameter("?ryfl",ryfl),
                new MySqlParameter("?sfzf",sfzf)
                };
                var rel = MySQLHelper.ExecuteNonQuery(sql, CommandType.Text, mySqlParameter) > 0;

                if (!rel)
                    return "";

                sql = @" SELECT a.*,b.yymc FROM hs_ryxx a JOIN 
                    hs_yydm b ON a.yyid = b.yyid WHERE a.yyid = ?yyid AND a.xh = ?xh AND a.zh = ?zh ";
                mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?yyid",yyid),
                new MySqlParameter("?xh",xh),
                new MySqlParameter("?zh",zh)
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
        /// 核酸统计
        /// </summary>
        /// <param name="yyid"></param>
        /// <returns></returns>
        public static DataTable GetHsryxxByYyid(string yyid, string btime, string etime, string para, string hsjg)
        {
            try
            {
                string sql = " SELECT a.*,b.yymc,c.hsjg,c.bgsj FROM hs_ryxx a JOIN hs_yydm b ON a.yyid = b.yyid LEFT JOIN hs_bgxx c ON a.yyid = c.yyid and c.bh  = CONCAT(a.yyid,'-',a.zh,'-',a.xh) WHERE a.yyid = ?yyid and a.lrsj >= ?btime and a.lrsj <= ?etime ";

                if(!string.IsNullOrEmpty(para))
                    sql += " and (a.xm = ?para or a.sfzh = ?para or a.sjh =?para) ";
                if (hsjg == "3")
                    sql += " and c.hsjg is null ";
                else if (hsjg == "1" || hsjg == "2")
                    sql += " and c.hsjg = ?hsjg ";

                sql += " ORDER BY a.lrsj DESC,a.zh,a.xh+0 ";

                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?yyid",yyid),
                new MySqlParameter("?btime",btime),
                new MySqlParameter("?etime",etime),
                new MySqlParameter("?para",para),
                new MySqlParameter("?hsjg",hsjg)
                };

                DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);

                if (dt == null)
                    return new DataTable();
                return dt;
            }
            catch (Exception)
            {
                return new DataTable();
            }
            
        }
        /// <summary>
        /// 核酸统计导出
        /// </summary>
        /// <param name="yyid"></param>
        /// <returns></returns>
        public static DataTable GetHsryxxExport(string yyid, string btime, string etime, string para, string hsjg)
        {
            try
            {
                string sql = @" SELECT b.yymc AS '医院名称', CONCAT(a.yyid,'-',a.zh,'-',a.xh) AS '编号',a.xm AS '姓名',a.sfzh AS '身份证号',a.xb AS '性别',a.nl           AS '年龄',a.sjh AS '手机号',a.xzz AS '现住址',
                            CASE WHEN a.gfx = 0 THEN '否' ELSE '是' END AS '是否去过高风险地区',
                            CASE WHEN a.fr = 0 THEN '否' ELSE '是' END AS '是否发热',a.lrsj AS '录入时间',
                            CASE WHEN c.hsjg = 1 THEN '阴性' WHEN c.hsjg = 2 THEN '阳性' ELSE '暂无' END AS '核酸结果',
                            c.bgsj AS '检测时间'
                            FROM hs_ryxx a JOIN hs_yydm b ON a.yyid = b.yyid 
                            LEFT JOIN hs_bgxx c ON a.yyid = c.yyid and c.bh  = CONCAT(a.yyid,'-',a.zh,'-',a.xh)
                            WHERE a.yyid = ?yyid and a.lrsj >= ?btime and a.lrsj <= ?etime ";

                if (!string.IsNullOrEmpty(para))
                    sql += " and (a.xm = ?para or a.sfzh = ?para or a.sjh =?para) ";
                if(hsjg == "3")
                    sql += " and c.hsjg is null ";
                else if (hsjg == "1" || hsjg == "2")
                    sql += " and c.hsjg = ?hsjg ";

                sql += " ORDER BY a.lrsj DESC,a.zh,a.xh+0 ";

                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?yyid",yyid),
                new MySqlParameter("?btime",btime),
                new MySqlParameter("?etime",etime),
                new MySqlParameter("?para",para),
                new MySqlParameter("?hsjg",hsjg)
                };

                DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);

                if (dt == null)
                    return new DataTable();
                return dt;
            }
            catch (Exception)
            {
                return new DataTable();
            }

        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="code"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static string Login(string code, string pwd)
        {
            try
            {
                string sql = " SELECT * FROM hs_yydm WHERE yymc = ?code AND pwd = MD5(?pwd) ";

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
        /// 验证密码是否正确
        /// </summary>
        /// <param name="yyid"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static bool GetYyInfo(string yyid, string pwd)
        {
            try
            {
                string sql = " SELECT * FROM hs_yydm WHERE yyid = ?yyid AND pwd = MD5(?pwd) ";

                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?yyid",yyid),
                new MySqlParameter("?pwd",pwd)
                };

                DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);
                if (dt.Rows.Count > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="yyid"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static string UpdatePwd(string yyid, string pwd)
        {
            try
            {
                string sql = " UPDATE hs_yydm SET pwd = MD5(?pwd) WHERE yyid = ?yyid ";

                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?yyid",yyid),
                new MySqlParameter("?pwd",pwd)
                };

                var rel = MySQLHelper.ExecuteNonQuery(sql, CommandType.Text, mySqlParameter) > 0;
                if (rel)
                    return "true";
                else
                    return "修改密码失败";
            }
            catch (Exception)
            {
                return "修改密码失败";
            }
        }
        /// <summary>
        /// 生成核酸结果
        /// </summary>
        /// <param name="jsonstr"></param>
        /// <returns></returns>
        public static string GenerateResult(string jsonstr)
        {
            try
            {
                JArray arr = JArray.Parse(jsonstr);
                if (arr.Count > 0)
                {
                    string sql = " INSERT INTO hs_bgxx (yyid,bh,hsjg,bgsj) VALUES ";
                    var sqlval = "";
                    for (int i = 0; i < arr.Count; i++)
                    {
                        var obj = (JObject)arr[i];
                        var bh = obj["yyid"] + "-" + obj["zh"] + "-" + obj["xh"];
                        if (string.IsNullOrEmpty(sqlval))
                            sqlval += " (" + obj["yyid"] + ",'" + bh + "',1,NOW()) ";
                        else
                            sqlval += " ,(" + obj["yyid"] + ",'" + bh + "',1,NOW()) ";
                    }
                    if (!string.IsNullOrEmpty(sqlval))
                    {
                        var rel = MySQLHelper.ExecuteNonQuery(sql + sqlval, CommandType.Text, null) > 0;
                        if(rel)
                            return "true";
                        else
                            return "false";
                    }
                    else
                        return "false";
                }
                else
                    return "false";
            }
            catch (Exception)
            {
                return "false";
            }
        }
        /// <summary>
        /// 修改核酸结果
        /// </summary>
        /// <param name="hsjg"></param>
        /// <param name="bh"></param>
        /// <param name="yyid"></param>
        /// <returns></returns>
        public static string UpdateHsjg(string hsjg, string bh, string yyid)
        {
            try
            {
                string sql = " UPDATE hs_bgxx SET hsjg = ?hsjg WHERE yyid = ?yyid AND bh = ?bh  ";
                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?yyid",yyid),
                new MySqlParameter("?hsjg",hsjg),
                new MySqlParameter("?bh",bh)
                };

                var rel = MySQLHelper.ExecuteNonQuery(sql, CommandType.Text, mySqlParameter) > 0;
                if (rel)
                    return "true";
                else
                    return "false";
            }
            catch (Exception)
            {
                return "false";
            }
        }
        /// <summary>
        /// 获取核酸结果
        /// </summary>
        /// <param name="yyid"></param>
        /// <param name="xm"></param>
        /// <param name="sfzh"></param>
        /// <returns></returns>
        public static DataTable GetHsjg(string yyid, string xm, string sfzh, string sjh)
        {
            try
            {
                string sql = @" SELECT a.xm,a.sfzh,a.yyid,b.yymc,c.hsjg,c.bgsj FROM hs_ryxx a
                                JOIN hs_yydm b ON a.yyid = b.yyid
                                JOIN hs_bgxx c ON a.yyid = c.yyid AND c.`bh` = CONCAT(a.yyid, '-', a.zh, '-', a.xh)
                                WHERE a.yyid = ?yyid AND a.xm = ?xm AND a.sfzh = ?sfzh AND a.sjh =?sjh ORDER BY a.lrsj DESC LIMIT 1 ";


                MySqlParameter[] mySqlParameter = new MySqlParameter[]
                {
                new MySqlParameter("?yyid",yyid),
                new MySqlParameter("?xm",xm),
                new MySqlParameter("?sfzh",sfzh),
                new MySqlParameter("?sjh",sjh)
                };

                DataTable dt = MySQLHelper.ExecuteDataTable(sql, CommandType.Text, mySqlParameter);

                if (dt == null)
                    return new DataTable();
                return dt;
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }
    }
}
