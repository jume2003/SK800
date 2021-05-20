using SKABO.Common.Enums;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.User;
using SKABO.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace SKABO.Common
{
    public class Constants
    {
        public static bool SkipOne = true;
        /// <summary>
        /// 是否被用户暂停
        /// </summary>
        public static bool IsPaused { get; set; }
        public static ManualResetEvent PauseResetEvent = new ManualResetEvent(true);
        /// <summary>
        /// 是否用户终止试验
        /// </summary>
        public static bool UserAbort { get; set; }
        /// <summary>
        /// 是否用户终止试验
        /// </summary>
        public const String UserAborMessage = "用户终止试验";
        /// <summary>
        /// 指示设备是否工作在双工模式
        /// </summary>
        public static bool IsDouble { get; set; }
        /// <summary>
        /// 扫描间隔时间
        /// </summary>
        public readonly static int Timespan = 5;
        private static String _MSN;
        public static String MSN
        {
            get
            {
                if (String.IsNullOrEmpty(_MSN))
                    _MSN = Tool.getAppSetting("MSN");
                return _MSN;
            }
        }
        private static String _OutputStartAddr;
        public static String OutputStartAddr
        {
            get
            {
                if (String.IsNullOrEmpty(_OutputStartAddr))
                    _OutputStartAddr = Tool.getAppSetting("OutputStart");
                return _OutputStartAddr;
            }
        }
        private static float? _ZeroDistance;
        public static float ZeroDistance
        {
            get
            {
                if (!_ZeroDistance.HasValue)
                {
                    string zero = Tool.getAppSetting("zero");
                    _ZeroDistance = float.Parse(zero);
                }
                return _ZeroDistance.Value;
            }
        }
        private static int? _DefaultBackSpeed;
        /// <summary>
        /// 加样通道默认回吸速度
        /// </summary>
        public static int DefaultBackSpeed
        {
            get
            {
                if (!_DefaultBackSpeed.HasValue)
                {
                    string zero = Tool.getAppSetting("DefaultBackSpeed");
                    _DefaultBackSpeed = int.Parse(zero);
                }
                return _DefaultBackSpeed.Value;
            }
        }
        private static decimal? _DefaultBackVol;
        /// <summary>
        /// 加样通道默认回吸量
        /// </summary>
        public static decimal DefaultBackVol
        {
            get
            {
                if (!_DefaultBackVol.HasValue)
                {
                    string zero = Tool.getAppSetting("DefaultBackVol");
                    _DefaultBackVol = decimal.Parse(zero);
                }
                return _DefaultBackVol.Value;
            }
        }
        private static Byte? _SampleRackCount;
        public static Byte SampleRackCount
        {
            get
            {
                if (!_SampleRackCount.HasValue)
                {
                    string str = Tool.getAppSetting("SampleRackCount");

                    if (Byte.TryParse(str, out Byte val))
                    {
                        _SampleRackCount = val;
                    }
                    else
                    {
                        _SampleRackCount = 5;
                    }
                }
                return _SampleRackCount.Value;
            }
        }
        private static Byte? _CouveuseCount;
        public static Byte CouveuseCount
        {
            get
            {
                if (!_CouveuseCount.HasValue)
                {
                    string str = Tool.getAppSetting("CouveuseCount");

                    if (Byte.TryParse(str, out Byte val))
                    {
                        _CouveuseCount = val;
                    }
                    else
                    {
                        _CouveuseCount = 2;
                    }
                }
                return _CouveuseCount.Value;
            }//
        }
        private static decimal? _VolOffset;
        /// <summary>
        /// 加样通道吸排切换时的补偿量
        /// </summary>
        public static decimal VolOffset
        {
            get
            {
                if (!_VolOffset.HasValue)
                {
                    var valStr = Tool.getAppSetting("VolOffset");
                    decimal v = 0;
                    if(decimal.TryParse(valStr,out v))
                    {
                        _VolOffset = v;
                    }
                    else
                    {
                        _VolOffset = 12;
                    }
                }
                return _VolOffset.Value;
            }
        }
        private static String _CompanyName;
        public static String CompanyName
        {
            get
            {
                if (String.IsNullOrEmpty(_CompanyName))
                    _CompanyName = Tool.getAppSetting("CompanyName");
                return _CompanyName;
            }
        }
        /// <summary>
        /// 通道数量
        /// </summary>
        public static readonly byte EntercloseCount = 4;
        /// <summary>
        /// 系统内置生科维护管理账号
        /// </summary>
        public static readonly String SKAccount = "SKADMIN";
        /// <summary>
        /// 系统内置管理员账号
        /// </summary>
        public static readonly String AdminAccount = "ADMIN";
        public static T_User Login { get; set; }
        public static String ConfigPwd { get; set; }
        /// <summary>
        /// 日志级别，1最高，5最小,程序将只记录比此值小或等的日志
        /// </summary>
        public static TraceLevelEnum TraceLevel { get; set; }
        public static IDictionary<String, IList<VBJ>> BJDict = new Dictionary<String, IList<VBJ>>();
        public static StringBuilder RunLogSB = new StringBuilder();
        /// <summary>
        /// 主窗体引用，用于在线程中更新UI
        /// </summary>
        public static Window MainWindow;
        /// <summary>
        /// 样本架脱离记录，最多记录50
        /// </summary>
        private static IList<ValueTuple<byte, DateTime>> TakeOutRackRecords = new List<ValueTuple<byte, DateTime>>(50);
        public static void AddTakeOutRackRecord(ValueTuple<byte, DateTime> record)
        {
            if (TakeOutRackRecords.Count == 50)
            {
                TakeOutRackRecords.RemoveAt(0);
            }
            TakeOutRackRecords.Add(record);
        }
        /// <summary>
        /// 查询特定的时间范围内样本载架是否脱离
        /// </summary>
        /// <param name="index"></param>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <returns></returns>
        public static bool TakedOutRack(int index,DateTime dt1,DateTime dt2)
        {
            return TakeOutRackRecords.Where(item => item.Item1 == index && item.Item2 > dt1 && item.Item2 < dt2).Count()>0;
        }
    }
}
