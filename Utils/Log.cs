using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class Log
    {


        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="methodName">报错方法</param>
        /// <param name="errorText">报错描述</param>
        public static void LogWrite(string methodName, string errorText,string yyid="qb")
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + @"\log";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            #region 删除之前的记录
            int det_i = -30;
            string del_path = path + "\\" + DateTime.Now.AddDays(det_i).ToString("yyyy-MM-dd").Split(' ')[0] + " "+yyid+" log.txt";
            while (File.Exists(del_path))
            {
                File.Delete(del_path);
                det_i = det_i - 1;
                del_path = path + "\\" + DateTime.Now.AddDays(det_i).ToString("yyyy-MM-dd").Split(' ')[0] + " " + yyid + " log.txt";
            }
            #endregion
            path += "\\" + DateTime.Now.ToString("yyyy-MM-dd").Split(' ')[0] + " " + yyid + " log.txt";


            //避免两个页面同时访问同一个log.txt而报错。
            try
            {
                System.IO.File.AppendAllText(path, "\r\n---------" + DateTime.Now + "---------\r\n日志方法：" + methodName + "；\r\n日志描述：" + errorText + "\r\n\r\n", Encoding.Default);
            }
            catch (Exception)
            {

            }

        }

    }
}
