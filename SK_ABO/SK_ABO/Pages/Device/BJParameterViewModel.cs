using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using SKABO.Common.Models.Attributes;
using System.Windows.Data;
using SKABO.Common.Models.BJ;
using SKABO.BLL.IServices.IDevice;
using SKABO.Common;
using SKABO.Common.Enums;
using System.Windows;
using SK_ABO.Views;
using SKABO.BLL.IServices.IUser;
using SKABO.ResourcesManager;
using SKABO.ActionEngine;

namespace SK_ABO.Pages.Device
{
    public class BJParameterViewModel:Screen
    {
        [StyletIoC.Inject]
        private IWindowManager windowManager;
        [StyletIoC.Inject]
        private IBJService bjService;
        [StyletIoC.Inject]
        private IUserService userService;
        public int BJIndex { get; set; }
        public Type T { get; set; }
        private DataGrid dg;
        private BindableCollection<VBJ> BJObjList;
        public VBJ SelectedObj { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            BJObjList = new BindableCollection<VBJ>();
            dg =this.View.GetControl<DataGrid>("ContentGrid");

            switch (BJIndex)
            {
                case 1:
                    {
                        T = typeof(T_BJ_Tip);
                        break;
                    }
                case 2:
                    {
                        T = typeof(T_BJ_SampleRack);
                        break;
                    }
                case 3:
                    {
                        T = typeof(T_BJ_AgentiaWarehouse);
                        break;
                    }
                case 4:
                    {
                        T = typeof(T_BJ_DeepPlate);
                        break;
                    }
                case 5:
                    {
                        T = typeof(T_BJ_Camera);
                        break;
                    }
                case 6:
                    {
                        T = typeof(T_BJ_Unload);
                        break;
                    }
                case 7:
                    {
                        T = typeof(T_BJ_GelSeat);
                        break;
                    }
                case 8:
                    {
                        T = typeof(T_BJ_Centrifuge);
                        break;
                    }
                case 9:
                    {
                        T = typeof(T_BJ_Piercer);
                        break;
                    }
                case 10:
                    {
                        T = typeof(T_BJ_GelWarehouse);
                        break;
                    }
                case 11:
                    {
                        T = typeof(T_BJ_Scaner);
                        break;
                    }
                case 12:
                    {
                        T = typeof(T_BJ_WastedSeat);
                        break;
                    }
            }
            if (T != null)
            {
                BJObjList.AddRange(bjService.QueryBJ(T.Name));
                /*
                if (Constants.BJDict.ContainsKey(T.Name))
                {
                    BJObjList.AddRange(Constants.BJDict[T.Name]);
                }
                else
                {
                    BJObjList.AddRange(bjService.QueryBJ(T.Name));
                }*/
            }
            InitGrid();
            }
        private void InitGrid()
        {
            if (dg == null) return;
            dg.AutoGenerateColumns = false;
            dg.CanUserAddRows = false;
            dg.Columns.Clear();
            var ps=T.GetProperties();
            var attrList = new List<GridColumnAttribute>();
            foreach(var p in ps)
            {
                var attribute = p.GetCustomAttributes(typeof(GridColumnAttribute), false).FirstOrDefault() as GridColumnAttribute;
                if (attribute == null)
                    continue;
                attribute.BindingPath = p.Name;
                attrList.Add(attribute);
            }
            var attrs=attrList.OrderBy(c => c.OrderIndex);
            attrList = null;
            bool canEdit = Constants.Login.CheckRight(RightEnum.DeviceParameterModify);
            foreach (var attr in attrs)
            {
                DataGridTextColumn col = new DataGridTextColumn();
                col.Header = attr.Header;
                if (attr.Width.HasValue)
                    col.Width = new DataGridLength(attr.Width.Value);
                Binding bindingValue = new Binding(attr.BindingPath) { Mode = BindingMode.TwoWay};
                bindingValue.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                if (!String.IsNullOrEmpty(attr.StringFormat))
                {
                    bindingValue.StringFormat = attr.StringFormat;
                }
                col.Binding= bindingValue;
                col.IsReadOnly = !canEdit;

                dg.Columns.Add(col);
            }
            Binding SelectedBindingValue = new Binding("SelectedObj") { Mode = BindingMode.TwoWay };
            SelectedBindingValue.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            dg.SetBinding(DataGrid.SelectedItemProperty, SelectedBindingValue);
            dg.ItemsSource = BJObjList;

            if (!canEdit)
            {
                dg.ContextMenu.Visibility = Visibility.Hidden;
            }
        }
        /// <summary>
        /// 新增
        /// </summary>
        public void Additem()
        {
            BJObjList.Add((VBJ)Activator.CreateInstance(T));
        }
        /// <summary>
        /// 删除
        /// </summary>
        public void Delitem()
        {
            if (SelectedObj == null) return;
            var result = windowManager.ShowMessageBox(String.Format("确定删除【{0}】吗?", SelectedObj.Name), "系统提示", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                if (!userService.ValidateConfigPWD(Constants.ConfigPwd))
                {
                    var v = IoC.Get<ValidatePwdViewModel>();
                    var res = windowManager.ShowDialog(v);
                    if (!res.HasValue || !res.Value)
                    {
                        return;
                    }
                }
                if (SelectedObj.ID == 0)
                {
                    BJObjList.Remove(SelectedObj);
                }
                else
                {
                    if (bjService.DeleteBJ(SelectedObj))
                    {
                        var id = SelectedObj.ID;
                        BJObjList.Remove(SelectedObj);
                        if (Constants.BJDict.ContainsKey(T.Name))
                        {
                            Constants.BJDict[T.Name].Remove(Constants.BJDict[T.Name].Where(c => c.ID == id).First());
                        }
                    }
                    else
                    {
                        MessageWin MW = new MessageWin("删除失败！");
                        this.View.ShowHint(MW);
                    }
                }
            }
        }
        public bool SaveParameter(out String Msg)
        {
            lock (ResManager.ui_lockObj)
            {
                Msg = "";
                if (!DataGridUtil.GetDataGridRowsHasError(dg))
                {
                    var result = bjService.SaveBJ(BJObjList);
                    if (!result)
                    {
                        Msg = "保存失败！";
                    }
                    else
                    {
                        IList<VBJ> tem = null;
                        if (Constants.BJDict.ContainsKey(T.Name))
                        {
                            tem = Constants.BJDict[T.Name];
                            Constants.BJDict.Remove(T.Name);
                        }
                        Constants.BJDict.Add(T.Name, bjService.QueryBJ(T.Name));
                        var rlist = Constants.BJDict[T.Name];
                        if (tem != null && rlist.Count == tem.Count)
                        {
                            for (int i = 0; i < rlist.Count; i++)
                            {
                                rlist[i].ChangedValueMap = tem[i].ChangedValueMap;
                                rlist[i].CopyValues(tem[i].Values);
                            }
                        }
                        Engine.getInstance().InitRes();
                    }
                    return result;
                }
                else
                {
                    Msg = "请检查数据";
                    return false;
                }
            }

        }
    }
}
