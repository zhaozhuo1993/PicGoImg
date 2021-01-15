using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Utils
{
    public class ExcelHelper
    {
        HSSFWorkbook workbook;
        List<CellMerged> cellMerged;
        public ExcelHelper()
        {
            workbook = new HSSFWorkbook();
            cellMerged = new List<CellMerged>();
        }
        //转诊统计导出
        public void ExportToExcel(DataTable dt, string createFileName)
        {
            ISheet sheet = workbook.CreateSheet();
            IRow row = sheet.CreateRow(0);
            string a = dt.Rows[0][0].ToString();
            string b = dt.Rows[0][0].ToString();
            int s = 1; int e = 1;
            ICellStyle style = workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                b = dt.Rows[i][0].ToString();
                row = sheet.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++) { row.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString()); }
                if (a == b) { if (i > 0) e++; }
                else
                {
                    a = dt.Rows[i][0].ToString();
                    cellMerged.Add(new CellMerged(s, e));
                    ++e; s = e;
                }
            }
            //设置列宽自适应
            for (int i = 0; i < dt.Columns.Count; i++) { sheet.GetRow(0).CreateCell(i).SetCellValue(dt.Columns[i].ColumnName); sheet.AutoSizeColumn(i); }
            for (int i = 0; i < cellMerged.Count(); i++)
            {
                sheet.AddMergedRegion(new CellRangeAddress(cellMerged[i].start, cellMerged[i].end, 0, 0));
                sheet.GetRow(cellMerged[i].start).GetCell(0).CellStyle = style;
            }
            WriteToFile(createFileName);
        }
        //通用导出
        public void ExportToExcel1(DataTable dt, string createFileName)
        {
            ISheet sheet = workbook.CreateSheet();
            IRow row = sheet.CreateRow(0);
            ICellStyle style = workbook.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                row = sheet.CreateRow(i + 1);

                for (int j = 0; j < dt.Columns.Count; j++)
                { row.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString()); }
            }
            //设置列宽自适应
            for (int i = 0; i < dt.Columns.Count; i++)
            { sheet.GetRow(0).CreateCell(i).SetCellValue(dt.Columns[i].ColumnName); sheet.AutoSizeColumn(i); }
            for (int i = 0; i < cellMerged.Count(); i++)
            {
                sheet.AddMergedRegion(new CellRangeAddress(cellMerged[i].start, cellMerged[i].end, 0, 0));
                sheet.GetRow(cellMerged[i].start).GetCell(0).CellStyle = style;
            }
            WriteToFile(createFileName);
        }
        /// <summary>
        ///  生成文件
        /// </summary>
        private void WriteToFile(string createFileName)
        {
            //设置生成文件的目录
            var path = HttpContext.Current.Server.MapPath("~/excel");
            if (!Directory.Exists(path))//如果没有文件夹就创建  
            {
                Directory.CreateDirectory(path);
            }
            DeleteAll(path);//清空文件夹
            FileStream file = new FileStream(path + @"/" + createFileName + ".xls", FileMode.Create);
            workbook.Write(file);
            file.Close();
        }
        private void DeleteAll(string path)
        {
            foreach (string d in Directory.GetFileSystemEntries(path))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1) { fi.Attributes = FileAttributes.Normal; }
                    File.Delete(d);
                }
                else
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    if (d1.GetFiles().Length != 0)
                    {
                        DeleteAll(d1.FullName);//递归删除子文件夹
                    }
                    Directory.Delete(d);
                }
            }
        }
        /// <summary>
        /// 读取excel
        /// </summary>
        public HSSFWorkbook GetExcelWorkBook(string fileName, Stream file)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(fileName);
                if (fileInfo.Extension.ToLower().Equals(".xls"))
                    workbook = new HSSFWorkbook(file);
            }
            catch (Exception ex)
            {
                return null;
            }
            return workbook;
        }
    }

    public class CellMerged
    {
        public CellMerged() { }
        public CellMerged(int start, int end) { this.start = start; this.end = end; }
        public int start { get; set; }
        public int end { get; set; }
    }
}