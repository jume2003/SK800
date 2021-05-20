using SKABO.BLL.IServices.IGel;
using SKABO.BLL.IServices.IJudger;
using SKABO.Common.Models.GEL;
using SKABO.Common.Utils;
using SKABO.Judger.Core.Models;
using Stylet;
using System;
using SKABO.Common;
using System.Windows.Controls;
using System.Windows.Input;
using SKABO.ActionEngine;

namespace SK_ABO.Views.GEL
{
    public class CreateOrModifyViewModel : Screen
    {
        private IGelService gelService;
        private readonly IWindowManager windowManager;
        private bool IsNew;
        public UnknownResult unknown { get; set; }
        public BindableCollection<TubeName> Tbs { get; set; } = new BindableCollection<TubeName>();

        public BindableCollection<T_GelStep> GelSteps { get; set; } = new BindableCollection<T_GelStep>();
        public T_Gel t_Gel { get; set; }
        public CreateOrModifyViewModel(IWindowManager windowManager, IGelService gelService)
        {
            this.windowManager = windowManager;
            this.gelService = gelService;
        }
        public void Window_Loaded()
        {
            if (t_Gel.ID==0)
            {
                IsNew = true;
                base.DisplayName = "新建";
            }
            else
            {
                base.DisplayName = "修改";
            }

            unknown = new UnknownResult(t_Gel.UnknownResult);

            Tbs.Clear();
            Tbs.Add(new TubeName(1, t_Gel.Name1));
            Tbs.Add(new TubeName(2, t_Gel.Name2));
            Tbs.Add(new TubeName(3, t_Gel.Name3));
            Tbs.Add(new TubeName(4, t_Gel.Name4));
            Tbs.Add(new TubeName(5, t_Gel.Name5));
            Tbs.Add(new TubeName(6, t_Gel.Name6));
            Tbs.Add(new TubeName(7, t_Gel.Name7));
            Tbs.Add(new TubeName(8, t_Gel.Name8));
            
            var count=t_Gel.GelSteps.Count;
            GelSteps.AddRange(t_Gel.GelSteps);
            for (int i = 0; i < 20 - count; i++)
            {
                GelSteps.Add(new T_GelStep() { StepIndex=count+i});
            }
        }
        public void Close()
        {
            this.RequestClose();
        }
        /// <summary>
        /// 保存前同步Tube管名
        /// </summary>
        private void SyncTubeName()
        {
            t_Gel.Name1 = Tbs[0].Name;
            t_Gel.Name2 = Tbs[1].Name;
            t_Gel.Name3 = Tbs[2].Name;
            t_Gel.Name4 = Tbs[3].Name;
            t_Gel.Name5 = Tbs[4].Name;
            t_Gel.Name6 = Tbs[5].Name;
            t_Gel.Name7 = Tbs[6].Name;
            t_Gel.Name8 = Tbs[7].Name;
        }
        /// <summary>
        /// 保存前同步实验步骤
        /// </summary>
        private void SyncStep()
        {
            t_Gel.GelSteps.Clear();
            GelSteps.AddRange(t_Gel.GelSteps);
            for (int i = 0; i < GelSteps.Count; i++)
            {
                if (String.IsNullOrEmpty(GelSteps[i].StepName))
                    continue;
                t_Gel.GelSteps.Add(GelSteps[i]);
            }
        }
        /// <summary>
        /// 确定 按钮
        /// </summary>
        /// <param name="dg"></param>
        public void ClickOK(DataGrid dg)
        {
            if (DataGridUtil.GetDataGridRowsHasError(dg))
            {
                SwitchTab(1);
                this.View.UpdateLayout();
                //windowManager.ShowMessageBox("请详细检查数据");
                dg.ShowHint(new MessageWin("请详细检查数据"));
                return;
            }
            SyncTubeName();
            SyncStep();
            unknown.Refresh();
            t_Gel.UnknownResult = unknown.ToString();

            var lastgel_list = gelService.QueryGelStepByGelId(t_Gel.ID);
            foreach(var lastgel in lastgel_list)
            {
                bool is_find = false;
                foreach (var gelstep in GelSteps)
                {
                    if (lastgel.ID == gelstep.ID)
                    {
                        is_find = true;
                        break;
                    }
                }
                if(is_find==false)
                gelService.DeleteGelStep(lastgel);
            }

            var lastgelres_list = gelService.QueryResMapByGelId(t_Gel.ID);
            foreach (var lastresgel in lastgelres_list)
            {
                bool is_find = false;
                foreach (var resmap in t_Gel.ResultMaps)
                {
                    if (lastresgel.ID == resmap.ID)
                    {
                        is_find = true;
                        break;
                    }
                }
                if (is_find == false)
                gelService.DeleteGelResutMap(lastresgel);
            }

            //按顺序排列setps
            for (int i=0;i < t_Gel.GelSteps.Count;i++)
            {
                t_Gel.GelSteps[i].StepIndex = i;
            }
            if (gelService.InserOrUpdateGEL(t_Gel))
            {
                Engine.getInstance().InitRes();
                windowManager.ShowMessageBox("保存成功！");
            }
            else
            {
                windowManager.ShowMessageBox("保存失败！");
            }
        }
        private void SwitchTab(int index)
        {
            var v = this.View as CreateOrModifyView;
            v.TabContent.SelectedIndex = index;
        }
        public void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Column.DisplayIndex != 2) return;
            System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var v = colorDialog.Color;
                
                    var Tmap = e.Row.Item as T_ResultMap;
                    Tmap.Color= String.Format("#{0}{1}{2}{3}", v.A.ToString("X2"), v.R.ToString("X2"), v.G.ToString("X2"), v.B.ToString("X2"));
                e.Row.Background = Tmap.ColorBrush;
                
            }
        }
        public void MoveUp(DataGrid dg)
        {
            if (dg.SelectedIndex <= 0) return;
            var index = dg.SelectedIndex;
            var data = GelSteps[index];
            GelSteps.RemoveAt(index);
            GelSteps.Insert(index - 1, data);
            dg.SelectedIndex = index - 1;
            GelSteps[index].StepIndex++;
            GelSteps[index - 1].StepIndex--;

        }
        public void MoveDown(DataGrid dg)
        {
            if (dg.SelectedIndex >= 19) return;
            var index = dg.SelectedIndex;
            var data = GelSteps[index];
            GelSteps.RemoveAt(index);
            GelSteps.Insert(index +1, data);
            dg.SelectedIndex = index + 1;
            GelSteps[index].StepIndex--;
            GelSteps[index+1].StepIndex++;
        }
        public void Delete(DataGrid dg)
        {
            if (dg.SelectedIndex >= 19) return;
            var index = dg.SelectedIndex;
            var data = GelSteps[index];
            GelSteps.RemoveAt(index);
        }
        public void EditStep(DataGrid dg)
        {
            if (dg.SelectedIndex == -1) return;
            showStepWin(dg.SelectedItem as T_GelStep);
        }
        private void showStepWin(T_GelStep t_GelStep)
        {
            var vm = IoC.Get<EditStepViewModel>();
            vm.t_GelStep = t_GelStep;
            windowManager.ShowDialog(vm);
        }
        public void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var Row = DataGridUtil.GetRowMouseDoubleClick(sender, e);
            if (Row == null) return;
            var t_GelStep = (T_GelStep)Row.Item;
            t_GelStep.GoSideID = Row.GetIndex();
            showStepWin(t_GelStep);
        }

    }
    public class TubeName{
        public TubeName(byte Index,String Name)
        {
            this.Index = Index;
            this.Name = Name;
        }
        public byte Index { get; set; }
        public String Name { get; set; }
        }
}
