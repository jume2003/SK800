using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using SKABO.Common.Models.Judger;
using Stylet;

namespace SK_ABO.Views.QcPic
{
    class QcPicViewModel
    {
        public QcPicViewModel()
        {
            this.Title = "曲线图";
        }
        public string Title { get; private set; }
        public List<DataPoint> TAPoints { get; private set; }
        public List<DataPoint> TBPoints { get; private set; }
        public List<DataPoint> TCPoints { get; private set; }
        public List<DataPoint> TDPoints { get; private set; }

        public void UpData(Stylet.BindableCollection<T_Result> SelectResultListTem)
        {
            if (TAPoints == null) TAPoints = new List<DataPoint>();
            if (TBPoints == null) TBPoints = new List<DataPoint>();
            if (TCPoints == null) TCPoints = new List<DataPoint>();
            if (TDPoints == null) TDPoints = new List<DataPoint>();
            TAPoints.Clear();
            TBPoints.Clear();
            TCPoints.Clear();
            TDPoints.Clear();
            float T1value = 0;
            float T2value = 0;
            float T3value = 0;
            float T4value = 0;
            float T5value = 0;
            float T6value = 0;
            float T7value = 0;
            float T8value = 0;
            int count = 0;
            foreach (var item in SelectResultListTem)
            {
                T1value = item.Picture.T1;
                T2value = item.Picture.T2;
                T3value = item.Picture.T3;
                T4value = item.Picture.T4;
                T5value = item.Picture.T5;
                T6value = item.Picture.T6;
                T7value = item.Picture.T7;
                T8value = item.Picture.T8;

                TAPoints.Add(new DataPoint(count, T1value));
                TBPoints.Add(new DataPoint(count, T2value));
                TCPoints.Add(new DataPoint(count, T4value));
                TDPoints.Add(new DataPoint(count, T3value));

                TAPoints.Add(new DataPoint(count, T1value));
                TBPoints.Add(new DataPoint(count, T2value));
                TCPoints.Add(new DataPoint(count, T4value));
                TDPoints.Add(new DataPoint(count, T3value));
                count++;
            }
            //T1value = T1value/ SelectResultListTem.Count;
            //T2value = T2value / SelectResultListTem.Count;
            //T3value = T3value / SelectResultListTem.Count;
            //T4value = T4value / SelectResultListTem.Count;
            //T5value = T5value / SelectResultListTem.Count;
            //T6value = T6value / SelectResultListTem.Count;
            //T7value = T7value / SelectResultListTem.Count;
            //T8value = T8value / SelectResultListTem.Count;


            //TAPoints.Add(new DataPoint(4, T5value));
            //TBPoints.Add(new DataPoint(5, T6value));
            //TCPoints.Add(new DataPoint(7, T8value));
            //TDPoints.Add(new DataPoint(6, T7value));
        }

    }
}
