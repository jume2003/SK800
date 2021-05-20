using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SK_ABO.UserCtrls.Base
{
    public class BJControl: UserControl
    {
        public delegate void AddedControlsHandler(Object sender, AddedControlsEventArgs e);
        public event AddedControlsHandler AddedControls;
        public int Index { get; set; }
        public void RaiseAddedControls()
        {
            var e = new AddedControlsEventArgs();
            e.TName = this.DataContext.GetType().Name;
            e.Index = this.Index;
            AddedControls?.Invoke(this, e);
        }
    }
    public class AddedControlsEventArgs
    {
        public String TName { get; set; }
        public int Index { get; set; }
    }
}
