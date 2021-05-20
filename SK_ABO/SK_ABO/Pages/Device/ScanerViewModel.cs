using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Hardware.Core;
using SKABO.Hardware.RunBJ;
using SKABO.Ihardware.Core;
using Stylet;
using System;
using System.Collections.Generic;
using SKABO.Common.Utils;
using SK_ABO.Views;
using SKABO.ActionEngine;
using SKABO.ResourcesManager;
using SKABO.Hardware.Enums;

namespace SK_ABO.Pages.Device
{
    public class ScanerViewModel:Screen
    {
        [StyletIoC.Inject]
        private OtherPartDevice op;
        private T_BJ_Scaner _SelectedItem;
        public T_BJ_Scaner SelectedItem { get
            {
                return _SelectedItem;
            }
            set
            {
                this.CanCloseScaner = this.CanOpenScaner = value != null;
                _SelectedItem = value;
            }
        }
        public IList<VBJ> ScanerList
        {
            get
            {
                if (Constants.BJDict.ContainsKey("T_BJ_Scaner"))
                {
                    return Constants.BJDict["T_BJ_Scaner"];
                }
                else
                {
                    return null;
                }
            }
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            this.CanOpenReaderRack = true;
            this.CanCloseReaderRack = false;
            op.CanComm.SetListenFun(op.OP.SampleRackCoils[0].Addr, CanFunCodeEnum.UPLOAD_REGISTER, RackChange);
        }
        public String ScanResult { get; set; } = "";
        public bool CanOpenScaner { get; set; }
        public bool CanCloseScaner { get; set; }
        private AbstractScaner Scaner;
        public void OpenScaner()
        {
            if (SelectedItem == null) return;
            Scaner = IoC.Get<AbstractScaner>(SelectedItem.ScanerType);
            if (Scaner == null)
            {
                ScanResult += "不支持的型号：" + SelectedItem.ScanerType + Environment.NewLine;
                ScanResult += "打开阅读器失败!" + Environment.NewLine;
            }
            else
            {
                if (Scaner.IsOpen) return;
                ScanResult += "正在打开阅读器......" + Environment.NewLine;
                try
                {
                    var res = true;
                    if (!Scaner.IsOpen)
                    {
                        Scaner.CancelAllEvent();
                        Scaner.Open(SelectedItem.Port);//115200
                        res = Scaner.Start(false);
                        if (res)
                        {
                            Scaner.DataReceived += Scaner_DataReceived;
                        }
                        else
                        {
                            Scaner.Close();
                        }
                    }
                    ScanResult += "打开阅读器" + (res ? "成功!" : "失败!") + Environment.NewLine;
                }catch(Exception ex)
                {
                    ScanResult+=ex.Message+ Environment.NewLine;
                    Scaner = null;
                }
            }
        }

        private void Scaner_DataReceived(AbstractScaner scaner)
        {
            ScanResult += scaner.Read()+ Environment.NewLine;
        }

        public void CloseScaner()
        {
            ScanResult += "正在关闭阅读器......" + Environment.NewLine;
            var res = false;
            if (Scaner != null)
            {
                Scaner.DataReceived -= Scaner_DataReceived;
                res = Scaner.Stop();
            }
            ScanResult += "关闭阅读器"+(res? "成功!":"失败!") + Environment.NewLine;
        }
        public void ClsLog()
        {
            this.ScanResult = "";
        }
        public bool CanOpenReaderRack { get; set; }
        public bool CanCloseReaderRack { get; set; }
        private bool OpenedRack = false;
        public int rack_index = 0;
        public int sample_index = 0;
        private AbstractScaner scaner { get; set; } = null;
        private string scaner_port { get; set; } = "";
        public void OpenReaderRack()
        {
            if (OpenedRack) return;
            OpenedRack = true;
            CanOpenReaderRack = false;
            CanCloseReaderRack = true;
            op.OnChangeSampleRackStatus += Pcb_OnChangeSampleRackStatus;
            this.View.ShowHint(new MessageWin(true));
        }
        
        private void Pcb_OnChangeSampleRackStatus(byte[] indexs, byte eventType)
        {
            Byte index = indexs[0];
            if (index > 7)//6路感应
            {
                if (eventType == 1)
                {
                    OpenScaner();
                    op.MoveScaner((byte)(index + 1-8));
                }
                
            }
            else
            {
                if(eventType==1)
                {
                    CloseScaner();
                    op.MoveScaner(0m);
                }
            }
        }

        public void CloseReaderRack()
        {
            if (!OpenedRack) return;
            OpenedRack = false;
            CanOpenReaderRack = true;
            CanCloseReaderRack = false;
            var res=op.MoveScaner(0m);
            CloseScaner();
            op.OnChangeSampleRackStatus -= Pcb_OnChangeSampleRackStatus;
            this.View.ShowHint(new MessageWin(res));
        }
        public void InitScanMotor()
        {
            var act = InitXyz.create(10000,true,false,false);
            act.runAction(op);
        }
        public decimal Distance { get; set; }
        public void MoveScanMotor()
        {
            var act = MoveTo.create(10000, (int)Distance);
            act.runAction(op);
        }
        public int GetRackIndex(byte data)
        {
            for (int i = 0; i < 6; i++)
            {
                if ((data & (0x01 << i)) != 0x00)
                {
                    return 5 - i;
                }
            }
            return -1;
        }
        public void RackChange(int tagerid, byte[] data)
        {
            if (!OpenedRack) return;
            rack_index = GetRackIndex(data[5]);
            var seq_move = Sequence.create();
            if (rack_index >= 0)
            {
                var rack_info = ResManager.getInstance().GetSampleRack(rack_index + 1);
                seq_move.AddAction(MoveTo.create(op, 3000, (int)rack_info.ReaderX));
            }
            else
            {
                seq_move.AddAction(MoveTo.create(op, 3000, 0));
            }
            seq_move.runAction(op);
        }
    }
}
