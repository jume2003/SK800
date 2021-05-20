using Ihardware.Common;
using SKABO.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Hardware.Factory
{
    public class StepFactory
    {
        private static IDictionary<String, IStep> StepMap = new Dictionary<String, IStep>();
        public static IStep CreateStep(String TypeStr)
        {
            if (StepMap.ContainsKey(TypeStr))
            {
                return StepMap[TypeStr];
            }
            Type type = Type.GetType(TypeStr);
            object obj = null;
            try
            {
                obj = Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Tool.AppLogFatal("加载实验步骤失败", ex);
            }
            if (obj != null && obj is IStep)
            {
                StepMap.Add(TypeStr, (IStep)obj);
                return (IStep)obj;
            }
            return null;
        }
    }
}
