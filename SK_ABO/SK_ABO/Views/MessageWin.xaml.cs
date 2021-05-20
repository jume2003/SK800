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

namespace SK_ABO.Views
{
    /// <summary>
    /// MessageWin.xaml 的交互逻辑
    /// </summary>
    public partial class MessageWin : Window
    {
        public String Message { get; set; }
        public MessageWin():this("Successfully!")
        {
            
        }
        public MessageWin(bool IsSuccess) : this(IsSuccess ? "Successfully!" : "Failed!")
        {

        }
        public MessageWin(String Message)
        {
            this.Message = Message;
            InitializeComponent();
            this.TipCloud.Content = Message;
        }
    }
}
