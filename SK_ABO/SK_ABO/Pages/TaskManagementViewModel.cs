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
using SKABO.ActionEngine;

namespace SK_ABO.Pages
{
    class TaskManagementData
    {
        public string TaskName { get; set; }
        public string TaskStartTime { get; set; }
        public string TaskState { get; set; }
        public string TaskActionName { get; set; }
        public int TaskSetpCount{ get; set; }
        public int TaskID { get; set; }
        public TaskManagementData(int taskID,string taskName,string taskStartTime,string taskState,string taskActionName, int taskSetpCount)
        {
            TaskID = taskID;
            TaskName = taskName;
            TaskStartTime = taskStartTime;
            TaskState = taskState;
            TaskActionName = taskActionName;
            TaskSetpCount = taskSetpCount;
        }
    }
    class TaskManagementViewModel : Screen
    {
        [StyletIoC.Inject]
        private IResultService resultService;
        [StyletIoC.Inject]
        private IGelService gelService;
        [StyletIoC.Inject]
        private IWindowManager windowManager;

        private Stylet.BindableCollection<TaskManagementData> SelectResultList = new BindableCollection<TaskManagementData>();


        private Stylet.BindableCollection<TaskManagementData> _ResultList = new BindableCollection<TaskManagementData>();
        public Stylet.BindableCollection<TaskManagementData> ResultList
        {
            get
            {
                return _ResultList;
            }
        }
        public bool is_updata = true;
        public System.Windows.Forms.Timer myTimer = null;
        public TaskManagementData GetResultListByID(int id)
        {
            foreach (var taskdata in ResultList)
            {
                if (taskdata.TaskID == id) return taskdata;
            }
            return null;
        }
        public void UpData(object sender, EventArgs e)
        {
            if(is_updata)
            {
                ResultList.Clear();
                var action_manager = ActionManager.getInstance();
                var actions = action_manager.getActions();
                foreach (var act in actions)
                {
                    string action_state = "";
                    int task_count = 1;
                    if (act is Sequence seque)
                    {
                        task_count = seque.actionlist.Count();
                    }
                    string task_state = "运行";
                    string gel_mask = "";
                    if (act.isInit()==false) task_state = "等待";
                    if (act.getIsStop()) task_state = "停止";
                    if (act.istimeout) task_state = "超时"+"_"+act.errmsg;
                    if (act.exp_pack != null) gel_mask = act.exp_pack.GetGelMask();
                    ResultList.Add(new TaskManagementData(act.getID(), act.getName()+" "+ gel_mask, act.getStartTime(), task_state, act.getActState(), task_count));
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
                SelectResultList.Add((TaskManagementData)item);
                
            }
            foreach (var item in e.RemovedItems)
            {
                SelectResultList.Remove((TaskManagementData)item);
            }
            is_updata = e.RemovedItems.Count != 0;
        }
        public void StopTask()
        {
            foreach(var task in SelectResultList)
            {
                var action_manager = ActionManager.getInstance();
                var actions = action_manager.getActions(task.TaskID);
                foreach(var act in actions)
                {
                    act.stop();
                    is_updata = true;
                }
            }
        }
        public void RunTask()
        {
            lock (ActionManager.lockObj)
            {
                foreach (var task in SelectResultList)
                {
                    var action_manager = ActionManager.getInstance();
                    var actions = action_manager.getActions(task.TaskID);
                    foreach (var act in actions)
                    {
                        act.istimeout = false;
                        is_updata = true;
                    }
                }
            }
        }
        public void DeleteTask()
        {
            lock (ActionManager.lockObj)
            {
                foreach (var task in SelectResultList)
                {
                    var action_manager = ActionManager.getInstance();
                    var actions = action_manager.getActions(task.TaskID);
                    foreach (var act in actions)
                    {
                        act.isdelete = true;
                        act.isfinish = true;
                        is_updata = true;
                    }
                }
            }
        }
    }
}
