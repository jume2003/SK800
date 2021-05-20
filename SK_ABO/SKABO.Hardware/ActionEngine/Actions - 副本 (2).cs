using SKABO.Hardware.Model;
using SKABO.Hardware.RunBJ;
using SKABO.MAI.ErrorSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK_ABO.ActionEngine
{
    public class ActionDevice
    {
        public AbstractCanDevice node = null;
        public ActionDevice(AbstractCanDevice nodetem)
        {
            node = nodetem;
        }
        public bool MoveX(int x)
        {
            bool ret = false;
            if(node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                ret = device.MoveX(x, true);
            }
            else if (node is InjectorDevice)
            {
                bool NeedToResetSpeed = false;
                var device = (InjectorDevice)node;
                ret = device.MoveX(x, out NeedToResetSpeed, true);
            }
            return ret;
        }
        public bool MoveY(int y)
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                ret = device.MoveY(y, true);
            }
            else if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                ret = device.MoveY(y, 0, true, device.GetSeleteced());
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.MoveYTo(y,true);
            }
            return ret;
        }
        public bool MoveZ(int z, bool is_frist = true)
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                if (is_frist) device.CanComm.SetRegister(device.Hand.ZMotor.Speed.Addr, device.Hand.ZMotor.Speed.SetValue);
                else device.CanComm.SetRegister(device.Hand.ZMotor.Speed.Addr, device.Hand.ZMotor.SecondSpeed.SetValue);
                ret = device.MoveZ(z, true);
            }
            else if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                ret = device.MoveZ(z,false,true,device.GetSeleteced());
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.MoveZTo(z, true);
            }
            return ret;
        }

        public bool MoveInjectY(int []y, bool is_frist = true)
        {
            bool ret = false;
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                for (int i=0;i<4;i++)
                {
                    if(y[i]>=0)
                    {
                        var inject = device.Injector.Entercloses[i];
                        ret = device.CanComm.MoveMotor(inject.YMotor, y[i], null, true);
                    }
                }
            }
            return ret;
        }

        public bool DoneInjectY(int index)
        {
            bool ret = false;
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                var inject = device.Injector.Entercloses[index];
                ret = device.CanComm.GetBool(inject.YMotor.DoneCoil.Addr, false);
            }
            return ret;
        }

        public bool MoveInjectZ(int[] z, bool is_frist = true)
        {
            bool ret = false;
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                for (int i = 0; i < 4; i++)
                {
                    if (z[i] >= 0)
                    {
                        var inject = device.Injector.Entercloses[i];
                        ret = device.CanComm.MoveMotor(inject.ZMotor, z[i], null, true);
                    }
                }
            }
            return ret;
        }

        public bool DoneInjectZ(int index)
        {
            bool ret = false;
            if (node is InjectorDevice)
            {
                var device = (InjectorDevice)node;
                var inject = device.Injector.Entercloses[index];
                ret = device.CanComm.GetBool(inject.ZMotor.DoneCoil.Addr, false);
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
                ret = true;
                var device = (InjectorDevice)node;
                ret = device.InitY(true);
                return ret;
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.InitY(true);
            }
            return ret;
        }

        public bool InitZ()
        {
            bool ret = false;
            if (node is MachineHandDevice)
            {
                var device = (MachineHandDevice)node;
                ret = device.InitZ(true);
            }
            else if (node is InjectorDevice)
            {
                ret = true;
                var device = (InjectorDevice)node;
                ret = device.InitZ(true);
                return ret;
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.InitZ(true);
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
                var device = (InjectorDevice)node;
                ret = device.CanComm.GetBool(device.Injector.TMotor.DoneCoil.Addr, false);
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.CanComm.GetBool(device.Pie.YMotor.DoneCoil.Addr, false);
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
                ret = true;
                var device = (InjectorDevice)node;
                foreach(var ent in device.GetSeleteced())
                {
                    ret = ret&&device.CanComm.GetBool(ent.ZMotor.DoneCoil.Addr, false);
                    if (ret == false) break;
                }
                return ret;
            }
            else if (node is PiercerDevice)
            {
                var device = (PiercerDevice)node;
                ret = device.CanComm.GetBool(device.Pie.ZMotor.DoneCoil.Addr, false);
            }
            return ret;
        }
    }

    public class TakeTipData
    {
        public int x;
        public int y;
        public int count;
        public TakeTipData(int xx,int yy,int ccount)
        {
            x = xx;
            y = yy;
            count = ccount;
        }
    }

    public class Action : System.ICloneable
    {
        public static int id_count = 0;
        public int id { get; set; } = 0;
        public string start_time { get; set; }
        public double time { get; set; } = 0;
        public bool isstop { get; set; } = false;
        public bool isinit { get; set; } = false;
        public bool istimeout { get; set; } = false;
        public int step { get; set; } = 0;
        public AbstractCanDevice node = null;
        public virtual void run(double dt) { }
        public virtual void init() { }
        public bool isfinish = false;
        public bool isInit() { return isinit;}
        public Action()
        {
            id = id_count++;
        }
        public virtual void finish()
        {
            isstop = true;
            isfinish = true;
        }
        public virtual void stop()
        {
            isstop = true;
        }
        public virtual void start()
        {
            isstop = false;
        }
        public bool getIsStop()
        {
            return isstop;
        }
        public int getStep()
        {
            return step;
        }
        public int getID()
        {
            return id;
        }
        public bool getIsFinish()
        {
            return isfinish && istimeout == false;
        }
        public void runAction(AbstractCanDevice nodetem)
        {
            node = nodetem;
            init();
            start_time = DateTime.Now.ToString();
            ActionManager.getInstance().addAction(this);
        }
        public void runAction()
        {
            init();
            start_time = DateTime.Now.ToString();
            ActionManager.getInstance().addAction(this);
        }
        public virtual string getName()
        {
            return "无";
        }

        public string getActState()
        {
            string action_state = "";
            if(this is Sequence seq)
            {
                action_state += "(";
                foreach (var act in seq.actionlist)
                {
                    action_state += act.getActState();
                }
                action_state += ")";
            }
            else if (this is Spawn spa)
            {
                foreach (var act in spa.actionlist)
                {
                    action_state += act.getActState();
                }
            }
            else
            {
                action_state += getName()+",";
            }
            return action_state;
        }

        public virtual string getStartTime()
        {
            return start_time;
        }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class MoveTo : Action
    {
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int z { get; set; } = 0;
        public bool xdone { get; set; } = false;
        public bool ydone { get; set; } = false;
        public bool zdone { get; set; } = false;
        public double sumdt { get; set; } = 0;
        public bool is_frist { get; set; } = false;
        public static MoveTo instance = null;
        public static MoveTo create(AbstractCanDevice nodetem,double ttime = 0, int xx = -1, int yy = -1, int zz = -1,bool iis_frist=true)
        {
            MoveTo acttem = create(ttime,xx,yy,zz, iis_frist);
            acttem.node = nodetem;
            return acttem;
        }
        public static MoveTo create(double ttime = 0, int xx = -1, int yy = -1, int zz = -1, bool iis_frist = true)
        {
            MoveTo acttem = new MoveTo();
            acttem.x = xx;
            acttem.y = yy;
            acttem.z = zz;
            acttem.time = ttime;
            acttem.is_frist = iis_frist;
            return acttem;
        }
        public override string getName()
        {
            return "移动";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            xdone = (x < 0);
            ydone = (y < 0);
            zdone = (z < 0);
        }
        public override void run(double dt)
        {
            bool resultx = true;
            bool resulty = true;
            bool resultz = true;
            sumdt += dt;
            if (sumdt >= time|| istimeout)
            {
                istimeout = true;
                isfinish = true;
                return;
            }
            var device = new ActionDevice(node);
            switch(step)
            {
                case 0:
                    if(x >= 0) resultx = device.MoveX((int)x);
                    if(y >= 0) resulty = device.MoveY((int)y);
                    if(z >= 0) resultz = device.MoveZ((int)z,is_frist);
                    if (resultx&& resulty && resultz) step++;
                    break;
                case 1:
                    if(xdone==false) xdone = device.DoneX();
                    if(ydone==false) ydone = device.DoneY();
                    if(zdone==false) zdone = device.DoneZ();
                    isfinish = xdone && ydone && zdone;
                    break;
            }
        }
        public static MoveTo getInstance()
        {
            if (instance == null) instance = new MoveTo();
            return instance;
        }
    }

    public class InjectMoveTo : Action
    {
        public int []y { get; set; } = new int[4];
        public bool []ydone { get; set; } = new bool[4];
        public double sumdt { get; set; } = 0;
        public static InjectMoveTo instance = null;
        public static InjectMoveTo create(AbstractCanDevice nodetem, double ttime,int []yy)
        {
            InjectMoveTo acttem = InjectMoveTo.create(ttime, yy);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectMoveTo create(double ttime, int[] yy)
        {
            InjectMoveTo acttem = new InjectMoveTo();
            for (int i = 0; i < 4; i++)
            {
                acttem.y[i] = yy[i];
                acttem.ydone[i] = false;
            }
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "加样器移动";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            for (int i = 0; i < 4; i++)
            {
                ydone[i] = y[i]<0;
            }
        }
        public override void run(double dt)
        {
            bool resulty = true;
            sumdt += dt;
            if (sumdt >= time || istimeout)
            {
                istimeout = true;
                isfinish = true;
                return;
            }
            var device = new ActionDevice(node);
            switch (step)
            {
                case 0:
                    resulty = device.MoveInjectY(y);
                    if ( resulty) step++;
                    break;
                case 1:
                    for(int i=0;i<4;i++)
                    {
                        if (ydone[i] == false) ydone[i] = device.DoneInjectY(i);
                    }
                    isfinish = ydone[0] && ydone[1] && ydone[2]&& ydone[3];
                    break;
            }
        }
        public static InjectMoveTo getInstance()
        {
            if (instance == null) instance = new InjectMoveTo();
            return instance;
        }
    }


    public class InitXyz : Action
    {
        public bool x { get; set; } = false;
        public bool y { get; set; } = false;
        public bool z { get; set; } = false;
        public bool xdone { get; set; } = false;
        public bool ydone { get; set; } = false;
        public bool zdone { get; set; } = false;
        public double sumdt { get; set; } = 0;
        public static InitXyz instance = null;
        public static InitXyz create(AbstractCanDevice nodetem, double ttime = 0, bool xx = true, bool yy = true, bool zz = true)
        {
            InitXyz acttem = InitXyz.create(ttime, xx, yy, zz);
            acttem.node = nodetem;
            return acttem;
        }
        public static InitXyz create(double ttime = 0, bool xx = true, bool yy = true, bool zz = true)
        {
            InitXyz acttem = new InitXyz();
            acttem.x = xx;
            acttem.y = yy;
            acttem.z = zz;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "初始化";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            xdone = !x;
            ydone = !y;
            zdone = !z;
        }
        public override void run(double dt)
        {
            bool resultx = true;
            bool resulty = true;
            bool resultz = true;
            sumdt += dt;
            if (sumdt >= time || istimeout)
            {
                istimeout = true;
                isfinish = true;
                return;
            }
            var device = new ActionDevice(node);
            switch (step)
            {
                case 0:
                    if (x) resultx = device.InitX();
                    if (y) resulty = device.InitY();
                    if (z) resultz = device.InitZ();
                    if (resultx && resulty && resultz) step++;
                    break;
                case 1:
                    if (xdone == false) xdone = device.DoneX();
                    if (ydone == false) ydone = device.DoneY();
                    if (zdone == false) zdone = device.DoneZ();
                    isfinish = xdone && ydone && zdone;
                    break;
            }
        }
        public static InitXyz getInstance()
        {
            if (instance == null) instance = new InitXyz();
            return instance;
        }
    }

    public class HandTakeCard : Action
    {
        public int z { get; set; } = 0;
        public int zl { get; set; } = 0;
        public int zc { get; set; } = 0;
        public int zb { get; set; } = 0;
        public int curz { get; set; } = 0;

        public double sumdt { get; set; } = 0;
        public double lasttime { get; set; } = 0;

        public Action act_movez { get; set; } = null;
        public Action act_movezl { get; set; } = null;
        public Action act_movezb { get; set; } = null;
        public Action act_movecurz { get; set; } = null;

        public static HandTakeCard instance = null;
        public static HandTakeCard create(MachineHandDevice nodetem, double ttime,int zz, int zzl, int zzc, int zzb=0)
        {
            HandTakeCard acttem = HandTakeCard.create(ttime, zz, zzl, zzc, zzb);
            acttem.node = nodetem;
            return acttem;
        }
        public static HandTakeCard create(double ttime,int zz, int zzl, int zzc, int zzb = 0)
        {
            HandTakeCard acttem = new HandTakeCard();
            acttem.z = zz;
            acttem.zl = zzl;
            acttem.zc = zzc;
            acttem.zb = zzb;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "抓卡";
        }
        public override void init()
        {
            isinit = true;
            sumdt = 0;
            curz = 0;
            step = 0;
            act_movez = MoveTo.create(node,time, -1, -1, z);
            act_movezl = MoveTo.create(node, time, -1, -1, zl,false);
            act_movezb = MoveTo.create(node, time, -1, -1, zb);
            act_movez.init();
            act_movezl.init();
            act_movezb.init();
        }
        public override void run(double dt)
        {
            bool resultz = true;
            sumdt += dt;
            if (sumdt >= time || istimeout)
            {
                istimeout = true;
                isfinish = true;
                return;
            }
            var device = new ActionDevice(node);
            var hand = (MachineHandDevice)node;
            switch (step)
            {
                case 0:
                    if (hand.CheckGel()==true)
                    {
                        ErrorSystem.WriteActError("抓手有卡！", true);
                        istimeout = true;
                        isfinish = true;
                        return;
                    }
                    else
                    {
                        step++;
                    }
                    break;
                case 1:
                    resultz = hand.Hand.IsOpen || (hand.Hand.IsOpen == false && hand.SwitchHand(true));
                    if (resultz) step++;
                    break;
                case 2:
                    act_movez.run(dt);
                    if (act_movez.getIsFinish()) step++;
                    break;
                case 3:
                    act_movezl.run(dt);
                    if (sumdt - lasttime > 50)
                    {
                        lasttime = sumdt;
                        if (hand.CheckGel() == true)
                        {
                            resultz = hand.CanComm.SetCoilOff(hand.Hand.ZMotor.StartCoil.Addr);
                            if (resultz) step++;
                        }
                    }
                    if (act_movezl.getIsFinish())
                    {
                        ErrorSystem.WriteActError("抓手抓不到卡！", true);
                        istimeout = true;
                        isfinish = true;
                        return;
                    }
                    break;
                case 4:
                    bool is_timeout = false;
                    hand.CanComm.ReadRegister(hand.Hand.ZMotor.RealDistance.Addr);
                    curz = hand.CanComm.GetIntBlock(hand.Hand.ZMotor.RealDistance.Addr, 1000, out is_timeout) + (int)zc;
                    if(is_timeout)
                    {
                        ErrorSystem.WriteActError("抓手超时", true);
                        istimeout = true;
                        isfinish = true;
                        return;
                    }
                    else
                    {
                        act_movecurz = MoveTo.create(node, time, -1, -1, curz);
                        act_movecurz.init();
                        step++;
                    }
                    break;
                case 5:
                    act_movecurz.run(dt);
                    if (act_movecurz.getIsFinish()) step++;
                    break;
                case 6:
                    resultz = hand.SwitchHand(false);
                    if(resultz) step++;
                    break;
                case 7:
                    act_movezb.run(dt);
                    if (act_movezb.getIsFinish()) step++;
                    break;
                case 8:
                    isfinish = true;
                    break;

            }

        }
        public static HandTakeCard getInstance()
        {
            if (instance == null) instance = new HandTakeCard();
            return instance;
        }
    }

    public class HandPutCard : Action
    {
        public int z { get; set; } = 0;
        public int zb { get; set; } = 0;

        public Action act_movez { get; set; } = null;
        public Action act_movezb { get; set; } = null;

        public double sumdt { get; set; } = 0;
        public static HandPutCard instance = null;
        public static HandPutCard create(MachineHandDevice nodetem, double ttime, int zz,int zzb = 0)
        {
            HandPutCard acttem = new HandPutCard();
            acttem.z = zz;
            acttem.zb = zzb;
            acttem.time = ttime;
            acttem.node = nodetem;
            return acttem;
        }
        public static HandPutCard create(double ttime, int zz,int zzb = 0)
        {
            HandPutCard acttem = new HandPutCard();
            acttem.z = zz;
            acttem.zb = zzb;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "放卡";
        }
        public override void init()
        {
            isinit = true;
            act_movez = MoveTo.create(node, time, -1, -1, z);
            act_movezb = MoveTo.create(node, time, -1, -1, zb);
            act_movez.init();
            act_movezb.init();
            sumdt = 0;
            step = 0;
        }
        public override void run(double dt)
        {
            bool resultz = true;
            sumdt += dt;
            if (sumdt >= time || istimeout)
            {
                istimeout = true;
                isfinish = true;
                return;
            }
            var device = new ActionDevice(node);
            var hand = (MachineHandDevice)node;
            switch (step)
            {
                case 0:
                    if (hand.CheckGel() == false)
                    {
                        ErrorSystem.WriteActError("抓手无卡！", true);
                        istimeout = true;
                        isfinish = true;
                        return;
                    }
                    else
                    {
                            step++;
                    }
                    break;
                case 1:
                    act_movez.run(dt);
                    if (act_movez.getIsFinish()) step++;
                    break;
                case 2:
                    resultz = hand.SwitchHand(true);
                    if (resultz) step++;
                    break;
                case 3:
                    act_movezb.run(dt);
                    if (act_movezb.getIsFinish()) step++;
                    break;
                case 4:
                    isfinish = true;
                    break;
            }
        }
        public static HandPutCard getInstance()
        {
            if (instance == null) instance = new HandPutCard();
            return instance;
        }
    }

    public class InjectTakeTip : Action
    {
        public TakeTipData tipdata;
        public int z { get; set; } = 0;
        public int zb { get; set; } = 0;
        public bool []zdone { get; set; } = new bool[4];
        public Action act_movexy { get; set; } = null;
        public Action act_movezb { get; set; } = null;
        //针头序号
        public int head_index { get; set; } = 0;
        public double sumdt { get; set; } = 0;
        public static InjectTakeTip instance = null;
        public static InjectTakeTip create(MachineHandDevice nodetem, double ttime, int zz,int zzb,int index, TakeTipData ttipdata)
        {
            InjectTakeTip acttem = InjectTakeTip.create(ttime, zz, zzb, index,ttipdata);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectTakeTip create(double ttime, int zz,int zzb, int index, TakeTipData ttipdata)
        {
            InjectTakeTip acttem = new InjectTakeTip();
            acttem.z = zz;
            acttem.zb = zzb;
            acttem.tipdata = ttipdata;
            acttem.head_index = index;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "取tip头";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            for (int i = 0; i < 4; i++)
            {
                zdone[i] = (i< head_index)||(i>= head_index+ tipdata.count);
            }
            act_movexy = MoveTo.create(node, tipdata.x, tipdata.y, -1);
            act_movezb = MoveTo.create(node, -1, -1, zb);
            act_movexy.init();
            act_movezb.init();
        }
        public override void run(double dt)
        {
            bool resultz = true;
            sumdt += dt;
            if (sumdt >= time || istimeout)
            {
                istimeout = true;
                isfinish = true;
                return;
            }
            var device = new ActionDevice(node);
            var inject = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    //是否已有吸头
                    //针头数据检测(针头序号+装针个数不能大于通道个数)
                    int inject_count = inject.GetSeleteced().Count();
                    if (head_index+ tipdata.count<= inject_count)
                    {
                        step++;
                    }
                    else
                    {
                        istimeout = true;
                        isfinish = true;
                    }
                    break;
                case 1:
                    act_movexy.run(dt);
                    if (act_movexy.getIsFinish()) step++;
                    break;
                case 2:
                    int[] z_tem = { -1, -1, -1, -1 };
                    for(int i=head_index;i<tipdata.count;i++)
                    {
                        z_tem[i] = z;
                    }
                    resultz = device.MoveInjectZ(z_tem, true);
                    if (resultz) step++;
                    break;
                case 3:
                    for (int i = head_index; i < tipdata.count; i++)
                    {
                        if (zdone[i] == false)zdone[i] = device.DoneInjectZ(i);
                    }
                    if (zdone[0]&&zdone[1]&&zdone[2]&&zdone[3]) step++;
                    break;
                case 4:
                    act_movezb.run(dt);
                    if (act_movezb.getIsFinish()) step++;
                    break;
                case 5:
                    isfinish = true;
                    break;
            }
        }
        public static InjectTakeTip getInstance()
        {
            if (instance == null) instance = new InjectTakeTip();
            return instance;
        }
    }

    public class SKSleep : Action
    {
       
        public double sumdt { get; set; } = 0;
        public static SKSleep instance = null;
        public static SKSleep create(double ttime = 0)
        {
            SKSleep acttem = new SKSleep();
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "等待";
        }
        public override void init()
        {
            isinit = true;
        }
        public override void run(double dt)
        {
            if(sumdt==0&&node!=null)
            Engine.Log("sleep:" + node.GetType() + " " + time);
            sumdt += dt;
            isfinish = sumdt >= time;
            
        }
        public static SKSleep getInstance()
        {
            if (instance == null) instance = new SKSleep();
            return instance;
        }
    }

    public class Sequence : Action
    {
        public List<Action> actionlist { get; set; } = new List<Action>();
        public static Sequence instance = null;
        public static Sequence create(AbstractCanDevice nodetem, params Action[] actions)
        {
            Sequence acttem = Sequence.create(actions);
            acttem.node = nodetem;
            return acttem;
        }
        public static Sequence create(params Action[] actions)
        {
            Sequence acttem = new Sequence();
            for (int i = 0; i < actions.Length; i++)
            {
                acttem.actionlist.Add(actions[i]);
                acttem.time += actions[i].time;
            }
            return acttem;
        }

        public static Sequence create(List<Action> actions)
        {
            Sequence acttem = new Sequence();
            for (int i = 0; i < actions.Count; i++)
            {
                acttem.actionlist.Add(actions[i]);
                acttem.time += actions[i].time;
            }
            return acttem;
        }
        public override string getName()
        {
            return "单任务";
        }
        public void AddAction(Action action)
        {
            actionlist.Add(action);
            time += action.time;
        }
        public static Sequence create(List<List<Action>> actions)
        {
            Sequence acttem = new Sequence();
            for (int i = 0; i < actions.Count; i++)
            {
                if(actions[i].Count==1)
                {
                    acttem.actionlist.Add(actions[i][0]);
                    acttem.time += actions[i][0].time;
                }
                else
                {
                    Action spawnact = Spawn.create(actions[i]);
                    acttem.actionlist.Add(spawnact);
                    acttem.time += spawnact.time;
                }
            }
            return acttem;
        }
        public override void init()
        {
            for (int i = 0; i < actionlist.Count; i++)
            {
                if(actionlist[i].node==null)
                actionlist[i].node = node;
                actionlist[i].init();
                actionlist[i].isfinish = false;
                actionlist[i].istimeout = false;
            }
            isinit = true;
        }
        public override void run(double dt)
        {
            if (actionlist.Count > 0)
            {
                if (actionlist[0].node==null)
                {
                    actionlist[0].node = node;
                }
                if (!actionlist[0].isInit())
                actionlist[0].init();
                if (actionlist[0] is SkCallBackFun)
                    ((SkCallBackFun)actionlist[0]).run(dt, this);
                else
                actionlist[0].run(dt);
                //删除完成的动作
                for (int i = actionlist.Count - 1; i >= 0; i--)
                {
                    if (actionlist[i].isfinish)
                    {
                        bool istimeouttem = actionlist[i].istimeout;
                        actionlist.Remove(actionlist[i]);
                        if(actionlist.Count!=0)
                        {
                            if (actionlist[0].node == null)
                            actionlist[0].node = node;
                            actionlist[0].istimeout = istimeouttem;
                        }
                        istimeout = istimeouttem;
                    }
                        
                }
            }
            isfinish = actionlist.Count == 0;
        }
        public static Sequence getInstance()
        {
            if (instance == null) instance = new Sequence();
            return instance;
        }

        public override object Clone()
        {
            var seq = new Sequence();
            foreach(var act in actionlist)
            {
                seq.actionlist.Add((Action)act.Clone());
            }
            return seq as object;
        }
    }


    public class Spawn : Action
    {
        public List<Action> actionlist { get; set; } = new List<Action>();
        public static Spawn instance = null;
        public static Spawn create(params Action[] actions)
        {
            Spawn acttem = new Spawn();
            for (int i = 0; i < actions.Length; i++)
            {
                acttem.actionlist.Add(actions[i]);
                if(acttem.time< actions[i].time)
                acttem.time = actions[i].time;
            }
            return acttem;
        }
        public static Spawn create(List<Action> actions)
        {
            Spawn acttem = new Spawn();
            for (int i = 0; i < actions.Count; i++)
            {
                acttem.actionlist.Add(actions[i]);
                if (acttem.time < actions[i].time)
                    acttem.time = actions[i].time;
            }
            return acttem;
        }
        public override string getName()
        {
            return "多任务";
        }
        public override void init()
        {
            for (int i = 0; i < actionlist.Count; i++)
            {
                if (actionlist[i].node == null)
                    actionlist[i].node = node;
            }
            isinit = true;
        }
        public override void run(double dt)
        {
            if (actionlist.Count > 0)
            {
                for (int i = actionlist.Count - 1; i >= 0; i--)
                {
                    if (actionlist[i].node == null)
                    {
                        actionlist[i].node = node;
                    }
                    if (!actionlist[i].isInit())
                        actionlist[i].init();

                    if (actionlist[i] is SkCallBackFun)
                        ((SkCallBackFun)actionlist[i]).run(dt, this);
                    else
                        actionlist[i].run(dt);

                    //删除完成的动作
                    if (actionlist[i].isfinish)
                    {
                        actionlist.Remove(actionlist[i]);
                    }
                }
            }
            isfinish = actionlist.Count == 0;
        }

        public static Spawn getInstance()
        {
            if (instance == null) instance = new Spawn();
            return instance;
        }

        public override object Clone()
        {
            var spa = new Spawn();
            foreach (var act in actionlist)
            {
                spa.actionlist.Add((Action)act.Clone());
            }
            return spa as object;
        }
    }

    public class Repeat : Action
    {
        public Action repeat_action { get; set; } = new Action();
        public Action repeat_clone { get; set; } = new Action();
        public int times { get; set; } = 0;
        public int times_count { get; set; } = 0;
        public static Repeat instance = null;
        public static Repeat create(AbstractCanDevice nodetem, Action action, int ttimes)
        {
            Repeat acttem = Repeat.create(action, ttimes);
            acttem.node = nodetem;
            return acttem;
        }
        public static Repeat create(Action action, int ttimes)
        {
            Repeat acttem = new Repeat();
            acttem.repeat_clone = action;
            acttem.times = ttimes;
            return acttem;
        }
        public override string getName()
        {
            string tem = string.Format("重复{0}/{1}", times_count, times);
            tem = tem + repeat_action.getActState();
            return tem;
        }
        public override void init()
        {
            repeat_action = (Action)repeat_clone.Clone();
            times_count = 0;
            isinit = true;
        }
        public override void run(double dt)
        {
            if (repeat_action.node == null)
            {
                repeat_action.node = node;
            }
            if (!repeat_action.isInit())
                repeat_action.init();
            repeat_action.run(dt);
            //删除完成的动作
            if (repeat_action.isfinish)
            {
                if(repeat_action.istimeout==false)
                {
                    repeat_action = (Action)repeat_clone.Clone();
                    times_count++;
                }
            }
            istimeout = repeat_action.istimeout;
            isfinish = times_count >= times || istimeout;
        }

        public static Repeat getInstance()
        {
            if (instance == null) instance = new Repeat();
            return instance;
        }
    }
    

    public class SkCallBackFun : Action
    {
        public delegate bool skcallbackfun(Action act);
        public skcallbackfun callbackfun;
        public static SkCallBackFun instance = null;
        public static SkCallBackFun create(skcallbackfun fun)
        {
            SkCallBackFun acttem = new SkCallBackFun();
            acttem.callbackfun = fun;
            return acttem;
        }
        public override void init()
        {
            isinit = true;
        }
        public void run(double dt, Action act)
        {
            isfinish = callbackfun(act);
        }
        public static SkCallBackFun getInstance()
        {
            if (instance == null) instance = new SkCallBackFun();
            return instance;
        }
    }
}
