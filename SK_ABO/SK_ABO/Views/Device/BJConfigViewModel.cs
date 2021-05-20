using SKABO.Common;
using SKABO.Common.Models.BJ;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using System.Windows.Controls;
using System.Windows.Data;
using SKABO.ResourcesManager;
using SKABO.ActionEngine;

namespace SK_ABO.Views.Device
{
    public class BJConfigViewModel:Screen
    {
        public bool CanSave { get; set; }
        public String Key { get; set; }
        /// <summary>
        /// DataGrid的数据源
        /// </summary>
        IList<DGContent> DataList { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            var dg = (this.View as BJConfigView).ContentDg;
            if (Constants.BJDict.ContainsKey(Key))
            {
                var list = Constants.BJDict[Key];
                InitGrid(dg, list);
            }
        }
        private void InitGrid(DataGrid dg,IList<VBJ> list)
        {
            if (dg == null) return;
            dg.AutoGenerateColumns = false;
            dg.CanUserAddRows = false;
            dg.CanUserSortColumns = false;
            dg.CanUserDeleteRows = false;
            dg.Columns.Clear();

            DataGridTextColumn indexCol = new DataGridTextColumn();
            indexCol.Header = "";
            indexCol.Width = 30;
            Binding bv = new Binding("Index") ;
            bv.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            indexCol.Binding = bv;
            indexCol.IsReadOnly = true;
            dg.Columns.Add(indexCol);

            DataList = new List<DGContent>();
            int index = 0;
            foreach (var bj in list)
            {
                DataGridTextColumn col = new DataGridTextColumn();
                col.Header = bj.Name;
                col.Width = 100;
                Binding bindingValue = new Binding("Y"+ index) { Mode = BindingMode.TwoWay };
                bindingValue.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                col.Binding = bindingValue;
                col.IsReadOnly = CanSave^true;
                dg.Columns.Add(col);

                if(bj is VBJ vbj)
                {
                    var p = typeof(DGContent).GetProperty("Y" + (index ));
                    if (p == null) break;
                    int row = vbj.Values.GetLength(0);
                    for(int i = 0; i < row; i++)
                    {
                        if (DataList.Count < i + 1)
                        {
                            DataList.Add(new DGContent() { Index = i + 1 });
                        }
                        var item = DataList[i];
                        string value = "";
                        if(vbj.Values[i, 0] is ResInfoData resinfo)
                        {
                            if (vbj is T_BJ_GelWarehouse)
                            {
                                if(resinfo.gel!=null)
                                    value = resinfo.gel.GelName + " " + resinfo.GetGelMask();
                                else if(resinfo.codes.Count!=0)
                                    value = resinfo.GetCodeAt(0);
                                else
                                    value = "未扫描";
                            }
                            else if (vbj is T_BJ_GelSeat)
                            {
                                var gel_mask = resinfo.GetGelMask();
                                value += gel_mask;
                                if (gel_mask == "") value += resinfo.GetCodeAt(0);
                                var exp = ExperimentLogic.getInstance().GetExpPackageByMask(gel_mask);
                                if(exp!=null)
                                {
                                    foreach (var samplecode in exp.samples_barcode)
                                        value += " " + samplecode;
                                }
                            }
                            else if (vbj is T_BJ_SampleRack)
                            {
                                value = resinfo.GetSampleBarcode();
                            }
                            else if (vbj is T_BJ_Centrifuge)
                            {
                                var gel_mask = resinfo.GetGelMask();
                                value += gel_mask;
                                if (gel_mask == "") value += resinfo.GetCodeAt(0);
                                var exp = ExperimentLogic.getInstance().GetExpPackageByMask(gel_mask);
                                if (exp != null)
                                {
                                    foreach (var samplecode in exp.samples_barcode)
                                        value += " " + samplecode;
                                }
                                value += " "+resinfo.Purpose;
                            }
                        }
                        else if(vbj.Values[i, 0]!=null)
                        {
                            value = vbj.Values[i, 0].ToString();
                        }

                        p.SetValue(item, value);
                        //items.Add(item);
                    }
                }
                index++;
            }
            dg.ItemsSource = DataList;
        }

        public void Close()
        {
            this.RequestClose();
        }
        public void Save()
        {
            if (Constants.BJDict.ContainsKey(Key))
            {
                var list = Constants.BJDict[Key];
                for (int i = 0; i < list.Count;i++) 
                {
                    var item = list[i];
                    if(item is VBJ vbj)
                    {
                        var p = typeof(DGContent).GetProperty("Y" + (i));
                        if (p == null) break;
                        var count = vbj.Values.GetLength(0);
                        for(int x = 0; x < Math.Min(count, DataList.Count); x++)
                        {
                            vbj.SetValue(x, 0, p.GetValue(DataList[x]));
                        }
                    }
                }
                VBJ.SaveConfig(Constants.BJDict[Key]);
                this.View.ShowHint(new MessageWin());
                Engine.getInstance().InitRes();
            }
        }
        public class DGContent
        {
            public int Index { get; set; }
            public Object Y0 { get; set; }
            public Object Y1 { get; set; }
            public Object Y2 { get; set; }
            public Object Y3 { get; set; }
            public Object Y4 { get; set; }
            public Object Y5 { get; set; }
            public Object Y6 { get; set; }
            public Object Y7 { get; set; }
            public Object Y8 { get; set; }
            public Object Y9 { get; set; }
            public Object Y10 { get; set; }
            public Object Y11 { get; set; }
        }
    }
}
