using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Config
{
    public class SysConfig
    {
        public SysConfig()
        { }
        public SysConfig(String SnKey, String SnValue)
        { this.SnKey = SnKey;
            this.SnValue = SnValue;
        }
        public String SnKey { get; set; }
        public String SnValue { get; set; }
    }
}
