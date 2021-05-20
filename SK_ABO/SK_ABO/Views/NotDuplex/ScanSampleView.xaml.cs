using SKABO.BLL.IServices.IGel;
using SKABO.Common;
using SKABO.Common.Models.GEL;
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

namespace SK_ABO.Views.NotDuplex
{
    /// <summary>
    /// ScanSampleView.xaml 的交互逻辑
    /// </summary>
    public partial class ScanSampleView : Window
    {
        public ScanSampleView()
        {
            InitializeComponent();
            PaintTestItem();
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
                    
                }
                return _GelList;
            }
        }
        private void PaintTestItem()
        {
            
            int i = 0;
            foreach(var gel in GelList)
            {
                i++;
                DataGridCheckBoxColumn col = new DataGridCheckBoxColumn() { Header = gel.TestName };
                col.Binding = new Binding("TestItem" + (i));
                TestDataGrid.Columns.Add(col);
            }

        }

        private void TestDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }
}
