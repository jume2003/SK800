using SK_ABO.UserCtrls;
using SK_ABO.Views;
using SKABO.BLL.IServices.IGel;
using SKABO.BLL.IServices.IJudger;
using SKABO.Common.Models.GEL;
using SKABO.Common.Models.Judger;
using SKABO.Common.Parameters.Judger;
using Stylet;
using System;
using System.Collections.Generic;
using SKABO.Common;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using SKABO.BLL.IServices.IUser;
using SKABO.Common.Utils;
using SKABO.Common.Models.Duplex;
using SK_ABO.MAI.ExcelSystem;
using SK_ABO.Views.CheckLots;
using SK_ABO.Views.QueryStaInfo;
using SKABO.Common.Enums;
using SKABO.BLL.IServices.ITrace;

namespace SK_ABO.Pages
{
    public class QueryViewModel:Screen
    {
        [StyletIoC.Inject]
        private IWindowManager windowManager;
        [StyletIoC.Inject]
        private IResultService resultService;
        [StyletIoC.Inject]
        private IGelService gelService;
        [StyletIoC.Inject]
        private IUserService userService;
        [StyletIoC.Inject]
        private ITraceService TraceService;
        /// <summary>
        /// 搜索面板是否打开
        /// </summary>
        public bool IsOpenSearchPanel { get; set; }
        public ResultParameter resultParameter { get; set; } = new ResultParameter { StartTime=DateTime.Today, EndTime=DateTime.Today};

        private Stylet.BindableCollection<T_Result> _ResultList;
        public Stylet.BindableCollection<T_Result> ResultList
        {
            get
            {
                if (_ResultList == null)
                {
                    _ResultList = new BindableCollection<T_Result>();
                    _ResultList.AddRange(resultService.QueryT_Result(resultParameter));
                }
                return _ResultList;
            }
        }
        private IList<T_Gel> _GeltList;
        public IList<T_Gel> GelList
        {
            get
            {
                if (_GeltList == null)
                {
                    _GeltList = gelService.QueryAllGel();
                }
                return _GeltList;
            }
        }
        public String ResultStr { get; set; }
        public SolidColorBrush ColorBrush { get; set; }
        public T_Gel SelectedGel { get; set; }
        public T_Result SelectedResult { get; set; }
        private IList<T_Result> _SelectedResults;
        public IList<T_Result> SelectedResults { get
            {
                if (_SelectedResults == null)
                {
                    _SelectedResults = new List<T_Result>();
                }
                return _SelectedResults;
            }
        }
        /// <summary>
        /// 导出excel文件
        /// </summary>
        public void ExportExcel()
        {
            if(ExcelSystem.Export(ResultList))
            windowManager.ShowMessageBox("导出excel文件");
        }
        /// <summary>
        /// 批量传输
        /// </summary>
        public void SendLots()
        {
            CheckLotsViewModel VM = IoC.Get<CheckLotsViewModel>();
            CheckLotsViewModel.SetReultList(ResultList);
            CheckLotsViewModel.TitleStr = "批量传输";
            var result = windowManager.ShowDialog(VM);
        }
        /// <summary>
        /// 重新传输
        /// </summary>
        public void ReSendLots()
        {
            CheckLotsViewModel VM = IoC.Get<CheckLotsViewModel>();
            if(VM.LastReportID!="")
            VM.SendReultList(VM.LastReportID);
        }
        /// <summary>
        /// 批量复核
        /// </summary>
        public void CheckLots()
        {
            CheckLotsViewModel VM = IoC.Get<CheckLotsViewModel>();
            CheckLotsViewModel.SetReultList(ResultList);
            CheckLotsViewModel.TitleStr = "批量复核";
            var result = windowManager.ShowDialog(VM);
        }
        /// <summary>
        /// 解除复核
        /// </summary>
        public void CancelLots()
        {
            CheckLotsViewModel VM = IoC.Get<CheckLotsViewModel>();
            CheckLotsViewModel.TitleStr = "解除复核";
            CheckLotsViewModel.SetReultList(ResultList);
            var result = windowManager.ShowDialog(VM);
        }
        // <summary>
        /// 显示统计
        /// </summary>
        public void StatisticsInfo()
        {
            Dictionary<string, int> statistcount = new Dictionary<string, int>();
            foreach (var item in ResultList)
            {
                string key = item.GelName + "\t" + item.Result;
                if(statistcount.ContainsKey(key))
                {
                    statistcount[key]++;
                }
                else
                {
                    statistcount.Add(key, 1);
                }
            }
            QueryStaInfoViewModel VM = IoC.Get<QueryStaInfoViewModel>();
            VM.SetStaInfo(statistcount);
            var result = windowManager.ShowDialog(VM);
        }

        public void TubeResultChanged(object sender, EventArgs e)
        {
            if (sender is TubeResult_Control tube && SelectedResult != null&& SelectedResult.Gel!=null) 
            {
                String color = null;
                T_Picture pic = new T_Picture();
                RefreshPicture(pic);
                ResultStr = this.resultService.FinishResult(SelectedResult.Gel, out color, resultService.GetResultEnums(pic, SelectedResult.TubeStartNo, SelectedResult.TubeCount));
                if (!String.IsNullOrEmpty(color))
                {
                    ColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color.Trim()));
                }
                else
                {
                    ColorBrush = null;
                }
            }
             
        }
        /// <summary>
        /// 结果查询
        /// </summary>
        public void Query()
        {
            IsOpenSearchPanel = false;
            ResultList.Clear();
            ResultList.AddRange(resultService.QueryT_Result(resultParameter));
        }
        /// <summary>
        /// 打印
        /// </summary>
        public void Printer()
        {
            QueryView VM = IoC.Get<QueryView>();
            VM.Printer();
        }
        /// <summary>
        /// 列表管理
        /// </summary>
        /// <param name="btn"></param>
        public void ShowContextMenu(Button btn)
        {
            btn.ContextMenu.IsOpen = true;
        }
        public void QueryToday()
        {
            resultParameter.Clear();
            resultParameter.StartTime = DateTime.Today;
            resultParameter.EndTime = DateTime.Today;
            Query();
        }
        /// <summary>
        /// 显示或隐藏搜索面板
        /// </summary>
        public void ShowSearch()
        {
            IsOpenSearchPanel = !IsOpenSearchPanel;
        }
        /// <summary>
        /// 重置查询条件
        /// </summary>
        public void ResetQuery()
        {
            resultParameter.Clear();
            resultParameter.StartTime = DateTime.Today;
            resultParameter.EndTime = DateTime.Today;
        }
        public void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            
            //(dg.Columns[0].CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty,))

            this.SelectedResults.Clear();
            foreach(var item in e.AddedItems)
            {
                this.SelectedResults.Add((T_Result)(item));
            }
            if (this.SelectedResults.Count > 0)
            {
                ShowRight(this.SelectedResults[this.SelectedResults.Count - 1]);
                //dg.Columns[0].
            }
            
        }
        private void ShowRight(T_Result result)
        {
            if (result.Gel == null)
            {
                result.Gel=GelList.Where(g => g.ID == result.GelID).FirstOrDefault();
            }
            if (result.Picture == null)
            {

            }
            this.SelectedResult = null;
            SK_ABO.UserCtrls.TubeResult_Control[]tubes = {
                (this.View as QueryView).Img_Tube1, (this.View as QueryView).Img_Tube2, (this.View as QueryView).Img_Tube3, (this.View as QueryView).Img_Tube4,
                (this.View as QueryView).Img_Tube5, (this.View as QueryView).Img_Tube6, (this.View as QueryView).Img_Tube7, (this.View as QueryView).Img_Tube8};
            for (int i = 0; i < 8; i++)
            {
                tubes[i].TestResult = null;
            }
            for (int i= result.TubeStartNo;i< result.TubeStartNo+ result.TubeCount;i++)
            {
                tubes[i].TestResult = result;
            }
            this.SelectedResult = result;
            this.ResultStr = result.Result;
            this.CanSaveChange = true;
        }
        /// <summary>
        /// 常用语选项改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ComboBox cmb)
            {
                if (SelectedResult != null)
                {
                    this.SelectedResult.Remark = cmb.SelectedValue.ToString();
                }
            }
        }
        public bool CanSaveChange { get; set; }
        public void SaveChange()
        {
            if (SelectedResult == null)
            {
                return;
            }
            var changed = RefreshPicture(SelectedResult.Picture);
            if (!SelectedResult.IsManJudger && changed)
            {
                SelectedResult.IsManJudger = true;
            }
            SelectedResult.EditTime = DateTime.Now;
            SelectedResult.EditUser = Constants.Login.LoginName;
            String color = null;
            string srcresult = SelectedResult.Result;
            SelectedResult.Result=this.resultService.FinishResult(SelectedResult.Gel, out color, resultService.GetResultEnums(SelectedResult.Picture, SelectedResult.TubeStartNo, SelectedResult.TubeCount));
            SelectedResult.Color = color;
            this.resultService.UpdateT_Result(SelectedResult);
            TraceService.InsertT_Trace(TraceLevelEnum.LowImportant, $"<结果修改>{SelectedResult.GelName};{SelectedResult.SmpBarcode};{srcresult}->{SelectedResult.Result}");
        }
        /// <summary>
        /// 更新手工结果
        /// </summary>
        /// <returns>是否人工修改过检测值</returns>
        private bool RefreshPicture(T_Picture picture)
        {
            var result = false;
            var end= SelectedResult.TubeStartNo+ SelectedResult.TubeCount;
            for (var i= SelectedResult.TubeStartNo;i< end; i++)
            {
                var tp=typeof(T_Picture).GetProperty($"T{i + 1}"); 
                var res=Convert.ToInt32(tp.GetValue(picture));
                var chgVal=(int)((this.View as QueryView).FindName($"Img_Tube{i + 1}") as TubeResult_Control).Value;
                if (!result)
                {
                    result = res != chgVal;
                }
                if(res != chgVal)
                {
                    tp.SetValue(picture, chgVal);
                }
            }
            
            return result;
        }
        
    }
}
