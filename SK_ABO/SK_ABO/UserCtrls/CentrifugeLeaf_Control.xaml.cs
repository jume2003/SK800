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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SK_ABO.UserCtrls
{
    /// <summary>
    /// CentrifugeLeaf_Control.xaml 的交互逻辑
    /// </summary>
    public partial class CentrifugeLeaf_Control : UserControl
    {
        private Brush OriginBrush;
        public CentrifugeLeaf_Control()
        {
            InitializeComponent();
            OriginBrush = Ellipse_1.Fill;
        }
        public void UnLoadCard(params byte[] indexes)
        {
            SetEllipseColor(OriginBrush, indexes);
        }
        public void LoadCard(params byte[] indexes)
        {
            SetEllipseColor(Brushes.Blue, indexes);
        }
        private void SetEllipseColor( Brush FillBrush,params byte[] indexes)
        {
            if (indexes == null) return;
            foreach (byte index in indexes)
            {
                var obj = this.FindName("Ellipse_" + index.ToString());
                if (obj != null && obj is Ellipse)
                {
                    Ellipse e = obj as Ellipse;
                    e.Fill = FillBrush;
                }
            }
        }
    }
}
