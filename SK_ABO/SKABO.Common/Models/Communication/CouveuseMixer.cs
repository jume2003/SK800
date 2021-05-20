using SKABO.Common.Models.Communication.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Communication
{
    public class CouveuseMixer
    {
        public CouveuseMixer()
        { }
            public CouveuseMixer(bool Init)
        {
            if (!Init) return;
            Couveuses = new Couveuse[Constants.CouveuseCount];
            for(byte i=0;i< Constants.CouveuseCount; i++)
            {
                Couveuses[i] = new Couveuse();
            }
            Mixer = new Mixer();
        }
        public void checkNull()
        {
            if (Couveuses == null)
            {
                Couveuses = new Couveuse[Constants.CouveuseCount];
                for (byte i = 0; i < Constants.CouveuseCount; i++)
                {
                    Couveuses[i] = new Couveuse();
                }
            }
            if (Mixer == null)
            {
                Mixer = new Mixer();
            }
        }
        public void Start()
        {
            
        }
        public void Stop()
        {

        }
        /// <summary>
        /// 孵育器
        /// </summary>
        public Couveuse[] Couveuses { get; set; }
        public Mixer Mixer { get; set; }
    }
}
