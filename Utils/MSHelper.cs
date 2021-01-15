using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;

namespace Utils
{
    public class MSHelper
    {
        private static readonly string connString = ConfigurationManager.ConnectionStrings["mysql"].ToString();
        private static MSHelper sqlHelp = null;

        public MSHelper()
        {

        }

        public static MSHelper CreateDPDAL()
        {
            if (sqlHelp == null)
            {
                sqlHelp = new MSHelper();
                return sqlHelp;
            }
            return sqlHelp;
        }

        /// <summary>
        /// 创建MySqlCommand
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="commandType">sql语句还是存储过程</param>
        /// <param name="mySqlParameter">不定长参数</param>
        /// <returns></returns>
        private static MySqlCommand ExecuteMySqlCommand(string sql, CommandType commandType, MySqlParameter[] mySqlParameter)
        {
            MySqlConnection mySqlConnection = new MySqlConnection(connString);
            MySqlCommand mySqlCommand = new MySqlCommand(sql, mySqlConnection);
            if (mySqlParameter != null && mySqlParameter.Length > 0)
            {
                mySqlCommand.Parameters.AddRange(mySqlParameter);
            }
            mySqlCommand.CommandType = commandType;
            return mySqlCommand;
        }

        /// <summary>
        /// 查询数据集
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="mySqlParameter"></param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string sql, CommandType commandType, MySqlParameter[] mySqlParameter)
        {
            DataTable dataSet = new DataTable();
            MySqlCommand mySqlCommand = null;
            try
            {
                mySqlCommand = ExecuteMySqlCommand(sql, commandType, mySqlParameter);
                mySqlCommand.Connection.Open();
                MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(mySqlCommand);
                mySqlDataAdapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
                Log.LogWrite("ExecuteDataSet();报错语句:" + sql, ex.Message);
                return null;
                throw;
            }
            finally
            {
                mySqlCommand.Connection.Close();
                mySqlCommand.Connection.Dispose();
            }
            return dataSet;
        }
        public static DataTable ExecuteDataTableSimple(string sql)
        {
            DataTable dt = ExecuteDataTable(sql, CommandType.Text, null);

            DataRow row = dt.NewRow();

            row[0] = "";
            row[1] = "";

            dt.Rows.InsertAt(row, 0);

            return dt;
        }
        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="mySqlParameter"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql, CommandType commandType, MySqlParameter[] mySqlParameter)
        {
            object ob;
            MySqlCommand mySqlCommand = ExecuteMySqlCommand(sql, commandType, mySqlParameter);
            try
            {
                mySqlCommand.Connection.Open();
                ob = mySqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Log.LogWrite("ExecuteScalar()-----报错语句:" + sql, ex.Message);
                throw;
            }
            finally
            {
                mySqlCommand.Connection.Close();
                mySqlCommand.Connection.Dispose();
            }
            return ob;
        }

        /// <summary>
        /// 执行增、删、改
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="commandType"></param>
        /// <param name="mySqlParameter"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql, CommandType commandType, MySqlParameter[] mySqlParameter)
        {
            int i = 0;
            MySqlCommand mySqlCommand = ExecuteMySqlCommand(sql, commandType, mySqlParameter);
            try
            {
                mySqlCommand.Connection.Open();
                i = mySqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Log.LogWrite("ExecuteNonQuery()------报错语句:" + sql, ex.Message);
                throw;
            }
            finally
            {
                mySqlCommand.Connection.Close();
                mySqlCommand.Connection.Dispose();
            }
            return i;
        }

        public static int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, CommandType.Text, null);
        }

        /// <summary>
        /// 批量执行sql语句
        /// </summary>
        /// <param name="listSql"></param>
        /// <returns></returns>
        public static bool ExecutTransaction(List<string> listSql)
        {
            MySqlConnection mySqlConnection = new MySqlConnection(connString);
            mySqlConnection.Open();
            MySqlTransaction mysqlTra = mySqlConnection.BeginTransaction();
            MySqlCommand mysqlcommand = new MySqlCommand();
            mysqlcommand.Connection = mySqlConnection;
            mysqlcommand.CommandType = CommandType.Text;
            mysqlcommand.Transaction = mysqlTra;
            //mysqlcommand.Connection.Open();
            try
            {
                for (int i = 0; i < listSql.Count; i++)
                {
                    mysqlcommand.CommandText = listSql[i].ToString();
                    mysqlcommand.ExecuteNonQuery();
                }
                mysqlTra.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWrite("ExecutTransaction", string.Join("/r/n", listSql) + "\r\n" + ex.Message);
                mysqlTra.Rollback();
                return false;
            }
            finally
            {
                mysqlcommand.Connection.Close();
                mysqlcommand.Connection.Dispose();
            }
        }

        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句字典集合（key：sql语句，value：MySqlParameter[]）</param>
        public static int ExecuteSqlTran(Dictionary<string, MySqlParameter[]> d)
        {
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                using (MySqlTransaction trans = conn.BeginTransaction())
                {
                    MySqlCommand cmd = new MySqlCommand();
                    try
                    {
                        int indentity = 0;
                        //循环
                        foreach (KeyValuePair<string, MySqlParameter[]> myDE in d)
                        {
                            string cmdText = myDE.Key.ToString();
                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                            foreach (MySqlParameter q in cmdParms)
                            {
                                if (q.Direction == ParameterDirection.InputOutput)
                                {
                                    q.Value = indentity;
                                }
                            }
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            indentity += cmd.ExecuteNonQuery();
                            foreach (MySqlParameter q in cmdParms)
                            {
                                if (q.Direction == ParameterDirection.Output)
                                {
                                    indentity = Convert.ToInt32(q.Value);
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                        return indentity;
                    }
                    catch (Exception ex)
                    {
                        Log.LogWrite("ExecuteSqlTran", ex.Message);
                        trans.Rollback();
                        return 0;
                    }
                }
            }
        }
        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open) { conn.Open(); }
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null) { cmd.Transaction = trans; }
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }
    }
}
