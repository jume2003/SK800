using SKABO.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SKABO.Common.Models.GEL
{
    public class T_ResultMap : System.ICloneable
    {

        public int ID { get; set; }
        public int GelID { get; set; }
        public string ResultMap { get; set; }
        public string ResultDesc { get; set; }
        private String color;
        public string Color { get=>color; set {
                color = value;
                colorBrush = null;
            } }

        public BloodSystemEnum _BloodSystem;
        public BloodSystemEnum BloodSystem { get=>_BloodSystem; set
            {
                _BloodSystem = value;
                _BloodSystemStr = Enum.GetName(typeof(BloodSystemEnum), value);
            }
            }
        public String BloodSystemStr
        {
            get => _BloodSystemStr;
            set { _BloodSystemStr = value;
                _BloodSystem = (BloodSystemEnum) Enum.Parse(typeof(BloodSystemEnum), value);
            }
        }
        private System.Windows.Media.SolidColorBrush colorBrush;
        

        public SolidColorBrush ColorBrush { get {
                if (colorBrush == null && !String.IsNullOrEmpty(Color==null?null:Color.Trim())) {
                colorBrush = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Color.Trim()));
                        }
                return colorBrush; } set {
                colorBrush = value; } }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
        public virtual T_ResultMap clone()
        {
            return (T_ResultMap)Clone();
        }

        private String _BloodSystemStr;
    }
}
