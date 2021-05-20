using SKABO.ActionEngine;
using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.Common.Exceptions;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Models.GEL;
using SKABO.Common.Models.Judger;
using SKABO.Common.Models.TestStep;
using SKABO.Common.Utils;
using SKABO.Common.Views;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.Ihardware.Common.IModel;
using SKABO.MAI.ErrorSystem;
using SKABO.ResourcesManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SKABO.Hardware.RunBJ
{
    public class InjectorDevice : AbstractCanDevice
    {
        public InjectorDevice(AbstractCanComm CanComm, Injector Injector)
        {
            this.Injector = Injector;
            this.CanComm = CanComm;
        }
        public Injector Injector { get; set; }
        public Enterclose[] done_t_injects { get; set; } = null;
        public Enterclose[] GetSeleteced()
        {
            return Injector.Entercloses.Where(et => et.Selected && et.Valid).ToArray();
        }
        //得到气压值
        public int GetPressure(int index)
        {
            return CanComm.GetInt(Injector.Entercloses[index].PumpMotor.Pressure.Addr, 0);
        }
        //得到最大针管用数
        public int GetMaxInjCount()
        {
            int max_count = 0;
            for (int i = 0; i < Engine.getInstance().injectorDevice.Injector.Entercloses.Count(); i++)
            {
                if (Engine.getInstance().injectorDevice.Injector.Entercloses[i].InjEnable)
                    max_count++;
            }
            return max_count;
        }
        public override void LoadPLCValue()
        {
            CanComm.ReadMotor(this.Injector.XMotor);
            CanComm.ReadMotor(this.Injector.TMotor);
            foreach (var item in this.Injector.Entercloses)
            {
                this.CanComm.ReadMotor(item.PumpMotor);
                this.CanComm.ReadMotor(item.ZMotor);
                this.CanComm.ReadMotor(item.YMotor);
            }
        }
        public override void Update2Plc()
        {
            CanComm.SetMotor(this.Injector.XMotor);
            CanComm.SetMotor(this.Injector.TMotor);
            var inject_list = Injector.Entercloses.Where(item => item.InjEnable).ToList();
            foreach (var item in inject_list)
            {
                this.CanComm.SetMotor(item.PumpMotor);
                this.CanComm.SetMotor(item.ZMotor);
                this.CanComm.SetMotor(item.YMotor);
            }
        }
        public bool MoveX(decimal x,bool OnlyStart = false)
        {
            var result = CanComm.MoveMotor(this.Injector.XMotor, x,OnlyStart);
            return result;
        }
        //T轴选位
        public byte MakeSwitchMotor(Enterclose[] injects)
        {
            int mask = 0xff;
            bool[] motors = { false, false, false, false, false, false, false, false };
            foreach (var inject in injects)
            {
                motors[3-inject.Index] = true;
            }

            for (int i = 0; i < 8; i++)
            {
                if (!motors[i])
                {
                    mask = mask & ((0x01 << i) ^ 0xff);
                }
            }
            done_t_injects = injects;
            return (byte)mask;
        }
        public bool MoveZ(decimal? z,bool OnlyStart=false , params Enterclose[] ents)
        {
            return CanComm.MoveMotor(ents[0].ZMotor, z,OnlyStart, true,0);
        }
        public bool MovePumpMotor(decimal? z,bool OnlyStart = false, params Enterclose[] ents)
        {
            return CanComm.MoveMotor(ents[0].PumpMotor, z,false, true, 0);
        }
        public bool InitX(bool OnlyStart = false)
        {
            return CanComm.InitMotor(this.Injector.XMotor, OnlyStart);
        }
        //得到恢复当前状态的动作
        public bool GetReSetAct(ref Sequence seque_act)
        {
            int x = 0;
            int []y = IMask.Gen(-1);
            int[] z = IMask.Gen(-1);
            var device = new ActionDevice(this);
            var inject_list = Injector.Entercloses.Where(item => item.InjEnable).ToList();
            bool ret = device.GetRealX(ref x);
            ret = ret && device.GetRealY(inject_list.ToArray(),ref y);
            ret = ret && device.GetRealZ(inject_list.ToArray(), ref z);
            for(int i=0;i< inject_list.Count; i++)
            {
                y[inject_list[i].Index] += (int)inject_list[i].TipDis;
            }
            seque_act.AddAction(InjectMoveTo.create(3000, inject_list.ToArray(), - 1,IMask.Gen(-1), IMask.Gen(0)));
            seque_act.AddAction(InjectMoveTo.create(3000, inject_list.ToArray(), x, y, IMask.Gen(-1)));
            seque_act.AddAction(InjectMoveTo.create(3000, inject_list.ToArray(), -1, IMask.Gen(-1), z));
            return ret;
        }
        public byte GenerateMask(params Enterclose[] ents)
        {
            var maxIndex=ents.Max(item => item.Index);
            var minIndex = ents.Min(item => item.Index);
            byte mask = 7;
            if (minIndex == 0)
            {
                switch (maxIndex)
                {
                    case 0:
                        {
                            mask = 0b111;
                            break;
                        }
                    case 1:
                        {
                            mask = 0b110;
                            break;
                        }
                    case 2:
                        {
                            mask = 0b100;
                            break;
                        }
                    case 3:
                        {
                            mask = 0b000;
                            break;
                        }
                }
                
            }else if (minIndex == 1)
            {
                switch(maxIndex){
                    case 1:
                        {
                        mask = 0b111;
                        break;
                    }
                    case 2:
                        {
                        mask = 0b101;
                        break;
                    }
                    case 3:
                        {
                        mask = 0b001;
                        break;
                    }
                }
            }
            else if (minIndex == 2)
            {
                switch (maxIndex)
                {
                    case 2:
                        {
                            mask = 0b111;
                            break;
                        }
                    case 3:
                        {
                            mask = 0b011;
                            break;
                        }
                }
            }
            return mask;
        }
        public bool InitZ(bool OnlyStart = false, params Enterclose[] ents)
        {
            bool result = true;
            foreach (var et in ents)
            {
                result= result&&CanComm.InitMotor(et.ZMotor, OnlyStart);
            }
            return result;
        }
        public bool InitY(Enterclose[] ents,bool OnlyStart = false)
        {
            bool result = true;
            foreach (var et in ents)
            {
                result = result && CanComm.InitMotor(et.YMotor, OnlyStart);
                et.SplitDistance = 0;
                et.YMotor.IsStarted = false;
                et.YMotor.CurrentDistance = 0m;
            }
            return result;
        }
        public override ActionBase InitAll()
        {
            var by = IMask.Gen(0);
            foreach (var ent in Injector.Entercloses)
            {
                by[ent.Index] = (int)ent.YZero;
            }
            var inject_list = Injector.Entercloses.Where(item => item.InjEnable).ToList();
            var act = Sequence.create(InitXyz.create(this,30000, inject_list.ToArray(), false, false, true));
            act.AddAction(InitXyz.create(this,30000, inject_list.ToArray(), true, true, false, by));
            

            var device = new ActionDevice(this);
            int x = 0;
            int[] y = new int[4];
            int []z = new int [4];
            device.GetRealX(ref x);
            device.GetRealY(inject_list.ToArray(), ref y);
            device.GetRealZ(inject_list.ToArray(), ref z);
            Injector.XMotor.CurrentDistance = x;
            for (int i=0;i< inject_list.Count();i++)
            {
                inject_list[i].YMotor.CurrentDistance = y[i] + (int)inject_list[i].TipDis;
                inject_list[i].ZMotor.CurrentDistance = z[i];
            }
            return act;
        }
        public int GetPressure(Enterclose ent)
        {
            bool is_timeout = false;
            CanComm.ReadRegister(ent.PumpMotor.Pressure.Addr);
            int pressure = CanComm.GetIntBlock(ent.PumpMotor.Pressure.Addr, 2000, out is_timeout);
            return is_timeout?-1:pressure;
        }
    }
}
