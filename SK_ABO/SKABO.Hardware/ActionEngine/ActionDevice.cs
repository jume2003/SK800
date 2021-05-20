using SKABO.ActionGeneraterEngine;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Hardware.Model;
using SKABO.Hardware.RunBJ;
using SKABO.MAI.ErrorSystem;
using SKABO.ResourcesManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SKABO.ActionEngine
{


    public class ActionDevice
    {
        public double contime = 99999;
        public AbstractCanDevice node = null;
        public static int hand_tx = 0;
        public static int inj_tx = 0;
        public static int hand_old_rx = 0;
        public static int inj_old_rx = 0;
        public static bool is_all_in_con_space = false;//是否两个都给限制了
        public static AbstractCanDevice con_space_device = null;
        public ActionDevice(AbstractCanDevice nodetem)
        {
            node = nodetem;
        }

        public static long CurrentTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long times = Convert.ToInt64(ts.TotalMilliseconds);
            return times;
        }

        public bool GetRealX(ref int x)
        {
            return GetRealX(node, ref x);
        }

        public bool GetRealY(ref int y)
        {
            return GetRealY(node, ref y);
        }

        public bool GetRealY(Enterclose[] injects, ref int[] y)
        {
            return GetRealY(node, injects, ref y);
        }

        public bool GetRealZ(ref int z)
        {
            return GetRealZ(node, ref z);
        }

        public bool GetRealZ(Enterclose[] injects, ref int[] z)
        {
            return GetRealZ(node, injects, ref z);
        }

        public bool GetRealPump(Enterclose[] injects, ref int[] pump)
        {
            return GetRealPump(node, injects, ref pump);
        }

        public bool GetRealX(AbstractCanDevice node_tem, ref int x)
        {
            bool ret = true;
            if (node_tem is MachineHandDevice)
            {
                var device = (MachineHandDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.Hand.XMotor.RealDistance.Addr);
                x = device.CanComm.GetInt(device.Hand.XMotor.RealDistance.Addr, 0);
                x = Convert.ToInt32(x / Convert.ToDecimal(device.Hand.XMotor.Factor.SetValue));
            }
            else if (node_tem is InjectorDevice)
            {
                var device = (InjectorDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.Injector.XMotor.RealDistance.Addr);
                x = device.CanComm.GetInt(device.Injector.XMotor.RealDistance.Addr, 0);
                x = Convert.ToInt32(x / Convert.ToDecimal(device.Injector.XMotor.Factor.SetValue));
            }
            else if (node_tem is GelWarehouseDevice)
            {
                var device = (GelWarehouseDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.GelWare.XMotor.RealDistance.Addr);
                x = device.CanComm.GetInt(device.GelWare.XMotor.RealDistance.Addr, 0);
                x = Convert.ToInt32(x / Convert.ToDecimal(device.GelWare.XMotor.Factor.SetValue));
            }
            else if (node_tem is OtherPartDevice)
            {
                var device = (OtherPartDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.OP.ScanMotor.RealDistance.Addr);
                x = device.CanComm.GetInt(device.OP.ScanMotor.RealDistance.Addr, 0);
                x = Convert.ToInt32(x / Convert.ToDecimal(device.OP.ScanMotor.Factor.SetValue));
            }
            return ret;
        }

        public bool GetRealY(AbstractCanDevice node_tem, ref int y)
        {
            bool ret = true;
            if (node_tem is MachineHandDevice)
            {
                var device = (MachineHandDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.Hand.YMotor.RealDistance.Addr);
                y = device.CanComm.GetInt(device.Hand.YMotor.RealDistance.Addr, 0);
                y = Convert.ToInt32(y / Convert.ToDecimal(device.Hand.YMotor.Factor.SetValue));
            }
            else if (node_tem is InjectorDevice)
            {
                var device = (InjectorDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.Injector.TMotor.RealDistance.Addr);
                y = device.CanComm.GetInt(device.Injector.TMotor.RealDistance.Addr, 0);
                y = Convert.ToInt32(y / Convert.ToDecimal(device.Injector.TMotor.Factor.SetValue));
            }
            else if (node_tem is PiercerDevice)
            {
                var device = (PiercerDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.Pie.YMotor.RealDistance.Addr);
                y = device.CanComm.GetInt(device.Pie.YMotor.RealDistance.Addr, 0);
                y = Convert.ToInt32(y / Convert.ToDecimal(device.Pie.YMotor.Factor.SetValue));
            }
            return ret;
        }

        public bool GetRealY(AbstractCanDevice node_tem, Enterclose[] injects, ref int[] y)
        {
            bool ret = true;
            System.Diagnostics.Debug.Assert(y.Count() == 4);
            if (node_tem is InjectorDevice)
            {
                var device = (InjectorDevice)node_tem;
                foreach (var inject in injects)
                {
                    ret = ret && device.CanComm.ReadRegister(inject.YMotor.RealDistance.Addr);
                    y[inject.Index] = device.CanComm.GetInt(inject.YMotor.RealDistance.Addr, 0);
                    y[inject.Index] = Convert.ToInt32(y[inject.Index] / Convert.ToDecimal(inject.YMotor.Factor.SetValue));
                }
            }
            return ret;
        }

        public bool GetRealZ(AbstractCanDevice node_tem, ref int z)
        {
            bool ret = true;
            if (node_tem is MachineHandDevice)
            {
                var device = (MachineHandDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.Hand.ZMotor.RealDistance.Addr);
                z = device.CanComm.GetInt(device.Hand.ZMotor.RealDistance.Addr, 0);
                z = Convert.ToInt32(z / Convert.ToDecimal(device.Hand.ZMotor.Factor.SetValue));
            }
            else if (node_tem is InjectorDevice)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            else if (node_tem is PiercerDevice)
            {
                var device = (PiercerDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.Pie.ZMotor.RealDistance.Addr);
                z = device.CanComm.GetInt(device.Pie.ZMotor.RealDistance.Addr, 0);
                z = Convert.ToInt32(z / Convert.ToDecimal(device.Pie.ZMotor.Factor.SetValue));
            }
            else if (node_tem is CentrifugeMDevice)
            {
                var device = (CentrifugeMDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.Centrifugem.Motor.RealDistance.Addr);
                z = device.CanComm.GetInt(device.Centrifugem.Motor.RealDistance.Addr, 0);
                z = Convert.ToInt32(z / Convert.ToDecimal(device.Centrifugem.Motor.Factor.SetValue));
            }
            else if (node_tem is CouveuseMixerDevice)
            {
                var device = (CouveuseMixerDevice)node_tem;
                ret = ret && device.CanComm.ReadRegister(device.CouMixer.Mixer.RealDistance.Addr);
                z = device.CanComm.GetInt(device.CouMixer.Mixer.RealDistance.Addr, 0);
                z = Convert.ToInt32(z / Convert.ToDecimal(device.CouMixer.Mixer.Factor.SetValue));
            }
            return ret;
        }

        public bool GetRealZ(AbstractCanDevice node_tem, Enterclose[] injects, ref int[] z)
        {
            bool ret = true;
            if (node_tem is InjectorDevice)
            {
                var device = (InjectorDevice)node_tem;
                foreach (var inject in injects)
                {
                    ret = ret && device.CanComm.ReadRegister(inject.ZMotor.RealDistance.Addr);
                    z[inject.Index] = device.CanComm.GetInt(inject.ZMotor.RealDistance.Addr, 0);
                    z[inject.Index] = Convert.ToInt32(z[inject.Index] / Convert.ToDecimal(inject.ZMotor.Factor.SetValue));
                }
            }
            return ret;
        }

        public bool GetRealPump(AbstractCanDevice node_tem, Enterclose[] injects, ref int[] pump)
        {
            bool ret = true;
            if (node_tem is InjectorDevice)
            {
                var device = (InjectorDevice)node_tem;
                foreach (var inject in injects)
                {
                    ret = ret && device.CanComm.ReadRegister(inject.PumpMotor.RealDistance.Addr);
                    if (inject.PumpMotor.Factor.SetValue <= 0)
                    {
                        ErrorSystem.WriteActError("加样通道:" + inject.Index + "校正因子不可为0", true, false, 9999);
                        return false;
                    }
                    if (ret)
                    {
                        pump[inject.Index] = (int)device.CanComm.GetInt(inject.PumpMotor.RealDistance.Addr, 0);
                        pump[inject.Index] = Convert.ToInt32(pump[inject.Index] / Convert.ToDecimal(inject.PumpMotor.Factor.SetValue));
                    }
                    else break;

                }
            }
            return ret;
        }

        public bool SetRealY(Enterclose[] injects, int[] y)
        {
            bool ret = true;
            System.Diagnostics.Debug.Assert(y.Count() == 4);
            if (node is InjectorDevice)
            {
                int index = 0;
                var device = (InjectorDevice)node;
                foreach (var inject in injects)
                {
                    index = inject.Index;
                    int distance = Convert.ToInt32(y[inject.Index] * Convert.ToDecimal(inject.YMotor.Factor.SetValue));
                    ret = ret && device.CanComm.SetRegister(inject.YMotor.RealDistance.Addr, distance);
                    Thread.Sleep(10);
                }
            }
            return ret;
        }

        public bool ReSetStartX()
        {
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                device.Hand.XMotor.IsStarted = false;
            }
            else if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                device.Injector.XMotor.IsStarted = false;
            }
            else if (node is GelWarehouseDevice)
            {
                var device = (GelWarehouseDevice)node;
                device.GelWare.XMotor.IsStarted = false;
            }
            else if (node is OtherPartDevice)
            {
                var device = (OtherPartDevice)node;
                device.OP.ScanMotor.IsStarted = false;
            }
            return true;
        }
        //是否全都限制
        public bool IsAllInConSpace()
        {
            return is_all_in_con_space;
        }
        //x轴移动约束
        public bool MoveXConstrains(int x,double dt)
        {
            if (node is MachineHandDevice || node is InjectorDevice)
            {
                contime += dt;
                if (contime > 500)
                {
                    contime = 0;
                    MachineHandDevice handDevice = Engine.getInstance().handDevice;
                    InjectorDevice injectorDevice = Engine.getInstance().injectorDevice;
                    OtherPartDevice opDevice = Engine.getInstance().opDevice;
                    int hand_rx = 0;
                    int inj_rx = 0;
                    var hand_device = new ActionDevice(handDevice);
                    var inj_device = new ActionDevice(injectorDevice);
                    hand_device.GetRealX(ref hand_rx);
                    inj_device.GetRealX(ref inj_rx);
                    if (node is MachineHandDevice) hand_tx = x;
                    if (node is InjectorDevice) inj_tx = x;
                    int hand_cx = hand_rx != hand_old_rx ? hand_tx : hand_rx;
                    int inj_cx = inj_rx !=inj_old_rx ? inj_tx : inj_rx;
                    int total_space = opDevice.OP.AvoidanceTotal.SetValue;
                    int limt_space = opDevice.OP.AvoidanceSpace.SetValue;
                    int l_space = total_space - (hand_cx + inj_cx) - limt_space;
                    int hand_lx = hand_cx + l_space;
                    int inj_lx = inj_cx + l_space;
                    hand_old_rx = hand_rx;
                    inj_old_rx = inj_rx;

                    bool is_hand_in_con_space = hand_rx >= hand_tx ? false : hand_tx >= hand_lx;
                    bool is_inj_in_con_space = inj_rx >= inj_tx ? false : inj_tx >= inj_lx;
                    //System.Diagnostics.Debug.Assert(!(is_hand_in_con_space && is_inj_in_con_space));
                    is_all_in_con_space = is_hand_in_con_space && is_inj_in_con_space;
                    if (node is MachineHandDevice)
                    {
                        return is_hand_in_con_space==false;
                    }
                    else if (node is InjectorDevice)
                    {
                        return is_inj_in_con_space==false;
                    }
                }
                return false;
            }
            return true;
        }
        //抓手打开关闭
        public bool SwitchHand(bool IsOpen)
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                ret = device.SwitchHand(IsOpen);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            return ret;
        }

        public bool DoneSwitchHand()
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                ret = device.CanComm.GetBool(device.Hand.HandDonedCoil.Addr, false);
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            return ret;
        }

        public bool MoveX(int x,int speed=0)
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                int set_speed = 0;
                if (speed == 0) set_speed = device.Hand.XMotor.Speed.SetValue;
                else set_speed = speed;
                if (device.Hand.XMotor.device_speed != set_speed)
                {
                    device.CanComm.SetRegister(device.Hand.XMotor.Speed.Addr, set_speed);
                    device.Hand.XMotor.device_speed = set_speed;
                }
                ret = device.MoveX(x, true);
            }
            else if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                int set_speed = 0;
                if (speed == 0) set_speed = device.Injector.XMotor.Speed.SetValue;
                else set_speed = speed;
                if (device.Injector.XMotor.device_speed != set_speed)
                {
                    device.CanComm.SetRegister(device.Injector.XMotor.Speed.Addr, set_speed);
                    device.Injector.XMotor.device_speed = set_speed;
                }
                ret = device.MoveX(x, true);
            }
            else if (node is GelWarehouseDevice)
            {
                var device = (GelWarehouseDevice)node;
                int set_speed = 0;
                if (speed == 0) set_speed = device.GelWare.XMotor.Speed.SetValue;
                else set_speed = speed;
                if (device.GelWare.XMotor.device_speed != set_speed)
                {
                    device.CanComm.SetRegister(device.GelWare.XMotor.Speed.Addr, set_speed);
                    device.GelWare.XMotor.device_speed = set_speed;
                }
                ret = device.MoveX(x,true);
            }
            else if (node is OtherPartDevice)
            {
                var device = (OtherPartDevice)node;
                ret = device.MoveX(x, true);
            }
            return ret;
        }

        public bool ReSetStartY()
        {
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                device.Hand.YMotor.IsStarted = false;
            }
            else if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                device.Injector.TMotor.IsStarted = false;
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                device.Pie.YMotor.IsStarted = false;
            }
            return true;
        }

        public bool ReSetStartY(Enterclose[] injects)
        {
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                foreach (var inject in injects)
                {
                    inject.YMotor.IsStarted = false;
                }
            }
            return true;
        }

        public bool MoveY(int y,int speed=0)
        {
            bool ret = true;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                int set_speed = 0;
                if (speed == 0) set_speed = device.Hand.YMotor.Speed.SetValue;
                else set_speed = speed;
                if (device.Hand.YMotor.device_speed != set_speed)
                {
                    ret = ret && device.CanComm.SetRegister(device.Hand.YMotor.Speed.Addr, set_speed);
                    if(ret)device.Hand.YMotor.device_speed = set_speed;
                }
                ret = device.MoveY(y, true);
            }
            else if (node is InjectorDevice)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.MoveYTo(y,true);
            }
            return ret;
        }

        //无速度手动限制
        public bool MoveY(Enterclose[] injects, int speed = 0, bool is_abs = false, params int[] y)
        {
            bool ret = false;
            System.Diagnostics.Debug.Assert(y.Count() == 4);
            System.Diagnostics.Debug.Assert(injects != null);
            if (injects.Length == 0) return true;
            if (node is InjectorDevice)
            {
                ret = true;
                var device = (InjectorDevice)node;
                var realy = IMask.Gen(0);
                GetRealY(injects, ref realy);
                foreach (var inject in injects)
                {
                    int set_speed = 0;
                    if (speed == 0) set_speed = inject.YMotor.Speed.SetValue;
                    else if (speed == 1) set_speed = inject.YMotor.SecondSpeed.SetValue;
                    else if (speed == 2) set_speed = inject.YMotor.DownSpeed.SetValue;
                    else set_speed = speed;
                    if (inject.YMotor.device_speed != set_speed)
                    {
                        ret = ret && device.CanComm.SetRegister(inject.YMotor.Speed.Addr, set_speed);
                        if (ret) inject.YMotor.device_speed = set_speed;
                    }
                    int pcby = 0;
                    int pcbry = realy[inject.Index];
                    int distance = Convert.ToInt32(y[inject.Index] * Convert.ToDecimal(inject.YMotor.Factor.SetValue));
                    if (is_abs)
                        pcby = distance;
                    else
                        pcby = distance - (int)inject.TipDis;
                    if(pcby<0) pcby = distance - (int)inject.InjWidth;
                    if (pcby < 0)
                    {
                        if (y[inject.Index] != 0)
                        {
                            ErrorSystem.WriteActError("加样器:" + (inject.Index + 1) + "无法到达" + y[inject.Index], true, false);
                            var act_list = ActionManager.getInstance().getAllRuningActions(node);
                            foreach (var act in act_list)
                            {
                                act.isdelete = true;
                            }
                            return false;
                        }
                        else
                        {
                            pcby = 0;
                        }
                    }
                    device.CanComm.Set2PLC(inject.YMotor.StopCoil);
                    ret = ret && device.CanComm.StopMotor(inject.YMotor);
                    ret = ret && device.CanComm.SetCoilOff(inject.YMotor.DoneCoil.Addr);
                    ret = ret && device.CanComm.SetRegister(inject.YMotor.Distance.Addr, pcby);
                }
                //Thread.Sleep(50);
                foreach (var inject in injects)
                {
                    Thread.Sleep(20);
                    ret = ret && device.CanComm.StartMotor(inject.YMotor);
                    Thread.Sleep(20);
                }
            }
            System.Diagnostics.Debug.Assert(ret);
            return ret;
        }

        public bool ReSetStartZ()
        {
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                device.Hand.ZMotor.IsStarted = false;
            }
            else if (node is InjectorDevice)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                device.Pie.ZMotor.IsStarted = false;
            }
            else if (node is CentrifugeMDevice)
            {
                var device = (CentrifugeMDevice)node;
                device.Centrifugem.Motor.IsStarted = false;
            }
            else if (node is CouveuseMixerDevice)
            {
                var device = (CouveuseMixerDevice)node;
                device.CouMixer.Mixer.IsStarted = false;
            }
            return true;
        }

        public bool ReSetStartZ(Enterclose[] injects)
        {
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                foreach (var inject in injects)
                {
                    inject.ZMotor.IsStarted = false;
                }
            }
            return true;
        }

        public bool ReSetStartPump(Enterclose[] injects)
        {
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                foreach (var inject in injects)
                {
                    inject.PumpMotor.IsStarted = false;
                }
            }
            return true;
        }

        public bool MoveZ(int z,int speed = 0)
        {
            bool ret = false;
            //System.Diagnostics.Debug.Assert(z>= 0);
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                int set_speed = 0;
                if (speed == 0) set_speed = device.Hand.ZMotor.Speed.SetValue;
                else if (speed == 1) set_speed = device.Hand.ZMotor.SecondSpeed.SetValue;
                else set_speed = speed;
                if (device.Hand.ZMotor.device_speed != set_speed)
                {
                    device.CanComm.SetRegister(device.Hand.ZMotor.Speed.Addr, set_speed);
                    device.Hand.ZMotor.device_speed = set_speed;
                }
                Engine.getInstance().opDevice.HandStop(true);
                ret = device.MoveZ(z, true);
            }
            else if (node is InjectorDevice)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.MoveZTo(z, true);
            }
            else if (node is CentrifugeMDevice)
            {
                var device = (CentrifugeMDevice)node;
                device.CanComm.Set2PLC(device.Centrifugem.Motor.StopCoil);
                int set_speed = 0;
                if (speed == 0) set_speed = device.Centrifugem.Motor.Speed.SetValue;
                else set_speed = speed;
                if (device.Centrifugem.Motor.device_speed != set_speed)
                {
                    device.CanComm.SetRegister(device.Centrifugem.Motor.Speed.Addr, set_speed);
                    device.Centrifugem.Motor.device_speed = set_speed;
                }
                ret = device.MoveZ(z, true);
            }
            else if (node is CouveuseMixerDevice)
            {
                var device = (CouveuseMixerDevice)node;
                device.CanComm.Set2PLC(device.CouMixer.Mixer.StopCoil);
                int set_speed = 0;
                if (speed == 0) set_speed = device.CouMixer.Mixer.Speed.SetValue;
                else set_speed = speed;
                if (device.CouMixer.Mixer.device_speed != set_speed)
                {
                    device.CanComm.SetRegister(device.CouMixer.Mixer.Speed.Addr, set_speed);
                    device.CouMixer.Mixer.device_speed = set_speed;
                }
                ret = device.MoveZ(z, true);
            }
            return ret;
        }

        public bool MoveZ(Enterclose[] injects, int speed, int[] rz, int[] z)
        {
            bool ret = false;
            System.Diagnostics.Debug.Assert(z.Count() == 4);
            System.Diagnostics.Debug.Assert(injects != null);
            //System.Diagnostics.Debug.Assert(z[0]>=0&& z[1] >= 0 && z[2] >= 0 && z[3] >= 0);
            if (injects.Length == 0) return true;
            if (node is InjectorDevice)
            {
                ret = true;
                var device = (InjectorDevice)node;
                foreach (var inject in injects)
                {
                    int set_speed = 0;
                    if (speed == 0) set_speed = inject.ZMotor.Speed.SetValue;
                    else if (speed == 1) set_speed = inject.ZMotor.SecondSpeed.SetValue;
                    else if (speed == 2) set_speed = inject.ZMotor.DownSpeed.SetValue;
                    else set_speed = speed;
                    if (inject.ZMotor.device_speed != set_speed)
                    {
                        ret = ret && device.CanComm.SetRegister(inject.ZMotor.Speed.Addr, set_speed);
                        if (ret) inject.ZMotor.device_speed = set_speed;
                    }
                    int pcbz = z[inject.Index];
                    ret = ret && device.CanComm.StopMotor(inject.ZMotor);
                    ret = ret && device.CanComm.SetCoilOff(inject.ZMotor.DoneCoil.Addr);
                    ret = ret && device.CanComm.SetRegister(inject.ZMotor.Distance.Addr, pcbz);
                    if (ret == false) break;
                    var forecast_move = inject.ZMotor.forecast_move;
                    if (forecast_move.ContainsKey(set_speed))
                    {
                        forecast_move[set_speed].beg_time = CurrentTimeStamp();
                        forecast_move[set_speed].distance = Math.Abs(rz[inject.Index] - z[inject.Index]);
                        forecast_move[set_speed].tager = z[inject.Index];
                    }
                    else
                    {
                        var forecastmove = new ForecastMoveData();
                        forecastmove.beg_time = CurrentTimeStamp();
                        forecastmove.distance = Math.Abs(rz[inject.Index] - z[inject.Index]);
                        forecastmove.tager = z[inject.Index];
                        forecast_move.Add(set_speed, forecastmove);
                    }
                }
                //Thread.Sleep(20);
                foreach (var inject in injects)
                {
                    ret = ret && device.CanComm.StartMotor(inject.ZMotor);
                    if (ret == false) break;
                }
            }
            System.Diagnostics.Debug.Assert(ret);
            return ret; 
        }

        public bool MovePump(Enterclose[] injects,int speed, params int[] absorbs)
        {
            bool ret = false;
            System.Diagnostics.Debug.Assert(absorbs.Count() == 4);
            if (node is InjectorDevice)
            {
                ret = true;
                var device = (InjectorDevice)node;
                foreach (var inject in injects)
                {
                    int set_speed = 0;
                    if (speed == 0) set_speed = inject.PumpMotor.Speed.SetValue;
                    else if (speed == 1) set_speed = inject.PumpMotor.SecondSpeed.SetValue;
                    else set_speed = speed;
                    if (inject.PumpMotor.device_speed != set_speed)
                    {
                        ret = ret && device.CanComm.SetRegister(inject.PumpMotor.Speed.Addr, set_speed);
                        if(ret)inject.PumpMotor.device_speed = set_speed;
                    }
                    ret = ret&&device.CanComm.MoveMotor(inject.PumpMotor, absorbs[inject.Index],true);
                    if (ret == false) break;
                }
            }
            return ret;
        }

        public bool GetPumpPressure(Enterclose[] injects, ref int[] pressures)
        {
            bool ret = false;
            if (node is InjectorDevice)
            {
                ret = true;
                var device = (InjectorDevice)node;
                foreach (var ent in injects)
                {
                    device.CanComm.ReadRegister(ent.PumpMotor.Pressure.Addr);
                    int pressure = device.CanComm.GetInt(ent.PumpMotor.Pressure.Addr, -1);
                    ret = pressure != -1;
                    if (ret == false) break;
                    pressures[ent.Index] = pressure;
                }
            }
            return ret;
        }

        public bool InitX()
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                ret = device.InitX(true);
            }
            else if (node is InjectorDevice)
            {
                ret = true;
                var device = (InjectorDevice)node;
                ret = device.InitX(true);
                return ret;
            }
            else if (node is GelWarehouseDevice)
            {
                ret = true;
                var device = (GelWarehouseDevice)node;
                ret = device.InitX(true);
                return ret;
            }
            else if (node is OtherPartDevice)
            {
                ret = true;
                var device = (OtherPartDevice)node;
                ret = device.InitX(true);
                return ret;
            }
            return ret;
        }

        public bool InitY()
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                ret = device.InitY(true);
            }
            else if (node is InjectorDevice)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.InitY(true);
            }
            return ret;
        }

        public bool InitY(Enterclose[] injects)
        {
            bool ret = false;
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                ret = device.InitY(injects, true);
            }
            return ret;
        }

        public bool InitZ()
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                Engine.getInstance().opDevice.HandStop(true);
                ret = device.InitZ(true);
            }
            else if (node is InjectorDevice)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.InitZ(true);
            }
            else if (node is CouveuseMixerDevice)
            {
                var device = (CouveuseMixerDevice)node;
                ret = device.InitMixer(true);
            }
            else if (node is CentrifugeMDevice)
            {
                ret = true;
                var device = (CentrifugeMDevice)node;
                ret = device.Init(true);
                return ret;
            }
            else if (node is CouveuseMixerDevice)
            {
                var device = (CouveuseMixerDevice)node;
                ret = device.Init(true);
            }
            return ret;
        }

        public bool InitZ(Enterclose[] injects)
        {
            bool ret = false;
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                ret = device.InitZ(true, injects);
            }
            return ret;
        }

        public bool InitPump(Enterclose[] injects)
        {
            bool ret = false;
            if (node is InjectorDevice)
            {
                ret = true;
                var device = (InjectorDevice)node;
                foreach (var inject in injects)
                {
                    ret = ret&&device.CanComm.MoveMotor(inject.PumpMotor,(int)inject.PumpMotor.Maximum.SetValue,true);
                    if (ret == false) break;
                }
            }
            return ret;
        }

        public bool DoneX()
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                ret = device.CanComm.GetBool(device.Hand.XMotor.DoneCoil.Addr, false);
            }
            else if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                ret = device.CanComm.GetBool(device.Injector.XMotor.DoneCoil.Addr, false);
            }
            else if (node is OtherPartDevice)
            {
                var device = (OtherPartDevice)node;
                ret = device.CanComm.GetBool(device.OP.ScanMotor.DoneCoil.Addr, false);
            }
            else if (node is GelWarehouseDevice)
            {
                var device = (GelWarehouseDevice)node;
                ret = device.CanComm.GetBool(device.GelWare.XMotor.DoneCoil.Addr, false);
            }
            return ret;
        }

        public bool DoneY()
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                ret = device.CanComm.GetBool(device.Hand.YMotor.DoneCoil.Addr, false);
            }
            else if (node is InjectorDevice)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.CanComm.GetBool(device.Pie.YMotor.DoneCoil.Addr, false);
            }
            return ret;
        }

        public bool DoneY(Enterclose[] injects,int done_count)
        {
            int count = 0;
            bool ret = false;
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                ret = true;
                foreach (var inject in injects)
                {
                    ret = device.CanComm.GetBool(inject.YMotor.DoneCoil.Addr, false);
                    if (ret) count++;
                    //if (ret == false) break;
                }
                return (count >= done_count);
            }
            return ret;
        }

        public bool DoneY(Enterclose[] injects)
        {
            bool ret = false;
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                ret = true;
                foreach (var inject in injects)
                {
                    ret = ret&&device.CanComm.GetBool(inject.YMotor.DoneCoil.Addr, false);
                    if (ret == false) break;
                }
            }
            return ret;
        }

        public bool DoneZ()
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                ret = device.CanComm.GetBool(device.Hand.ZMotor.DoneCoil.Addr, false);
            }
            else if (node is InjectorDevice)
            {
                System.Diagnostics.Debug.Assert(false);
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.CanComm.GetBool(device.Pie.ZMotor.DoneCoil.Addr, false);
            }
            else if (node is CentrifugeMDevice)
            {
                var device = (CentrifugeMDevice)node;
                ret = device.CanComm.GetBool(device.Centrifugem.Motor.DoneCoil.Addr, false);
            }
            else if (node is CouveuseMixerDevice)
            {
                var device = (CouveuseMixerDevice)node;
                ret = device.CanComm.GetBool(device.CouMixer.Mixer.DoneCoil.Addr, false);
            }
            return ret;
        }

        public bool DoneZ(Enterclose[] injects,int speed)
        {
            bool ret = true;
            var device = (InjectorDevice)node;
            foreach (var ent in injects)
            {
                ret = ret && device.CanComm.GetBool(ent.ZMotor.DoneCoil.Addr, false);
                if (ent.ZMotor.forecast_move.Count != 0)
                {
                    int set_speed = 0;
                    if (speed == 0) set_speed = ent.ZMotor.Speed.SetValue;
                    else if (speed == 1) set_speed = ent.ZMotor.SecondSpeed.SetValue;
                    else if (speed == 2) set_speed = ent.ZMotor.DownSpeed.SetValue;
                    else set_speed = speed;
                    if (ent.ZMotor.forecast_move.ContainsKey(set_speed))
                    {
                        var forecastmove = ent.ZMotor.forecast_move[set_speed];
                        if (ret == false && forecastmove.is_bin&& forecastmove.distance!=0 && forecastmove.tager==0)
                        {
                            double min_dis = 2000;
                            double wait_time = forecastmove.setp_time * (forecastmove.distance- min_dis);
                            if (CurrentTimeStamp() - forecastmove.beg_time >= wait_time)
                            {
                                var real_z = IMask.Gen(0);
                                Enterclose[] ents = { ent };
                                GetRealZ(ents, ref real_z);
                                ret = Math.Abs(forecastmove.tager -real_z[ent.Index])< min_dis;
                            }
                        }
                        else if(ret&& forecastmove.distance!=0&& forecastmove.is_bin==false)
                        {
                            var use_time = CurrentTimeStamp() - forecastmove.beg_time;
                            forecastmove.setp_time = use_time/forecastmove.distance;
                            forecastmove.is_bin = true;
                        }
                    }
                }
                if (ret == false) break;
            }
            return ret;
        }

        public bool DonePump(Enterclose[] injects)
        {
            bool ret = false;
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                ret = true;
                foreach (var inject in injects)
                {
                    ret = ret&&device.CanComm.GetBool(inject.PumpMotor.DoneCoil.Addr, false);
                    if (ret == false) return ret;
                }
            }
            return ret;
        }
    }
}
