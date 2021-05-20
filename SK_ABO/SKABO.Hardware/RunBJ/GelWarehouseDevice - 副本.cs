using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Communication;
using SKABO.Common.Utils;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.Hardware.Scaner;
using SKABO.Ihardware.Core;
using System;
using System.Linq;
using System.Threading;

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
        public AbstractComm PcbComm;
        public GelWarehouseDevice(AbstractCanComm CanComm, AbstractComm PcbComm, GelWarehouse GelWare)
        {
            this.CanComm = CanComm;
            this.GelWare = GelWare;
            this.PcbComm = PcbComm;
        }

        public GelWarehouse GelWare { get;  set; }

        public override void LoadPLCValue()
        {
            /*LoadPLC<bool>(this.GelWare.DoorCoil, this.Comm);
            LoadMotor(this.GelWare.XMotor, this.Comm);
            this.GelWare.TestResults = TestGelCard();*/
        }
        public override void Update2Plc()
        {
            /*SetMotor(this.GelWare.XMotor,this.Comm);*/
        }
        public override void Update2Pcb()
        {
            CanComm.SetMotor(this.GelWare.XMotor);
        }
        public bool InitX()
        {
            return CanComm.InitMotor(this.GelWare.XMotor);
        }
        public bool MoveTo(decimal? distance)
        {
            return CanComm.MoveMotor(this.GelWare.XMotor,distance,null);
        }
        public bool MoveTo(T_BJ_GelWarehouse t_BJ_GelWarehouse)
        {
            return MoveTo(t_BJ_GelWarehouse.StoreX);
        }
        /// <summary>
        /// 测试GEL卡
        /// </summary>
        /// <returns></returns>
        public bool[] TestGelCard()
        {
<<<<<<< .mine
            this.CanComm.ReadRegister(this.GelWare.FirstCoil.Addr,100);  
||||||| .r52
            var canComm = IoC.Get<AbstractCanComm>();
            canComm.SetRegister("03-0002", 90);

            //this.CanComm.ReadRegister(this.GelWare.FirstCoil.Addr);
=======
            //var canComm = IoC.Get<AbstractCanComm>();
            //canComm.SetRegister("03-0002", 90);

            //this.CanComm.ReadRegister(this.GelWare.FirstCoil.Addr);
>>>>>>> .r60
            var GelVal= this.CanComm.GetInt(this.GelWare.FirstCoil.Addr,-1,true);
            
                //(this.PcbComm as PcbComm).TestGelCard(this.GelWare.FirstCoil.SetValue, 2, this.GelWare.FirstCoil.Addr);
            bool[] ExistGels = GelVal<0?null: this.CanComm.IntToBools(this.GelWare.FirstCoil.SetValue, GelVal,true);
            if (ExistGels != null) ExistGels = ExistGels.Reverse().ToArray();
            return ExistGels;
        }
        /// <summary>
        /// 测试卡仓门
        /// </summary>
        /// <returns></returns>
        public bool? TestGWDoor()
        {
            var result = this.CanComm.ReadCoil(this.GelWare.DoorCoil.Addr);
            if (result == null)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// 扫描整个卡仓内卡信息
        /// </summary>
        public bool ScanGelCards(out String msg)
        {
            var result = true;
            msg = null;
            result= result &&this.handDevice.InitGelScaner(out msg);
            try
            {
                var key = typeof(T_BJ_GelWarehouse).Name;
                if (Constants.BJDict.ContainsKey(key))
                {
                    var gelseats = Constants.BJDict[key];
                    result = result && MoveTo(0m);
                    var ges = TestGelCard();
                    if (ges == null)
                    {
                        msg = "检测卡失败";
                        return false;
                    }
                    for (var index = gelseats.Count - 1; index >= 0; index--)
                    {
                        if(ges.All(item => !item) && index == 1)
                        {
                            break;
                        }
                        result = result && MoveTo(gelseats[index] as T_BJ_GelWarehouse);
                        if (result)
                        {

                            var startIndex = 0;
                            var len = (gelseats[index] as T_BJ_GelWarehouse).Count;
                            if (index % 2 == 1)
                            {
                                startIndex = 12;
                            }
                            if (gelseats[index] is VBJ vb)
                            {
                                String barCode = null;
                                for (var i = startIndex; i < startIndex + len; i++)
                                {

                                    if (barCode == null && ges[i])
                                    {
                                        result = result && this.handDevice.ScanGel((gelseats[index], (byte)(i - startIndex)), null, out barCode, out msg);
                                        //break;
                                    }
                                    if (result)
                                    {
                                        Constants.MainWindow.Dispatcher.Invoke(new Action(() => vb.SetValue(i - startIndex, 0, ges[i] ? barCode : null)));
                                    }
                                    else { break; }
                                }
                            }
                        }
                        if (index % 2 == 0 && index != 0)
                        {
                            ges = TestGelCard();
                        }
                    }
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                Tool.AppLogError(ex);
                result = false;
            }
            finally
            {
                //scanDevice.GelScaner.DataReceived -= GelScaner_DataReceived;
                result = result && handDevice.Move(0, 0, 0);
                this.handDevice.CloseGelScaner();
            }

            /*
            if (!scanDevice.OpenGelScaner())
            {
                var err = $"打开Gel卡扫码器失败，端口：{scanDevice.GelBJScaner.Port},型号：{scanDevice.GelBJScaner.ScanerType}";
                Tool.AppLogError(err);
                throw new Exception(err);
            }
            scanDevice.GelScaner.DataReceived += GelScaner_DataReceived;
            var key = typeof(T_BJ_GelWarehouse).Name;
            if (Constants.BJDict.ContainsKey(key))
            {
                var gelseats = Constants.BJDict[key];
                result = result && MoveTo(0m);
                var ges = TestGelCard();
                for (var index= gelseats.Count-1; index >= 0; index--)
                {
                    result= result && MoveTo(gelseats[index] as T_BJ_GelWarehouse);
                    if (result)
                    {
                        
                        var startIndex = 0;
                        var len = (gelseats[index] as T_BJ_GelWarehouse).Count;
                        if (index % 2 == 1)
                        {
                            startIndex = 12;
                        }
                        if(gelseats[index] is VBJ vb)
                        {
                            String barCode = null;
                            for(var i= startIndex; i < startIndex+ len; i++)
                            {
                                if (barCode==null && ges[i])
                                {
                                    CurrentGelBarcode = null;
                                    String msg = null;
                                    var res = handDevice.TakeGel(gelseats[index], (byte)(i - startIndex),  out msg);
                                    System.Threading.Thread.Sleep(1);
                                    if (CurrentGelBarcode != null)
                                    {
                                        barCode = CurrentGelBarcode;
                                       
                                        res = handDevice.PutDownGel(gelseats[index], (byte)(i - startIndex),  out msg);
                                        CurrentGelBarcode = null;
                                        if (!res)
                                        {
                                            Tool.AppLogError(msg);
                                            return false;
                                        }
                                    }
                                    else {
                                        scanDevice.GelScaner.DataReceived -= GelScaner_DataReceived;
                                        res = handDevice.PutDownGel(this.scanDevice.GelBJScaner, 0, 0, out msg);
                                        if (!res)
                                        {
                                            Tool.AppLogError(msg);
                                            return false;
                                        }
                                        barCode = scanDevice.GelScaner.Read();
                                        byte tryCount = 0;
                                        while (String.IsNullOrEmpty(barCode) && tryCount > 0)
                                        {
                                            this.scanDevice.GelScaner.Stop();
                                            this.scanDevice.OpenGelScaner();
                                            barCode = scanDevice.GelScaner.Read();

                                            tryCount--;
                                        }
                                        this.handDevice.PutDownGel(gelseats[index], (byte)(i - startIndex), 0, out msg);
                                        scanDevice.GelScaner.DataReceived += GelScaner_DataReceived;
                                    }
                                    //break;
                                }
                                Constants.MainWindow.Dispatcher.Invoke(new Action(()=> vb.SetValue(i - startIndex, 0, ges[i]?barCode:null) ));
                                
                            }
                        }
                    }
                    if (index % 2 == 0 && index!=0)
                    {
                        ges = TestGelCard();
                    }
                }
                scanDevice.GelScaner.DataReceived -= GelScaner_DataReceived;
                result = result && handDevice.Move(0, 0, 0);
                scanDevice.GelScaner.Stop();
                return result;
            }*/
            return result;//没有卡仓组件
        }

        private void GelScaner_DataReceived(AbstractScaner scaner, T_BJ_SampleRack sampleRack)
        {
            CurrentGelBarcode = scaner.Read();
        }

        public override bool InitAll()
        {
            var res = InitX();
            return res;
        }
    }
}
