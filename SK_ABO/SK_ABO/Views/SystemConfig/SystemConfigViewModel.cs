using SKABO.BLL.IServices.IUser;
using SKABO.Common.Models.Duplex;
using Stylet;
using SKABO.Common.Utils;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using SKABO.Common.Models.GEL;
using SKABO.Common;
using SKABO.BLL.IServices.IGel;

namespace SK_ABO.Views.SystemConfig
{
    public class SystemConfigViewModel:Screen
    {
        [StyletIoC.Inject]
        private IUserService userService;
        public LisConifg LisConf { get; set; }
        public bool HasLoaded { get; set; }
        public void Close()
        {
            this.RequestClose();
        }
        private IList<T_Gel> _GelList;
        public IList<T_Gel> GelList
        {
            get
            {
                if (_GelList == null)
                {
                    var gelService = IoC.Get<IGelService>();
                    _GelList = gelService.QueryAllGel();
                    _GelList.Insert(0, new T_Gel() { LisGelClass = -1, TestName = "" });
                }
                return _GelList;
            }
        }
        public void SeletcDir(string DorR)
        {
            System.Windows.Forms.FolderBrowserDialog m_Dialog = new System.Windows.Forms.FolderBrowserDialog();
            if ("D" == DorR)
            {
                m_Dialog.SelectedPath = LisConf.DuplexDir;
            }
            else
            {
                m_Dialog.SelectedPath = LisConf.ResultDir;
            }
            
            System.Windows.Forms.DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            m_Dir=m_Dir.Replace(AppDomain.CurrentDomain.BaseDirectory, "");
            if ("D" == DorR)
            {
                LisConf.DuplexDir = m_Dir;
            }
            else
            {
                LisConf.ResultDir = m_Dir;
            }
        }
        protected override void OnViewLoaded()
        {
            if (HasLoaded) return;
            base.OnViewLoaded();
            var sc=userService.QuerySysConfig("LisConifg");
            if (sc != null)
            {
                this.LisConf = sc.SnValue.ToInstance<LisConifg>();
            }
            
                this.LisConf = LisConf?? new LisConifg() { DuplexDir = @"lis\1", ResultDir = @"lis\2", NeedConfirm = true,AutoSendResult=false,IncludePic=false };
            
            
        }
        private int SelectedIndex;
        public void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if( sender is TabControl tab)
            {
                SelectedIndex = tab.SelectedIndex;
            }
        }
        public void Save()
        {
            switch (SelectedIndex)
            {
                case 2:
                    {
                        var res = userService.UpdateSysConfig(new SKABO.Common.Models.Config.SysConfig("LisConifg", this.LisConf.ToJsonStr()));
                        this.View.ShowHint(new SK_ABO.Views.MessageWin(res > 0));
                        break;
                    }
            }
        }
    }
}
