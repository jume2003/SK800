using SKABO.Common.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.BJ
{
    /// <summary>
    /// 离心机
    /// </summary>
    public class T_BJ_Centrifuge : VBJ
    {
        public long LastUseTime { get; set; }//使用排序
        public override int ID { get; set; }
        [GridColumn("名称", 100D)]
        public override string Name { get; set; }
        [GridColumn("HandX(mm)", 100, "{0:#0.00}")]
        public decimal HandX { get; set; }
        [GridColumn("HandY(mm)", 100, "{0:#0.00}")]
        public decimal HandY { get; set; }
        [GridColumn("HandZ(mm)", 100, "{0:#0.00}")]
        public decimal HandZ { get; set; }
        [GridColumn("ZLimit(mm)", 100, "{0:#0.00}")]
        public decimal ZLimit { get; set; }
        [GridColumn("ZCatch(mm)", 100, "{0:#0.00}")]
        public decimal ZCatch { get; set; }
        [GridColumn("ZPut(mm)", 100, "{0:#0.00}")]
        public decimal ZPut { get; set; }
        [GridColumn("Gel0", 100, "{0:#0.00}")]
        public decimal Gel0 { get; set; }
        [GridColumn("Gel1", 100, "{0:#0.00}")]
        public decimal Gel1 { get; set; }
        [GridColumn("Gel2", 100, "{0:#0.00}")]
        public decimal Gel2 { get; set; }
        [GridColumn("Gel3", 100, "{0:#0.00}")]
        public decimal Gel3 { get; set; }
        [GridColumn("Gel4", 100, "{0:#0.00}")]
        public decimal Gel4 { get; set; }
        [GridColumn("Gel5", 100, "{0:#0.00}")]
        public decimal Gel5 { get; set; }
        [GridColumn("Gel6", 100, "{0:#0.00}")]
        public decimal Gel6 { get; set; }
        [GridColumn("Gel7", 100, "{0:#0.00}")]
        public decimal Gel7 { get; set; }
        [GridColumn("Gel8", 100, "{0:#0.00}")]
        public decimal Gel8 { get; set; }
        [GridColumn("Gel9", 100, "{0:#0.00}")]
        public decimal Gel9 { get; set; }
        [GridColumn("Gel10", 100, "{0:#0.00}")]
        public decimal Gel10 { get; set; }
        [GridColumn("Gel11", 100, "{0:#0.00}")]
        public decimal Gel11 { get; set; }
        [GridColumn("HandY0", 100, "{0:#0.00}")]
        public decimal HandY0 { get; set; }
        [GridColumn("HandY1", 100, "{0:#0.00}")]
        public decimal HandY1 { get; set; }
        [GridColumn("HandY2", 100, "{0:#0.00}")]
        public decimal HandY2 { get; set; }
        [GridColumn("HandY3", 100, "{0:#0.00}")]
        public decimal HandY3 { get; set; }
        [GridColumn("HandY4", 100, "{0:#0.00}")]
        public decimal HandY4 { get; set; }
        [GridColumn("HandY5", 100, "{0:#0.00}")]
        public decimal HandY5 { get; set; }
        [GridColumn("HandY6", 100, "{0:#0.00}")]
        public decimal HandY6 { get; set; }
        [GridColumn("HandY7", 100, "{0:#0.00}")]
        public decimal HandY7 { get; set; }
        [GridColumn("HandY8", 100, "{0:#0.00}")]
        public decimal HandY8 { get; set; }
        [GridColumn("HandY9", 100, "{0:#0.00}")]
        public decimal HandY9 { get; set; }
        [GridColumn("HandY10", 100, "{0:#0.00}")]
        public decimal HandY10 { get; set; }
        [GridColumn("HandY11", 100, "{0:#0.00}")]
        public decimal HandY11 { get; set; }

        [GridColumn("X1ForDoorOpen", 150D, "{0:#0.00}")]
        public decimal X1ForDoorOpen { get; set; }
        [GridColumn("Y1ForOpen", 100D, "{0:#0.00}")]
        public decimal Y1ForOpen { get; set; }
        [GridColumn("X2ForDoorOpen", 150D, "{0:#0.00}")]
        public decimal X2ForDoorOpen { get; set; }
        [GridColumn("Y2ForOpen", 100D, "{0:#0.00}")]
        public decimal Y2ForOpen { get; set; }
        [GridColumn("XForDoorOpenSpeed", 150D, "{0:#0.00}")]
        public decimal XForDoorOpenSpeed { get; set; }
        [GridColumn("YForDoorOpenSpeed", 150D, "{0:#0.00}")]
        public decimal YForDoorOpenSpeed { get; set; }

        [GridColumn("X1ForDoorClose", 150D, "{0:#0.00}")]
        public decimal X1ForDoorClose { get; set; }
        [GridColumn("Y1ForClose", 100D, "{0:#0.00}")]
        public decimal Y1ForClose { get; set; }
        [GridColumn("X2ForDoorClose", 150D, "{0:#0.00}")]
        public decimal X2ForDoorClose { get; set; }
        [GridColumn("Y2ForClose", 100D, "{0:#0.00}")]
        public decimal Y2ForClose { get; set; }
        [GridColumn("XForDoorCloseSpeed", 150D, "{0:#0.00}")]
        public decimal XForDoorCloseSpeed { get; set; }
        [GridColumn("YForDoorCloseSpeed", 150D, "{0:#0.00}")]
        public decimal YForDoorCloseSpeed { get; set; }
        [GridColumn("ZForDoor", 100D, "{0:#0.00}")]
        public decimal ZForDoor { get; set; }
       
        [GridColumn("启用?\r\n0：禁用\r\n1：启用", 100D)]
        public int Status { get; set; }
        [GridColumn("代号", 100D)]
        public String Code
        {
            get
            {
                return _Code;
            }
            set
            {
                _Code = value;
            }
        }
        /// <summary>
        /// 舱门是否打开
        /// </summary>
        public bool IsOpen { get; set; }
        private byte? _RobotNo;
        System.Threading.SpinLock robotNoLock = new System.Threading.SpinLock();
        /// <summary>
        /// 机器人编号，表示几号机器人正在使用
        /// 能成功设置的时，表示可用此离心机
        /// </summary>
        public byte? RobotNo
        {
            get { return _RobotNo; }
            set
            {
                bool lockTaken = false;
                try
                {
                    //申请获取锁
                    robotNoLock.Enter(ref lockTaken);
                    if ((!_RobotNo.HasValue) ||!value.HasValue)
                    {
                        _RobotNo = value;
                    }
                }
                finally
                {
                    if (lockTaken)
                        robotNoLock.Exit();
                }
            }
        }
        public override Object[,] Values
        {
            get
            {
                if (_Values == null)
                {
                    _Values = new object[12, 1];
                }
                return _Values;
            }
        }
        public int GetGelPoint(int index)
        {
            int[] seatsz = { (int)Gel0, (int)Gel1, (int)Gel2, (int)Gel3, (int)Gel4, (int)Gel5, (int)Gel6, (int)Gel7, (int)Gel8, (int)Gel9, (int)Gel10, (int)Gel11 };
            return seatsz[index % 12];
        }
        public int GetHandY(int index)
        {
            int[] seatsz = { (int)HandY0, (int)HandY1, (int)HandY2, (int)HandY3, (int)HandY4, (int)HandY5, (int)HandY6, (int)HandY7, (int)HandY8, (int)HandY9, (int)HandY10, (int)HandY11 };
            return seatsz[index % 12];
        }
    }
}
