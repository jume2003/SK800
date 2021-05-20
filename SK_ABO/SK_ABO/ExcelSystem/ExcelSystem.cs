using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SKABO.Common.Models.Judger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK_ABO.MAI.ExcelSystem
{
    class ExcelSystem
    {
        private static string ShowSaveFileDialog()
        {
            string localFilePath = "";
            //string localFilePath, fileNameExt, newFileName, FilePath; 
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            //设置文件类型 
            sfd.Filter = "Excel表格（*.xls）|*.xls";

            //设置默认文件类型显示顺序 
            sfd.FilterIndex = 1;

            //保存对话框是否记忆上次打开的目录 
            sfd.RestoreDirectory = true;

            //点了保存按钮进入 
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                localFilePath = sfd.FileName.ToString(); //获得文件路径 
                string fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1); //获取文件名，不带路径
            }
            return localFilePath;
        }

        public static bool Export(Stylet.BindableCollection<T_Result> ResultList)
        {
            try
            {
                //设置新建文件路径及名称
                string savePath = ShowSaveFileDialog();
                if (savePath == "") return false;
                //创建一个工作簿
                IWorkbook workbook = new XSSFWorkbook();
                //创建一个 sheet 表
                ISheet sheet = workbook.CreateSheet("test");
                sheet.SetColumnWidth(0, 30 * 256);
                sheet.SetColumnWidth(1, 30 * 256);
                sheet.SetColumnWidth(2, 30 * 256);
                sheet.SetColumnWidth(3, 10 * 256);
                sheet.SetColumnWidth(4, 10 * 256);
                sheet.SetColumnWidth(5, 30 * 256);
                sheet.SetColumnWidth(6, 10 * 256);
                sheet.SetColumnWidth(7, 10 * 256);
                sheet.SetColumnWidth(8, 10 * 256);
                sheet.SetColumnWidth(9, 10 * 256);
                sheet.SetColumnWidth(10, 10 * 256);
                sheet.SetColumnWidth(11, 10 * 256);
                sheet.SetColumnWidth(12, 10 * 256);
                sheet.SetColumnWidth(13, 10 * 256);
                sheet.SetColumnWidth(14, 10 * 256);
                sheet.SetColumnWidth(15, 10 * 256);
                //创建一行
                IRow rowH = sheet.CreateRow(0);
                //创建一个单元格
                ICell cell = null;
                //创建单元格样式
                ICellStyle cellStyle = workbook.CreateCellStyle();
                //创建格式
                IDataFormat dataFormat = workbook.CreateDataFormat();
                //设置为文本格式，也可以为 text，即 dataFormat.GetFormat("text");
                cellStyle.DataFormat = dataFormat.GetFormat("@");
                string[] colname = {
                    "GEL卡名称", "GEL卡条码", "完成时间", "结果", "人工判定", "样本条码", "献血员条码",
                    "备注", "实验人", "修改人", "复核人", "报告人", "载架位置", "LED", "载架脱离"
                };
                for (int i = 0; i < colname.Length; i++)
                {
                    rowH.CreateCell(i).SetCellValue(colname[i]);
                    rowH.Cells[i].CellStyle = cellStyle;
                }

                for (int i = 0; i < ResultList.Count; i++)
                {
                    //跳过第一行，第一行为列名
                    IRow row = sheet.CreateRow(i + 1);
                    string[] cellvaluestr = {
                        ResultList[i].GelName, ResultList[i].GelBarcode, ResultList[i].EndTime.ToString(), ResultList[i].Result, ResultList[i].IsManJudger.ToString(),
                        ResultList[i].SmpBarcode, ResultList[i].DonorBarcode, ResultList[i].Remark, ResultList[i].TestUser, ResultList[i].EditUser,
                        ResultList[i].VerifyUser, ResultList[i].ReportUser, ResultList[i].RackIndex, ResultList[i].LED, ResultList[i].Outed.ToString()};
                    for (int j = 0; j < cellvaluestr.Length; j++)
                    {
                        cell = row.CreateCell(j);
                        cell.SetCellValue(cellvaluestr[j]);
                        cell.CellStyle = cellStyle;
                    }
                }
                //创建文件
                FileStream file = new FileStream(savePath, FileMode.CreateNew, FileAccess.Write);
                //创建一个 IO 流
                MemoryStream ms = new MemoryStream();
                //写入到流
                workbook.Write(ms);
                //转换为字节数组
                byte[] bytes = ms.ToArray();
                file.Write(bytes, 0, bytes.Length);
                file.Flush();
                //释放资源
                bytes = null;
                ms.Close();
                ms.Dispose();
                file.Close();
                file.Dispose();
                workbook.Close();
                sheet = null;
                workbook = null;
            }
            catch (Exception ex)
            {
               
            }
            return true;
        }
    }
}
