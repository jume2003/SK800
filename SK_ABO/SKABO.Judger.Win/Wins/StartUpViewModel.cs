using SKABO.BLL.IServices.IJudger;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SKABO.Judger.Win.Wins
{
    public class StartUpViewModel:Screen
    {
        public StartUpViewModel() : base()
        {
            
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            (this.View as Window).DataContext = this.View;
        }
    }
}
