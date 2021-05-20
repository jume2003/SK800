using SKABO.BLL.IServices.IJudger;
using SKABO.Common;
using SKABO.Common.Models.Judger;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKABO.BLL.IServices.IUser;
using SKABO.Common.Utils;
using SKABO.Common.Models.Duplex;
using System.IO;
using SKABO.Common.Views;
using SKABO.Hardware.RunBJ;
using SKABO.Common.Parameters.Judger;
using SKABO.Common.Models.GEL;
using SKABO.BLL.IServices.IGel;
using SK_ABO.Views.QcPic;
using System.Windows.Controls;
using SK_ABO.MAI.ExcelSystem;

namespace SK_ABO.Pages
{
    class QcEtcViewModel : Screen
    {
        [StyletIoC.Inject]
        private IResultService resultService;
        [StyletIoC.Inject]
        private IGelService gelService;
        [StyletIoC.Inject]
        private IWindowManager windowManager;

        private Stylet.BindableCollection<T_Result> SelectResultList = new BindableCollection<T_Result>();
        public ResultParameter resultParameter { get; set; } = new ResultParameter { StartTime = DateTime.Today, EndTime = DateTime.Today};
        private Stylet.BindableCollection<T_Result> _ResultList = new BindableCollection<T_Result>();
        public Stylet.BindableCollection<T_Result> ResultList
        {
            get
            {
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
        private string _QcSmpbarCode;
        public string QcSmpbarCode
        {
            get
            {
                return _QcSmpbarCode;
            }
            set
            {
                _QcSmpbarCode = value;
            }
        }


        
        public T_Gel SelectedGel { get; set; }
        /// <summary>
        /// 结果查询
        /// </summary>
        public void Query()
        {
            ResultList.Clear();
            resultParameter.SmpBarcode = QcSmpbarCode;
            ResultList.AddRange(resultService.QueryT_Result(resultParameter));
        }
        public void Printer()
        {
          
        }
        public void ShowQcPic()
        {
            var vm = IoC.Get<QcPicViewModel>();
            vm.UpData(SelectResultList);
            windowManager.ShowDialog(vm);
        }
        public void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            //SelectResultList.Clear();
            //dg.Columns
            foreach (var item in e.AddedItems)
            {
                SelectResultList.Add((T_Result)item);
            }
            foreach (var item in e.RemovedItems)
            {
                SelectResultList.Remove((T_Result)item);
            }
        }
    }
}
