using SKABO.BLL.IServices.IJudger;
using SKABO.Common;
using SKABO.Common.Models.Judger;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using SKABO.ActionEngine;
using SKABO.ResourcesManager;

namespace SK_ABO.Pages
{
    class ExperimentManagementData
    {
        public string ExperimentName { get; set; }
        public string ExperimentStartTime { get; set; }
        public string ExperimentState { get; set; }
        public string ExperimentActionName { get; set; }
        public int ExperimentSetpCount{ get; set; }
        public ExperimentManagementData(string taskName,string taskStartTime,string taskState,string taskActionName, int taskSetpCount)
        {
            ExperimentName = taskName;
            ExperimentStartTime = taskStartTime;
            ExperimentState = taskState;
            ExperimentActionName = taskActionName;
            ExperimentSetpCount = taskSetpCount;
        }
    }
    class ExperimentManagementViewModel : Screen
    {
        [StyletIoC.Inject]
        private IResultService resultService;
        [StyletIoC.Inject]
        private IGelService gelService;
        [StyletIoC.Inject]
        private IWindowManager windowManager;

        private Stylet.BindableCollection<ExperimentManagementData> SelectResultList = new BindableCollection<ExperimentManagementData>();
        private Stylet.BindableCollection<ExperimentManagementData> _ResultList = new BindableCollection<ExperimentManagementData>();
        public Stylet.BindableCollection<ExperimentManagementData> ResultList
        {
            get
            {
                return _ResultList;
            }
        }
        public bool is_updata = true;
        public System.Windows.Forms.Timer myTimer = null;
        public void UpData(object sender, EventArgs e)
        {
            if(is_updata)
            {
                ResultList.Clear();
                List<ExperimentPackage> explist = new List<ExperimentPackage>(ExperimentLogic.getInstance().experiment_package_list.ToArray());
                foreach (var exp in explist)
                {
                    if(exp.action_list.Count!=0)
                    {
                        var gel = ResManager.getInstance().GetGelTestByTestId(exp.gel_test_id);
                        if(gel!=null)
                        {
                            string time_str = exp.hatch_cur_time!=0?exp.hatch_cur_time + "/" + exp.hatch_time:"";
                            ResultList.Add(new ExperimentManagementData(gel.GelName + "(" + exp.GetGelMask() + ")", exp.start_time, "正常", exp.action_list[0].GetGelStep().StepName+ time_str, exp.action_list.Count));

                        }
                    }
                    
                }
            }
        }
        protected override void OnViewLoaded()
        {
            myTimer = new System.Windows.Forms.Timer();
            myTimer.Tick += new EventHandler(UpData);
            myTimer.Enabled = true;
            myTimer.Interval = 300;
            myTimer.Start();
            is_updata = true;
            base.OnViewLoaded();
        }
        protected override void OnClose()
        {
            myTimer.Stop();
            myTimer.Dispose();
            base.OnClose();
        }
        public void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            foreach (var item in e.AddedItems)
            {
                SelectResultList.Add((ExperimentManagementData)item);
                
            }
            foreach (var item in e.RemovedItems)
            {
                SelectResultList.Remove((ExperimentManagementData)item);
            }
            is_updata = e.RemovedItems.Count != 0;
        }
    }
}
