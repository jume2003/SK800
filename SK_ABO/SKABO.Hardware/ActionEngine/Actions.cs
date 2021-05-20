using SKABO.ActionGeneraterEngine;
using SKABO.Common.Enums;
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
        public int sort_index { get; set; } = 0;
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
        public bool is_no_card { get; set; } = false;//卡仓无卡
        public string errmsg = "";
        public ActionBase father = null;
        public skcallbackfun retryfun = null;
        public skcallbackfun destroyfun = null;
        public skcallbackfun successfun = null;
        public ExperimentPackage exp_pack = null;
        public ActionBase GetFather()
        {
            return father;
        }
        public ActionBase GetRoot()
        {
            if (father == null)return this;
            return father.GetRoot();
        }
        public bool CountTime(double dt)
        {
            sumdt += dt;
            if (getIsFinish()==false&&(sumdt >= time || istimeout))
            {
                errmsg += this.GetType().ToString();
                istimeout = true;
                //ErrorSystem.WriteActError("错误:" + getDivName()+"动作:"+getName()+"失败", true, false);
                return true;
            }
            return false;
        }
        public bool TimeOutMsg()
        {
            if(istimeout)
            {
                var ret = ErrorSystem.WriteActError(getDivName()+"错误:\n" +  getName() + "失败!");
                if(ret)
                {
                    sumdt = 0;
                    istimeout = false;
                    return true;
                }
                else
                {
                    isdelete = true;
                }
            }
            return false;
        }
        public void Destroy()
        {
            if (this is Sequence seq)
            {
                foreach (var act in seq.actionlist)
                {
                    act.Destroy();
                }
            }
            else if (this is Spawn spa)
            {
                foreach (var act in spa.actionlist)
                {
                    act.Destroy();
                }
            }
            if (destroyfun != null && getIsFinish() == false)
                destroyfun(this);
            if (successfun != null && getIsFinish())
                successfun(this);
        }
        public bool GetIsNoCard()
        {
            if (this is Sequence seq)
            {
                foreach (var act in seq.actionlist)
                {
                    if (act.is_no_card) return true;
                }
            }
            else if (this is Spawn spa)
            {
                foreach (var act in spa.actionlist)
                {
                    if (act.is_no_card) return true;
                }
            }
            return is_no_card;
        }
        public ActionBase()
        {
            id = id_count++;
            sort_index = id;
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
        public virtual void setsort(int index)
        {
            sort_index = index;
        }
        public virtual void start()
        {
            isstop = false;
            istimeout = false;
            sumdt = 0;
            if (this is Sequence seq)
            {
                foreach (var act in seq.actionlist)
                {
                    act.start();
                }
            }
            else if (this is Spawn spa)
            {
                foreach (var act in spa.actionlist)
                {
                    act.start();
                }
            }
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
            deviname = deviname.Replace("Pie", "破孔器");
            deviname = deviname.Replace("Mac", "机械手");
            deviname = deviname.Replace("Inj", "加样器");
            deviname = deviname.Replace("Gel", "卡仓");
            deviname = deviname.Replace("Cen", "离心机");
            return deviname;
        }
        public bool getIsFinish()
        {
            return isfinish && istimeout == false&& isdelete==false;
        }
        public virtual bool getIsDelete()
        {
            return isdelete;
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


        public int getAllActCount(AbstractCanDevice nodetem)
        {
            List<ActionBase> action_list = new List<ActionBase>();
            getAllAct(nodetem, ref action_list);
            return action_list.Count;
        }

        public void getAllAct(AbstractCanDevice nodetem, ref List<ActionBase> action_list)
        {
            bool is_cok = (this is Sequence == false) && (this is Spawn == false) && (this is Repeat == false) && (this is SKSleep == false);
            if (is_cok && this.isfinish == false && node == nodetem)
            {
                action_list.Add(this);
            }
            if (this is Sequence seq)
            {
                foreach (var act in seq.actionlist)
                {
                    act.getAllAct(nodetem, ref action_list);
                }
            }
            else if (this is Spawn spa)
            {
                foreach (var act in spa.actionlist)
                {
                    act.getAllAct(nodetem, ref action_list);
                }
            }
            else if (this is Repeat rep)
            {
                rep.repeat_action.getAllAct(nodetem, ref action_list);
            }
        }
        //得到第一个跑的
        public ActionBase getFristRunAct(int index,ref int count)
        {
            if (this is Sequence seq)
            {
                foreach (var act in seq.actionlist)
                {
                    var act_tem = act.getFristRunAct(index,ref count);
                    if (act_tem != null) return act_tem;
                }
            }
            else if (this is Spawn spa)
            {
                foreach (var act in spa.actionlist)
                {
                    var act_tem = act.getFristRunAct(index, ref count);
                    if (act_tem != null) return act_tem;
                }
            }
            if(count== index) return this;
            count++;
            return null;
        }
    }
    public class SwHand : ActionBase
    {
        public bool swdone { get; set; } = false;
        public bool is_open { get; set; } = false;
        public bool last_open { get; set; } = false;
        public int restimes = 0;
        public ActionDevice device { get; set; } = null;
        public static SwHand instance = null;
        public static SwHand create(MachineHandDevice nodetem, double ttime, bool iis_open)
        {
            SwHand acttem = create(ttime, iis_open);
            acttem.node = nodetem;
            return acttem;
        }
        public static SwHand create(double ttime, bool iis_open)
        {
            SwHand acttem = new SwHand();
            acttem.is_open = iis_open;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            string msg = is_open ? "打开抓手" : "关闭抓手";
            return getDivName() + msg;
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            device = new ActionDevice(node);
        }
        public override void run(double dt)
        {
            bool resulsw = true;
            CountTime(dt);
            device.node = node;
            switch (step)
            {
                case 0:
                    last_open = ((MachineHandDevice)node).Hand.IsOpen;
                    resulsw = device.SwitchHand(is_open);
                    if(resulsw) step++;
                    break;
                case 1:
                    if (swdone == false) swdone = device.DoneSwitchHand();
                    isfinish = swdone|| last_open== is_open;
                    if (isfinish) { istimeout = false; }
                    if (istimeout && restimes < 3) {
                        var hand_device = (MachineHandDevice)node;
                        hand_device.InitHandTongs(true);
                        restimes++;
                        istimeout = false;
                        sumdt = 0;
                        step = 0;
                    }
                    break;
            }
        }
        public static SwHand getInstance()
        {
            if (instance == null) instance = new SwHand();
            return instance;
        }
    }

    public class MoveTo : ActionBase
    {
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int z { get; set; } = 0;
        public int old_x = 0;
        public int old_y = 0;
        public int old_z  = 0;
        public int beg_x = 0;
        public int beg_y = 0;
        public int beg_z = 0;
        public int restimes = 0;
        public bool isconsx { get; set; } = false;
        public bool xdone { get; set; } = false;
        public bool ydone { get; set; } = false;
        public bool zdone { get; set; } = false;
        public int speed { get; set; } = 0;
        public double wait_time { get; set; } = 0;
        //限制处理
        public int con_hand_rx = 0;
        public int con_hand_ry = 0;
        public int con_hand_rz = 0;
        public ActionBase con_act = null;
        public ActionDevice device { get; set; } = null;
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
            return getDivName() + "移动"+ movexyz+ (isconsx? "(限制等待)" : "");
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            restimes = 0;
            xdone = (x < 0);
            ydone = (y < 0);
            zdone = (z < 0);
            device = new ActionDevice(node);
        }
        public override void run(double dt)
        {
            bool resultx = true;
            bool resulcon = true;
            bool resulty = true;
            bool resultz = true;
            CountTime(dt);
            wait_time += dt;
            device.node = node;
            switch (step)
            {
                case 0:
                    if (x >= 0) resultx = device.GetRealX(ref old_x);
                    if (y >= 0) resulty = device.GetRealY(ref old_y);
                    if (z >= 0) resultz = device.GetRealZ(ref old_z);
                    if (resultx && resulty && resultz)
                    {
                        istimeout = false;
                        sumdt = 0;
                        beg_x = old_x;
                        beg_y = old_y;
                        beg_z = old_z;
                        step++;
                    }
                    break;
                case 1:
                    if (x >= 0) resulcon = device.MoveXConstrains(x,dt);
                    if (x >= 0) resultx = resulcon && device.MoveX((int)x,speed);
                    if (y >= 0) resulty = resultx&&device.MoveY((int)y,speed);
                    if (z >= 0) resultz = resulty&&device.MoveZ((int)z, speed);
                    if (resulcon==false){istimeout = false;sumdt = 0;}
                    isconsx = !resulcon;
                    if (node is MachineHandDevice && device.IsAllInConSpace() && x >= 0)
                    {
                        istimeout = false;
                        sumdt = 0;
                        step = 3;
                    }else if (resultx && resulty && resultz) 
                    {
                        istimeout = false;
                        sumdt = 0;
                        restimes++;
                        step++;
                    }
                    break;
                case 2:
                    if(xdone==false) xdone = device.DoneX();
                    if(ydone==false) ydone = device.DoneY();
                    if(zdone==false) zdone = device.DoneZ();
                    isfinish = xdone && ydone && zdone;
                    if (isfinish){istimeout = false;}
                    if (isfinish == false&&(istimeout|| wait_time >1000)&& restimes<3)
                    {
                        wait_time = 0;
                        if (istimeout)
                        {
                            int cur_x = 0;
                            int cur_y = 0;
                            int cur_z = 0;
                            if (x >= 0) resultx = device.GetRealX(ref cur_x);
                            if (y >= 0) resulty = device.GetRealY(ref cur_y);
                            if (z >= 0) resultz = device.GetRealZ(ref cur_z);
                            if (resultx && resulty && resultz)
                            {
                                if (old_x != cur_x || old_y != cur_y || old_z != cur_z)//还在移动
                                {
                                    step = 2;
                                }
                                else
                                {
                                    step = 1;
                                    resultx = xdone || device.ReSetStartX();
                                    resulty = ydone || device.ReSetStartY();
                                    resultz = zdone || device.ReSetStartZ();
                                }
                                old_x = cur_x;
                                old_y = cur_y;
                                old_z = cur_z;
                                istimeout = false;
                                sumdt = 0;
                            }
                        }
                        else if (xdone && ydone&& zdone)//移动完成
                        {
                            isfinish = true;
                            istimeout = false;
                        }
                    }
                    break;
                //限制解决
                case 3:
                    if(device.GetRealX(ref con_hand_rx)&&device.GetRealY(ref con_hand_ry)&&device.GetRealZ(ref con_hand_rz))
                    {
                        int back_x = con_hand_rx - 2000;
                        if (back_x < 0) back_x = 0;
                        con_act = Sequence.create(
                           MoveTo.create(node, 3000, -1, -1, 0),
                           MoveTo.create(node, 3000, back_x, -1, -1),
                           MoveTo.create(node, 3000, con_hand_rx, con_hand_ry,-1),
                           MoveTo.create(node, 3000, -1, -1, con_hand_rz));
                        step++;
                    }
                    break;
                case 4:
                    con_act.run(dt);
                    istimeout = con_act.istimeout;
                    if (con_act.getIsFinish()) step = 0;
                    break;
            }
            if(TimeOutMsg())
            {
                init();
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
        public int old_x  = 0;
        public int[] old_y  = new int[4];
        public int[] old_z = new int[4];
        public int cur_x = 0;
        public int[] cur_y = new int[4];
        public int[] cur_z = new int[4];
        public bool xdone { get; set; } = false;
        public bool ydone { get; set; } = false;
        public bool zdone { get; set; } = false;
        public bool isconsx { get; set; } = false;
        public double wait_time { get; set; } = 0;
        public int speed = 0;
        public int restimes = 0;
        public bool is_abs = false;
        public ActionDevice device = null;
        public Enterclose[] injects { get; set; } = null;
        public static InjectMoveTo instance = null;
        public bool is_init_step = true;
        public bool resulcon = false;
        public bool resultx = false;
        public bool resulty = false;
        public bool resultz = false;

        public static InjectMoveTo create(AbstractCanDevice nodetem, double ttime, Enterclose[] iinjects,int xx,int []yy, int[] zz, int sspeed = 0)
        {
            InjectMoveTo acttem = InjectMoveTo.create(ttime, iinjects,xx, yy,zz, sspeed);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectMoveTo create(double ttime, Enterclose[] iinjects, int xx, int[] yy, int[] zz, int sspeed = 0)
        {
            iinjects = iinjects.Where(item => item.InjEnable).ToArray();
            InjectMoveTo acttem = new InjectMoveTo();
            bool is_nomove = xx == -1;
            for (int i = 0; i < 4; i++)
            {
                acttem.y[i] = yy[i];
                acttem.z[i] = zz[i];
            }
            for (int i = 0; i < 4; i++)
            {
                if (is_nomove) is_nomove = yy[i] == -1&&zz[i] == -1;
                else break;
            }
            acttem.x = xx;
            acttem.time = ttime;
            acttem.speed = sspeed;
            acttem.injects = iinjects;
            if(zz[0]==0)
            {
                acttem.id = acttem.id;
            }
            return is_nomove?null:acttem;
        }
        public override string getName()
        {
            return getDivName() + "移动"+(isconsx?"(限制等待)":"");
        }
        public override void init()
        {
            isinit = true;
            if(is_init_step)step = 0;
            sumdt = 0;
            restimes = 0;
            xdone = x<0;
            ydone = y[0] < 0 && y[1] < 0 && y[2] < 0 && y[3] < 0;
            zdone = z[0] < 0 && z[1] < 0 && z[2] < 0 && z[3] < 0;
            resulcon = false;
            resultx = false;
            resulty = false;
            resultz = false;
            device = new ActionDevice(node);
        }
        public override void run(double dt)
        {
            CountTime(dt);
            wait_time += dt;
            device.node = node;
            var inject_device = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    resultx = device.GetRealX(ref cur_x);
                    resulty = device.GetRealY(injects, ref cur_y);
                    resultz = device.GetRealZ(injects, ref cur_z);
                    if (resultx && resulty && resultz)
                    {
                        istimeout = false;
                        resulcon = false;
                        resultx = false;
                        resulty = false;
                        resultz = false;
                        sumdt = 0;
                        old_x = cur_x;
                        IMask.Copy(cur_y, old_y);
                        IMask.Copy(cur_z, old_z);
                        step++;
                    }
                    break;
                case 1:
                    resulcon = !resulcon ? (xdone || device.MoveXConstrains(x, dt)) : true;
                    resultx = !resultx ? (xdone || resulcon && device.MoveX(x)) : true;
                    resulty = !resulty ? (ydone || resultx && device.MoveY(injects, speed, is_abs, y)) : true;
                    resultz = !resultz ? (zdone || resulty && device.MoveZ(injects, speed, cur_z, z)) : true;
                    if (resulcon == false) { istimeout = false; sumdt = 0; }
                    isconsx = !resulcon;
                    if (resultx && resulty && resultz)
                    {
                        istimeout = false;
                        sumdt = 0;
                        restimes++;
                        step++;
                    }
                    else
                    {
                        step = 0;
                    }
                    break;
                case 2:
                    if (xdone == false) xdone = device.DoneX();
                    if (ydone == false) ydone = device.DoneY(injects, injects.Count()-1);
                    if (zdone == false) zdone = device.DoneZ(injects, speed);
                    isfinish = xdone && ydone && zdone;
                    if (isfinish)istimeout = false;
                    break;
            }
            if (istimeout)
            {
                bool[] done_y = new bool[4];
                resultx = device.GetRealX(ref cur_x);
                resulty = device.GetRealY(injects, ref cur_y);
                resultz = device.GetRealZ(injects, ref cur_z);
                if (resultx && resulty && resultz)
                {
                    if (old_x != cur_x || IMask.IsEquate(old_y, cur_y) == false || IMask.IsEquate(old_z, cur_z) == false)//还在移动
                    {
                        istimeout = false;
                        sumdt = 0;
                    }
                    old_x = cur_x;
                    IMask.Copy(cur_y, old_y);
                    IMask.Copy(cur_z, old_z);
                }
            }
            if (TimeOutMsg())
            {
                init();
            }
        }
        public static InjectMoveTo getInstance()
        {
            if (instance == null) instance = new InjectMoveTo();
            return instance;
        }
    }

    public class PressureTest : ActionBase
    {
        public ActionDevice device = null;
        public Enterclose[] injects { get; set; } = null;
        public int[] pressure { get; set; } = IMask.Gen(0);
        public int[] movez { get; set; } = IMask.Gen(0);
        public int[] movezup { get; set; } = IMask.Gen(0);
        public int speed { get; set; } = 0;
        public int lenght { get; set; } = 0;
        public double pressuretime { get; set; } = 0;
        public static PressureTest instance = null;
        public static PressureTest create(AbstractCanDevice nodetem, double ttime, Enterclose[] iinjects,int[] mmovez,int sspeed,int llenght)
        {
            PressureTest acttem = PressureTest.create(ttime, iinjects, mmovez, sspeed, llenght);
            acttem.node = nodetem;
            return acttem;
        }
        public static PressureTest create(double ttime, Enterclose[] iinjects, int[] mmovez, int sspeed, int llenght)
        {
            iinjects = iinjects.Where(item => item.InjEnable).ToArray();
            PressureTest acttem = new PressureTest();
            for (int i = 0; i < 4; i++)
            {
                acttem.movez[i] = mmovez[i];
                acttem.movezup[i] = mmovez[i]- llenght;
            }
            acttem.speed = sspeed;
            acttem.time = ttime;
            acttem.injects = iinjects;
            acttem.lenght = llenght;
            return acttem;
        }
        public override string getName()
        {
            return getDivName() + "探压力";
        }
        public override void init()
        {
            isinit = true;
            sumdt = 0;
            device = new ActionDevice(node);
        }
        public override void run(double dt)
        {
            CountTime(dt);
            device.node = node;
            var inject_device = (InjectorDevice)node;
            bool resultpump = true;
            switch (step)
            {
                case 0:
                    resultpump = device.MovePump(injects, speed, movezup);
                    if (resultpump) step++;
                    break;
                case 1:
                    pressuretime += dt;
                    if (pressuretime > 50)
                    {
                        var pressure_tem = IMask.Gen(0);
                        device.GetPumpPressure(injects, ref pressure_tem);
                        for (int i = 0; i < 4; i++)
                        {
                            if (pressure_tem[i]< pressure[i]|| pressure[i]==0)
                            pressure[i] = pressure_tem[i];
                            Console.WriteLine("压力 {0}", pressure_tem[i]);
                        }
                    }
                    if (device.DonePump(injects)) step++;
                    break;
                case 2:
                    resultpump = device.MovePump(injects, 0, movez);
                    if (resultpump) step++;
                    break;
                case 3:
                    if (device.DonePump(injects)) step++;
                    break;
                case 4:
                    isfinish = true;
                    break;
            }
            if (TimeOutMsg())
            {
                init();
            }
        }
        public static PressureTest getInstance()
        {
            if (instance == null) instance = new PressureTest();
            return instance;
        }
    }


    public class InjectAbsorb : ActionBase
    {
        public bool pumpdone { get; set; } = false;
        public bool pressuredone { get; set; } = false;
        public double pressuretime { get; set; } = 0;
        public int[] absorbs { get; set; } = IMask.Gen(0);
        public int speed { get; set; } = 0;
        public int[] movez { get; set; } = IMask.Gen(0);
        public int[] cur_z = new int[4];
        public int[] old_z = new int[4];
        public int restimes = 0;
        public bool resultz = false;
        public int[] pressure = IMask.Gen(0);
        public double wait_time { get; set; } = 0;
        public PressureTest pressuretest_act { get; set; } = null;
        public Sequence retset_act = null;
        public Enterclose[] injects { get; set; } = null;
        public static InjectAbsorb instance = null;

        public Sequence CrateReSetAct()
        {
            var resmanager = ResManager.getInstance();
            var sequ_act = Sequence.create(node);
            List<ActionPoint> unload_seat = new List<ActionPoint>();
            var inject_unload = resmanager.unload_list;
            if (inject_unload.Count() == 1)
            {
                var unloader = inject_unload[0];
                for (int i = 0; i < 4; i++)
                {
                    ActionPoint unload_point = new ActionPoint(-1, -1, -1);
                    unload_point = new ActionPoint((int)unloader.X, (int)unloader.Y + i * (int)unloader.FZ, (int)unloader.Z, TestStepEnum.PutTip);
                    unload_point.puttip_x = (int)unloader.FirstX;
                    unload_seat.Add(unload_point);
                }
                sequ_act.AddAction(InjectMoveActs.create(5001, unload_seat.ToArray(), true));
            }
            //生成装帧动作
            var tip_seat = resmanager.GetFreeTipActPoint(resmanager.last_zt_count, 2, "");
            while (tip_seat == null)
            {
                ErrorSystem.WriteActError("吸管已用完!请配置好吸管后点击确定。", true, false, 999);
                tip_seat = resmanager.GetFreeTipActPoint(resmanager.last_zt_count, 2, "");
            }
            var sequ_taketip = InjectMoveActs.create(5001, tip_seat, false);

            sequ_act.AddAction(sequ_taketip);


            return sequ_act;
        }

        public static InjectAbsorb create(AbstractCanDevice nodetem, double ttime, Enterclose[] iinjects,int sspeed,int[] aabsorbs,int[] ppressure)
        {
            InjectAbsorb acttem = InjectAbsorb.create(ttime, iinjects, sspeed, aabsorbs, ppressure);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectAbsorb create(double ttime, Enterclose[] iinjects, int sspeed,int[] aabsorbs, int[] ppressure)
        {
            iinjects = iinjects.Where(item => item.InjEnable).ToArray();
            InjectAbsorb acttem = new InjectAbsorb();
            acttem.injects = iinjects;
            acttem.absorbs = (int[])aabsorbs.Clone();
            acttem.pressure = (int[])ppressure.Clone();
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
            pressuredone = IMask.IsEquate(pressure, IMask.Gen(0));
        }
        public override void run(double dt)
        {
            bool resultpump = true;
            CountTime(dt);
            wait_time += dt;
            var device = new ActionDevice(node);
            var inject_device = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    device.ReSetStartPump(injects);
                    resultz = device.GetRealPump(injects, ref cur_z);
                    if (resultz)
                    {
                        istimeout = false;
                        resultz = false;
                        sumdt = 0;
                        IMask.Copy(cur_z, old_z);
                        step++;
                    }
                    break;
                case 1:
                    //容量检测
                    bool is_ok = true;
                    int index = 0;
                    foreach (var inject in injects)
                    {
                        int curz = old_z[inject.Index];
                        movez[inject.Index] = curz + absorbs[inject.Index];
                        bool is_enough = movez[inject.Index] <= (int)inject.PumpMotor.Maximum.SetValue && movez[inject.Index] >= 0;
                        if (is_enough == false)
                        {
                            index = inject.Index;
                            is_ok = false;
                            break;
                        }
                    }
                    if (is_ok) step++;
                    else
                    {
                        var ret = ErrorSystem.WriteActError("液泵:" + index + "容量不足!");
                        if(ret)
                        {
                            init();
                        }
                        else
                        {
                            istimeout = true;
                            isdelete = true;
                            errmsg = "液泵:" + index + "容量不足!";
                            return;
                        }
                    }
                    if (injects.Count() == 0)
                    {
                        ErrorSystem.WriteActError("没有可用加样器",true,false);
                        istimeout = true;
                        isdelete = true;
                        errmsg = "没有可用加样器";
                        return;
                    }
                    break;
                case 2:
                    resultpump = device.MovePump(injects, speed, movez);
                    if (resultpump)
                    {
                        istimeout = false;
                        sumdt = 0;
                        restimes++;
                        step++;
                    }
                    break;
                case 3:
                    pumpdone = device.DonePump(injects);
                    if (pumpdone)
                    {
                        if(IMask.IsEquate(pressure, IMask.Gen(0)))
                        {
                            step = 7;
                        }
                        else
                        {
                            int[] pupz = IMask.Gen(0);
                            device.GetRealPump(injects.ToArray(), ref pupz);
                            pressuretest_act = PressureTest.create(node, 10000, injects.ToArray(), pupz, 70, 100);
                            pressuretest_act.init();
                            step++;
                        }
                    }
                    break;
                case 4:
                    //压力收集
                    pressuretest_act.run(dt);
                    if (pressuretest_act.getIsFinish())
                    {
                        pressuredone = IMask.IsLessThan(pressure, pressuretest_act.pressure);
                        if(pressuredone)
                        {
                            step = 7;
                        }
                        else if (pressuredone == false)
                        {
                            var ret = ErrorSystem.WriteActError("压力值不正确", true, true);
                            if (ret)
                            {
                                step = 5;
                                return;
                            }
                            else
                            {
                                istimeout = true;
                                isdelete = true;
                                errmsg = "压力值不正确";
                                return;
                            }
                        }
                    }
                    break;
                case 5:
                    retset_act = CrateReSetAct();
                    if (Engine.getInstance().injectorDevice.GetReSetAct(ref retset_act))
                        step = 6;
                    break;
                //压力不够脱针重来
                case 6:
                    retset_act.run(dt);
                    istimeout = retset_act.istimeout;
                    isdelete = retset_act.isdelete;
                    if(retset_act.getIsFinish())
                    {
                        step = 0;
                        init();
                    }
                    break;
                case 7:
                    isfinish = true;
                    break;
            }
            if (istimeout)
            {
                resultz = device.GetRealPump(injects, ref cur_z);
                if (resultz)
                {
                    if (IMask.IsEquate(old_z, cur_z) == false)//还在移动
                    {
                        istimeout = false;
                        sumdt = 0;
                    }
                    IMask.Copy(cur_z, old_z);
                }
            }
            if (TimeOutMsg())
            {
                init();
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
        public int[] cur_z = new int[4];
        public int[] old_z = new int[4];
        public int restimes = 0;
        public bool resultz = false;
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
            iinjects = iinjects.Where(item => item.InjEnable).ToArray();
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
            resultz = false;
            isinit = true;
            step = 0;
            sumdt = 0;
        }
        public override void run(double dt)
        {
            CountTime(dt);
            var device = new ActionDevice(node);
            var inject_device = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    device.ReSetStartPump(injects);
                    resultz = device.GetRealPump(injects, ref cur_z);
                    if (resultz)
                    {
                        istimeout = false;
                        resultz = false;
                        sumdt = 0;
                        IMask.Copy(cur_z, old_z);
                        step++;
                    }
                    break;
                case 1:
                    resultz = device.MovePump(injects, speed, absorbs);
                    if (resultz)
                    {
                        istimeout = false;
                        sumdt = 0;
                        restimes++;
                        step++;
                    }
                    break;
                case 2:
                    isfinish = device.DonePump(injects);
                    if (isfinish) istimeout = false;
                    break;
            }
            if (istimeout)
            {
                resultz = device.GetRealPump(injects, ref cur_z);
                if (resultz)
                {
                    if (IMask.IsEquate(old_z, cur_z) == false)//还在移动
                    {
                        istimeout = false;
                        sumdt = 0;
                    }
                    IMask.Copy(cur_z, old_z);
                }
            }
            if (TimeOutMsg())
            {
                init();
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
        public int []by { get; set; } = null;//到零点后往回跑
        public bool xdone { get; set; } = false;
        public bool ydone { get; set; } = false;
        public bool zdone { get; set; } = false;
        public double wait_time { get; set; } = 0;
        public Enterclose[] injects { get; set; } = null;
        public List<InjectMoveTo> injectsby_list = new List<InjectMoveTo>();
        public ActionBase injbyact = null;
        public static InitXyz instance = null;
        public static InitXyz create(AbstractCanDevice nodetem, double ttime = 0, bool xx = true, bool yy = true, bool zz = true)
        {
            InitXyz acttem = InitXyz.create(ttime, xx, yy, zz);
            acttem.node = nodetem;
            return acttem;
        }
        public static InitXyz create(AbstractCanDevice nodetem, double ttime = 0, Enterclose[] iinjects = null, bool xx = true, bool yy = true, bool zz = true,int[] bby=null)
        {
            InitXyz acttem = InitXyz.create(ttime, iinjects, xx, yy, zz, bby);
            acttem.node = nodetem;
            return acttem;
        }
        public static InitXyz create(double ttime = 0, Enterclose[] iinjects=null, bool xx = true, bool yy = true, bool zz = true,int []bby = null)
        {
            iinjects = iinjects.Where(item => item.InjEnable).ToArray();
            InitXyz acttem = InitXyz.create(ttime, xx, yy, zz);
            acttem.injects = iinjects;
            acttem.by = bby;
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
            string[] str = { "X", "Y", "Z" };
            bool[] initxyz = { x, y, z };
            string name = "";
            for (int i = 0; i < 3; i++)
                if (initxyz[i]) name += str[i];
            return getDivName() + "初始化"+ name;
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            xdone = !x;
            ydone = !y;
            zdone = !z;
            if(by!=null)
            {
                foreach (var inject in injects)
                {
                    List<Enterclose> ents = new List<Enterclose>();
                    ents.Add(inject);
                    var act = InjectMoveTo.create(node, time, ents.ToArray(), -1, by, IMask.Gen(-1), 10);
                    act.is_abs = true;
                    act.init();
                    injectsby_list.Add(act);
                }
            }
        }
        public override void run(double dt)
        {
            bool resultx = true;
            bool resulty = true;
            bool resultz = true;
            CountTime(dt);
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
                        if (zdone == false) zdone = device.DoneZ(injects,-1);
                    }
                    if (xdone && ydone && zdone) step = 3;
                    break;
                case 2:
                    wait_time += dt;
                    if (wait_time > 200)
                    {
                        wait_time = 0;
                        step++;
                    }
                    break;
                case 3:
                    if (injects == null||by==null)
                    {
                        isfinish = true;

                    }
                    else
                    {
                        if(injectsby_list.Count != 0)
                        {
                           
                            var injectby = injectsby_list[injectsby_list.Count - 1];
                            injectby.run(dt);
                            istimeout = injectby.istimeout;
                            isdelete = injectby.isdelete;
                            if (injectby.getIsFinish())
                            {
                                injectsby_list.Remove(injectby);
                                step = 2;
                            }
                        }else if(injectsby_list.Count == 0)
                        {
                            Thread.Sleep(1000);
                            isfinish = device.SetRealY(injects, IMask.Gen(0));
                        }
                    }
                    break;
            }
            if(TimeOutMsg())
            {
                init();
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
        public int speed { get; set; } = 0;
        public static InitInjectPump create(AbstractCanDevice nodetem, double ttime, Enterclose[] iinjects)
        {
            InitInjectPump acttem = InitInjectPump.create(ttime, iinjects);
            acttem.node = nodetem;
            return acttem;
        }
        public static InitInjectPump create(double ttime,Enterclose[] iinjects)
        {
            iinjects = iinjects.Where(item => item.InjEnable).ToArray();
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
            CountTime(dt);
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
                            speed = ent.PumpMotor.InitSpeed.SetValue;
                        }
                        device.ReSetStartPump(injects);
                        resultx = device.MovePump(injects, speed, absorbs);
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
                        resultx = device.MovePump(injects, speed, absorbs);
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
            if(TimeOutMsg())
            {
                init();
            }
        }
        public static InitInjectPump getInstance()
        {
            if (instance == null) instance = new InitInjectPump();
            return instance;
        }
    }

    public class SKLight : ActionBase
    {
        public bool pdone { get; set; } = false;
        public static SKLight instance = null;
        public int speed { get; set; } = 0;
        public LightEnum light_type;
        public enum LightEnum
        {
            Red = 0,
            Green = 1,
            Blue = 2,
            RedBlink = 3,
        }

        public static SKLight create(AbstractCanDevice nodetem, double ttime, LightEnum lighttype)
        {
            SKLight acttem = SKLight.create(ttime, lighttype);
            acttem.node = nodetem;
            return acttem;
        }
        public static SKLight create(double ttime, LightEnum lighttype)
        {
            SKLight acttem = new SKLight();
            acttem.light_type = lighttype;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return getDivName() + "灯光";
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
            CountTime(dt);
            var device = new ActionDevice(node);
            var op_device = (OtherPartDevice)node;
            switch (step)
            {
                case 0:
                    {
                       if(light_type==LightEnum.Red)
                        {
                            op_device.LedRed();
                        }
                        else if (light_type == LightEnum.Green)
                        {
                            op_device.LedGreen();
                        }
                        else if (light_type == LightEnum.Blue)
                        {
                            op_device.LedBule();
                        }
                        else if (light_type == LightEnum.RedBlink)
                        {
                            op_device.LedRedBlink();
                        }
                        step++;
                    }
                    break;
                case 1:
                    isfinish = true;
                    break;
            }
            if (TimeOutMsg())
            {
                init();
            }
        }
        public static SKLight getInstance()
        {
            if (instance == null) instance = new SKLight();
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
                actions[i].father = acttem;
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
                actions[i].father = acttem;
            }
            return acttem;
        }
        public override string getName()
        {
            return getDivName() + "单任务";
        }
        public override bool getIsDelete()
        {
            if(actionlist.Count!=0)
            return isdelete||actionlist[0].getIsDelete();
            return false;
        }
        public void AddAction(ActionBase action)
        {
            if(action!=null)
            {
                actionlist.Add(action);
                action.father = this;
                if (exp_pack != null)
                action.exp_pack = exp_pack;
                time += action.time;
            }
        }
        public void InsertAction(int index,ActionBase action)
        {
            if (action != null)
            {
                actionlist.Insert(index,action);
                action.father = this;
                time += action.time;
            }
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
                    actions[i][0].father = acttem;
                }
                else
                {
                    ActionBase spawnact = Spawn.create(actions[i]);
                    acttem.actionlist.Add(spawnact);
                    acttem.time += spawnact.time;
                    spawnact.father = acttem;
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
                if (actionlist[0].exp_pack == null && exp_pack != null)
                {
                    actionlist[0].exp_pack = exp_pack;
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
                        var actiontem = actionlist[i];
                        actionlist.Remove(actionlist[i]);
                        actiontem.Destroy();
                        if (actionlist.Count!=0)
                        {
                            if (actionlist[0].node == null && node != null)
                            actionlist[0].node = node;
                            if (actionlist[0].exp_pack == null && exp_pack != null)
                                actionlist[0].exp_pack = exp_pack;
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
                actions[i].father = acttem;
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
                actions[i].father = acttem;
            }
            return acttem;
        }
        public void AddAction(ActionBase action)
        {
            if (action != null)
            {
                actionlist.Add(action);
                action.father = this;
                for (int i = 0; i < actionlist.Count; i++)
                {
                    if (time < actionlist[i].time)
                        time = actionlist[i].time;
                }
            }
        }
        public override string getName()
        {
            return getDivName() + "多任务";
        }
        public override bool getIsDelete()
        {
            foreach(var act in actionlist)
            {
                if(act.getIsDelete())
                return true;
            }
            return false;
        }
        public override void init()
        {
            for (int i = 0; i < actionlist.Count; i++)
            {
                if (actionlist[i].node == null&&node!=null)
                    actionlist[i].node = node;
                if (actionlist[i].exp_pack == null && exp_pack != null)
                    actionlist[i].exp_pack = exp_pack;
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
                    if (actionlist[i].exp_pack == null && exp_pack != null)
                    {
                        actionlist[i].exp_pack = exp_pack;
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
                    {
                        var actiontem = actionlist[i];
                        actionlist.Remove(actionlist[i]);
                        actiontem.Destroy();
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
        public override bool getIsDelete()
        {
            return isdelete;
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
            if (repeat_action.exp_pack == null&& exp_pack!=null)
            {
                repeat_action.exp_pack = exp_pack;
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
