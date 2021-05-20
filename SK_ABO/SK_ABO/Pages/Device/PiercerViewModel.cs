using SKABO.Hardware.RunBJ;
using Stylet;
using SKABO.Common.Utils;
using SK_ABO.Views;
using System;
using SKABO.Common.Models.BJ;
using SKABO.Common;
using System.Linq;
using SKABO.ActionEngine;
using SKABO.ResourcesManager;

namespace SK_ABO.Pages.Device
{
    public class PiercerViewModel:Screen
    {
        [StyletIoC.Inject]
        private PiercerDevice piercerDevice;
        [StyletIoC.Inject]
        private GelWarehouseDevice gelwareDevice;
        bool loaded = false;
        /// <summary>
        /// 卡仓信息
        /// </summary>
        public String GWInfo { get; set; }
        protected override void OnViewLoaded()
        {
            if (loaded)
            {
                return;
            }
            base.OnViewLoaded();
            StepLen = 10;
            DeepLen = 8;
            TestCount = 10;

            this.StepXValue =this.StepYValue=this.StepZValue= 0.1m;


        }
        public decimal StepLen { get; set; }
        public decimal DeepLen { get; set; }
        public double TestCount { get; set; }
        /// <summary>
        /// Z点动步长
        /// </summary>
        public decimal StepZValue { get; set; }
        /// <summary>
        /// Y点动步长
        /// </summary>
        public decimal StepYValue { get; set; }
        public decimal PiercerDistanceZ { get; set; }
        public decimal PiercerDistanceY { get; set; }
        public void DoTest()
        {

            ResManager resmanager = ResManager.getInstance();
            var piercer = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "4");
            if(piercer != null)
            {
                int init_speed = (int)piercerDevice.Pie.ZMotor.InitSpeed.SetValue;
                for (int i = 0; i < TestCount; i++)
                {
                    int index = i % 12;
                    var act = Sequence.create(MoveTo.create(30000, -1, index * (int)piercer.PiercerGap + (int)piercer.PiercerY, -1), MoveTo.create(30000, -1, -1, (int)piercer.PiercerZ),
                        SkCallBackFun.create((ActionBase actem) => { piercerDevice.CanComm.SetRegister(piercerDevice.Pie.ZMotor.InitSpeed.Addr, 200); return true; }),
                        InitXyz.create(3000, false, false, true),
                        SkCallBackFun.create((ActionBase actem) => { piercerDevice.CanComm.SetRegister(piercerDevice.Pie.ZMotor.InitSpeed.Addr, init_speed); return true; }));
                    act.runAction(piercerDevice);
                }
            }
        }
        /// <summary>
        /// 初始化破孔器Z
        /// </summary>
        public void InitPiercerZ()
        {
            //var result=piercerDevice.InitZ();
            //ShowResult(result);
            var act = InitXyz.create(7000, false, false, true);
            act.runAction(piercerDevice);
        }
        /// <summary>
        /// 初始化破孔器Y
        /// </summary>
        public void InitPiercerY()
        {
            var act = InitXyz.create(20000, false, true, false);
            act.runAction(piercerDevice);
        }
        public void MovePiercerZ()
        {
            //var result=this.piercerDevice.MoveZTo(PiercerDistanceZ);
            //ShowResult(result);
            var act = MoveTo.create(3000, -1, -1, (int)PiercerDistanceZ);
            act.runAction(piercerDevice);
        }
        public void MovePiercerY()
        {
            var act = MoveTo.create(3000, -1, (int)PiercerDistanceY, -1);
            act.runAction(piercerDevice);
            //var result = this.piercerDevice.MoveYTo(PiercerDistanceY);
            //ShowResult(result);
        }
        public void ExecutePiercerMove(String flag)
        {
            bool result = false;
            switch (flag)
            {
                case "Z+":
                    {
                        PiercerDistanceZ += StepZValue;
                        PiercerDistanceZ = Math.Min(PiercerDistanceZ, Convert.ToDecimal( this.piercerDevice.Pie.ZMotor.Maximum.SetValue));
                        result = this.piercerDevice.MoveZTo(PiercerDistanceZ);
                        break;
                    }
                case "Z-":
                    {
                        PiercerDistanceZ -= StepZValue;
                        PiercerDistanceZ = Math.Max(PiercerDistanceZ, 0);
                        result = this.piercerDevice.MoveZTo(PiercerDistanceZ);
                        break;
                    }
                case "Y+":
                    {
                        PiercerDistanceY += StepYValue;
                        PiercerDistanceY = Math.Min(PiercerDistanceY, Convert.ToDecimal(this.piercerDevice.Pie.YMotor.Maximum.SetValue));
                        result = this.piercerDevice.MoveYTo(PiercerDistanceY);
                        break;
                    }
                case "Y-":
                    {
                        PiercerDistanceY -= StepYValue;
                        PiercerDistanceY = Math.Max(PiercerDistanceY, 0);
                        result = this.piercerDevice.MoveYTo(PiercerDistanceY);
                        break;
                    }
            }
        }
        #region 卡仓
        public decimal GelWareDistanceX { get; set; }
        public decimal StepXValue { get; set; }
        public System.Collections.Generic.IList<VBJ> GelWareList
        {
            get
            {
                var key = typeof(T_BJ_GelWarehouse).Name;
                if (Constants.BJDict.ContainsKey(key))
                {
                    return Constants.BJDict[key];
                }
                else
                {
                    return null;
                }
            }
        }
        /// <summary>
        /// 移动卡仓到指定的卡位
        /// </summary>
        /// <param name="index"></param>
        public void MoveGelWareToIndex(double index)
        {
            if(GelWareList==null || GelWareList.Count == 0 ||index>= GelWareList.Count)
            {
                return;
            }
            var t_gelWare = GelWareList[Convert.ToByte( index)] as T_BJ_GelWarehouse;
            var act = MoveTo.create(5000, (int)t_gelWare.StoreX);
            act.runAction(gelwareDevice);
        }
        /// <summary>
        /// 探测卡
        /// </summary>
        public void TestGelCard(double index)
        {
            if (GelWareList == null || GelWareList.Count == 0 || index >= GelWareList.Count)
            {
                return;
            }
            GWInfo = "";
            var t_gelWare = GelWareList[Convert.ToByte(index)] as T_BJ_GelWarehouse;
            var seq = Sequence.create(
                MoveTo.create(5000, (int)t_gelWare.DetectX),
                SkCallBackFun.create((ActionBase acttem) => {
                    var res = this.gelwareDevice.TestGelCard();
                    if (res != null)
                    {
                        GWInfo = String.Join(" ", res.Select((item, indexx) => { return item ? (indexx + 1) + "" : ""; }).ToArray());
                    }
                    return true;
                }));
            seq.runAction(gelwareDevice);
        }
        /// <summary>
        /// 探测仓门
        /// </summary>
        public void TestGWDoor()
        {
            GWInfo = "";
            var res = this.gelwareDevice.TestGWDoor();
            if(res.HasValue)
                GWInfo = res.Value ? "仓门开" : "仓门关";
            ShowResult(res.HasValue);
        }
        /// <summary>
        /// 初始化卡仓X轴
        /// </summary>
        public void InitGelWareX()
        {
            var act = InitXyz.create(10000,true,false,false);
            act.runAction(gelwareDevice);
        }
        /// <summary>
        /// 移动指定长度位置
        /// </summary>
        public void MoveGelWareX()
        {
            var act = MoveTo.create(3000,(int)GelWareDistanceX);
            act.runAction(gelwareDevice);
        }
        public void ExecuteGWMove(String flag)
        {
            bool result = false;
            switch (flag) {
                case "X+":
                    {
                        GelWareDistanceX += StepXValue;
                        GelWareDistanceX = Math.Min(GelWareDistanceX, Convert.ToDecimal(this.gelwareDevice.GelWare.XMotor.Maximum.SetValue));
                        var act = MoveTo.create(3000, (int)GelWareDistanceX);
                        act.runAction(gelwareDevice);
                        break;
                    }
                case "X-":
                    {
                        GelWareDistanceX -= StepXValue;
                        GelWareDistanceX = Math.Max(GelWareDistanceX, 0);
                        var act = MoveTo.create(3000, (int)GelWareDistanceX);
                        act.runAction(gelwareDevice);
                        break;
                    }
            }
            ShowResult(result);
        }
        #endregion
        private void ShowResult(bool IsSuccess)
        {
            this.View.ShowHint(new MessageWin(IsSuccess ? "Successfully!" : "Failed!"));
        }
    }
}
