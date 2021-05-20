using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;
using SKABO.MAI.ErrorSystem;
using SKABO.ResourcesManager;
using SKABO.Common;
using SKABO.BLL.IServices.IGel;
using SKABO.Hardware.RunBJ;
using SKABO.Hardware.Enums;

namespace SKABO.ActionEngine
{
    public class MsgItem
    {
        public string msg { get; set; }
    }
    public class Engine
    {
        public IGelService gelService = IoC.Get<IGelService>();
        public CameraDevice cameraDevice = null;
        public PiercerDevice piercerDevice = null;
        public MachineHandDevice handDevice = null;
        public InjectorDevice injectorDevice = null;
        public GelWarehouseDevice gelwareDevice = null;
        public CentrifugeMrg cenMrg = null;
        public OtherPartDevice opDevice = null;
        public CouveuseMixerDevice couveuseMixer = null;

        public ActionManager action_manager = null;
        public ExperimentLogic experiment_logic = null;
        public ResManager res_manager = null;
        public Thread work_thread = null;
        public static Engine engineinstance = null;
        //public ObservableCollection<MsgItem> msgdata = new ObservableCollection<MsgItem>();
        public List<string> msgdata = new List<string>();
        public bool isexit = false;
        public bool isstop = false;
        public long enginetime = 0;
        public long enginedt = 0;
        public int luastateid = 0;
        public bool isskipdt = true;
        public bool is_gelware_ok = false;
        public int last_gelware_storex = 0;//开门时的位置
        public long last_gelware_door_time = 0;
        public static void Log(string msgtem)
        {

            engineinstance.msgdata.Add(msgtem);
            ErrorSystem.WriteActError(msgtem, true,false);
            //engineinstance.msgdata.Add(new MsgItem() { msg = msgtem });
        }

        public static Engine getInstance()
        {
            if (engineinstance == null)
            {
                engineinstance = new Engine();
            }
            return engineinstance;
        }

        public Engine getEngine()
        {
            return engineinstance;
        }
        public long getSystemMs()
        {
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
        public void InitRes()
        {
            res_manager.Init();
            res_manager.gel_list = gelService.QueryAllGel().ToList();
        }
        public void init()
        {
            action_manager = ActionManager.getInstance();
            res_manager = ResManager.getInstance();
            experiment_logic = ExperimentLogic.getInstance();
            experiment_logic.handDevice = handDevice;
            experiment_logic.piercerDevice = piercerDevice;
            experiment_logic.gelwareDevice = gelwareDevice;
            experiment_logic.injectorDevice = injectorDevice;
            experiment_logic.cenMrg = cenMrg;
            experiment_logic.cameraDevice = cameraDevice;
            experiment_logic.opDevice = opDevice;
            InitRes();
            handDevice.CanComm.SetListenFun(handDevice.Hand.ZMotor.GetErrorAdress(), CanFunCodeEnum.UPLOAD_COIL, ErrorCode);
            gelwareDevice.CanComm.SetListenFun(gelwareDevice.GelWare.DoorCoil.Addr, CanFunCodeEnum.UPLOAD_COIL, GelWareDoor);
            opDevice.CanComm.SetListenFun(opDevice.OP.EmergencyStopCoil.Addr, CanFunCodeEnum.UPLOAD_COIL, StopAll);

            
            isstop = false;
        }
        public void StopAll(int tagerid, byte[] data)
        {
            if (data[5] == 0xff)
            {
                var task = new Task(() =>
                {
                    stop();
                    handDevice.CanComm.StopMotor(handDevice.Hand.XMotor);
                    handDevice.CanComm.StopMotor(handDevice.Hand.YMotor);
                    handDevice.CanComm.StopMotor(handDevice.Hand.ZMotor);
                    injectorDevice.CanComm.StopMotor(injectorDevice.Injector.XMotor);
                    foreach(var ent in injectorDevice.Injector.Entercloses)
                    {
                        injectorDevice.CanComm.StopMotor(ent.YMotor);
                        injectorDevice.CanComm.StopMotor(ent.ZMotor);
                    }
                    ActionManager.getInstance().removeAllActions();
                    ExperimentLogic.getInstance().DelAllPackage();
                    ErrorSystem.WriteActError("系统急停!请重新开机", true, false, 999999);
                    start();
                });
                task.Start();
            }
        }
        public void GelWareDoor(int tagerid, byte[] data)
        {
            //开门
            if (is_gelware_ok==false&&(getSystemMs()-last_gelware_door_time)>1000)
            {
                is_gelware_ok = true;
                var task = new Task(() =>
                {
                    stop();
                    var t_gelWare = res_manager.SearchGelCard("T_BJ_GelWarehouse", "", "", 0, res_manager.gelwarehouse_list.Count-3);
                    int storex = data[5] == 0xff ? (int)t_gelWare.StoreX : 0;
                    var act_tem = MoveTo.create(gelwareDevice, 5000, storex);
                    act_tem.init();
                    while (true)
                    {
                        long dt = 100;
                        act_tem.run(dt);
                        Thread.Sleep(100);
                        if (act_tem.isfinish) break;
                    }
                    if (data[5] == 0x00) start();
                    is_gelware_ok = false;
                });
                task.Start();
            }
            last_gelware_door_time = getSystemMs();

        }
        public void ErrorCode(int tagerid,byte[] data)
        {
            string[] error_msg = { "电机归零超时", "电机运动超时", "电机运动中停止" };
            int error_index = data[7] % 3;
            int moto_index = handDevice.Hand.ZMotor.GetMotoIndex();
            if(error_index==2&& moto_index== data[5])
            { 
                //初始化电机
                var task = new Task(() =>
                {
                    stop();
                    var act_tem = InitXyz.create(handDevice,30000, false, false, true);
                    act_tem.init();
                    long lasttime = getSystemMs();
                    while (true)
                    {
                        long dt = 100;
                        act_tem.run(dt);
                        Thread.Sleep(100);
                        if (act_tem.isfinish) break;
                    }
                    start();
                });
                task.Start();
            }
            if(error_index!=2)
            ErrorSystem.WriteActError(error_msg[error_index], true, false);
        }
        public void start()
        {
            init();
            if (work_thread == null)
            {
                work_thread = new Thread(WorkThread);
                work_thread.Start();

            }
        }
        public void stop()
        {
            isstop = true;
        }
        public void exit()
        {
            isexit = true;
        }
        public void WorkThread()
        {
            try
            {
                long lasttime = getSystemMs();
                while (true)
                {
                    if (isstop==false)
                    {
                        lock (ResManager.ui_lockObj)
                        {
                            enginedt = getSystemMs() - lasttime;
                            lasttime = getSystemMs();
                            if (isskipdt)
                            {
                                enginedt = 0;
                                isskipdt = false;
                            }
                            enginetime += enginedt;
                            action_manager.runLoop(enginetime);
                            experiment_logic.runLoop(enginetime);
                        }
                    }
                    if (isexit) break;
                    //时间戳最快10毫秒
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Engine.Log(ex.ToString());
                Engine.Log("engine stop enter ok for restart");
                work_thread = null;
            }

        }
    }
}