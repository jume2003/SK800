using SKABO.BLL.IServices.IGel;
using SKABO.Common.Models.GEL;
using SKABO.Common.Utils;
using Stylet;
using System;
using System.Collections.Generic;
using SKABO.Common;
using System.Windows.Controls;
using System.Windows.Input;


namespace SK_ABO.Views.GEL
{
    public class GelManagerViewModel:Screen
    {
        private IGelService gelService;
        private IWindowManager windowManager;
        public List<T_Gel> SelectedGel { get; set; } = new List<T_Gel>();
        public GelManagerViewModel(IWindowManager windowManager, IGelService gelService)
        {
            this.windowManager = windowManager;
            this.gelService = gelService;
        }
        public BindableCollection<T_Gel> Gels { get; set; } = new BindableCollection<T_Gel>();
        public void Window_Loaded()
        {
            var list = gelService.QueryAllGel();
            if(list!=null)
                Gels.AddRange(list);
        }
        public void Close()
        {
            this.RequestClose();
        }
        public void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            foreach (var item in e.AddedItems)
            {
                SelectedGel.Add((T_Gel)item);

            }
            foreach (var item in e.RemovedItems)
            {
                SelectedGel.Remove((T_Gel)item);
            }
        }
        public void CreateGel()
        {
            var VM = IoC.Get<CreateOrModifyViewModel>();
            VM.t_Gel = new T_Gel();
            VM.t_Gel.ResultMaps = new List<T_ResultMap>();
            VM.t_Gel.GelSteps = new List<T_GelStep>();
            windowManager.ShowDialog(VM);
        }
        public void ModifyGel(DataGrid dataGrid)
        {
            if (SelectedGel == null)
            {
                windowManager.ShowMessageBox("请选择要修改的Gel卡");
                return;
            }
            showGelWin(SelectedGel[0]);
        }
        private void showGelWin(T_Gel t_Gel)
        {
            var VM = IoC.Get<CreateOrModifyViewModel>();
            VM.t_Gel = t_Gel;
            windowManager.ShowDialog(VM);
        }
        public void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var Row = DataGridUtil.GetRowMouseDoubleClick(sender, e);
            if (Row == null) return;
            var t_Gel = (T_Gel)Row.Item;
            showGelWin(t_Gel);
        }
        public void CopyGel()
        {
            if (SelectedGel == null)
            {
                windowManager.ShowMessageBox("请选择要复制的Gel卡");
                return;
            }
            List<T_Gel> selsteps = new List<T_Gel>(SelectedGel.ToArray());
            foreach (var selstep in selsteps)
            {
                var TargetGel = gelService.CopyGel(selstep);
                Gels.Add(TargetGel);
            }
        }
        public void DeleteGel()
        {
            if (SelectedGel == null)
            {
                windowManager.ShowMessageBox("请选择要删除的Gel卡");
                return;
            }
            List<T_Gel> selsteps = new List<T_Gel>(SelectedGel.ToArray());
            foreach (var selstep in selsteps)
            {
                var result = windowManager.ShowMessageBox(String.Format("确定删除【{0}】吗?", selstep.GelName), "系统提示", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk);
                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    if (gelService.DeleteGel(selstep))
                    {
                        Gels.Remove(selstep);
                    }
                    else
                    {
                        windowManager.ShowMessageBox("删除失败！");
                        return;
                    }
                }
            }
        }
    }
}
