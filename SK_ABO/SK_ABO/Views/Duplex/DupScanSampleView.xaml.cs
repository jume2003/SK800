using SKABO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SK_ABO.Views.Duplex
{
    /// <summary>
    /// DupScanSampleView.xaml 的交互逻辑
    /// </summary>
    public partial class DupScanSampleView : Window
    {
        public DupScanSampleView()
        {
            InitializeComponent();
            PaintTestItem();

        }
        private void PaintTestItem()
        {
            for (var i=0;i<  Constants.SampleRackCount;i++)
            {
                DataGridTextColumn col = new DataGridTextColumn() { Header = $"载架-{i+1}#" };
                col.Binding = new Binding("Barcode" + (i+1));
                TestDataGrid.Columns.Add(col);

            }

        }
        private void TestDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}
