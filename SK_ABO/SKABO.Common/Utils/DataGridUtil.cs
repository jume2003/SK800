using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SKABO.Common.Utils
{
    public class DataGridUtil
    {
        public static bool GetDataGridRowsHasError(DataGrid dg)
        {
            bool hasError = false;
            for (int i = 0; i < dg.Items.Count; i++)
            {
                DependencyObject o = dg.ItemContainerGenerator.ContainerFromIndex(i);
                if (o == null)
                {
                    continue;
                }
                hasError = Validation.GetHasError(o);
                if (hasError)
                {
                    break;
                }
            }
            return hasError;
        }
        /// <summary>
        /// 获取DataGrid的第一个被发现的验证错误结果。
        /// </summary>
        /// <param name="dg">被检查的DataGrid实例。</param>
        /// <returns>错误结果。</returns>
        public static ValidationError GetDataGridRowsFirstError(DataGrid dg)
        {
            ValidationError err = null;
            for (int i = 0; i < dg.Items.Count; i++)
            {
                DependencyObject o = dg.ItemContainerGenerator.ContainerFromIndex(i);
                bool hasError = Validation.GetHasError(o);
                if (hasError)
                {
                    err = Validation.GetErrors(o)[0];
                    break;
                }
            }
            return err;
        }
        /// <summary>
        /// 执行检查DataGrid，并提示第一个错误。重新定位到错误单元格。
        /// </summary>
        /// <param name="dg">被检查的DataGrid实例。</param>
        /// <returns>true 有错并定位，false 无错、返回</returns>
        public static bool ExcutedCheckedDataGridValidation(DataGrid dg)
        {
            ValidationError err = DataGridUtil.GetDataGridRowsFirstError(dg);
            if (err != null)
            {
                string errColName = "";
                if (err.BindingInError is System.Windows.Data.BindingGroup ebg)
                {
                    errColName = ((System.Windows.Data.BindingExpression)(ebg.BindingExpressions[0])).ParentBinding.Path.Path;
                }
                else
                {
                    errColName = ((System.Windows.Data.BindingExpression)err.BindingInError).ParentBinding.Path.Path;
                }
                DataGridColumn errCol = dg.Columns.Single(p =>
                {
                    if (((Binding)((DataGridTextColumn)p).Binding).Path.Path == errColName)
                        return true;
                    else return false;
                });
                //string errRow = ((DataRowView)((System.Windows.Data.BindingExpression)err.BindingInError).DataItem)["SWH"].ToString();
                //dg.Items.IndexOf(((System.Windows.Data.BindingExpression)err.BindingInError).DataItem);
                dg.SelectedItem = ((System.Windows.Data.BindingExpression)err.BindingInError).DataItem;
                int errRowIndex = dg.SelectedIndex;
                MessageBox.Show(string.Format("第\"{0}\"行 的\"{1}\"列的单元格数据不合法(以红色标出)，请填写正确后再执行其他操作。", errRowIndex + 1, errCol.Header), "系统消息", MessageBoxButton.OK, MessageBoxImage.Warning);

                dg.CurrentCell = new DataGridCellInfo(dg.SelectedItem, errCol);
                if (!((DataRowView)dg.CurrentItem).IsEdit)
                {
                    ((DataRowView)dg.CurrentItem).BeginEdit();

                }
                if (dg.CurrentColumn.GetCellContent(dg.CurrentItem) is TextBox txt)
                    txt.Focus();
                return true;
            }
            else
            {
                return false;
            }
        }
        public static DataGridRow GetRowMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid datagrid = sender as DataGrid;
            Point aP = e.GetPosition(datagrid);
            IInputElement obj = datagrid.InputHitTest(aP);
            DependencyObject target = obj as DependencyObject;


            while (target != null)
            {
                if (target is DataGridRow)
                {
                    break;
                }
                target = VisualTreeHelper.GetParent(target);
            }
            return (DataGridRow)target;
        }
    }
}
