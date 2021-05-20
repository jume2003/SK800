using SKABO.ActionGeneraterEngine;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Hardware.Model;
using SKABO.Hardware.RunBJ;
using SKABO.MAI.ErrorSystem;
using SKABO.ResourcesManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SKABO.ActionEngine
{
    public class ActionBase : System.ICloneable
    {
        public delegate bool skcallbackfun(ActionBase act);
        public static int id_count = 0;
        public int id { get; set; } = 0;
        public string start_time { get; set; }
        public double time { get; set; } = 0;
        public bool isstop { get; set; } = false;
        public bool isinit { get; set; } = false;
        public bool istimeout { get; set; } = false;
        public int step { get; set; } = 0;
        public double sumdt { get; set; } = 0;
        public AbstractCanDevice node = null;
        public virtual void run(double dt) { }
        public virtual void init() { }
        public bool isfinish = false;
        public bool isdelete = false;
        public bool isInit() { return isinit;}
        public string errmsg = "";
        public skcallbackfun destroyfun = null;
        public bool CountTime(double dt)
        {
            sumdt += dt;
            if (sumdt >= time || istimeout)
            {
                istimeout = true;
                return true;
            }
            return false;
        }
        public void Destroy()
        {
            if (this is Sequence seq)
            {
                foreach (var act in seq.actionlist)
                {
                    act.isdelete = isdelete;
                    act.Destroy();
                }
            }
            else if (this is Spawn spa)
            {
                foreach (var act in spa.actionlist)
                {
                    act.isdelete = isdelete;
                    act.Destroy();
                }
            }
            if (destroyfun != null && isdelete)
                destroyfun(this);
        }
        public ActionBase()
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
        public string getDivName()
        {
            string deviname = "";
            if (node!=null)
            deviname = node.GetType().Name.Substring(0, 3);
            return deviname;
        }
        public bool getIsFinish()
        {
            return isfinish && istimeout == false;
        }
        public void runAction(AbstractCanDevice nodetem)
        {
            node = nodetem;
            runAction();
        }
        public void runAction()
        {
            //init();
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

        public List<ActionBase> getRuningSleepAct()
        {
            List<ActionBase> action_list = new List<ActionBase>();
            bool is_cok = (this is SKSleep);
            if (is_cok && this.isinit && this.isfinish == false)
            {
                action_list.Add(this);
            }
            if (this is Sequence seq)
            {
                foreach (var act in seq.actionlist)
                {
                    action_list = action_list.Concat(act.getRuningSleepAct()).ToList<ActionBase>();
                }
            }
            else if (this is Spawn spa)
            {
                foreach (var act in spa.actionlist)
                {
                    action_list = action_list.Concat(act.getRuningSleepAct()).ToList<ActionBase>();
                }
            }
            else if (this is Repeat rep)
            {
                action_list = action_list.Concat(rep.repeat_action.getRuningSleepAct()).ToList<ActionBase>();
            }
            return action_list;
        }

        public int getAllRuningActCount(AbstractCanDevice nodetem)
        {
            List<ActionBase> action_list = new List<ActionBase>();
            getAllRuningAct(nodetem,ref action_list);
            return action_list.Count;
        }

        public void getAllRuningAct(AbstractCanDevice nodetem,ref List<ActionBase> action_list)
        {
            bool is_cok = (this is Sequence==false)&& (this is Spawn == false)&& (this is Repeat == false) && (this is SKSleep == false);
            if (is_cok&&this.isinit&&this.isfinish==false&&node== nodetem)
            {
                action_list.Add(this);
            }
            if(this is Sequence seq)
            {
                foreach (var act in seq.actionlist)
                {
                    act.getAllRuningAct(nodetem, ref action_list);
                }
            }
            else if(this is Spawn spa)
            {
                foreach(var act in spa.actionlist)
                {
                    act.getAllRuningAct(nodetem,ref action_list);
                }
            }else if(this is Repeat rep)
            {
                rep.repeat_action.getAllRuningAct(nodetem,ref action_list);
            }
        }

        public void getAllRuningDevice(ref List<AbstractCanDevice> device_list)
        {
            bool is_cok = (this is Sequence == false) && (this is Spawn == false) && (this is Repeat == false) && (this is SKSleep == false);
            if (is_cok && this.isinit && this.istimeout == false && this.isfinish == false&& node!=null)
            {
                device_list.Add(this.node);
            }
            if (this is Sequence seq)
            {
                foreach (var act in seq.actionlist)
                {
                    act.getAllRuningDevice(ref device_list);
                }
            }
            else if (this is Spawn spa)
            {
                foreach (var act in spa.actionlist)
                {
                    act.getAllRuningDevice(ref device_list);
                }
            }
            else if (this is Repeat rep)
            {
                rep.repeat_action.getAllRuningDevice(ref device_list);
            }
        }
    }

    public class MoveTo : ActionBase
    {
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int z { get; set; } = 0;
        public bool xdone { get; set; } = false;
        public bool ydone { get; set; } = false;
        public bool zdone { get; set; } = false;
        public int speed { get; set; } = 0;
        public double wait_time { get; set; } = 0;
        public static MoveTo instance = null;
        public static MoveTo create(AbstractCanDevice nodetem,double ttime = 0, int xx = -1, int yy = -1, int zz = -1,int sspeed = 0)
        {
            MoveTo acttem = create(ttime,xx,yy,zz, sspeed);
            acttem.node = nodetem;
            return acttem;
        }
        public static MoveTo create(double ttime = 0, int xx = -1, int yy = -1, int zz = -1, int sspeed = 0)
        {
            MoveTo acttem = new MoveTo();
            acttem.x = xx;
            acttem.y = yy;
            acttem.z = zz;
            acttem.time = ttime;
            acttem.speed = sspeed;
            return acttem;
        }
        public override string getName()
        {
            string movexyz = (x != -1 ? "x" : "") + (y != -1 ? "y" : "") + (z != -1 ? "z" : "");
            return getDivName() + "移动"+ movexyz;
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
            int device_rx = 0;
            int device_cx = 0;
            AbstractCanDevice checkdev = null;
            var handdev = ActionGenerater.getInstance().handDevice;
            var injectdev = ActionGenerater.getInstance().injectorDevice;
            if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            if (node is InjectorDevice)
            {
                checkdev = ActionGenerater.getInstance().handDevice;
                device_cx = (int)handdev.Hand.XMotor.CurrentDistance;
            }   
            else if (node is MachineHandDevice)
            {
                checkdev = ActionGenerater.getInstance().injectorDevice;
                device_cx = (int)injectdev.Injector.XMotor.CurrentDistance;
            }
            switch (step)
            {
                case 0:
                    if (x >= 0&& checkdev != null&&false)
                    {
                        sumdt = 0;
                        wait_time += dt;
                        if (wait_time > 500)
                        {
                            wait_time = 0;
                            bool is_timeout = false;
                            if (checkdev is MachineHandDevice)
                            {
                                handdev.CanComm.ReadRegister(handdev.Hand.XMotor.RealDistance.Addr);
                                device_rx = handdev.CanComm.GetIntBlock(handdev.Hand.XMotor.RealDistance.Addr, 1000, out is_timeout);
                            }
                            else if (checkdev is InjectorDevice)
                            {
                                injectdev.CanComm.ReadRegister(injectdev.Injector.XMotor.RealDistance.Addr);
                                device_rx = injectdev.CanComm.GetIntBlock(injectdev.Injector.XMotor.RealDistance.Addr, 1000, out is_timeout);
                            }
                            if (is_timeout == false)
                            {
                                int device_max = 10000 - x;
                                if (device_max < 0) device_max = 0;
                                if (device_rx <= device_max && device_cx <= device_max)
                                {
                                    step++;
                                }
                                else if (ActionManager.getInstance().getAllRuningActionsCount(checkdev) == 0)
                                {
                                    Sequence sequ = null;
                                    List<Enterclose> ents = new List<Enterclose>();
                                    ents.Add(injectdev.Injector.Entercloses[0]);
                                    if (checkdev is MachineHandDevice)
                                        sequ = Sequence.create(MoveTo.create(checkdev, 3000, -1, -1, 0), MoveTo.create(checkdev, 3000, device_max, -1, -1));
                                    else if (checkdev is InjectorDevice)
                                        sequ = Sequence.create(InjectMoveTo.create(3000, ents.ToArray(), -1, IMask.Gen(-1), IMask.Gen(0)), MoveTo.create(checkdev, 3000, device_max, -1, -1));
                                    sequ.runAction(checkdev);
                                }
                            }
                        }
                    }
                    else
                    {
                       
                    }
                    step++;
                    break;
                case 1:
                    if(x >= 0) resultx = device.MoveX((int)x);
                    if(y >= 0) resulty = device.MoveY((int)y);
                    if(z >= 0) resultz = device.MoveZ((int)z,speed);
                    if (resultx&& resulty && resultz) step++;
                    break;
                case 2:
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

    public class InjectMoveTo : ActionBase
    {

        public int x { get; set; } = 0;
        public int []y { get; set; } = new int[4];
        public int []z { get; set; } = new int[4];
        public bool xdone { get; set; } = false;
        public bool ydone { get; set; } = false;
        public bool zdone { get; set; } = false;
        public int speed = 0;
        public Enterclose[] injects { get; set; } = null;
        public static InjectMoveTo instance = null;

        public static InjectMoveTo create(AbstractCanDevice nodetem, double ttime, Enterclose[] iinjects,int xx,int []yy, int[] zz, int sspeed = 0)
        {
            InjectMoveTo acttem = InjectMoveTo.create(ttime, iinjects,xx, yy,zz, sspeed);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectMoveTo create(double ttime, Enterclose[] iinjects, int xx, int[] yy, int[] zz, int sspeed = 0)
        {
            InjectMoveTo acttem = new InjectMoveTo();
            for (int i = 0; i < 4; i++)
            {
                acttem.y[i] = yy[i];
                acttem.z[i] = zz[i];
            }
            acttem.x = xx;
            acttem.time = ttime;
            acttem.speed = sspeed;
            acttem.injects = iinjects;
            return acttem;
        }
        public override string getName()
        {
            return getDivName() + "加样器移动";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            xdone = x<0;
            ydone = y[0] < 0 && y[1] < 0 && y[2] < 0 && y[3] < 0;
            zdone = z[0] < 0 && z[1] < 0 && z[2] < 0 && z[3] < 0;
        }
        public override void run(double dt)
        {
            bool resultx = true;
            bool resulty = true;
            bool resultz = true;
            if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            var inject_device = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    //inject_device.CanComm.SetByte(inject_device.Injector.TMotor.PickTMotor.Addr, 0x0000);
                    resultx = xdone||device.MoveX(x);
                    resulty = ydone||device.MoveY(injects,speed, y);
                    resultz = zdone||device.MoveZ(injects,speed, z);
                    if (resultx&&resulty && resultz) step++;
                    break;
                case 1:
                    if (xdone == false) xdone = device.DoneX();
                    if (ydone == false) ydone = device.DoneY(injects);
                    if (zdone == false) zdone = device.DoneZ(injects);
                    isfinish = xdone && ydone && zdone;
                    break;
            }
        }
        public static InjectMoveTo getInstance()
        {
            if (instance == null) instance = new InjectMoveTo();
            return instance;
        }
    }

    public class InjectAbsorb : ActionBase
    {
        public int[] absorbs { get; set; } = { 0, 0, 0, 0 };
        public int speed { get; set; } = 0;
        public int[] movez { get; set; } = { 0, 0, 0, 0 };
        public Enterclose[] injects { get; set; } = null;
        public static InjectAbsorb instance = null;

        public static InjectAbsorb create(AbstractCanDevice nodetem, double ttime, Enterclose[] iinjects,int sspeed, params int[] aabsorbs)
        {
            InjectAbsorb acttem = InjectAbsorb.create(ttime, iinjects, sspeed, aabsorbs);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectAbsorb create(double ttime, Enterclose[] iinjects, int sspeed, params int[] aabsorbs)
        {
            InjectAbsorb acttem = new InjectAbsorb();
            acttem.injects = iinjects;
            acttem.absorbs = (int[])aabsorbs.Clone();
            acttem.time = ttime;
            acttem.speed = sspeed;
            return acttem;
        }
        public override string getName()
        {
            return getDivName() + "吸液分液";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
        }
        public override void run(double dt)
        {
            bool resulty = true;
            if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            var inject_device = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    //容量检测
                    bool is_ok = true;
                    int index = 0;
                    foreach(var inject in injects)
                    {
                        bool is_timeout = false;
                        inject_device.CanComm.ReadRegister(inject.PumpMotor.RealDistance.Addr);
                        int curz = inject_device.CanComm.GetIntBlock(inject.PumpMotor.RealDistance.Addr, 1000, out is_timeout);
                        movez[inject.Index] = curz + absorbs[inject.Index];
                        bool is_enough = is_timeout == false && movez[inject.Index] <= (int)inject.PumpMotor.Maximum.SetValue && movez[inject.Index] >= 0;
                        if (is_enough==false|| is_timeout)
                        {
                            index = inject.Index;
                            is_ok = false;
                            break;
                        }
                    }
                    if(is_ok) step++;
                    else
                    {
                        bool ret = ErrorSystem.WriteActError("液泵:" + index + "容量不足!");
                        if (ret==false)
                        {
                            istimeout = true;
                            errmsg = "液泵:" + index + "容量不足!";
                            return;
                        }
                    }
                    if(injects.Count()==0)
                    {
                        bool ret = ErrorSystem.WriteActError("没有可用加样器");
                        if (ret == false)
                        {
                            istimeout = true;
                            errmsg = "没有可用加样器";
                            return;
                        }
                    }
                    break;
                case 1:
                    resulty = device.MovePump(injects, speed, movez);
                    if (resulty) step++;
                    break;
                case 2:
                    isfinish = device.DonePump(injects);
                    break;
            }
        }
        public static InjectAbsorb getInstance()
        {
            if (instance == null) instance = new InjectAbsorb();
            return instance;
        }
    }

    public class InjectAbsorbMove : ActionBase
    {
        public int[] absorbs { get; set; } = {0,0,0,0 };
        public int speed { get; set; } = 0;
        public Enterclose[] injects { get; set; } = null;
        public static InjectAbsorbMove instance = null;

        public static InjectAbsorbMove create(AbstractCanDevice nodetem, double ttime, Enterclose[] iinjects, int sspeed, params int[] aabsorbs)
        {
            InjectAbsorbMove acttem = InjectAbsorbMove.create(ttime, iinjects, sspeed, aabsorbs);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectAbsorbMove create(double ttime, Enterclose[] iinjects, int sspeed, params int[] aabsorbs)
        {
            InjectAbsorbMove acttem = new InjectAbsorbMove();
            acttem.injects = iinjects;
            acttem.absorbs = (int[])aabsorbs.Clone();
            acttem.time = ttime;
            acttem.speed = sspeed;
            return acttem;
        }
        public override string getName()
        {
            return getDivName() + "液泵Z轴移动";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
        }
        public override void run(double dt)
        {
            bool resulty = true;
            if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            var inject_device = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    resulty = device.MovePump(injects, speed, absorbs);
                    if (resulty) step++;
                    break;
                case 1:
                    isfinish = device.DonePump(injects);
                    break;
            }
        }
        public static InjectAbsorbMove getInstance()
        {
            if (instance == null) instance = new InjectAbsorbMove();
            return instance;
        }
    }

    public class InitXyz : ActionBase
    {
        public bool x { get; set; } = false;
        public bool y { get; set; } = false;
        public bool z { get; set; } = false;
        public bool xdone { get; set; } = false;
        public bool ydone { get; set; } = false;
        public bool zdone { get; set; } = false;
        public Enterclose[] injects { get; set; } = null;
        public static InitXyz instance = null;
        public static InitXyz create(AbstractCanDevice nodetem, double ttime = 0, bool xx = true, bool yy = true, bool zz = true)
        {
            InitXyz acttem = InitXyz.create(ttime, xx, yy, zz);
            acttem.node = nodetem;
            return acttem;
        }
        public static InitXyz create(double ttime = 0, Enterclose[] iinjects=null, bool xx = true, bool yy = true, bool zz = true)
        {
            InitXyz acttem = InitXyz.create(ttime, xx, yy, zz);
            acttem.injects = iinjects;
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
            return getDivName() + "初始化";
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
            if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            switch (step)
            {
                case 0:
                    if(injects==null)
                    {
                        if (x) resultx = device.InitX();
                        if (y) resulty = device.InitY();
                        if (z) resultz = device.InitZ();
                    }
                    else
                    {
                        if (x) resultx = device.InitX();
                        if (y) resulty = device.InitY(injects);
                        if (z) resultz = device.InitZ(injects);
                    }
                    if (resultx && resulty && resultz) step++;
                    break;
                case 1:
                    if (injects == null)
                    {
                        if (xdone == false) xdone = device.DoneX();
                        if (ydone == false) ydone = device.DoneY();
                        if (zdone == false) zdone = device.DoneZ();
                    }
                    else
                    {
                        if (xdone == false) xdone = device.DoneX();
                        if (ydone == false) ydone = device.DoneY(injects);
                        if (zdone == false) zdone = device.DoneZ(injects);
                    }
                    
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


    public class InitInjectPump : ActionBase
    {
        public bool pdone { get; set; } = false;
        public Enterclose[] injects { get; set; } = null;
        public static InitInjectPump instance = null;
        public static InitInjectPump create(AbstractCanDevice nodetem, double ttime, Enterclose[] iinjects)
        {
            InitInjectPump acttem = InitInjectPump.create(ttime, iinjects);
            acttem.node = nodetem;
            return acttem;
        }
        public static InitInjectPump create(double ttime,Enterclose[] iinjects)
        {
            InitInjectPump acttem = new InitInjectPump();
            acttem.injects = iinjects;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return getDivName() + "初始化气泵";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            pdone = false;
        }
        public override void run(double dt)
        {
            bool resultx = true;
            if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            var inject_device = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    {
                        int[] absorbs = { 0, 0, 0, 0 };
                        foreach (var ent in injects)
                        {
                            absorbs[ent.Index] = (int)ent.PumpMotor.Maximum.SetValue;
                        }
                        resultx = device.MovePump(injects,100, absorbs);
                        if (resultx) step++;
                    }
                    break;
                case 1:
                    if(device.DonePump(injects)) step++;
                    isfinish = injects.Count() == 0;
                    break;
                case 2:
                    {
                        int[] absorbs = { 0, 0, 0, 0 };
                        resultx = device.MovePump(injects, 100, absorbs);
                        if (resultx) step++;
                    }
                    break;
                case 3:
                    if (device.DonePump(injects)) step++;
                    break;
                case 4:
                    isfinish = true;
                    break;
            }
        }
        public static InitInjectPump getInstance()
        {
            if (instance == null) instance = new InitInjectPump();
            return instance;
        }
    }


    public class SKSleep : ActionBase
    {
       
        public static SKSleep instance = null;
        public static SKSleep create(double ttime = 0)
        {
            SKSleep acttem = new SKSleep();
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return getDivName() + "等待["+ sumdt+"/"+ time + "]";
        }
        public override void init()
        {
            isinit = true;
        }
        public override void run(double dt)
        {
            sumdt += dt;
            if (sumdt >= time)
            {
                isfinish = true;
            }
        }
        public static SKSleep getInstance()
        {
            if (instance == null) instance = new SKSleep();
            return instance;
        }
    }

    public class Sequence : ActionBase
    {
        public List<ActionBase> actionlist { get; set; } = new List<ActionBase>();
        public static Sequence instance = null;
        public static Sequence create(AbstractCanDevice nodetem, params ActionBase[] actions)
        {
            Sequence acttem = Sequence.create(actions);
            acttem.node = nodetem;
            return acttem;
        }
        public static Sequence create(params ActionBase[] actions)
        {
            Sequence acttem = new Sequence();
            for (int i = 0; i < actions.Length; i++)
            {
                acttem.actionlist.Add(actions[i]);
                acttem.time += actions[i].time;
            }
            return acttem;
        }

        public static Sequence create(List<ActionBase> actions)
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
            return getDivName() + "单任务";
        }
        public void AddAction(ActionBase action)
        {
            actionlist.Add(action);
            time += action.time;
        }
        public static Sequence create(List<List<ActionBase>> actions)
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
                    ActionBase spawnact = Spawn.create(actions[i]);
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
                if(actionlist[i].node==null&& node!=null)
                actionlist[i].node = node;
                actionlist[i].isfinish = false;
                actionlist[i].istimeout = false;
            }
            isinit = true;
        }
        public override void run(double dt)
        {
            if (actionlist.Count > 0)
            {
                if (actionlist[0].node==null&& node!=null)
                {
                    actionlist[0].node = node;
                }
                if (!actionlist[0].isInit())
                actionlist[0].init();

                if(actionlist[0].istimeout==false)
                {
                    if (actionlist[0] is SkCallBackFun)
                        ((SkCallBackFun)actionlist[0]).run(dt, this);
                    else
                        actionlist[0].run(dt);
                }
                //删除完成的动作
                for (int i = actionlist.Count - 1; i >= 0; i--)
                {
                    if (actionlist[i].isfinish)
                    {
                        bool is_sleep_last = actionlist[i] is SKSleep;
                        actionlist.Remove(actionlist[i]);
                        if(actionlist.Count!=0)
                        {
                            if (actionlist[0].node == null && node != null)
                            actionlist[0].node = node;
                            if (!actionlist[0].isInit()&& is_sleep_last==false)
                                actionlist[0].init();
                        }
                    }else if(actionlist[i].istimeout)
                    {
                        istimeout = actionlist[i].istimeout;
                        errmsg = actionlist[i].errmsg;
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
                seq.actionlist.Add((ActionBase)act.Clone());
            }
            return seq as object;
        }
    }


    public class Spawn : ActionBase
    {
        public List<ActionBase> actionlist { get; set; } = new List<ActionBase>();
        public static Spawn instance = null;
        public static Spawn create(params ActionBase[] actions)
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
        public static Spawn create(List<ActionBase> actions)
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
        public void AddAction(ActionBase action)
        {
            actionlist.Add(action);
            for (int i = 0; i < actionlist.Count; i++)
            {
                if (time < actionlist[i].time)
                    time = actionlist[i].time;
            }
        }
        public override string getName()
        {
            return getDivName() + "多任务";
        }
        public override void init()
        {
            for (int i = 0; i < actionlist.Count; i++)
            {
                if (actionlist[i].node == null&&node!=null)
                    actionlist[i].node = node;
            }
            isinit = true;
        }
        public override void run(double dt)
        {
            if (actionlist.Count > 0)
            {
                for (int i = 0; i < actionlist.Count; i++)
                {
                    if (actionlist[i].node == null && node != null)
                    {
                        actionlist[i].node = node;
                    }
                    if (!actionlist[i].isInit())
                        actionlist[i].init();

                    if (actionlist[i] is SkCallBackFun)
                        ((SkCallBackFun)actionlist[i]).run(dt, this);
                    else if (actionlist[i].istimeout == false)
                        actionlist[i].run(dt);

                    if (istimeout == false)
                    {
                        istimeout = actionlist[i].istimeout;
                        errmsg = actionlist[i].errmsg;
                    }
                }
                for (int i = actionlist.Count - 1; i >= 0; i--)
                {
                    //删除完成的动作
                    if (actionlist[i].isfinish)
                    actionlist.Remove(actionlist[i]);
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
                spa.actionlist.Add((ActionBase)act.Clone());
            }
            return spa as object;
        }
    }

    public class Repeat : ActionBase
    {
        public ActionBase repeat_action { get; set; } = new ActionBase();
        public ActionBase repeat_clone { get; set; } = new ActionBase();
        public int times { get; set; } = 0;
        public int times_count { get; set; } = 0;
        public static Repeat instance = null;
        public static Repeat create(AbstractCanDevice nodetem, ActionBase action, int ttimes)
        {
            Repeat acttem = Repeat.create(action, ttimes);
            acttem.node = nodetem;
            return acttem;
        }
        public static Repeat create(ActionBase action, int ttimes)
        {
            Repeat acttem = new Repeat();
            acttem.repeat_clone = action;
            acttem.times = ttimes;
            return acttem;
        }
        public override string getName()
        {
            string tem = string.Format("重复{0}/{1}", times_count, times);
            tem = getDivName() + tem + repeat_action.getActState();
            return tem;
        }
        public override void init()
        {
            repeat_action = (ActionBase)repeat_clone.Clone();
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
                repeat_action = (ActionBase)repeat_clone.Clone();
                times_count++;
            }
            istimeout = repeat_action.istimeout;
            errmsg = repeat_action.errmsg;
            isfinish = times_count >= times;
        }

        public static Repeat getInstance()
        {
            if (instance == null) instance = new Repeat();
            return instance;
        }
    }
    

    public class SkCallBackFun : ActionBase
    {
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
        public void run(double dt, ActionBase act)
        {
            sumdt += dt;
            isfinish = callbackfun(act);
        }
        public static SkCallBackFun getInstance()
        {
            if (instance == null) instance = new SkCallBackFun();
            return instance;
        }
    }

    public class SkWaitForAction : ActionBase
    {
        public ActionBase action { get; set; } = null;
        public static SkWaitForAction instance = null;
        public static SkWaitForAction create(AbstractCanDevice nodetem, ActionBase aaction)
        {
            SkWaitForAction acttem = SkWaitForAction.create(aaction);
            acttem.node = nodetem;
            return acttem;
        }
        public static SkWaitForAction create(ActionBase aaction)
        {
            SkWaitForAction acttem = new SkWaitForAction();
            acttem.action = aaction;
            return acttem;
        }
        public override string getName()
        {
            string tem = "等待动作完成";
            return tem;
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
        }
        public override void run(double dt)
        {
            if (action==null||(action != null && action.getIsFinish()))
            isfinish = true;
        }

        public static SkWaitForAction getInstance()
        {
            if (instance == null) instance = new SkWaitForAction();
            return instance;
        }
    }

}
