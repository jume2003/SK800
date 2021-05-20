using SKABO.BLL.IServices.IGel;
using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.Common.Exceptions;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Models.GEL;
using SKABO.Common.Models.Judger;
using SKABO.Common.Models.TestStep;
using SKABO.Common.Utils;
using SKABO.Common.Views;
using SKABO.Hardware.Model;
using SKABO.Hardware.RunBJ;
using SKABO.Hardware.Scaner;
using SKABO.Ihardware.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SK_ABO.robot
{
    public class TestRobot
    {
        /// <summary>
        /// 启动下一个线程加样程序
        /// </summary>
        internal static AutoResetEvent StartNextResetEvent { get; set; } = new AutoResetEvent(false);
        internal static AutoResetEvent TakeGelResetEvent = new AutoResetEvent(true);
        private static readonly Object normalQueueLock = new Object();
        private static readonly Object hightQueueLock = new Object();
        private static readonly Object superHightQueueLock = new Object();
        //private static readonly decimal BackVol = 100;//回吸速度
        public static Object HandLock = new object();
        /// <summary>
        /// True 分配了血清，则表示GelSeat中含有样本条码信息
        /// </summary>
        private bool IsFPXQ;
        public static bool GetHandDevice(MachineHandDevice machineHandDevice,int TaskID, HandStatusEnum statusEnum)
        {
            var result = false;
            lock (HandLock)
            {
                if (TaskID == machineHandDevice.TaskId)
                {
                    machineHandDevice.StatusEnum = statusEnum;
                    result = true;
                }
                else
                {
                    if (machineHandDevice.StatusEnum == HandStatusEnum.NoWork)
                    {
                        machineHandDevice.TaskId = TaskID;
                        machineHandDevice.StatusEnum = statusEnum;
                        result = true;
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// 是否能远行测试，主要用于结束测试线程，为false时，测试线程无法启动
        /// </summary>
        internal static bool IsEnabled;
        static ConcurrentQueue<TestBag> _NormalQueue;
        /// <summary>
        /// 正常测试样本包队列
        /// </summary>
        static ConcurrentQueue<TestBag> NormalQueue { get
            {
                if (_NormalQueue == null)
                {
                    lock (normalQueueLock)
                    {
                        _NormalQueue = _NormalQueue??new ConcurrentQueue<TestBag>();
                    }
                    
                }
                return _NormalQueue;
            }
        }
        static ConcurrentQueue<TestBag> _HightQueue;
        /// <summary>
        /// 加急测试样本包队列
        /// </summary>
        static ConcurrentQueue<TestBag> HightQueue
        {
            get
            {
                if (_HightQueue == null)
                {
                    lock (hightQueueLock)
                    {
                        _HightQueue = _HightQueue?? new ConcurrentQueue<TestBag>();
                    }

                }
                return _HightQueue;
            }
        }
        static ConcurrentQueue<TestBag> _SuperHightQueue;
        /// <summary>
        /// 特急测试样本包队列
        /// </summary>
        static ConcurrentQueue<TestBag> SuperHightQueue
        {
            get
            {
                if (_SuperHightQueue == null)
                {
                    lock (superHightQueueLock)
                    {
                        _SuperHightQueue = _SuperHightQueue ?? new ConcurrentQueue<TestBag>();
                    }
                }
                return _SuperHightQueue;
            }
        }
        public static void ClearTesgBag() {
            ClearQueue(NormalQueue);
            ClearQueue(HightQueue);
            ClearQueue(SuperHightQueue);

        }
        private static void ClearQueue(ConcurrentQueue<TestBag> queue)
        {
            while (!queue.IsEmpty)
            {
                TestBag bag = null;
                queue.TryDequeue(out bag);
                bag = null;
            }
        }
        /// <summary>
        /// 增加样本测试包
        /// </summary>
        /// <param name="testBag"></param>
        public static void AddTestBag(IList<TestBag> testBags)
        {
            if (testBags == null) return;
            foreach (var testBag in testBags)
            {
                if (testBag.TestLevel == TestLevelEnum.Normal)
                {
                    NormalQueue.Enqueue(testBag);
                }
                else if (testBag.TestLevel == TestLevelEnum.Hight)
                {
                    HightQueue.Enqueue(testBag);
                }
                else if (testBag.TestLevel == TestLevelEnum.SuperHight)
                {
                    SuperHightQueue.Enqueue(testBag);
                }
            }
        }
        
        private static Object QueueLock = new object();
        public static TestBag Take()
        {
            TestBag result = null;

            lock (QueueLock)
            {
                bool res = false;
                if (!res && !SuperHightQueue.IsEmpty)
                {
                    res = SuperHightQueue.TryDequeue(out result);
                }
                if (!res && !HightQueue.IsEmpty)
                {
                    res = HightQueue.TryDequeue(out result);
                }
                if (!res && !NormalQueue.IsEmpty)
                {
                    res = NormalQueue.TryDequeue(out result);
                }
            }
            return result;
        }
        /// <summary>
        /// 终止所有测试项目
        /// </summary>
        public static void StopTest()
        {
            TestRobot.IsEnabled = false;
            ResetVBJ<T_BJ_GelSeat>();
            ResetVBJ<T_BJ_Centrifuge>();
        }
        public static void ResetVBJ<T>()
        {
            String key = typeof(T).Name;
            if (!Constants.BJDict.ContainsKey(key))
            {
                return;
            }
            var vbjs = Constants.BJDict[key];
            bool setPeiPing = false;
            foreach (var vb in vbjs)
            {
                if(vb is T_BJ_AgentiaWarehouse || vb is T_BJ_Camera || vb is T_BJ_DeepPlate
                    ||  vb is T_BJ_Scaner || vb is T_BJ_Tip 
                    || vb is T_BJ_Unload)
                {
                    continue;
                }
                else if(vb is T_BJ_Centrifuge cent)
                {
                    Constants.MainWindow.Dispatcher.Invoke(() => { cent.FillAll(null); });
                }
                else if(vb is T_BJ_SampleRack sr)
                {
                    Constants.MainWindow.Dispatcher.Invoke(() => { sr.FillAll(null); });
                }
                else if (vb is T_BJ_GelSeat gelSeat)
                {
                    Constants.MainWindow.Dispatcher.Invoke(() => { gelSeat.FillAll(null); });

                    if (!setPeiPing && (gelSeat.Purpose == (int)GelSeatPurposeEnum.配平与节约位))
                    {
                        setPeiPing = true;
                        Constants.MainWindow.Dispatcher.Invoke(() => { gelSeat.SetValue(0, 0, true); });
                        Constants.MainWindow.Dispatcher.Invoke(() => { gelSeat.SetValue(1, 0, true); });
                    }
                }
            }
        }
        public TestRobot(byte TaksID)
        {
            this.TaskID = TaksID;
            this.bjDic = Constants.BJDict;
            RunSemaphore = new Semaphore(0, 1);
            gelSeatList = this.bjDic[typeof(T_BJ_GelSeat).Name];

            this.injectorDevice = IoC.Get<InjectorDevice>();
            this.centrifugeDevice = IoC.Get<CentrifugeDevice>();
            this.machineHandDevice = IoC.Get<MachineHandDevice>();
            this.gelWarehouseDevice = IoC.Get<GelWarehouseDevice>();
            this.otherPartDevice = IoC.Get<OtherPartDevice>();
            this.piercerDevice = IoC.Get<PiercerDevice>();
            this.couveuseMixerDevice = IoC.Get<CouveuseMixerDevice>();
            this.scanDevice = IoC.Get<ScanDevice>();
            this.cameraDevice = IoC.Get<CameraDevice>();
        }
        /// <summary>
        /// 需要的新卡数据
        /// </summary>
        public byte NeedNewCardCount { get; set; }
        /// <summary>
        /// 需要的节约卡数量
        /// </summary>
        public byte NeedUsedCardCount { get; set; }

        public byte TaskID { get; set; }
        public delegate void ErrorHandler(String msg, TestRobot robot);
        public event ErrorHandler OnError;
        public Semaphore RunSemaphore { get; set; }
        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPause { get; set; }
        private String _Message;
        public String Message { get { return _Message; } set {
                _Message = value;
                if (!String.IsNullOrEmpty(value))
                {
                    OnError?.Invoke(value, this);
                }
            } }
        private InjectorDevice injectorDevice;
        private CentrifugeDevice centrifugeDevice;
        private MachineHandDevice machineHandDevice;
        private GelWarehouseDevice gelWarehouseDevice;
        private OtherPartDevice otherPartDevice;
        private PiercerDevice piercerDevice;
        private CouveuseMixerDevice couveuseMixerDevice;
        private ScanDevice scanDevice;
        private CameraDevice cameraDevice;

        private IDictionary<String,IList<IBJ>> bjDic;
        private IList<IBJ> gelSeatList;

         
    
        public void RunTest()
        {
            while (true)
            {
                if (!IsEnabled)
                {
                    break;
                }
                var bag = Take();
                if (bag == null)
                {
                    Thread.Sleep(500);
                }
                else
                {
                    //取到测试包，正式开始测试
                    try
                    {
                        Testing(bag);
                    }catch(Exception ex)
                    {
                        BackZeroCorrid();
                        Tool.AppLogError($"{bag.GelType.TestName}");
                        Tool.AppLogError(ex);
                        SetAlarm(ex.Message);
                    }
                    
                }
            }
        }
        private void InitData()
        {
            
            //if (injectorDevice.Injector.XMotor.CurrentDistance > 0)
            //{
            //    var ents = injectorDevice.GetValid();
            //    injectorDevice.MoveZ(0,true, false, ents);
            //    injectorDevice.MoveXY(0, 0, 0, ents);
            //}
            //this.NeedNewCardCount = 2;
            //this.NeedUsedCardCount = 0;
           // this.couveuseMixerDevice.StartMixer();
        }
        private void BackZeroCorrid()
        {
            //var ents = injectorDevice.GetValid();
            //var result=machineHandDevice.MoveZ(0);
            //result= result && machineHandDevice.Move(0, 0,false);
            // ents = ents.Where(ent => ent.ZMotor.CurrentDistance > ent.Zero).ToArray();
            //if (ents.Length > 0)
            //{
            //    result = injectorDevice.MoveZ(0,true, false, ents);
            //    result = result && injectorDevice.MoveXY(0, 0, 0, ents);
            //}
            //else
            //{
            //    injectorDevice.MoveXY(0, 0, 0, ents);
            //}
        }
        public bool Testing(TestBag bag)
        {
            return true;
            //IsFPXQ = false;
            ////InitData();
            //TestRobot.StartNextResetEvent.WaitOne();
            //bag.SetStartTime();
            //var result = true;
            //String msg = null;
            //if (bag.GelType == null || bag.GelType.GelSteps==null || bag.GelType.GelSteps.Count==0 || bag.SamplesInfo.Count==0)
            //{
            //    result = false;
            //}
            //else
            //{
            //    byte ToLxjSourceType = 0;//转卡到离心机的卡源类型，0：分配位 1：孵育位
                
            //    for(int StepIndex = 0; StepIndex< bag.GelType.GelSteps.Count; StepIndex++)
            //    {
            //        var step=bag.GelType.GelSteps[StepIndex];
                    
            //        switch (step.StepClass)
            //        {
            //            case TestStepEnum.FPBRXQ:
            //                {
            //                    FPBRXQStepParameter action= step.StepParamters.ToInstance<FPBRXQStepParameter>();
            //                    action.BackSpeed =  Constants.DefaultBackSpeed ;
            //                    action.BackAbsVol =Constants.DefaultBackVol ;
            //                    result = result &&FPBRXQ(action, bag, out msg);
            //                    IsFPXQ = true;
            //                    break;
            //                }
            //            case TestStepEnum.FPBRXSHXB:
            //                {
            //                    BlockHand(HandStatusEnum.Waiting);
            //                    FPBRXSHXBStepParameter action = step.StepParamters.ToInstance<FPBRXSHXBStepParameter>();
            //                    result = result && FPBRXSHXB_New(action, bag, out msg);
            //                    break;
            //                }
            //            case TestStepEnum.FPSJ:
            //                {
            //                    using (Task<bool> task = new Task<bool>(() =>
            //                           this.couveuseMixerDevice.StopMixer(true)
            //                       ))
            //                    {
            //                        task.Start();
            //                        try
            //                        {
            //                            FPSJStepParameter action = step.StepParamters.ToInstance<FPSJStepParameter>();
            //                            action.BackSpeed =  Constants.DefaultBackSpeed ;
            //                            action.BackAbsVol =  Constants.DefaultBackVol ;
            //                            result = result && FPSJ_STEP_1(action, bag, task, out msg);
            //                                //this.FPSY(action,  bag, task, out msg);
            //                            if (result)
            //                            {
            //                                this.couveuseMixerDevice.StartMixer();
            //                            }
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            msg = ex.Message;
            //                            result = false;
            //                        }
            //                    }
            //                    //action.
            //                    break;
            //                }
            //            case TestStepEnum.FPXXYXQ:
            //                {
            //                    FPBRXQStepParameter action = step.StepParamters.ToInstance<FPBRXQStepParameter>();
            //                    action.BackSpeed = Constants.DefaultBackSpeed ;
            //                    action.BackAbsVol =Constants.DefaultBackVol ;
            //                    result = result && this.FPXXYXQ(action, bag, out msg);
            //                    break;
            //                }
            //            case TestStepEnum.FPXXYXSHXB:
            //                {
            //                    BlockHand(HandStatusEnum.Waiting);
            //                    FPBRXSHXBStepParameter action = step.StepParamters.ToInstance<FPBRXSHXBStepParameter>();
            //                    //action.BackSpeed = action.BackSpeed == 0 ? Constants.DefaultBackSpeed : action.BackSpeed;
            //                    //action.BackAbsVol = action.BackAbsVol == 0 ? Constants.DefaultBackVol : action.BackAbsVol;
            //                    result = result && this.FPXXYXSHXB(action, bag, out msg);
            //                    break;
            //                }
            //            case TestStepEnum.JYJS:
            //                {
            //                    try
            //                    {
            //                        result = result && this.injectorDevice.MoveZ(0,true, false, this.injectorDevice.GetValid());
            //                        result = result && this.injectorDevice.MoveXY(0, 0, 0, this.injectorDevice.GetValid());
            //                        BlockHand(HandStatusEnum.NoWork);
            //                    }
            //                    catch(Exception ex)
            //                    {
            //                        msg = ex.Message;
            //                        result = false;
            //                    }
            //                    break;
            //                }
            //            case TestStepEnum.KaiKongGel:
            //                {
            //                    result = result && KaiKongGel();
            //                    break;
            //                }
            //            case TestStepEnum.LoadGel:
            //                {
            //                    try
            //                    {
            //                        BlockHand(HandStatusEnum.LoadingGEL);
            //                        result = TakeGelToPiercer(bag,out msg);
            //                        BlockHand(HandStatusEnum.NoWork);
            //                    }
            //                    catch(Exception ex)
            //                    {
            //                        msg = ex.Message;
            //                        result = false;
            //                    }
            //                    finally
            //                    {
            //                        TakeGelResetEvent.Set();
            //                    }
            //                    break;
            //                }
            //            case TestStepEnum.LXJDZ:
            //                {
                                
            //                    result = result && machineHandDevice.MoveZ(0m);
            //                    result = result && machineHandDevice.Move(0m, 0m,false);
            //                    BlockHand(HandStatusEnum.NoWork);
            //                    foreach (var c in BJCentrifuges.Where(c => c.RobotNo.HasValue && c.RobotNo.Value == this.TaskID))
            //                    {
            //                        result =result &&this.centrifugeDevice.RunCentrifuge(c.Code);
            //                    }
                                
            //                    break;
            //                }
            //            case TestStepEnum.XJPD:
            //                {
            //                    BlockHand(HandStatusEnum.PD);
            //                    result = result && this.otherPartDevice.CameraLight(true);
            //                    result = result && this.cameraDevice.Open();
            //                    try
            //                    {
            //                        result = result && XJPD(bag);
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        msg = ex.Message;
            //                        result = false;
            //                    }
            //                    finally
            //                    {
            //                        cameraDevice.Close();
            //                        this.otherPartDevice.CameraLight(false);
            //                        var res= this.machineHandDevice.MoveZ(0);
            //                        res = res && this.machineHandDevice.Move(0, 0, false);
            //                    }
            //                    BlockHand(HandStatusEnum.NoWork);
            //                    break;
            //                }
            //            case TestStepEnum.YS:
            //                {
            //                    YSStepParameter action = new YSStepParameter(step.StepParamters);
            //                    Thread.Sleep(action.YsTime);
            //                    break;
            //                }
            //            case TestStepEnum.ZKDCW:
            //                {
            //                    //不知道怎么转常温
            //                    break;
            //                }
            //            case TestStepEnum.ZKDFY:
            //                {
            //                    BlockHand(HandStatusEnum.MoveGEL2FY);
            //                    result = result && MoveGelToFy(out msg);
            //                    ToLxjSourceType = 1;
            //                    break;
            //                }
            //            case TestStepEnum.ZKDLXJ:
            //                {
            //                    //BlockHand(HandStatusEnum.MoveGEL2Cent);
            //                    //移到里面，在有离心机资源的前提下才会去尝试获取HandLock
            //                    result = result && MoveGetToLXJ(ToLxjSourceType,out msg);
            //                    StartNextResetEvent.Set();
            //                    break;
            //                }

            //        }//end switch
            //        if (!result)
            //        {
            //            var info = $"第{StepIndex + 1}步【{step.StepName}】执行失败！\r\n{msg}";
            //            Tool.AppLogError(info);
            //            //BackZeroCorrid();
            //            SetAlarm(info);
            //            break;
            //        }
            //    }
            //}
            //return result;
        }
        private void BlockHand(HandStatusEnum statusEnum)
        {
            var lockHand = GetHandDevice(machineHandDevice,this.TaskID, statusEnum);
            while (!lockHand)
            {
                Thread.Sleep(100);//等待100ms
                lockHand = GetHandDevice(machineHandDevice, this.TaskID, statusEnum);
            }
        }
        public bool XJPD(TestBag bag)
        {
            //T_BJ_Camera BJCamera = Constants.BJDict[typeof(T_BJ_Camera).Name][0] as T_BJ_Camera;
            //T_BJ_WastedSeat BJFeiCard = Constants.BJDict[typeof(T_BJ_WastedSeat).Name].Where(item=>(item as T_BJ_WastedSeat).Purpose==1)
            //    .FirstOrDefault() as T_BJ_WastedSeat;
            //var result = true;
            //var targets = BJCentrifuges.Where(c => c.RobotNo.Value == this.TaskID);
            //foreach(var target in targets)
            //{
            //    result = result && machineHandDevice.OpenDoorForCentrifuge(target, true);
            //    for (byte b = 0; b < 12; b++)
            //    {
            //        if (!result)
            //        {
            //            break;
            //        }
            //        if (target.Values[b, 0] != null)
            //        {
            //            String msg = null;
            //            if (target.Values[b,0] is bool IsPeiPing)
            //            {
            //                var seat = GetGelSeats(GelSeatPurposeEnum.配平与节约位).FirstOrDefault();
            //                byte DDIndex = 0;
            //                if (seat.Values[0, 0] == null)
            //                {
            //                    DDIndex = 0;
            //                }
            //                else
            //                {
            //                    DDIndex = 1;
            //                }
            //                result = result && machineHandDevice.TakeMoveGel(target as IBJ, b, seat, DDIndex,  out msg);
            //                InvokeSetValue(target, b, 0, null);
            //                InvokeSetValue(seat, DDIndex, 0, true);
            //            }
            //            else
            //            {
                            
            //                result = result && machineHandDevice.TakeMoveGel(target as IBJ, b, BJCamera as IBJ, 0,  out msg);
            //                if (cameraDevice.Camera == null)
            //                {
            //                    Tool.AppLogError("打开相机失败");
            //                    return false;
            //                }
            //                if (result)
            //                {
            //                    String Led = "888";
            //                    //发送LED显示命令
            //                    var bm = cameraDevice.CaptureImage();
                                
            //                    var testVal = (Gel)target.Values[b, 0];
            //                    this.cameraDevice.Save(bag, testVal, bm, Led);

            //                    foreach(var sampCode in testVal.SampleBarcodes)
            //                    {
            //                        var si=bag.SamplesInfo.Where(item => item.Item1 == sampCode).FirstOrDefault();
            //                        InvokeSetFinishStatus(SampleRacks[si.Item2-1], si.Item3,0);
            //                    }

            //                    var sr = SampleRacks[bag.SamplesInfo[0].Item2-1];
            //                    bool IsFinished = true;
            //                    for(byte x = 0; x < sr.Count; x++)
            //                    {
            //                        IsFinished = IsFinished && (sr.Values[x, 0] == null || sr.Values[x, 0].ToString().EndsWith(",F"));
            //                    }
            //                    if (IsFinished)
            //                    {
            //                        otherPartDevice.SetFinishForRack((byte)(sr.Index-1));
            //                    }
            //                } 
            //                result = result && machineHandDevice.PutDownGel(BJFeiCard, 0,  out msg);
                            
            //                InvokeSetValue(target, b, 0, null);
                            
            //                //处理判读数据
            //                //InvokeSetValue(seat, DDIndex, 0, true);
            //            }
            //        }
            //    }
            //    result= result&& machineHandDevice.CloseDoorForCentrifuge(target);
            //    (target as T_BJ_Centrifuge).RobotNo = null;
            //}
            return true;
        }
        private bool FPXXYXSHXB(FPBRXSHXBStepParameter param, TestBag bag, out String msg)
        {
            msg = null;
            var result = true;
            return result;
        }
        private bool FPXXYXQ(FPBRXQStepParameter param, TestBag bag, out String msg)
        {
            msg = null;
            var result = true;
           
            return result;
        }
        /// <summary>
        /// 分配病人红细胞
        /// </summary>
        /// <param name="param"></param>
        /// <param name="bag"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool FPBRXSHXB(FPBRXSHXBStepParameter param, TestBag bag, out String msg)
        {
            msg = null;
            var result = true;
            //var ents = injectorDevice.GetValid();
            //var len = ents.Length;
            //(String code, byte RackNo, byte Index, T_Result Result)? preSample = null;
            //byte start = 0;
            //byte end = 0;
            //byte DoIndex = 0;
            //for (byte b = 0; b < bag.SamplesInfo.Count; b++)
            //{
            //    (String code, byte RackNo, byte Index, T_Result Result) SampleInfo = bag.SamplesInfo[b];
            //    end = b;
            //    if (!preSample.HasValue)
            //    {
            //        start = b;
            //        preSample = SampleInfo;
            //    }
            //    else
            //    {
            //        if (SampleInfo.Index - preSample.Value.Index == 1 && SampleInfo.RackNo == preSample.Value.RackNo)
            //        {//连续的
            //            var gs1 = QueryGelSeat((byte)(start / bag.GelType.GelRenFen));
            //            var gs2 = QueryGelSeat((byte)(end / bag.GelType.GelRenFen));
            //            if (gs1.Item1.ID != gs2.Item1.ID)
            //            {//跨分配位
            //                byte EntCount =(byte) ((end - start) / bag.GelType.GelRenFen );
            //                result = result && FPHXB(start, (byte)(start / bag.GelType.GelRenFen),(byte)(end-start), bag, DoIndex, param, out msg, ents.Take(EntCount).ToArray());
            //                preSample = SampleInfo;
            //                start = b;
            //                DoIndex++;
            //            }
            //            else if (end - start + 1 == len*bag.GelType.GelRenFen)
            //            {
                            
            //                preSample = null;
            //                result = result && FPHXB(start, (byte)(start / bag.GelType.GelRenFen),(byte)(end-start+1), bag, DoIndex, param, out msg, ents);
            //                DoIndex++;
            //            }
            //            else
            //            {
            //                preSample = SampleInfo;
            //            }
            //        }
            //        else//不连续
            //        {
            //            byte EntCount = (byte)((end - start) / bag.GelType.GelRenFen );
            //            result = result && FPHXB(start, (byte)(start / bag.GelType.GelRenFen),(byte)(end-start), bag, DoIndex, param, out msg, ents.Take(EntCount).ToArray());
            //            DoIndex++;
            //            start = b;
            //            preSample = SampleInfo;
            //        }
            //    }

            //}
            //if (preSample != null)
            //{
            //    byte EntCount = (byte)((end - start+1) / bag.GelType.GelRenFen);
            //    result = result && FPHXB(start, (byte)(start / bag.GelType.GelRenFen),(byte)(end-start+1), bag, DoIndex, param, out msg, ents.Take(EntCount).ToArray());
            //}
            return result;
        }
        /// <summary>
        /// 加载稀释液，一次性加载所有稀释液
        /// </summary>
        /// <param name="EntCount">最大工作通道数</param>
        /// <param name="param"></param>
        /// <param name="bag"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private IList<(T_BJ_DeepPlate,byte, byte,byte)> LoadXSY(byte EntCount,FPBRXSHXBStepParameter param, TestBag bag, out String msg)
        {

            msg = null;
            //var result = true;
            //(String code, byte RackNo, byte Index, T_Result Result) samp = bag.SamplesInfo[0];
            //var xsy = queryAgent(param.XSYCode);
            //if (!xsy.HasValue)
            //{
            //    msg=$"稀释液【{param.XSYCode}】不存在";
            //    return null;
            //}
            //var ents = this.injectorDevice.GetValid();
            //EntCount = Math.Min(EntCount, (byte)bag.SamplesInfo.Count);
            //if (ents.Length > EntCount)
            //{
            //    ents = ents.Take(EntCount).ToArray();
            //}
            
            //var list = this.injectorDevice.LoasXSY(ents, xsy.Value, param, bag, out msg);
            return null;
        }
        /// <summary>
        /// 分配稀释红细胞时的最大通道数
        /// </summary>
        private byte MaxEnts_HXB = 4;
        public bool FPBRXSHXB_New(FPBRXSHXBStepParameter param, TestBag bag, out String msg)
        {
            msg = null;
            //var ents = injectorDevice.GetValid();
            //byte maxEntCount = (byte)Math.Min(MaxEnts_HXB, ents.Length);
            //var result = true;
            ////injectorDevice.TakeTip(true, ents.Length == maxEntCount ? ents : ents.Take(maxEntCount).ToArray());
            //IList<(T_BJ_DeepPlate dp, byte x, byte y, byte count)> list = null;
            //if (result)
            //{
            //    list= this.LoadXSY(maxEntCount, param, bag, out msg);
            //    result=String.IsNullOrEmpty(msg);
                
            //}
            //(String code, byte RackNo, byte Index, T_Result Result) samp = bag.SamplesInfo[0];
            //var samRack = SampleRacks.Where(item => item.Index == samp.RackNo).FirstOrDefault();

            
            //if (!result)
            //{
            //    return result;
            //}
            //byte SampleStartIndex = 0;
            //byte GelSeatIndex = 0;

            //if (!IsFPXQ)
            //{
            //    var fullCards = bag.SamplesInfo.Count / (bag.GelType.GelRenFen * maxEntCount) *maxEntCount;
            //    var noFullCards = NeedNewCardCount - fullCards;
            //    for (byte b = 0; b < fullCards* bag.GelType.GelRenFen; b++)
            //    {
            //        var baseCount = maxEntCount * (b / (bag.GelType.GelRenFen * maxEntCount));
            //        GelSeatIndex = (byte)(baseCount +
            //            +b % (maxEntCount)
            //            );
            //        var g = QueryGelSeat((byte)(GelSeatIndex));
            //        var gel = (Gel)g.Item1.Values[g.Item2, 0];
            //        var rk = bag.SamplesInfo[b];
            //        gel.SampleBarcodes.Add(rk.Item1);
            //    }
            //    if (noFullCards > 0) {
            //        var lastG = QueryGelSeat((byte)(NeedNewCardCount-1));
            //        var LastGel = (Gel)lastG.Item1.Values[lastG.Item2, 0];
            //        //最后一张卡可测样品数量
            //        var lastCount = bag.SamplesInfo.Count - (fullCards + noFullCards - 1) * bag.GelType.GelRenFen;
            //        for (byte b = (byte)(fullCards * bag.GelType.GelRenFen); b < bag.SamplesInfo.Count; b++)
            //        {
            //            var baseIndex = b - bag.GelType.GelRenFen * fullCards;

            //            if (noFullCards % 2 == 1)//奇数张卡
            //            {
            //                if (b < bag.SamplesInfo.Count - lastCount)
            //                {//先装前面的偶数张卡
            //                    GelSeatIndex = (byte)(fullCards + baseIndex % (noFullCards - 1));
            //                }
            //                else
            //                {//最后的lastCount全部进最后一张
            //                    GelSeatIndex = (byte)(fullCards + noFullCards-1);
            //                }
            //            }
            //            else//偶数张卡
            //            {
            //                //最后一张卡没有装完时
            //                if (LastGel.SampleBarcodes.Count< lastCount)
            //                {
            //                    GelSeatIndex = (byte)(fullCards + baseIndex % (noFullCards));
            //                }
            //                else
            //                {//最后一张卡没有装完后
            //                    var newIndex = baseIndex - lastCount * noFullCards;
            //                    GelSeatIndex = (byte)(fullCards + newIndex % (noFullCards-1));
            //                }

            //            }
            //            var g = QueryGelSeat((byte)(GelSeatIndex));
            //            var gel = (Gel)g.Item1.Values[g.Item2, 0];
            //            var rk = bag.SamplesInfo[b];
            //            gel.SampleBarcodes.Add(rk.Item1);
            //        }
            //    }
            //}
            // SampleStartIndex = 0;
            // GelSeatIndex = 0;
            //for (byte b = 0; b < list.Count; b++)
            //{
            //    //Gel
            //    (T_BJ_DeepPlate dp, byte x, byte y, byte count) item = list[b];
            //    var curEnts = ents.Length == item.count ? ents : ents.Take(item.count).ToArray();
            //    IList<(T_BJ_GelSeat, byte,byte)> seatList = new List<(T_BJ_GelSeat, byte, byte)>();

            //    for (byte c = 0; c < item.count; c++)
            //    {
            //        var sam = bag.SamplesInfo[SampleStartIndex+c].Item1;
            //        seatList.Add(QueryGelSeat(sam, bag.GelType));
                    
            //    }
                
            //    result = result && this.injectorDevice.XS_AND_FP_For_Lot(curEnts, item, true,param, samRack,SampleStartIndex, bag, seatList, out msg);
            //    SampleStartIndex += item.count;
            //    if (!result)
            //    {
            //        break;
            //    }
            //}
            return true;
        }
        /// <summary>
        /// 稀释分配红细胞
        /// </summary>
        /// <param name="StartNo">第一个样本索引号</param>
        /// <param name="GelSeatIndex">当前测试第一个分配位的索引号</param>
        /// <param name="SampCount">此次分配的样本数量</param>
        /// <param name="bag">测试数据包</param>
        /// <param name="DoIndex">当前包中的测试次数</param>
        /// <param name="param">分配参数</param>
        /// <param name="msg">异常信息</param>
        /// <param name="ents">当前使用的通道数</param>
        /// <returns></returns>
        private bool FPHXB(byte StartNo, byte GelSeatIndex,byte SampCount, TestBag bag, byte DoIndex, FPBRXSHXBStepParameter param, out String msg, Enterclose[] ents)
        {
            
            msg = null;
            (String code, byte RackNo, byte Index, T_Result Result) samp = bag.SamplesInfo[StartNo];
            var result = true;
            var samRack = SampleRacks.Where(item => item.Index == samp.RackNo).FirstOrDefault();
            if (samRack == null)
            {
                result = false;
            }
            else
            {
                var TubeCount = bag.GelType.GelType / bag.GelType.GelRenFen;
                var firstGelSeat = QueryGelSeat(GelSeatIndex);
                if(firstGelSeat.Item1.Values[firstGelSeat.Item2, 0] == null)
                {
                    this.InvokeSetValue(firstGelSeat.Item1, firstGelSeat.Item2, 0, new Gel("TEST001"));
                }
                var info =(Gel) firstGelSeat.Item1.Values[firstGelSeat.Item2, 0];
                if (info.SampleBarcodes.Count == 0 && info.FirstNo == 0)//表示是此卡没有样本号在内
                {
                    var Count = SampCount / (bag.GelType.GelRenFen * ents.Length) * bag.GelType.GelRenFen;//需要的卡数量，这些卡将完全用完
                    
                    for (byte b = 0; b < Count; b++)
                    {
                        var firstNo = (byte)((b % bag.GelType.GelRenFen) * TubeCount);
                        result = result && FPHXB_One((byte)(StartNo + ents.Length * b), samRack, firstGelSeat.Item1, firstGelSeat.Item2, firstNo, bag, DoIndex, param,out msg, ents);
                    }
                    var yuSamCount = SampCount % (bag.GelType.GelRenFen * ents.Length);
                    if (yuSamCount > bag.GelType.GelRenFen)
                    {
                        result = result && FPHXB((byte)(StartNo + ents.Length * Count * bag.GelType.GelRenFen), (byte)(GelSeatIndex + Count * ents.Length/ bag.GelType.GelRenFen), SampCount, bag, DoIndex, param, out msg, ents.Take(ents.Length - 1).ToArray());
                    }
                    else
                    {
                        for (byte b = 0; b < yuSamCount; b++)
                        {
                            var tubeNo = (byte)((b % bag.GelType.GelRenFen) * TubeCount);
                            result = result && FPHXB_One((byte)(StartNo + ents.Length * Count * bag.GelType.GelRenFen), samRack, firstGelSeat.Item1, firstGelSeat.Item2, tubeNo, bag, DoIndex, param,out msg, ents.Take(1).ToArray());
                        }

                    }
                }
                else
                {
                    var tubeNo = 0;
                    if (info.SampleBarcodes.Count == 0 && info.FirstNo > 0)//存在节约卡的情况
                    {
                        tubeNo = info.FirstNo;
                    }
                    else if (info.SampleBarcodes.Count > 0 && info.FirstNo == 0)//上次没有将卡装满
                    {
                        tubeNo = info.SampleBarcodes.Count * TubeCount;
                    }
                    else if (info.SampleBarcodes.Count > 0 && info.FirstNo > 0)//上次没有将卡装满并有节约卡
                    {
                        tubeNo= info.FirstNo+ info.SampleBarcodes.Count * TubeCount;
                    }
                    var sigleSamp = (bag.GelType.GelType - tubeNo) / TubeCount;
                    for (byte b = 0; b < Math.Min(SampCount,sigleSamp); b++)
                    {
                        var tubeStartNo = (byte)((tubeNo + b  * TubeCount));
                        result = result && FPHXB_One((byte)(StartNo + b), samRack, firstGelSeat.Item1, firstGelSeat.Item2, tubeStartNo, bag, DoIndex, param,out msg, ents.Take(1).ToArray());
                    }
                    StartNo += (byte)Math.Min(SampCount, sigleSamp);
                    if (SampCount > sigleSamp)
                        FPHXB(StartNo,(byte) (GelSeatIndex + 1), (byte)(SampCount - sigleSamp), bag, DoIndex, param, out msg, ents);
                }

            }

            return result;
        }
        
        /// <summary>
        /// 稀释分配一个最小单位的红细胞
        /// </summary>
        /// <returns></returns>
        public bool FPHXB_One(byte StartNo,T_BJ_SampleRack samRack,T_BJ_GelSeat GelSeat,byte CurrIndex, byte firstNo, TestBag bag, byte DoIndex, FPBRXSHXBStepParameter param,out string msg,  Enterclose[] ents)
        {
            msg = null;
            var result = true;
            //(String code, byte RackNo, byte Index, T_Result Result) samp = bag.SamplesInfo[StartNo];
            //var xsy = queryAgent(param.XSYCode);
            //if (!xsy.HasValue)
            //{
            //    throw new MyException($"稀释液【{param.XSYCode}】不存在");
            //}
            //(IBJ agBj, byte X, Byte Y) = queryAgent(param.XSYCode).Value;
            //result = result && injectorDevice.XS_AND_FP(param,(GelSeat, CurrIndex, firstNo), (agBj as T_BJ_AgentiaWarehouse, X), true,samRack, StartNo, bag,out msg, ents);
            ////结束稀释
            return result;
        }
        /// <summary>
        /// 分配病人血清
        /// </summary>
        /// <param name="param"></param>
        /// <param name="bag"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool FPBRXQ(FPBRXQStepParameter param,TestBag bag, out String msg)
        {
            msg = null;
            var result = true;
            //var ents = injectorDevice.GetValid();
            //var len = ents.Length;
            //(String code, byte RackNo, byte Index, T_Result Result)? preSample=null;
            //byte start = 0;
            //byte end = 0;
            //byte DoIndex = 0;
            //for (byte b=0;b< bag.SamplesInfo.Count; b++)
            //{
            //    (String code, byte RackNo, byte Index, T_Result Result) SampleInfo = bag.SamplesInfo[b];
            //    end = b;
            //    if (!preSample.HasValue)
            //    {
            //        start = b;
            //        preSample = SampleInfo;
            //    }
            //    else
            //    {
            //        if(SampleInfo.Index-preSample.Value.Index==1 && SampleInfo.RackNo == preSample.Value.RackNo)
            //        {//连续的
            //            var gs1 = QueryGelSeat((byte)(start / bag.GelType.GelRenFen));
            //            var gs2 = QueryGelSeat((byte)(end / bag.GelType.GelRenFen));
            //            if (gs1.Item1.ID != gs2.Item1.ID)
            //            {//跨分配位
            //                byte EntCount = (byte)((end - start) / bag.GelType.GelRenFen);
            //                result = result && FPXQ(start, (byte)(start / bag.GelType.GelRenFen), (byte)(end - start), bag, DoIndex, param, out msg, ents.Take(EntCount).ToArray());
            //                preSample = SampleInfo;
            //                start = b;
            //                DoIndex++;
            //            }
            //            else if (end - start + 1 == len * bag.GelType.GelRenFen)
            //            {
            //                preSample = null;
            //                result = result && FPXQ(start, (byte)(start / bag.GelType.GelRenFen), (byte)(end - start + 1),bag, DoIndex, param, out msg, ents);
            //                DoIndex++;
            //            }
            //            else
            //            {
            //                preSample = SampleInfo;
            //            }
            //        }
            //        else//不连续
            //        {
            //            byte EntCount = (byte)((end - start) / bag.GelType.GelRenFen);
            //            result = result && FPXQ(start, (byte)(start / bag.GelType.GelRenFen), (byte)(end - start), bag, DoIndex, param, out msg, ents.Take(EntCount).ToArray());
            //            //result = result && FPXQ(bag.SamplesInfo[start], (byte)(b / bag.GelType.GelRenFen),bag, DoIndex, param, out msg, ents.Take(end - start).ToArray());
            //            DoIndex++;
            //            start = b;
            //            preSample = SampleInfo;
            //        }
            //    }
                
            //}
            //if (preSample != null)
            //{
            //    byte EntCount = (byte)((end - start + 1) / bag.GelType.GelRenFen);
            //    result = result && FPXQ(start, (byte)(start / bag.GelType.GelRenFen), (byte)(end - start + 1), bag, DoIndex, param, out msg, ents.Take(EntCount).ToArray());
            //    //result =result &&FPXQ(bag.SamplesInfo[start], (byte)(end / bag.GelType.GelRenFen),bag, DoIndex, param, out msg, ents.Take(end - start+1).ToArray());
            //}
            
            return result;
        }
        private bool FPXQ(byte StartNo, byte GelSeatIndex, byte SampCount
            
            ,TestBag bag,byte DoIndex, FPBRXQStepParameter param,out String msg, Enterclose[] ents)
        {
            var result = true;
            msg = null;
            //(String code, byte RackNo, byte Index, T_Result Result) samp = bag.SamplesInfo[StartNo];
            //var samRack = SampleRacks.Where(item => item.Index == samp.RackNo).FirstOrDefault();
            //if (samRack == null)
            //{
            //    result = false;
            //}
            //else
            //{
            //    var TubeCount = bag.GelType.GelType / bag.GelType.GelRenFen;
            //    var firstGelSeat = QueryGelSeat(GelSeatIndex);
            //    if (firstGelSeat.Item1.Values[firstGelSeat.Item2, 0] == null)
            //    {
            //        this.InvokeSetValue(firstGelSeat.Item1, firstGelSeat.Item2, 0, new Gel("TEST001"));
            //    }
            //    var info = (Gel)firstGelSeat.Item1.Values[firstGelSeat.Item2, 0];
            //    if (info.SampleBarcodes.Count == 0 && info.FirstNo == 0)//表示是此卡没有样本号在内
            //    {
            //        var Count = SampCount / (bag.GelType.GelRenFen * ents.Length) * bag.GelType.GelRenFen;//需要的卡数量，这些卡将完全用完

            //        for (byte b = 0; b < Count; b++)
            //        {
            //            var firstNo = (byte)((b % bag.GelType.GelRenFen) * TubeCount);
            //            result = result && FPXQ_One((byte)(StartNo + ents.Length * b), samRack, firstGelSeat.Item1, firstGelSeat.Item2, firstNo, bag, DoIndex, param,out msg, ents);
            //        }
            //        var yuSamCount = SampCount % (bag.GelType.GelRenFen * ents.Length);
            //        if (yuSamCount > bag.GelType.GelRenFen)
            //        {
            //            result = result && FPXQ((byte)(StartNo + ents.Length * Count * bag.GelType.GelRenFen), (byte)(GelSeatIndex + Count * ents.Length / bag.GelType.GelRenFen), SampCount, bag, DoIndex, param, out msg, ents.Take(ents.Length - 1).ToArray());
            //        }
            //        else
            //        {
            //            for (byte b = 0; b < yuSamCount; b++)
            //            {
            //                var tubeNo = (byte)((b % bag.GelType.GelRenFen) * TubeCount);
            //                result = result && FPXQ_One((byte)(StartNo + ents.Length * Count * bag.GelType.GelRenFen), samRack, firstGelSeat.Item1, firstGelSeat.Item2, tubeNo, bag, DoIndex, param,out msg, ents.Take(1).ToArray());
            //            }

            //        }
            //    }
            //    else
            //    {
            //        var tubeNo = 0;
            //        if (info.SampleBarcodes.Count == 0 && info.FirstNo > 0)//存在节约卡的情况
            //        {
            //            tubeNo = info.FirstNo;
            //        }
            //        else if (info.SampleBarcodes.Count > 0 && info.FirstNo == 0)//上次没有将卡装满
            //        {
            //            tubeNo = info.SampleBarcodes.Count * TubeCount;
            //        }
            //        else if (info.SampleBarcodes.Count > 0 && info.FirstNo > 0)//上次没有将卡装满并有节约卡
            //        {
            //            tubeNo = info.FirstNo + info.SampleBarcodes.Count * TubeCount;
            //        }
            //        var sigleSamp = (bag.GelType.GelType - tubeNo) / TubeCount;
            //        for (byte b = 0; b < Math.Min(SampCount, sigleSamp); b++)
            //        {
            //            var tubeStartNo = (byte)((tubeNo + b * TubeCount));
            //            result = result && FPXQ_One((byte)(StartNo + b), samRack, firstGelSeat.Item1, firstGelSeat.Item2, tubeStartNo, bag, DoIndex, param,out msg, ents.Take(1).ToArray());
            //        }
            //        StartNo += (byte)Math.Min(SampCount, sigleSamp);
            //        if (SampCount > sigleSamp)
            //            FPXQ(StartNo, (byte)(GelSeatIndex + 1), (byte)(SampCount - sigleSamp), bag, DoIndex, param, out msg, ents);
            //    }

            //}
            ////脱针
            //result = result && injectorDevice.OutTip(ents);
            return result;
            
        }
        public bool FPXQ_One(byte StartNo, T_BJ_SampleRack samRack, T_BJ_GelSeat GelSeat, byte CurrIndex, byte firstNo, TestBag bag, byte DoIndex, FPBRXQStepParameter param,out String msg, Enterclose[] ents)
        {
            msg = "";
            var result = true;
            //(String code, byte RackNo, byte Index, T_Result Result) samp = bag.SamplesInfo[StartNo];

            ////吸液
            //var tubeCount = bag.GelType.GelType / bag.GelType.GelRenFen;
            //var idList = Tool.ParseTubeNo(param.TubeValue).Where(no => no >= firstNo && no < firstNo + tubeCount).ToArray();
            //var totalVol = param.Vol * idList.Length;
            
            //result = result && injectorDevice.TakeTip(true, ents);
            ////result = result && injectorDevice.FenZhenTo(samRack.FZ);
            //result = result && injectorDevice.MoveTo((samRack, samp.Index, 0),false, ents);
            //decimal[] lzs = null;
            //result = result && injectorDevice.DetectAndXY(ents, samRack.Z, samRack.Limit, ents[0].PumpMotor.Speed.SetValue, totalVol, Constants.DefaultBackSpeed, Constants.DefaultBackVol, samRack.DeepForUl,out lzs,out msg, (samRack, samp.Index, 0));
       
            ////result = result && injectorDevice.MoveZ(0, true, false, ents);
            ////结束吸液
            ////result = result && injectorDevice.MoveTo((GelSeat, CurrIndex, 0), ents);

            //result = result &&  injectorDevice.FP(idList, (GelSeat, CurrIndex), ents,param.Deep,ents[0].PumpMotor.Speed.SetValue,param.Vol, Constants.DefaultBackSpeed, Constants.DefaultBackVol, Constants.DefaultBackVol);
            //if (result)
            //{
            //    for(byte b = 0; b < ents.Length; b++)
            //    {
            //        var gel = (Gel)GelSeat.Values[CurrIndex + b, 0];
            //        var rk = bag.SamplesInfo[StartNo + b];
            //        gel.SampleBarcodes.Add(rk.Item1);
            //    }
            //}
            return result;

        }
        private (T_BJ_GelSeat GSeat, Byte X ,byte TubeFirstNo) QueryGelSeat(String samCode,T_Gel gel)
        {
            T_BJ_GelSeat g = null;
            byte? index = null;
            byte TubeFirstNo = 0;
            for (byte b = 0; b < AllocationSeat.Length; b++)
            {
                g = AllocationSeat[b];
                for(byte c = 0; c < g.Count; c++)
                {
                    if (((Gel)g.Values[c, 0]).SampleBarcodes.Contains(samCode)) {
                        index = c;
                        TubeFirstNo = (byte)(gel.GelType / gel.GelRenFen * ((Gel)g.Values[c, 0]).SampleBarcodes.IndexOf(samCode));
                        break;
                    } 
                }
                if (index.HasValue)
                {
                    break;
                }
            }
            return (g, index.HasValue?index.Value:(byte)0, TubeFirstNo);
        }
        private (T_BJ_GelSeat,Byte) QueryGelSeat(byte GelSeatIndex)
        {
            T_BJ_GelSeat g = null;
            byte index = 0;
            byte TotalGel = 0;
            for(byte b = 0; b < AllocationSeat.Length; b++)
            {
                TotalGel += (byte)AllocationSeat[b].Count;
                if (GelSeatIndex < TotalGel)
                {
                    g = AllocationSeat[b];
                    index =(byte)( GelSeatIndex + AllocationSeat[b].Count- TotalGel);
                    break;
                }
            }
            return (g, index);
        }
        /*
        private bool FP(T_BJ_GelSeat g,byte CurNo,FPBRXQStepParameter param,Enterclose[] ents)
        {
            var result = true;
            result = result && injectorDevice.MoveX(g.InjectorX + CurNo * g.InjectorGapX);
            result = result && injectorDevice.MoveZ(param.Deep, false, ents);
            result = result && injectorDevice.Distribute(ents, null, param.Vol, null, param.BackAbsVol);
            result = result && injectorDevice.MoveZ(g.Z, false, ents);
            return result;
        }*/
        /// <summary>
        /// 分配试剂第一步
        /// </summary>
        /// <param name="param">分配试剂参数</param>
        /// <param name="bag">测试包</param>
        /// <param name="mixTask">混匀器停止任务</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool FPSJ_STEP_1(FPSJStepParameter param, TestBag bag, Task<bool> mixTask, out String msg)
        {
            msg = null;
            var result = true;
            var gw = queryAgent(param.AgentCode);
            if (!gw.HasValue)
            {
                this.SetAlarm($"试剂【{param.AgentCode}】不存在");
                result = false;
            }
            var EntVols = CalcSJVol(param, bag);
            for(byte b = 0; b < EntVols.Count; b++)
            {
                var vols = EntVols[b].entVols;
                result = result && FPSJ_STEP_2(gw.Value, param, EntVols[b].startIndex, EntVols[b].endIndex, mixTask, vols, out msg);
            }
            return result;
        }
        public bool FPSJ_STEP_2((IBJ bj, byte x, byte y) vw,FPSJStepParameter param,  byte startIndex, byte endIndex, Task<bool> mixTask,decimal[] vols, out String msg)
        {
            msg = "";
            //var vs = vols.Where(v => v > 0).ToArray();
            //var ents = injectorDevice.GetValid().Take(vs.Length).ToArray();
            //var takeTip = injectorDevice.TakeTip(true, ents);
            //if (!takeTip)
            //{
            //    msg = "装针失败";
            //    return false;
            //}
            //var result = FPSJ_STEP_3(vw,vs, startIndex,endIndex,mixTask,param,out msg,ents);
            //result = result && injectorDevice.OutTip(ents);
            return true;
        }
        private bool FPSJ_STEP_3((IBJ bj, byte x, byte y) vw, decimal[] vs, byte startIndex,byte endIndex, Task<bool> mixTask, FPSJStepParameter param,out String msg, Enterclose[] ents)
        {

            msg = "";
            var result = true;
            //(IBJ bj, byte x, byte y) = vw;
            //if (bj is T_BJ_AgentiaWarehouse aw)
            //{
            //    aw.CurrentZ = null;
            //    //吸取试剂开始
            //    for (byte j = 0; j < ents.Length; j++)
            //    {
            //        int AbsorbSpeed = param.AbsorbSpeed == 0 ? ents[j].PumpMotor.Speed.SetValue : param.AbsorbSpeed;
            //        int ZSpeed = Convert.ToInt32(AbsorbSpeed * aw.DeepForUl * ents[j].ZS_SpeedFactor);
                    
            //        result = result && injectorDevice.MoveXY(aw, x, 0, ents[j]);
            //        decimal LiqZ = 0;
            //        mixTask.Wait();
            //        result = result && mixTask.Result;
            //        if (!aw.CurrentZ.HasValue)
            //        {//没有时做探测液面
            //            decimal[] lzs = null;
            //            result = result && injectorDevice.DetectAndXY(new Enterclose[] { ents[j] }, aw.Z, aw.Limit, param.AbsorbSpeed, vs[j], Constants.DefaultBackSpeed,  Constants.DefaultBackVol, aw.DeepForUl, out lzs,out msg,(aw, x, 0));
                        
            //            aw.CurrentZ = lzs[0];
            //        }
            //        else
            //        {
            //            result = result && injectorDevice.MoveZ(aw.CurrentZ,false, false, new Enterclose[] { ents[j] });
            //            result = result && injectorDevice.Absorb(ents[j], aw.Z, aw.Limit, param.AbsorbSpeed, vs[j], Constants.DefaultBackSpeed, Constants.DefaultBackVol, ZSpeed, true, out LiqZ);
            //            aw.CurrentZ = LiqZ;
            //        }
            //    }
            //    //吸液结束
            //    //分配
            //    T_BJ_GelSeat g = null;
            //    byte Yindex = 0;
            //    if (startIndex < AllocationSeat[0].Count && endIndex< AllocationSeat[0].Count)
            //    {
            //        g = AllocationSeat[0];
            //        Yindex = startIndex;
            //        result = result && FPSJ_STEP_4(g, Yindex, endIndex, param, ents);
            //    }
            //    else if (startIndex < AllocationSeat[0].Count && endIndex >= AllocationSeat[0].Count)
            //    {
            //        g = AllocationSeat[0];
            //        Yindex = startIndex;
            //        result = result && FPSJ_STEP_4(g, Yindex, (byte)(AllocationSeat[0].Count-1), param, ents);
            //        g = AllocationSeat[1];
            //        Yindex = 0;
            //        result = result && FPSJ_STEP_4(g, Yindex, (byte)(endIndex- AllocationSeat[0].Count), param, ents);
            //    }
            //    else
            //    {
            //        g = AllocationSeat[1];
            //        Yindex = (byte)(startIndex - AllocationSeat[0].Count);
            //        result = result && FPSJ_STEP_4(g, Yindex, (byte)(endIndex - AllocationSeat[0].Count), param, ents);
            //    }
            //}
            return result;
        }
        private bool FPSJ_STEP_4(T_BJ_GelSeat g,byte StartIndex,byte EndIndex, FPSJStepParameter param,Enterclose[] Ents)
        {
            var result = true;
            //var count = EndIndex - StartIndex+1;
            //var fullCount = count / Ents.Length;
            //var yuCount = count % Ents.Length;

            //var nos = Tool.ParseTubeNo(param.TubeValue);
            //bool isLastLot = false;
            //for (byte b = 0; b < fullCount; b++)
            //{
            //    isLastLot = yuCount == 0 && fullCount == b + 1;
            //    result = result && injectorDevice.FP(nos.ToArray(), (g, (byte)(StartIndex+b* Ents.Length)), Ents, param.Deep, param.AllotSpeed, param.Vol, Constants.DefaultBackSpeed, Constants.DefaultBackVol, Constants.DefaultBackVol, isLastLot);
            //}
            //if (yuCount > 0)
            //{
            //    isLastLot = true;
            //    result = result && injectorDevice.FP(nos.ToArray(), (g, (byte)(StartIndex + fullCount * Ents.Length)), Ents,(byte)yuCount, param.Deep, param.AllotSpeed, param.Vol, Constants.DefaultBackSpeed, Constants.DefaultBackVol, Constants.DefaultBackVol, isLastLot);
            //}
            return result;
        }
        /// <summary>
        /// 计算抽取试剂体积，及分几次
        /// </summary>
        /// <returns></returns>
        private IList<(decimal[] entVols, byte startIndex, byte endIndex)>  CalcSJVol( FPSJStepParameter param, TestBag bag)
        {
            decimal MaxVol = 800m;
            //int entCount =  NeedNewCardCount >= 4 ? 2 : 1;
            //Enterclose[] Ents = injectorDevice.GetValid();
            //Ents= Ents.Take(Math.Min(Ents.Length,entCount)).ToArray();
            //var len = Ents.Length;
            //IList<(decimal[] entVols, byte startIndex, byte endIndex)> entVols = new List<(decimal[] entVols, byte startIndex, byte endIndex)>();
            ////测试样本总数
            //byte count = (byte)bag.SamplesInfo.Count;
            //var tubeCount = bag.GelType.GelType / bag.GelType.GelRenFen;
            //byte firstNo = 0;
            //var idList = Tool.ParseTubeNo(param.TubeValue).Where(no => no >= firstNo && no < firstNo + tubeCount).ToArray();
            //decimal TotalVol = idList.Length * param.Vol * count;
            //decimal[] vols = new decimal[len];
            //byte startIndex = 0;
            //for(byte b=0;b<NeedNewCardCount;b++)
            //{
            //    var index = b % len;
            //    var addVol = 0m;

            //    if (count>=(b+1)* bag.GelType.GelRenFen) {
            //        addVol= param.Vol * idList.Length * bag.GelType.GelRenFen;
            //    }
            //    else
            //    {
            //        addVol= param.Vol * idList.Length * ((b + 1) * bag.GelType.GelRenFen-count);
            //    }
            //    if (index == 0 && vols[index] + addVol > MaxVol)
            //    {
            //        startIndex = b;
            //        entVols.Add((vols, startIndex,(byte)(b-1)));
            //        vols = new decimal[len];
            //    }
            //    vols[index] = vols[index] + addVol;
                
            //}
            //entVols.Add((vols, startIndex, (byte)(NeedNewCardCount-1)));
            return null;
        }
        private bool FPSY(FPSJStepParameter param,TestBag bag, Task<bool> mixTask,out String msg)
        {
            msg = null;
            var result = true;
            //var gw = queryAgent(param.AgentCode);
            //if (!gw.HasValue)
            //{
            //    this.SetAlarm($"试剂【{param.AgentCode}】不存在");
            //    result = false;
            //}
            //else
            //{
                
            //    //测试样本总数
            //    byte count=(byte)bag.SamplesInfo.Count;
            //    Enterclose[] RealEnts = null;

            //    var indexList = Tool.ParseTubeNo(param.TubeValue);
            //    var ents = this.injectorDevice.GetValid();
            //    var totalVol = indexList.Count * param.Vol;
            //    var jyCount = count / (ents.Length * bag.GelType.GelRenFen);
            //    var noDoCount = count % (ents.Length * bag.GelType.GelRenFen);

            //    bool takeTip = false;
            //    if (jyCount == 0 && noDoCount > 0)
            //    {
            //        RealEnts = ents.Take(noDoCount).ToArray();
            //    }
            //    else
            //    {
            //        RealEnts = ents;
            //    }
            //    takeTip = injectorDevice.TakeTip(true, RealEnts);
            //    bool flag = true;
            //    while (!takeTip && flag)
            //    {
            //        this.SetAlarm("吸头数量不够，装针失败！请补充吸头后再试");
            //        switch (AlertView.ResultEnuml)
            //        {
            //            case UserResultEnum.ABORT:
            //                {
            //                    flag = false;
            //                    throw new UserAbortException($"吸头数量不够，用户终止实验");
            //                }
            //            case UserResultEnum.IGNORE:
            //                {
            //                    flag = false;
            //                    takeTip = true;
            //                    break;
            //                }
            //            case UserResultEnum.RETRY:
            //                {
            //                    if (jyCount == 0 && noDoCount > 0)
            //                    {
            //                        RealEnts = ents.Take(noDoCount).ToArray();
            //                    }
            //                    else
            //                    {
            //                        RealEnts = ents;
            //                    }
            //                    takeTip = injectorDevice.TakeTip(true, RealEnts);
            //                    break;
            //                }
            //            default:
            //                {
            //                    flag = false;
            //                    break;
            //                }
            //        }
            //    }
            //    if (!takeTip)
            //    {
            //        msg = "装针失败";
            //        return false;
            //    }

            //    for (byte i = 0; i < jyCount; i++)
            //    {
            //        result=result &&FPSJ(gw.Value, totalVol, (byte)(i * ents.Length), mixTask, param,out msg, ents);
            //    }
                
            //    if (noDoCount > 0)
            //    {
            //        RealEnts = ents.Take(noDoCount).ToArray();
            //       result =result && FPSJ(gw.Value, totalVol, (byte)(jyCount * ents.Length), mixTask,param,out msg, RealEnts);
            //    }
            //    (gw.Value.Item1 as T_BJ_AgentiaWarehouse).CurrentZ = null;
            //    result = result && injectorDevice.OutTip(jyCount==0?RealEnts: ents);
            //}
            
            return result;
        }
        /// <summary>
        /// 已装针
        /// </summary>
        /// <returns></returns>
        private bool FPSJ((IBJ bj, byte x, byte y) vw, decimal totalVol,byte startIndex, Task<bool> mixTask, FPSJStepParameter param,out String msg, Enterclose[] ents)
        {

            msg = null;
            var result = true;
            //(IBJ bj, byte x, byte y) = vw;
            //if (bj is T_BJ_AgentiaWarehouse aw)
            //{
            //    for (byte j = 0; j < ents.Length; j++)
            //    {
            //        int AbsorbSpeed = param.AbsorbSpeed == 0 ? ents[j].PumpMotor.Speed.SetValue : param.AbsorbSpeed;
            //        int ZSpeed = Convert.ToInt32(AbsorbSpeed * aw.DeepForUl*ents[j].ZS_SpeedFactor);
            //        /*
            //        if (j > 0)
            //        {//调整Y轴坐标  //- ents[j].SplitDistance
            //            var adjustDis = aw.Y + aw.Gap * (x - j);
            //                //injectorDevice.CurrentY  - ents[j].MinDistance;
            //            if (adjustDis < 0)
            //            {
            //                result = false;
            //                injectorDevice.MoveY(0, 0,false, ents[j]);
            //                SetAlarm($"吸取试剂失败！无法吸取【{param.AgentCode}】");
            //            }
            //            else
            //            {
            //                result= result &&injectorDevice.MoveY(adjustDis, 0, false, ents[j]);
            //            }
            //        }
            //        else
            //        {
            //            //第一次需要整体移动*/
            //            result = result && injectorDevice.MoveXY(aw, x, 0, ents[j]);
            //        //}
            //        decimal LiqZ = 0;
            //        mixTask.Wait();
            //        result = result && mixTask.Result;
            //        if (!aw.CurrentZ.HasValue)
            //        {//没有时做探测液面
            //            decimal[] lzs = null;
            //            result = result && injectorDevice.DetectAndXY(new Enterclose[] { ents[j] }, aw.Z, aw.Limit, param.AbsorbSpeed, totalVol,Constants.DefaultBackSpeed, Constants.DefaultBackVol,aw.DeepForUl,out lzs,out msg,(aw, x, 0));
            //                //injectorDevice.Detect(aw, x, new Enterclose[] { ents[j] }, aw.Z, aw.Limit, out msg);
            //            //result = true;
            //            aw.CurrentZ = lzs[0];
            //        }
            //        else
            //        {
            //            result = result && injectorDevice.MoveZ(aw.CurrentZ,false, false, new Enterclose[] { ents[j] });
            //            result = result && injectorDevice.Absorb(ents[j], aw.Z, aw.Limit, param.AbsorbSpeed, totalVol, Constants.DefaultBackSpeed, Constants.DefaultBackVol, ZSpeed,true, out LiqZ);
            //            //injectorDevice.Detect(aw, x, new Enterclose[] { ents[j] }, aw.Z, aw.Limit, out msg);
            //            //result = true;
            //            aw.CurrentZ = LiqZ;
            //        }
            //    }
            //    T_BJ_GelSeat g = null;
            //    byte Yindex = 0;
            //    if (startIndex < AllocationSeat[0].Count) {
            //        g = AllocationSeat[0];
            //        Yindex = startIndex;
            //    }
            //    else {
            //        g = AllocationSeat[1];
            //        Yindex=(byte)(startIndex - AllocationSeat[0].Count);
            //    }

            //    var nos = Tool.ParseTubeNo(param.TubeValue);

            //    result = result && injectorDevice.FP(nos.ToArray(), (g, Yindex), ents, param.Deep, param.AllotSpeed, param.Vol, Constants.DefaultBackSpeed, Constants.DefaultBackVol, Constants.DefaultBackVol);
            //}
            return result;
        }
        
        private ValueTuple<IBJ,byte,byte>? queryAgent(String code)
        {
            ValueTuple<IBJ, byte, byte>? result = null;
            foreach (var gw in BJAgentiaWares)
            {
                var len0 = gw.Values.GetLength(0);
                var len1 = gw.Values.GetLength(1);
                for(byte i = 0; i < len0; i++)
                {
                    for(byte j = 0; j < len1; j++)
                    {
                        if(gw.Values[i,j]!=null && gw.Values[i, j].ToString() == code)
                        {
                            result=(gw,i,j);
                            break;
                        }
                    }
                    if (result.HasValue) break;
                }
                if (result.HasValue) break;
            }
            return result;
        }
        private bool MoveGetToLXJ(byte ToLxjSourceType,out String msg)
        {
            var result = true;
            msg = null;
            //IBJ[] sources = null;
            //if (ToLxjSourceType == 0)
            //{
            //    sources = AllocationSeat;
            //}
            //else
            //{
            //    sources = GetGelSeats(GelSeatPurposeEnum.孵育位);
            //}
            //byte zkCount = (byte)(NeedNewCardCount / 12 + (NeedNewCardCount % 12 == 0 ? 0 : 1));
            //var target = BJCentrifuges.Where(c => !c.RobotNo.HasValue).FirstOrDefault();
            //if (target != null) target.RobotNo = this.TaskID;
            //while (target == null || target.RobotNo != this.TaskID)
            //{
            //    Thread.Sleep(1000);
            //    target = BJCentrifuges.Where(c => !c.RobotNo.HasValue).FirstOrDefault();
            //    if (target != null) target.RobotNo = this.TaskID;
            //}
            //if (target != null)
            //{
            //    BlockHand(HandStatusEnum.MoveGEL2Cent);
            //    for (byte i = 0; i < zkCount; i++)
            //    {
            //        result = result && machineHandDevice.OpenDoorForCentrifuge(target, true);
            //        result = result && ZK(sources[i], target, out msg);
                    
            //        result = result && PeiPing(target);
            //        result = result && machineHandDevice.CloseDoorForCentrifuge(target);
            //    }
                
            //}
            //else
            //{
            //    result = false;
            //}
            return result;
        }
        /// <summary>
        /// 离心机配平
        /// </summary>
        /// <returns></returns>
        private bool PeiPing(T_BJ_Centrifuge cent)
        {
            //byte tc = (byte)cent.GetUsedCount();
            //String msg = "";
            //if (tc % 2 == 1)
            //{
            //    var seat=GetGelSeats(GelSeatPurposeEnum.配平与节约位).FirstOrDefault();
            //    if (seat != null)
            //    {
            //        byte x = 0;
            //        if (seat.Values[0, 0] != null)
            //        {
            //            x =(byte)( 0);
            //        }
            //        else
            //        {
            //            x = (byte)(1);
            //        }
            //        var result = this.machineHandDevice.TakeMoveGel(seat, x, cent, tc, out msg);
            //        if (result)
            //        {
            //            this.InvokeSetValue(cent, tc, 0, true);
            //            this.InvokeSetValue(seat, x, 0, null);
                       
            //        }
            //        return result;
            //    }
            //    else
            //    {
            //        Tool.AppLogFatal("没有设置找到配平位");
            //        return false;
            //    }
            //}
            return true;
        }
        private bool MoveGelToFy(out String msg)
        {
            var result = true;
            msg = null;
            var fys = GetGelSeats(GelSeatPurposeEnum.孵育位);
            result = ZK(AllocationSeat, fys,out msg);
            return result;
        }
        private bool ZK(IBJ[] sources,IBJ[] targets,out String msg)
        {
            var result = true;
            msg = null;
            for (var sIndex = 0; sIndex < sources.Length; sIndex++)
            {
                result = result && ZK(sources[sIndex], targets[sIndex],out msg);
            }
            return result;
        }
        private bool ZK(IBJ source, IBJ target, out String msg)
        {
            var result = true;
            msg = null;
            
                //var gs = source as VBJ;
                //var fy = target as VBJ;
                //byte count = (byte)gs.Values.GetLength(0);
                //for (byte i = 0; i < count; i++)
                //{
                //    if (gs.Values[i, 0] != null)
                //    {
                //        result = result && machineHandDevice.TakeMoveGel(gs as IBJ, i, fy as IBJ, i, out msg);
                //        if (result)
                //        {
                //            InvokeSetValue(fy, i, 0, gs.Values[i, 0]);
                //            InvokeSetValue(gs,i, 0, null);
                //        }
                //        else
                //        {
                //            break;
                //        }
                //    }
                //}
            return result;
        }
        private bool KaiKongGel()
        {
            var result = true;
            foreach (var gs in AllocationSeat)
            {
                for(byte i = 0; i < gs.Count; i++)
                {
                    if (gs.Values[i, 0] != null)
                    {
                        //result= result && this.piercerDevice.KaiKong(gs, i);
                        //var gel = (Gel)gs.Values[i, 0];
                        //gel.Pierce = DateTime.Now;
                        //gs.SetValue(i, 0, gel);
                    }
                }
            }
            result = result &&piercerDevice.MoveZTo(0) && piercerDevice.MoveYTo(0);
            return result;
        }
        /// <summary>
        /// 抓卡到打孔位
        /// </summary>
        /// <param name="bag"></param>
        private bool TakeGelToPiercer(TestBag bag,out String msg)
        {
             msg = "";
            bool result = true;
            //bool flag = false;
            //TakeGelResetEvent.WaitOne();//如果不加信号量，可能计P算出来的卡数量不正确
            //int usedGelCount = 0;//节约卡可测试样本数量;
            //int normalGelCount = 0;//正常卡测试样本数量;
            //IList<(T_BJ_GelSeat, byte, byte)> usedList=null;
            //if (bag.GelType.IsUsedGel && bag.GelType.AfterKKTime > 0)
            //{
            //    usedList = QueryUsedGel(bag);
            //    foreach(var item in usedList)
            //    {
            //        var tubeCount = bag.GelType.GelType - ((Gel)(item.Item1.Values[item.Item2, item.Item3])).FirstNo;
            //        usedGelCount += Math.Max(tubeCount / (bag.GelType.GelType / bag.GelType.GelRenFen), 0);
            //    }
            //}
            
            //normalGelCount = (bag.SamplesInfo.Count - usedGelCount);
            ////需要新卡的数量
            //NeedNewCardCount =(byte)( normalGelCount / bag.GelType.GelRenFen + (normalGelCount % bag.GelType.GelRenFen == 0 ? 0 : 1));

            
            //byte CardCount = 0;
            //#region 处理节约卡
            //if (usedGelCount > 0)
            //{
            //    var count = 0;
            //    foreach(var item in usedList)
            //    {
            //        var val=(Gel)item.Item1.Values[item.Item2, item.Item3];
            //        var tubeCount = bag.GelType.GelType - val.FirstNo;
            //        var currentCount= Math.Max(tubeCount / (bag.GelType.GelType / bag.GelType.GelRenFen), 0);
            //        if (currentCount > 0)
            //        {
            //            count += currentCount;
            //            CardCount++;
            //            var target = GetAllocation(CardCount);
            //            var moveResult=this.machineHandDevice.TakeMoveGel(item.Item1,item.Item2, target.Item1, target.Item2,out msg);
            //            if (moveResult)
            //            {
            //                TranVal(item, target);
            //                if (count >= usedGelCount)
            //                {
            //                    break;
            //                }
            //            }
            //            else
            //            {

            //            }
                        
            //        }
            //    }
                
            //}
            //#endregion 处理节约卡
            //#region 处理新卡
            //if (NeedNewCardCount > 0)
            //{
            //    var gelInfo = GetGelPointInfo(bag);
            //    flag = true;
            //    while (flag && NeedNewCardCount > gelInfo.Count)
            //    {
            //        this.SetAlarm($"【{bag.GelType.GelName}】数量不足，请补充后继续！");
            //        switch (AlertView.ResultEnuml)
            //        {
            //            case UserResultEnum.ABORT:
            //                {
            //                    flag = false;
            //                    throw new UserAbortException($"【{bag.GelType.GelName}】数量不足");
            //                }
            //            case UserResultEnum.IGNORE:
            //                {
            //                    flag = false;
            //                    break;
            //                }
            //            case UserResultEnum.RETRY:
            //                {
            //                    result = result && gelWarehouseDevice.InitX();
            //                    result=result && gelWarehouseDevice.ScanGelCards(out msg);
            //                    gelInfo = GetGelPointInfo(bag);
            //                    break;
            //                }
            //            default:
            //                {
            //                    flag = false;
            //                    break;
            //                }
            //        }
            //    }
            //    if (flag)
            //    {
            //        result = result && machineHandDevice.InitGelScaner(out msg);
            //        try
            //        {
            //            for (byte i = 0; i < NeedNewCardCount; i++)
            //            {
            //                var source = gelInfo[i];
            //                CardCount++;
            //                var gs = GetAllocation(CardCount);
            //                String barCode = null;
            //                result = result && this.machineHandDevice.ScanGel((source.Item1, source.Item2), (gs.Item1, gs.Item2), out barCode, out msg);
            //                if (result)
            //                {
            //                    InvokeSetValue(source.Item1, source.Item2, source.Item3, null);
            //                    if (gs.Item1 is VBJ vb)
            //                    {
            //                        Gel gel = new Gel(barCode);
            //                        InvokeSetValue(vb, gs.Item2, gs.Item3, gel);
            //                    }
            //                }
            //                if (!result)
            //                {
            //                    break;
            //                }
            //            }
            //        }catch(Exception ex)
            //        {
            //            msg = ex.Message;
            //            result = false;
            //        }
            //        finally
            //        {
            //            machineHandDevice.CloseGelScaner();
            //        }

            //        /*
            //        scanDevice.OpenGelScaner();
            //        for (byte i=0;i< NeedNewCardCount; i++)
            //        {
            //            var source = gelInfo[i];
            //            this.machineHandDevice.TakeMoveGel(source.Item1, source.Item2,  scanDevice.GelBJScaner, 0,  out msg);
                        
            //            var barCode = scanDevice.GelScaner.Read();
            //            byte tryCount = 3;
            //            while (String.IsNullOrEmpty(barCode) && tryCount > 0)
            //            {
            //                this.scanDevice.GelScaner.Stop();
            //                this.scanDevice.OpenGelScaner();
            //                barCode = scanDevice.GelScaner.Read();
            //            }
            //            if (String.IsNullOrEmpty(barCode))
            //            {
            //                result = false;
            //                break;
            //            }
            //            CardCount++;
            //            var gs = GetAllocation(CardCount);
            //            result= result&& machineHandDevice.PutDownGel(gs.Item1, gs.Item2,  out msg);
            //            InvokeSetValue(source.Item1,source.Item2, source.Item3,null);
            //            if (gs.Item1 is VBJ vb)
            //            {
            //                Gel gel = new Gel(barCode);
            //                InvokeSetValue(vb,gs.Item2, gs.Item3, gel);
            //            }
            //         }
            //        scanDevice.GelScaner.Stop();
            //        */
            //    }
            //}
            //#endregion 处理新卡
            //result = result && machineHandDevice.MoveZ(0);
            //result = result && machineHandDevice.Move(0, 0, false);
            return result;
        }
        /// <summary>
        /// 传输Source 中对应位置的值到Target
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Target"></param>
        private void TranVal(ValueTuple<IBJ, byte, byte> Source, ValueTuple<IBJ, byte, byte> Target)
        {
            if(Source.Item1 is VBJ vsource && Target.Item1 is VBJ vtarget)
            {
                InvokeSetValue(vtarget,Target.Item2, Target.Item3, vsource.Values[Source.Item2, Source.Item3]);
                InvokeSetValue(vsource,Source.Item2, Source.Item3, null);
            }
        }
        
        private T_BJ_Centrifuge[] _BJCentrifuges;
        /// <summary>
        /// 离心机
        /// </summary>
        private T_BJ_Centrifuge[] BJCentrifuges
        {
            get
            {
                if (_BJCentrifuges == null)
                {
                    _BJCentrifuges =Constants.BJDict[typeof(T_BJ_Centrifuge).Name].Where(item => (item as T_BJ_Centrifuge).Status == 1).Select(item=>item as T_BJ_Centrifuge).ToArray();
                }
                return _BJCentrifuges;
            }
        }
        private T_BJ_DeepPlate[] _BJDeepPlates;
        /// <summary>
        /// 样本稀释板
        /// </summary>
        private T_BJ_DeepPlate[] BJDeepPlates
        {
            get
            {
                if (_BJDeepPlates == null)
                {
                    _BJDeepPlates = Constants.BJDict[typeof(T_BJ_DeepPlate).Name].Select(item => item as T_BJ_DeepPlate).ToArray();
                }
                return _BJDeepPlates;
            }
        }
        private T_BJ_SampleRack[] _SampleRacks;
        /// <summary>
        /// 样本架
        /// </summary>
        private T_BJ_SampleRack[] SampleRacks
        {
            get
            {
                if (_SampleRacks == null)
                {
                    _SampleRacks= Constants.BJDict[typeof(T_BJ_SampleRack).Name].Select(item => item as T_BJ_SampleRack).ToArray();
                }
                return _SampleRacks;
            }
        }
        private T_BJ_AgentiaWarehouse[] _BJAgentiaWares;
        /// <summary>
        /// 试剂仓
        /// </summary>
        private T_BJ_AgentiaWarehouse[] BJAgentiaWares
        {
            get
            {
                if (_BJAgentiaWares == null)
                {
                    _BJAgentiaWares = Constants.BJDict[typeof(T_BJ_AgentiaWarehouse).Name].Select(item=>item as T_BJ_AgentiaWarehouse).ToArray();
                }
                return _BJAgentiaWares;
            }
        }
        private T_BJ_GelSeat[] _AllocationSeat;
        /// <summary>
        /// 所有的分配位
        /// </summary>
        public T_BJ_GelSeat[] AllocationSeat
        {
            get
            {
                if (_AllocationSeat == null)
                {
                    _AllocationSeat = GetGelSeats(GelSeatPurposeEnum.分配位);
                }
                return _AllocationSeat;
            }
        }
        private ValueTuple<IBJ,byte ,byte > GetAllocation(byte Count)
        {
            T_BJ_GelSeat seat = null;
            int totalCount = 0;
            for(byte i = 0; i < AllocationSeat.Length; i++)
            {
                if(AllocationSeat.ElementAt(i) is T_BJ_GelSeat gelSeat)
                {
                    if (gelSeat.Count+ totalCount >= Count)
                    {
                        seat = gelSeat;
                        break;
                    }
                    else
                    {
                        totalCount += gelSeat.Count;
                    }
                }
            }
            if (seat != null)
            {
                var p=seat.GetNext();
                if (p.HasValue)
                {
                    return (seat, p.Value.X, p.Value.Y);
                }
            }
            return (default(IBJ), (byte)0, (byte)0); 
        }
        private T_BJ_GelSeat[] GetGelSeats(GelSeatPurposeEnum purposeEnum)
        {
            return gelSeatList.Where(item => (item as T_BJ_GelSeat).Purpose == (int)purposeEnum).Select(item=>item as T_BJ_GelSeat).ToArray();
        }
        public IList<ValueTuple<T_BJ_GelSeat, byte, byte>> QueryUsedGel(TestBag bag)
        {
            IList<ValueTuple<T_BJ_GelSeat, byte, byte>> result = new List<ValueTuple<T_BJ_GelSeat, byte, byte>>();
            if (!bag.GelType.IsUsedGel || bag.GelType.KeepTime == 0)
            {
                return result;
            }


            var list = GetGelSeats(GelSeatPurposeEnum.配平与节约位);
            foreach (var seat in list)
            {
                if (seat is VBJ vseat)
                {
                    var vals = vseat.Values;
                    for (byte x = 0; x < vals.GetLength(0); x++)
                    {
                        for (byte y = 0; y < vals.GetLength(1); y++)
                        {
                            var obj = vals.GetValue(x, y);
                            if (obj is Gel gel)
                            {
                                if (gel.Pierce.HasValue && (DateTime.Now - gel.Pierce.Value).TotalMinutes < bag.GelType.AfterKKTime)
                                {
                                    var barcode = gel.BarCode;
                                    ValueTuple<T_BJ_GelSeat, byte, byte>? item = null;
                                    if (bag.GelType.IsMaskAtEnd)
                                    {
                                        if (barcode.EndsWith(bag.GelType.GelMask))
                                        {
                                            item = ValueTuple.Create<T_BJ_GelSeat, byte, byte>(vseat as T_BJ_GelSeat, x, y);
                                        }
                                    }
                                    else
                                    {
                                        if (barcode.StartsWith(bag.GelType.GelMask))
                                        {
                                            item = ValueTuple.Create<T_BJ_GelSeat, byte, byte>(vseat as T_BJ_GelSeat, x, y);
                                        }
                                    }
                                    if (item.HasValue)
                                    {
                                        result.Add(item.Value);
                                    }
                                }
                            }
                        }
                    }
                }
                
            }
            return result;
        }
        /// <summary>
        /// 查询当前卡仓中对应实验的卡总数及位置
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        private IList<ValueTuple<T_BJ_GelWarehouse, byte,byte>> GetGelPointInfo(TestBag bag)
        {
            IList<ValueTuple<T_BJ_GelWarehouse, byte, byte>> result = new List<ValueTuple<T_BJ_GelWarehouse, byte, byte>>();
            var gelWareList = bjDic[typeof(T_BJ_GelWarehouse).Name];
            foreach (var seat in gelWareList)
            {
                if (seat is VBJ vseat)
                {
                    var vals = vseat.Values;
                    for (byte x = 0; x < vals.GetLength(0); x++)
                    {
                        for (byte y = 0; y < vals.GetLength(1); y++)
                        {
                            var obj = vals.GetValue(x, y);
                            if (obj is String barcode)
                            {
                                ValueTuple<T_BJ_GelWarehouse, byte, byte>? item=null ;
                                if (bag.GelType.IsMaskAtEnd)
                                {
                                    if (barcode.EndsWith(bag.GelType.GelMask))
                                    {
                                        item = ValueTuple.Create<T_BJ_GelWarehouse, byte, byte>(vseat as T_BJ_GelWarehouse, x, y);
                                    }
                                }
                                else
                                {
                                    if (barcode.StartsWith(bag.GelType.GelMask))
                                    {
                                        item = ValueTuple.Create<T_BJ_GelWarehouse, byte, byte>(vseat as T_BJ_GelWarehouse, x, y);
                                    }
                                }
                                if (item.HasValue)
                                {
                                    result.Add(item.Value);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        } 
        /// <summary>
        /// 会阻塞，直到用户给出响应
        /// </summary>
        /// <param name="errMsg"></param>
        private void SetAlarm(String errMsg)
        {
            this.Message = errMsg;
            SKABO.Common.Utils.Tool.AppLogInfo(this.Message);
            this.IsPause = true;
            RunSemaphore.WaitOne();
        }
        private void InvokeSetValue(VBJ vb,byte x,byte y,Object val)
        {
            Constants.MainWindow.Dispatcher.Invoke(new Action(() => {
                vb.SetValue(x, y, val);
            } ));
        }
        private void InvokeSetFinishStatus(VBJ vb, byte x, byte y)
        {
            Constants.MainWindow.Dispatcher.Invoke(new Action(() => {
                vb.SetFinishStatus(x, y);
            }));
        }
    }
}
