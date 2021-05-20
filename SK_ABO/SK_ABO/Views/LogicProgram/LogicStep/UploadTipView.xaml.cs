using SKABO.Common.Utils;
using System.Windows;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    /// <summary>
    /// UploadTipView.xaml 的交互逻辑
    /// </summary>
    public partial class UploadTipView : Window
    {
        public UploadTipView()
        {
            InitializeComponent();
            Tool.PaintLogicE(entPanel);
        }
    }
}
