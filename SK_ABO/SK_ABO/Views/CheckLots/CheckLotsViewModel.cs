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
using SKABO.ActionEngine;

namespace SK_ABO.Views.CheckLots
{
    public class CheckLotsViewModel : Screen
    {
        [StyletIoC.Inject]
        private IWindowManager windowManager;
        [StyletIoC.Inject]
        private IResultService resultService;
        [StyletIoC.Inject]
        private IUserService userService;
        private static Stylet.BindableCollection<T_Result> LotsResultList = new BindableCollection<T_Result>();
        public static string _TitleStr;
        public static string TitleStr
        {
            get
            {
                return _TitleStr;
            }

            set
            {
                _TitleStr = value;
            }
        }
        private static Stylet.BindableCollection<T_Result> _ResultList;
        public static Stylet.BindableCollection<T_Result> ResultList
        {
            get
            {
                if (_ResultList == null)
                {
                    _ResultList = new BindableCollection<T_Result>();
                }
                return _ResultList;
            }
        }
        public string LastReportID = "";

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
        }

        public static void SetReultList(Stylet.BindableCollection<T_Result> result)
        {
            ResultList.Clear();
            foreach (var item in result)
            {
                ResultList.Add(item);
            }
            ResultList.Refresh();
        }

        public void Ok()
        {
            SecurityViewModel VM = IoC.Get<SecurityViewModel>();
            var result = windowManager.ShowDialog(VM);
            if (result.HasValue && result.Value)
            {
                LotsResultList.Clear();
                foreach (var item in ResultList)
                {
                    if(item.IsSel)
                        LotsResultList.Add(item);
                }
                if (TitleStr == "批量复核")
                    CheckReultList(VM.LoginID);
                else if (TitleStr == "解除复核")
                    CancelReultList(VM.LoginID);
                else if (TitleStr == "批量传输")
                    SendReultList(VM.LoginID);
            }
            RequestClose();
        }
        //批量复核
        public void CheckReultList(string VerifyUser)
        {
            foreach (var item in LotsResultList)
            {
                item.VerifyUser = VerifyUser;
                item.VerifyTime = DateTime.Today;
                resultService.UpdateT_Result(item);
            }
        }
        //批量解除复核
        public void CancelReultList(string VerifyUser)
        {
            foreach (var item in LotsResultList)
            {
                item.VerifyUser = "";
                resultService.UpdateT_Result(item);
            }
        }
        //批量传输
        public void SendReultList(string ReportID)
        {
            //更新结果文本
            LastReportID = ReportID;
            var listConf = userService.QuerySysConfig("LisConifg").SnValue.ToInstance<LisConifg>();
            if(Directory.Exists(listConf.ResultDir)&& Directory.Exists(listConf.DuplexDir))
            {
                if(LotsResultList.Count!=0)
                {
                    foreach (var item in LotsResultList)
                    {
                        item.ReportUser = ReportID;
                        item.ReportTime = DateTime.Today;
                        resultService.UpdateT_Result(item);

                    }
                    var his_system = HisSystem.getInstance();
                    his_system.SetDirs(listConf.ResultDir, listConf.DuplexDir);
                    his_system.WriteResul(LotsResultList.ToList());
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("没有选中任何一项!!");
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("双工文件夹不存在!!");
            }
        }
        
        public void SelAll()
        {
            foreach (var item in ResultList)
            {
                item.IsSel = true;
            }
            ResultList.Refresh();
        }

        public void CancelAll()
        {
            foreach (var item in ResultList)
            {
                item.IsSel = false;
            }
            ResultList.Refresh();
        }

        public void Cancel()
        {
            RequestClose();
        }
    }
}
