using SKABO.ActionEngine;
using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Communication;
using SKABO.Common.Utils;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.Hardware.Scaner;
using SKABO.Ihardware.Core;
using SKABO.MAI.ErrorSystem;
using SKABO.ResourcesManager;
using System;
using System.Linq;

namespace SKABO.Hardware.RunBJ
{
    public class GelWarehouseDevice : AbstractCanDevice
    {
        private ScanDevice _scanDevice;
        private ScanDevice scanDevice
        {
            get
            {
                return IoC.Get<ScanDevice>();
            }
        }
        private MachineHandDevice _handDevice;
        private MachineHandDevice handDevice
        {
            get
            {
                return IoC.Get<MachineHandDevice>();
            }
        }
        /// <summary>
        /// 当前的Gel条码
        /// </summary>
        private String CurrentGelBarcode;
        public GelWarehouseDevice(AbstractCanComm CanComm,GelWarehouse GelWare)
        {
            this.CanComm = CanComm;
            this.GelWare = GelWare;
        }

        public GelWarehouse GelWare { get;  set; }

        public override void LoadPLCValue()
        {
            CanComm.ReadMotor(GelWare.XMotor);
        }
        public override void Update2Plc()
        {
            CanComm.SetMotor(GelWare.XMotor);
        }
        public bool InitX(bool OnlyStart = false)
        {
            return CanComm.InitMotor(GelWare.XMotor, OnlyStart);
        }
        public bool MoveX(decimal Distance, bool OnlyStart = false)
        {
            var result = false;
            result = CanComm.MoveMotor(GelWare.XMotor, Distance,OnlyStart);
            return result;
        }
        /// <summary>
        /// 测试GEL卡
        /// </summary>
        /// <returns></returns>
        public bool[] TestGelCard()
        {
            bool[] ExistGels = new bool[24];
            for (int i=0;i<5;i++)
            {
                this.CanComm.ReadRegister(this.GelWare.FirstCoil.Addr);
                bool is_timeout = false;
                var GelVal = this.CanComm.GetIntBlock(this.GelWare.FirstCoil.Addr, 2000, out is_timeout);
                bool[] ExistGelsTem = GelVal < 0 ? null : this.CanComm.IntToBools(this.GelWare.FirstCoil.SetValue, GelVal, true);
                if (ExistGelsTem != null) ExistGelsTem = ExistGelsTem.Reverse().ToArray();
                for(int j=0;j< ExistGels.Length;j++)
                {
                    ExistGels[j] = ExistGelsTem[j]||ExistGels[j];
                }
            }
            return ExistGels;
        }
        /// <summary>
        /// 测试卡仓门
        /// </summary>
        /// <returns></returns>
        public bool? TestGWDoor()
        {
            var result = this.CanComm.ReadCoil(this.GelWare.DoorCoil.Addr);
            return result;
        }

        private void GelScaner_DataReceived(AbstractScaner scaner, T_BJ_SampleRack sampleRack)
        {
            CurrentGelBarcode = scaner.Read();
        }

        public override ActionBase InitAll()
        {
            var seque = Sequence.create(InitXyz.create(this, 10000, true, false, false));
            foreach(var ware_seat in ResManager.getInstance().gelwarehouse_list)
            {
                seque.AddAction(MoveTo.create(this,5000, (int)ware_seat.DetectX));
                seque.AddAction(SkCallBackFun.create((ActionBase acttem) => {
                    var res = TestGelCard();
                    if(res!=null)
                    {
                        for (int i = 0; i < ware_seat.Count; i++)
                        {
                            ware_seat.Values[i, 0] = null;
                            if (res[i+ (int)ware_seat.DoorX])
                            {
                                var resinfo = new ResInfoData();
                                ware_seat.Values[i, 0] = resinfo;
                            }
                        }
                    }
                    return true;
                }));
            }
            seque.AddAction(ScanGelFromWare.create(handDevice, 20000, this));
            seque.AddAction(MoveTo.create(this, 5000, 0));
            seque.AddAction(InitXyz.create(this, 10000, true,false,false));
            return seque;
        }
    }
}
