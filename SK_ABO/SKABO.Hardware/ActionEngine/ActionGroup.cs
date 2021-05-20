using SKABO.ActionGeneraterEngine;
using SKABO.Common;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Hardware.Model;
using SKABO.Hardware.RunBJ;
using SKABO.Ihardware.Core;
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
    public class ActionGroup : ActionBase
    {
        
    }
    
    public class HandTakeCard : ActionGroup
    {
        public int z { get; set; } = 0;
        public int zl { get; set; } = 0;
        public int zc { get; set; } = 0;
        public int zb { get; set; } = 0;
        public int rettimes { get; set; } = 0;
        public int curz = 0;
        public int retrx  = 0;
        public int retry  = 0;
        public int cenz = 0;
        public int cenpz = 0;
        public int cendevice_error_times = 0;//离心抓卡失败次数
        public CentrifugeMDevice cendevice { get; set; } = null;//离心抓卡增强
        public double lasttime { get; set; } = 0;
        public bool is_fromware { get; set; } = false;//是否从卡仓中抓取
        public int store_x { get; set; } = 0;//卡仓位值
        public ActionBase act_gelwaremove { get; set; } = null;
        public ActionBase act_swopen { get; set; } = null;
        public ActionBase act_movez { get; set; } = null;
        public ActionBase act_movezl { get; set; } = null;
        public ActionBase act_movezb { get; set; } = null;
        public ActionBase act_movecurz { get; set; } = null;
        public ActionBase act_censtrong { get; set; } = null;
        public ActionBase act_ceninit { get; set; } = null;

        public ActionBase act_swhand { get; set; } = null;
        public ActionBase act_moveret { get; set; } = null;


        public static HandTakeCard instance = null;
        public static HandTakeCard create(MachineHandDevice nodetem, double ttime,int zz, int zzl, int zzc, int zzb=0, CentrifugeMDevice ccendevice = null)
        {
            HandTakeCard acttem = HandTakeCard.create(ttime, zz, zzl, zzc, zzb, ccendevice);
            acttem.node = nodetem;
            return acttem;
        }
        public static HandTakeCard create(double ttime,int zz, int zzl, int zzc, int zzb = 0, CentrifugeMDevice ccendevice = null)
        {
            HandTakeCard acttem = new HandTakeCard();
            acttem.z = zz;
            acttem.zl = zzl;
            acttem.zc = zzc;
            acttem.zb = zzb;
            acttem.cendevice = ccendevice;
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
            lasttime = 0;
            var hand = (MachineHandDevice)node;
            act_swopen = SwHand.create(hand, time + 5000, true);
            act_movez = MoveTo.create(node,time, -1, -1, z, (int)hand.Hand.ZMotor.DownSpeed.SetValue);
            act_movezl = MoveTo.create(node, time, -1, -1, zl,(int)hand.Hand.ZMotor.SecondSpeed.SetValue);
            act_movezb = MoveTo.create(node, time, -1, -1, zb);
            act_swhand = SwHand.create(hand, time+ 5000, false);
            act_gelwaremove = MoveTo.create(Engine.getInstance().gelwareDevice, 3001, store_x, -1, -1);
            var device = new ActionDevice(node);
            if (rettimes>=1&& device.GetRealX(ref retrx)&& device.GetRealY(ref retry))
            {
                act_moveret = Sequence.create(
                    MoveTo.create(node, time, -1, -1, 0), 
                    MoveTo.create(node, time, 0, 0, 0),
                    InitXyz.create(node, 20000, true, true, true),  
                    MoveTo.create(node, time, retrx, retry, 0));
            }
            else
            {
                act_moveret = Sequence.create(InitXyz.create(node, 20000, false, false, true), MoveTo.create(node, time, -1, -1, zb));
            }
            act_swopen.init();
            act_movez.init();
            act_movezl.init();
            act_movezb.init();
            act_swhand.init();
            act_moveret.init();
            act_gelwaremove.init();
            act_swopen.father = this;
            act_movez.father = this;
            act_movezl.father = this;
            act_movezb.father = this;
            act_swhand.father = this;
            act_moveret.father = this;
            act_gelwaremove.father = this;
        }
        public override void run(double dt)
        {
            bool resultz = true;
            sumdt += dt;
            var device = new ActionDevice(node);
            var hand = (MachineHandDevice)node;
            var cen_device = new ActionDevice(cendevice);
            switch (step)
            {
                case 0:
                    if (hand.CheckGel()==true)
                    {
                        resultz = ErrorSystem.WriteActError("抓手有卡！");
                        if (resultz)
                        {
                            step = 0;
                        }
                        else
                        {
                            step = 14;
                            errmsg = "抓手有卡";
                            return;
                        }
                    }
                    else
                    {
                        step++;
                    }
                    break;
                case 1:
                    act_swopen.run(dt);
                    istimeout = act_swopen.istimeout;
                    isdelete = act_swopen.getIsDelete();
                    if (act_swopen.getIsFinish()) step++;
                    break;
                case 2:
                    act_movez.run(dt);
                    istimeout = act_movez.istimeout;
                    isdelete = act_movez.getIsDelete();
                    if (act_movez.getIsFinish()) step++;
                    break;
                case 3:
                    act_movezl.run(dt);
                    istimeout = act_movezl.istimeout;
                    isdelete = act_movezl.getIsDelete();
                    if (sumdt - lasttime > 100)
                    {
                        lasttime = sumdt;
                        if (hand.CheckGel() == true)
                        {
                            resultz = hand.CanComm.StopMotor(hand.Hand.ZMotor);
                            if (resultz)
                            {
                                if(zc!=0)
                                    step++;
                                else
                                    step = 6;
                                break;
                            }
                        }
                    }
                    if (hand.CheckGel()==false&&act_movezl.getIsFinish())
                    {
                        resultz = ErrorSystem.WriteActError("卡位无卡！");
                        if(resultz)
                        {
                            step = 10;
                        }
                        else 
                        {
                            step = 14;
                            errmsg = "卡位无卡";
                            return;
                        }
                    }
                    break;
                case 4:
                    var ret = device.GetRealZ(hand, ref curz);
                    curz = curz + (int)zc;
                    if(ret)
                    {
                        act_movecurz = MoveTo.create(node, time, -1, -1, curz);
                        act_movecurz.init();
                        act_movecurz.father = this;
                        step++;
                    }
                    else
                    {
                        resultz = ErrorSystem.WriteActError("抓手超时！");
                        if (resultz)
                        {
                            step = 10;
                        }
                        else
                        {
                            step = 14;
                            errmsg = "抓手超时";
                            return;
                        }
                    }
                    break;
                case 5:
                    act_movecurz.run(dt);
                    istimeout = act_movecurz.istimeout;
                    isdelete = act_movecurz.getIsDelete();
                    if (act_movecurz.getIsFinish())
                    {
                        if (cendevice!=null&&cen_device.GetRealZ(ref cenz))
                        {
                            step = 11;
                        }
                        else if(cendevice==null)
                        {
                            step++;
                        }
                    }
                    break;
                case 6:
                    act_swhand.run(dt);
                    istimeout = act_swhand.istimeout;
                    isdelete = act_swhand.getIsDelete();
                    if (act_swhand.getIsFinish())
                    {
                        step++;
                    }
                    break;
                case 7:
                    act_movezb.run(dt);
                    istimeout = act_movezb.istimeout;
                    isdelete = act_movezb.getIsDelete();
                    if (act_movezb.getIsFinish()) step++;
                    break;
                case 8:
                    if (hand.CheckGel() == true)
                    {
                        step++;
                    }
                    else
                    {
                        resultz = ErrorSystem.WriteActError("抓手卡脱落！");
                        if (resultz)
                        {
                            step = 10;
                        }
                        else
                        {
                            step = 14;
                            errmsg = "抓手卡脱落";
                            return;
                        }
                    }
                    break;
                case 9:
                    isfinish = true;
                    break;
                case 10://重试
                    act_moveret.run(dt);
                    istimeout = act_moveret.istimeout;
                    isdelete = act_moveret.getIsDelete();
                    if (act_moveret.getIsFinish())
                    {
                        if (is_fromware)
                        {
                            step = 101;
                        }
                        else
                        {
                            init();
                            rettimes++;
                        }
                    }
                    break;
                case 101://重试
                    act_gelwaremove.run(dt);
                    istimeout = act_gelwaremove.istimeout;
                    isdelete = act_gelwaremove.getIsDelete();
                    if (act_gelwaremove.getIsFinish())
                    {
                        init();
                        rettimes++;
                    }
                    break;
                //离心抓卡增强
                case 11:
                    {
                        int[] pz = {1, -1, 2, -2};
                        act_censtrong = Sequence.create(
                            SwHand.create(hand, time, false),
                            MoveTo.create(hand, time, -1, -1, z-1000));
                        act_censtrong.init();
                        act_censtrong.father = this;
                        step++;
                    }
                    break;
                case 12://离心抓卡增强
                    act_censtrong.run(dt);
                    istimeout = act_censtrong.istimeout;
                    isdelete = act_censtrong.getIsDelete();
                    if (act_censtrong.getIsFinish())
                    {
                        if (hand.CheckGel() == true)
                            step = 7;
                        else
                        {
                            act_ceninit = Sequence.create(InitXyz.create(cendevice,time,false,false,true),MoveTo.create(cendevice, time, -1, -1, cenz));
                            act_ceninit.init();
                            act_ceninit.father = this;
                            step++;
                        }
                    }
                    break;
                case 13:
                    {
                        act_ceninit.run(dt);
                        istimeout = act_ceninit.istimeout;
                        isdelete = act_ceninit.getIsDelete();
                        if (act_ceninit.getIsFinish())
                        {
                            cendevice_error_times++;
                            if(cendevice_error_times>=2)
                            {
                                resultz = ErrorSystem.WriteActError("离心机无法抓卡！");
                                if (resultz)
                                {
                                    step = 10;
                                }
                                else
                                {
                                    step = 14;
                                    errmsg = "离心机无法抓卡";
                                    return;
                                }
                            }
                            else
                            {
                                init();
                            }
                        }
                    }
                    break;
                //检测错误Z返回
                case 14:
                    {
                        act_movezb.run(dt);
                        istimeout = act_movezb.istimeout;
                        isdelete = act_movezb.getIsDelete();
                        if (act_movezb.getIsFinish())
                        {
                            istimeout = true;
                            isdelete = true;
                        }
                    }
                    break;

            }

        }
        public static HandTakeCard getInstance()
        {
            if (instance == null) instance = new HandTakeCard();
            return instance;
        }
    }

    public class HandPutCard : ActionGroup
    {
        public int z { get; set; } = 0;
        public int zb { get; set; } = 0;

        public ActionBase act_movez { get; set; } = null;
        public ActionBase act_movezb { get; set; } = null;
        public ActionBase act_moveret { get; set; } = null;
        public ActionBase act_swhand { get; set; } = null;

        public static HandPutCard instance = null;
        public static HandPutCard create(MachineHandDevice nodetem, double ttime, int zz,int zzb = 0)
        {
            HandPutCard acttem = create(ttime, zz, zzb);
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
            var hand = (MachineHandDevice)node;
            act_movez = MoveTo.create(node, time, -1, -1, z, (int)hand.Hand.ZMotor.DownSpeed.SetValue);
            act_movezb = MoveTo.create(node, time, -1, -1, zb);
            act_moveret = Sequence.create(InitXyz.create(node, 10000, false, false, true), MoveTo.create(node, time, -1, -1, zb));
            act_swhand = SwHand.create(hand, time + 5000, true);
            act_movez.init();
            act_movezb.init();
            act_moveret.init();
            act_swhand.init();

            act_movez.father = this;
            act_movezb.father = this;
            act_moveret.father = this;
            act_swhand.father = this;
            sumdt = 0;
            step = 0;
        }
        public override void run(double dt)
        {
            bool resultz = true;
            //if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            var hand = (MachineHandDevice)node;
            switch (step)
            {
                case 0:
                    if (hand.CheckGel() == false)
                    {
                        resultz = ErrorSystem.WriteActError("抓手无卡！");
                        if (resultz)
                        {
                            step = 0;
                        }
                        else
                        {
                            istimeout = true;
                            isdelete = true;
                            errmsg = "抓手无卡";
                            return;
                        }
                    }
                    else
                    {
                            step++;
                    }
                    break;
                case 1:
                    act_movez.run(dt);
                    istimeout = act_movez.istimeout;
                    isdelete = act_movez.getIsDelete();
                    if (act_movez.getIsFinish())
                    {
                        step++;
                    }
                    break;
                case 2:
                    act_swhand.run(dt);
                    istimeout = act_swhand.istimeout;
                    isdelete = act_swhand.getIsDelete();
                    if (act_swhand.getIsFinish()) step++;
                    break;
                case 3:
                    act_movezb.run(dt);
                    istimeout = act_movezb.istimeout;
                    isdelete = act_movezb.getIsDelete();
                    if (act_movezb.getIsFinish()) step++;
                    break;
                case 4:
                    if (hand.CheckGel() == false)
                    {
                        step++;
                    }
                    else
                    {
                        resultz = ErrorSystem.WriteActError("抓手放卡失败！");
                        if (resultz)
                        {
                            hand.Hand.IsOpen = false;
                            step = 6;
                        }
                        else
                        {
                            istimeout = true;
                            isdelete = true;
                            errmsg = "抓手放卡失败";
                            return;
                        }
                    }
                    break;
                case 5:
                    isfinish = true;
                    break;
                case 6:
                    act_moveret.run(dt);
                    istimeout = act_moveret.istimeout;
                    isdelete = act_moveret.getIsDelete();
                    if (act_moveret.getIsFinish())
                    {
                        init();
                    }
                    break;
            }
        }
        public static HandPutCard getInstance()
        {
            if (instance == null) instance = new HandPutCard();
            return instance;
        }
    }


    public class HandScanCard : ActionGroup
    {
        public int x { get; set; } = 0;
        public int y1 { get; set; } = 0;
        public int y2 { get; set; } = 0;
        public int z { get; set; } = 0;
        public int zb { get; set; } = 0;
        public string scan_code { get; set; } = "";
        public double wait_scan { get; set; } = 0;
        public bool is_scan { get; set; } = false;

        public ActionBase act_movexyz { get; set; } = null;
        public ActionBase act_moveret { get; set; } = null;
        private AbstractScaner scaner { get; set; } = null;
        private string scaner_port { get; set; } = "";

        public static HandScanCard instance = null;
        public static HandScanCard create(MachineHandDevice nodetem, double ttime,int xx,int yy1,int yy2, int zz, int zzb = 0)
        {
            HandScanCard acttem = create(ttime, xx,yy1,yy2,zz,zzb);
            acttem.node = nodetem;
            return acttem;
        }
        public static HandScanCard create(double ttime, int xx, int yy1, int yy2, int zz, int zzb = 0)
        {
            HandScanCard acttem = new HandScanCard();
            acttem.x = xx;
            acttem.y1 = yy1;
            acttem.y2 = yy2;
            acttem.z = zz;
            acttem.zb = zzb;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "扫描卡";
        }
        public override void init()
        {
            wait_scan = 0;
            isinit = true;
            is_scan = false;
            var hand = (MachineHandDevice)node;
            var scaner_info = ResManager.getInstance().GetScaner("1");
            if(scaner_info!=null)
            {
                scaner = IoC.Get<AbstractScaner>(scaner_info.ScanerType);
                scaner_port = scaner_info.Port;
            }
            act_movexyz = Sequence.create(
                MoveTo.create(node, time, -1, -1, 0), 
                MoveTo.create(node, time, x, y1, -1), 
                MoveTo.create(node, time, -1, -1, z), 
                MoveTo.create(node, time, -1, y2, -1));
            act_moveret = Sequence.create(InitXyz.create(node, 10000, false, false, true), MoveTo.create(node, time, -1, -1, zb));
            act_movexyz.init();
            act_moveret.init();
            act_movexyz.father = this;
            act_moveret.father = this;
            sumdt = 0;
            step = 0;
            scan_code = "无法识别";
        }
        private void Scaner_DataReceived(AbstractScaner scaner_tem)
        {
            scan_code = scaner_tem.Read();
            is_scan = true;
            Thread.Sleep(200);
            scaner_tem.Stop();
        }
        public override void run(double dt)
        {
            bool resultz = true;
            //if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            var hand = (MachineHandDevice)node;
            switch (step)
            {
                case 0:
                    if (hand.CheckGel() == false)
                    {
                        resultz = ErrorSystem.WriteActError("抓手无卡！");
                        if (resultz)
                        {
                            step = 0;
                        }
                        else
                        {
                            istimeout = true;
                            isdelete = true;
                            errmsg = "抓手无卡";
                            return;
                        }
                    }
                    else
                    {
                        step++;
                    }
                    break;
                case 1:
                    {
                        //打开扫描器
                        if (scaner != null)
                        {
                            scaner.CancelAllEvent();
                            scaner.Open(scaner_port);
                            var res = scaner.Start(false);
                            if (res)
                            {
                                scaner.DataReceived += Scaner_DataReceived;
                                step++;
                            }
                            else
                            {
                                scaner.Close();
                                resultz = ErrorSystem.WriteActError("打开扫描器失败！");
                                if (resultz)
                                {
                                    step = 4;
                                }
                                else
                                {
                                    istimeout = true;
                                    isdelete = true;
                                    errmsg = "打开扫描器失败";
                                    return;
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    act_movexyz.run(dt);
                    istimeout = act_movexyz.istimeout;
                    isdelete = act_movexyz.getIsDelete();
                    if (act_movexyz.getIsFinish()) step++;
                    break;
                case 3:
                    wait_scan += dt;
                    if (wait_scan>2000|| is_scan)
                    {
                        scaner.Stop();
                        isfinish = true;
                    }
                    break;
                case 4:
                    act_moveret.run(dt);
                    istimeout = act_moveret.istimeout;
                    isdelete = act_moveret.getIsDelete();
                    if (act_moveret.getIsFinish())
                    {
                        init();
                    }
                    break;
            }
        }
        public static HandScanCard getInstance()
        {
            if (instance == null) instance = new HandScanCard();
            return instance;
        }
    }

    public class HandTakeGelFromWare : ActionGroup
    {

        public GelWarehouseDevice gelwareDevice { get; set; } = null;
        public string gel_mask { get; set; } = "";
        public int gel_mask_id { get; set; } = 0;
        public string sample_code { get; set; } = "";
        public string[] ware_seat_rack_name = { "box001","box002", "box003","box004","box005", "box006", "box007", "box008" };
        public static Dictionary<string, int> ware_seat_dic = new Dictionary<string, int>();
        public List<ResInfoData> take_seat_list { get; set; } = new List<ResInfoData>();
        public Sequence putback_seque = null;
        public Sequence take_seque { get; set; } = null;
        public ActionBase movez_seque { get; set; } = null;
        public ResInfoData take_seat { get; set; } = null;
        public ActionBase scan_waregel { get; set; } = null;
        public static HandTakeGelFromWare instance = null;
        public static HandTakeGelFromWare create(MachineHandDevice nodetem,double ttime, GelWarehouseDevice ggelwareDevice,int ggel_mask_id,string ggel_mask,string ssample_code)
        {
            HandTakeGelFromWare acttem = create(ttime, ggelwareDevice, ggel_mask_id, ggel_mask, ssample_code);
            acttem.node = nodetem;
            return acttem;
        }
        public static HandTakeGelFromWare create(double ttime,GelWarehouseDevice ggelwareDevice, int ggel_mask_id, string ggel_mask, string ssample_code)
        {
            HandTakeGelFromWare acttem = new HandTakeGelFromWare();
            acttem.gelwareDevice = ggelwareDevice;
            acttem.gel_mask_id = ggel_mask_id;
            acttem.gel_mask = ggel_mask;
            acttem.sample_code = ssample_code;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "卡仓抓卡和扫描";
        }
        public override void init()
        {
            isinit = true;
            sumdt = 0;
            step = 0;
        }
        public override void run(double dt)
        {
            switch (step)
            {
                case 0:
                    {
                        var handDevice = (MachineHandDevice)node;
                        take_seat = ResManager.getInstance().GetResByCode(gel_mask + "*", "T_BJ_GelWarehouse", "", "");
                        if (take_seat == null)
                        {
                            bool is_find = false;
                            int box_index = 0;
                            if (ware_seat_dic.ContainsKey(gel_mask)) box_index = ware_seat_dic[gel_mask];
                            for (int i = 0; i < ResManager.getInstance().gelwarehouse_list.Count; i++)
                            {
                                for (int j = 0; j < ResManager.getInstance().gelwarehouse_list[i].Values.Length; j++)
                                {
                                    int box_index_tem = (i + box_index) % ware_seat_rack_name.Length;
                                    take_seat = ResManager.getInstance().SearchGelCard("T_BJ_GelWarehouse", ware_seat_rack_name[box_index_tem], "", j, 0, false);
                                    if (take_seat != null && take_seat_list.Find((ResInfoData infotem) => infotem == take_seat) == null)
                                    {
                                        if (take_seat.codes.Count == 0 || take_seat.FindCode(gel_mask + "*", false))
                                        {
                                            if (ware_seat_dic.ContainsKey(gel_mask))
                                                ware_seat_dic[gel_mask] = box_index_tem;
                                            else
                                                ware_seat_dic.Add(gel_mask, box_index_tem);
                                            take_seat_list.Add(take_seat);
                                            is_find = true;
                                            break;
                                        }
                                    }
                                }
                                if (is_find) break;
                            }

                            if (is_find==false)
                            {
                                var ret = ErrorSystem.WriteActError("卡仓无:" + ResManager.getInstance().GetGelTestByMask(gel_mask).GelName+"请及时补卡",true,true,999);
                                if(ret)
                                {
                                    scan_waregel = gelwareDevice.InitAll();
                                    step = 6;
                                    break;
                                }
                                else
                                {
                                    is_no_card = true;
                                    istimeout = true;
                                    isdelete = true;
                                    return;
                                }
                            }
                        }
                        take_seque = Sequence.create();
                        var move_act = Spawn.create(
                               MoveTo.create(gelwareDevice, 3001, take_seat.StoreX, -1, -1),
                               MoveTo.create(handDevice, 3001, take_seat.X, take_seat.Y));
                        take_seque.AddAction(move_act);
                        //抓手抓卡
                        var take_act = HandTakeCard.create(handDevice, 3001, take_seat.Z, take_seat.ZLimit, take_seat.ZCatch, 0);
                        take_act.is_fromware = true;
                        take_act.store_x = take_seat.StoreX;
                        take_seque.AddAction(take_act);
                        //扫描卡
                        var scan_seat = ResManager.getInstance().GetScaner("1");
                        var scan_act = HandScanCard.create(handDevice, 3001, (int)scan_seat.HandX, (int)scan_seat.HandY, (int)scan_seat.HandY, (int)scan_seat.HandZ);
                        take_seque.AddAction(scan_act);
                        scan_act.successfun = (ActionBase act) =>
                        {
                            var gel = ResManager.getInstance().GetGelTestByMask(scan_act.scan_code);
                            ResManager.getInstance().SetGelMaskByID(gel_mask_id, scan_act.scan_code);
                            take_seat.SetCode(scan_act.scan_code);
                            take_seat.sampleinfo = ResManager.getInstance().GetSampleInfo(sample_code);
                            take_seat.gel = gel;
                            return true;
                        };
                        //回到z0
                        movez_seque = MoveTo.create(handDevice, 3000, -1, -1, 0);
                        movez_seque.init();
                        //把测试卡放在卡仓Gel位中
                        var geltem = take_seat.Values[take_seat.CountX, 0];
                        take_seat.Values[take_seat.CountX, 0] = null;
                        this.successfun = (ActionBase act) =>
                        {
                            ResManager.getInstance().handseat_resinfo = (ResInfoData)geltem;
                            return true;
                        };
                        this.destroyfun = (ActionBase act) =>
                        {
                            if(take_seat!=null)
                            take_seat.Values[take_seat.CountX, 0] = geltem;
                            return true;
                        };
                        step++;
                    }
                    break;
                case 1:
                    take_seque.run(dt);
                    istimeout = take_seque.istimeout;
                    isdelete = take_seque.getIsDelete();
                    if (take_seque.getIsFinish()) step++;
                    break;
                case 2:
                    if (take_seat.GetCode(gel_mask + "*") != "")
                    {
                        step = 3;
                    }
                    else
                    {
                        step = 4;
                    }
                    break;
                case 3:
                    movez_seque.run(dt);
                    istimeout = movez_seque.istimeout;
                    isdelete = movez_seque.getIsDelete();
                    if (movez_seque.getIsFinish()) isfinish = true;
                    break;
                case 4:
                    {
                        //如果卡不对放回原来位
                        putback_seque = Sequence.create();
                        ActionGenerater.getInstance().GeneratePutGelToWare(take_seat, take_seat, ref putback_seque);
                        ware_seat_dic[gel_mask]++;
                        step++;
                    }
                    break;
                case 5:
                    putback_seque.run(dt);
                    istimeout = putback_seque.istimeout;
                    isdelete = putback_seque.getIsDelete();
                    if (putback_seque.getIsFinish())
                    {
                        init();
                    }
                    break;
                case 6:
                    //重新扫描卡
                    scan_waregel.run(dt);
                    istimeout = scan_waregel.istimeout;
                    isdelete = scan_waregel.getIsDelete();
                    if (scan_waregel.getIsFinish())
                    {
                        init();
                    }
                    break;
            }
        }
        public static HandTakeGelFromWare getInstance()
        {
            if (instance == null) instance = new HandTakeGelFromWare();
            return instance;
        }
    }

    public class HandPutGelToNormal : ActionGroup
    {
        public Sequence put_seque { get; set; } = null;
        public ResInfoData put_seat { get; set; } = null;
        public GelWarehouseDevice gelwareDevice { get; set; } = null;
        public static HandPutGelToNormal instance = null;
        public static HandPutGelToNormal create(MachineHandDevice nodetem, double ttime, GelWarehouseDevice ggelwareDevice, ResInfoData pput_seat)
        {
            HandPutGelToNormal acttem = create(ttime, ggelwareDevice, pput_seat);
            acttem.node = nodetem;
            return acttem;
        }
        public static HandPutGelToNormal create(double ttime, GelWarehouseDevice ggelwareDevice, ResInfoData pput_seat)
        {
            HandPutGelToNormal acttem = new HandPutGelToNormal();
            acttem.gelwareDevice = ggelwareDevice;
            acttem.put_seat = pput_seat;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "放卡到普通位";
        }
        public override void init()
        {
            isinit = true;
            sumdt = 0;
            step = 0;
        }
        public override void run(double dt)
        {
            switch (step)
            {
                case 0:
                    {
                        var handDevice = (MachineHandDevice)node;
                        var put_gel = ResManager.getInstance().handseat_resinfo;
                        put_seque = Sequence.create();
                        //抓手移动
                        put_seque.AddAction(MoveTo.create(handDevice, 3001, (int)put_seat.X, (int)(put_seat.Y), 0));
                        //抓手放卡
                        var put_act = HandPutCard.create(handDevice, 3001, (int)put_seat.ZPut, 0);
                        put_seque.AddAction(put_act);
                        //把测试卡放在Gel位中
                        if (put_seat.Values != null)
                            put_seat.Values[put_seat.CountX, 0] = put_gel;
                        if (put_gel != null)
                            put_gel.PutOk = false;
                        this.successfun = (ActionBase act) =>
                        {
                            if (put_gel != null)
                            {
                                put_gel.PutOk = true;
                                put_gel.SetSeatInfo(put_seat);
                            }
                            ResManager.getInstance().handseat_resinfo = null;
                            return true;
                        };
                        this.destroyfun = (ActionBase act) =>
                        {
                            if (put_seat.Values != null)
                                put_seat.Values[put_seat.CountX, 0] = null;
                            ResManager.getInstance().handseat_resinfo = null;
                            return true;
                        };
                        step++;
                    }
                    break;
                case 1:
                    put_seque.run(dt);
                    istimeout = put_seque.istimeout;
                    isdelete = put_seque.getIsDelete();
                    if (put_seque.getIsFinish()) step++;
                    break;
                case 2:
                    isfinish = true;
                    break;
            }
        }
        public static HandPutGelToNormal getInstance()
        {
            if (instance == null) instance = new HandPutGelToNormal();
            return instance;
        }
    }


    public class ScanGelFromWare : ActionGroup
    {
        public GelWarehouseDevice gelwareDevice { get; set; } = null;
        public string[] ware_seat_rack_name = { "box001", "box003", "box005", "box007",};
        public int[] scan_box = { 0, 1, 2, 3 };
        public int scan_setp = 0;
        public int scan_boxindex = 0;
        public bool is_find = false;
        public Sequence putback_seque = null;
        public Sequence take_seque { get; set; } = null;
        public ActionBase movez_seque { get; set; } = null;
        public ResInfoData take_seat { get; set; } = null;
        public ActionBase scan_waregel { get; set; } = null;
        public ActionBase move_zero_seque { get; set; } = null;
        public static ScanGelFromWare instance = null;
        public static ScanGelFromWare create(MachineHandDevice nodetem, double ttime, GelWarehouseDevice ggelwareDevice)
        {
            ScanGelFromWare acttem = create(ttime, ggelwareDevice);
            acttem.node = nodetem;
            return acttem;
        }
        public static ScanGelFromWare create(double ttime, GelWarehouseDevice ggelwareDevice)
        {
            ScanGelFromWare acttem = new ScanGelFromWare();
            acttem.gelwareDevice = ggelwareDevice;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "卡仓抓卡和扫描";
        }
        public override void init()
        {
            isinit = true;
            sumdt = 0;
            step = 0;
        }
        public override void run(double dt)
        {
            switch (step)
            {
                case 0:
                    {
                        var handDevice = (MachineHandDevice)node;
                        take_seat = null;
                        for (; scan_boxindex < 4; scan_boxindex++)
                        {
                            for (scan_setp = 0; scan_setp < 24; scan_setp++)
                            {
                                take_seat = ResManager.getInstance().SearchGelCard("T_BJ_GelWarehouse", ware_seat_rack_name[scan_boxindex], "", scan_setp, 0, false);
                                if (take_seat != null) break;
                            }
                            if (take_seat != null) {
                                scan_boxindex++;
                                is_find = true;
                                break;
                            };
                        }
                        if (is_find==false)
                        {
                            istimeout = true;
                            isdelete = true;
                            ErrorSystem.WriteActError("卡仓无卡实验已取消", true, false, 3);
                            return;
                        }else if(take_seat==null)
                        {
                            step = 7;
                            return;
                        }
                        take_seque = Sequence.create();
                        var move_act = Spawn.create(
                               MoveTo.create(gelwareDevice, 3001, take_seat.StoreX, -1, -1),
                               MoveTo.create(handDevice, 3001, take_seat.X, take_seat.Y));
                        take_seque.AddAction(move_act);
                        //抓手抓卡
                        var take_act = HandTakeCard.create(handDevice, 3001, take_seat.Z, take_seat.ZLimit, take_seat.ZCatch, 0);
                        take_act.is_fromware = true;
                        take_act.store_x = take_seat.StoreX;
                        take_seque.AddAction(take_act);
                        //扫描卡
                        var scan_seat = ResManager.getInstance().GetScaner("1");
                        var scan_act = HandScanCard.create(handDevice, 3001, (int)scan_seat.HandX, (int)scan_seat.HandY, (int)scan_seat.HandY, (int)scan_seat.HandZ);
                        take_seque.AddAction(scan_act);
                        scan_act.successfun = (ActionBase act) =>
                        {
                            var gel = ResManager.getInstance().GetGelTestByMask(scan_act.scan_code);
                            take_seat.SetCode(scan_act.scan_code);
                            take_seat.gel = gel;
                            //记录卡
                            if(scan_act.scan_code.Length>10)
                            {
                                var gel_mask = scan_act.scan_code.Substring(0, 10);
                                if (!HandTakeGelFromWare.ware_seat_dic.ContainsKey(gel_mask))
                                    HandTakeGelFromWare.ware_seat_dic.Add(gel_mask, scan_boxindex);
                            }
                            return true;
                        };
                        //回到z0
                        movez_seque = MoveTo.create(handDevice, 3000, -1, -1, 0);
                        movez_seque.init();
                        //把测试卡放在卡仓Gel位中
                        var geltem = take_seat.Values[take_seat.CountX, 0];
                        take_seat.Values[take_seat.CountX, 0] = null;
                        this.successfun = (ActionBase act) =>
                        {
                            ResManager.getInstance().handseat_resinfo = (ResInfoData)geltem;
                            return true;
                        };
                        this.destroyfun = (ActionBase act) =>
                        {
                            take_seat.Values[take_seat.CountX, 0] = geltem;
                            return true;
                        };
                        //抓手回零
                        move_zero_seque = MoveTo.create(handDevice,3000, 0, 0, 0);
                        move_zero_seque.init();
                        step++;
                    }
                    break;
                case 1:
                    take_seque.run(dt);
                    istimeout = take_seque.istimeout;
                    isdelete = take_seque.getIsDelete();
                    if (take_seque.getIsFinish()) step++;
                    break;
                case 2:
                    if (scan_boxindex >= 3&& scan_setp>=23)
                    {
                        step = 3;
                    }
                    else
                    {
                        step = 4;
                    }
                    break;
                case 3:
                    movez_seque.run(dt);
                    istimeout = movez_seque.istimeout;
                    isdelete = movez_seque.getIsDelete();
                    if (movez_seque.getIsFinish()) isfinish = true;
                    break;
                case 4:
                    {
                        //如果卡不对放回原来位
                        putback_seque = Sequence.create();
                        ActionGenerater.getInstance().GeneratePutGelToWare(take_seat, take_seat, ref putback_seque);
                        step++;
                    }
                    break;
                case 5:
                    putback_seque.run(dt);
                    istimeout = putback_seque.istimeout;
                    isdelete = putback_seque.getIsDelete();
                    if (putback_seque.getIsFinish())
                    {
                        init();
                        if (scan_boxindex >= 3 && scan_setp >= 23)
                        {
                            step = 7;
                        }
                        
                    }
                    break;
                case 6:
                    //重新扫描卡
                    scan_waregel.run(dt);
                    istimeout = scan_waregel.istimeout;
                    isdelete = scan_waregel.getIsDelete();
                    if (scan_waregel.getIsFinish())
                    {
                        init();
                    }
                    break;
                case 7:
                    move_zero_seque.run(dt);
                    istimeout = move_zero_seque.istimeout;
                    isdelete = move_zero_seque.getIsDelete();
                    if (move_zero_seque.getIsFinish())
                    {
                        isfinish = true;
                    }
                    break;
            }
        }
        public static ScanGelFromWare getInstance()
        {
            if (instance == null) instance = new ScanGelFromWare();
            return instance;
        }
    }


    public class InjectCheckTip : ActionGroup
    {
        public double lasttime { get; set; } = 0;
        public ActionBase act_move { get; set; } = null;
        public List<int> point_list = new List<int>();
        //针头序号
        public int inject_index { get; set; } = 0;
        //最小值气压
        public int minper { get; set; } = 0;
        public static InjectCheckTip instance = null;
        public static InjectCheckTip create(AbstractCanDevice nodetem, double ttime, int mminper, int index)
        {
            InjectCheckTip acttem = InjectCheckTip.create(ttime,mminper,index);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectCheckTip create(double ttime,int mminper, int index)
        {
            InjectCheckTip acttem = new InjectCheckTip();
            acttem.minper = mminper;
            acttem.inject_index = index;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "针头检测";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
        }
        public override void run(double dt)
        {
            if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            var inject = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    {
                        int[] a_value = { -1, -1, -1, -1 };
                        int s_value = (int)inject.Injector.Entercloses[inject_index].PumpMotor.Speed.SetValue;
                        inject.CanComm.SetCoil(inject.Injector.Entercloses[inject_index].PumpMotor.PressureSwitch.Addr, true);
                        a_value[inject_index] = (int)0;
                        act_move = InjectAbsorbMove.create(node, time, inject.GetSeleteced(), s_value, a_value);
                        act_move.init();
                        step++;
                    }
                    break;
                case 1:
                    act_move.run(dt);
                    if (act_move.getIsFinish())
                    {
                        int[] a_value = { -1, -1, -1, -1 };
                        int s_value = (int)inject.Injector.Entercloses[inject_index].PumpMotor.Speed.SetValue;
                        inject.CanComm.SetCoil(inject.Injector.Entercloses[inject_index].PumpMotor.PressureSwitch.Addr, true);
                        a_value[inject_index] = (int)(inject.Injector.Entercloses[inject_index].PumpMotor.Maximum.SetValue);
                        act_move = InjectAbsorbMove.create(node, time, inject.GetSeleteced(), s_value, a_value);
                        act_move.init();
                        step++;
                    }
                    break;
                case 2:
                    act_move.run(dt);
                    if (sumdt - lasttime > 10)
                    {
                        lasttime = sumdt;
                        int pressure = inject.GetPressure(inject_index);
                        point_list.Add(pressure);
                        if (point_list.Count > 100) point_list.RemoveAt(0);
                    }
                    if (act_move.getIsFinish())
                    {
                        int[] a_value = { -1, -1, -1, -1 };
                        int s_value = (int)inject.Injector.Entercloses[inject_index].PumpMotor.Speed.SetValue;
                        a_value[inject_index] = 0;
                        act_move = InjectAbsorbMove.create(node, time, inject.GetSeleteced(), s_value, a_value);
                        act_move.init();
                        step++;
                    }
                    break;
                case 3:
                    act_move.run(dt);
                    if (sumdt - lasttime > 100)
                    {
                        lasttime = sumdt;
                        int pressure = inject.GetPressure(inject_index);
                        point_list.Add(pressure);
                        if (point_list.Count > 100) point_list.RemoveAt(0);
                    }
                    if (act_move.getIsFinish())
                    {
                        
                        step++;
                    }
                    break;
                case 4:
                    int min = 4000;
                    int max = 0;
                    int count = 0;
                    foreach(var point in point_list)
                    {
                        if (point > max) max = point;
                        if (point < min) min = point;
                        count += point;
                    }
                    int absval = max - min;
                    count = count - max- min;
                    double mean = count / (point_list.Count()-2);
                    isfinish = true;
                    inject.CanComm.SetCoil(inject.Injector.Entercloses[inject_index].PumpMotor.PressureSwitch.Addr, false);
                    break;
            }
        }
        public static InjectCheckTip getInstance()
        {
            if (instance == null) instance = new InjectCheckTip();
            return instance;
        }
    }

    public class InjectTakeTip : ActionGroup
    {
        public TakeTipData tipdata;
        public int z { get; set; } = 0;
        public int zb { get; set; } = 0;
        public ActionBase act_movexy { get; set; } = null;
        public ActionBase act_movezb { get; set; } = null;
        //针头序号
        public int head_index { get; set; } = 0;
        public Enterclose[] injects { get; set; } = null;
        public List<int> point_list = new List<int>();
        public static InjectTakeTip instance = null;
        public static InjectTakeTip create(AbstractCanDevice nodetem, double ttime, int zz,int zzb,int index, TakeTipData ttipdata)
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
            var inject = (InjectorDevice)node;
            injects = new Enterclose[tipdata.Count];
            for (int i = 0; i < tipdata.Count; i++)
            {
                injects[i] = inject.Injector.Entercloses[head_index+i];
            }
            act_movexy = MoveTo.create(node,time, tipdata.X, tipdata.Y, -1);
            int[] y_value = { -1, -1, -1, -1 };
            int[] z_value = { zb, zb, zb, zb };
            act_movezb = InjectMoveTo.create(node, time, injects, - 1, y_value, z_value);
            act_movexy.init();
            act_movezb.init();
        }
        public override void run(double dt)
        {
            bool resultz = true;
            if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            var inject = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    //是否已有吸头
                    //针头数据检测(针头序号+装针个数不能大于通道个数)
                    int inject_count = inject.GetSeleteced().Count();
                    if (head_index+ tipdata.Count<= inject_count)
                    {
                        step++;
                    }
                    else
                    {
                        istimeout = true;
                        isdelete = true;
                        errmsg = "针头号超出";
                    }
                    break;
                case 1:
                    act_movexy.run(dt);
                    if (act_movexy.getIsFinish()) step++;
                    break;
                case 2:
                    int[] z_tem = { -1, -1, -1, -1 };
                    foreach (var ent in injects)
                    z_tem[ent.Index] = z;
                    resultz = device.MoveZ(injects,0,IMask.Gen(-1), z_tem);
                    if (resultz) step++;
                    break;
                case 3:
                    if (device.DoneZ(injects,0)) step++;
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


    public class InjectPutTip : ActionGroup
    {
        public TakeTipData tipdata;
        public int z { get; set; } = 0;
        public int zb { get; set; } = 0;
        public int xf { get; set; } = 0;
        public ActionBase act_movexy { get; set; } = null;
        public ActionBase act_movexyf { get; set; } = null;
        public ActionBase act_movezt { get; set; } = null;
        public ActionBase act_movezb { get; set; } = null;
        //针头序号
        public int head_index { get; set; } = 0;
        public Enterclose[] injects { get; set; } = null;
        public List<int> point_list = new List<int>();
        public static InjectPutTip instance = null;
        public static InjectPutTip create(AbstractCanDevice nodetem, double ttime, int zz, int zzb,int xxf, int index, TakeTipData ttipdata)
        {
            InjectPutTip acttem = InjectPutTip.create(ttime, zz, zzb, xxf, index, ttipdata);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectPutTip create(double ttime, int zz, int zzb, int xxf, int index, TakeTipData ttipdata)
        {
            InjectPutTip acttem = new InjectPutTip();
            acttem.z = zz;
            acttem.zb = zzb;
            acttem.xf = xxf;
            acttem.tipdata = ttipdata;
            acttem.head_index = index;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "脱tip头";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            var inject = (InjectorDevice)node;
            injects = new Enterclose[tipdata.Count];
            for (int i = 0; i < tipdata.Count; i++)
            {
                injects[i] = inject.Injector.Entercloses[head_index + i];
            }
            act_movexy = MoveTo.create(node, time, tipdata.X, tipdata.Y, -1);
            act_movexyf = MoveTo.create(node, time, xf, -1, -1);
            int[] y_value = { -1, -1, -1, -1 };
            int[] z_value = { zb, zb, zb, zb };
            int[] z0_value = { 0, 0, 0, 0 };
            act_movezt = InjectMoveTo.create(node, time, injects, -1, y_value, z_value,2);
            act_movezb = InjectMoveTo.create(node, time, injects, -1, y_value, z0_value);
            act_movexy.init();
            act_movexyf.init();
            act_movezt.init();
            act_movezb.init();
        }
        public override void run(double dt)
        {
            bool resultz = true;
            if (CountTime(dt)) return;
            var device = new ActionDevice(node);
            var inject = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    //是否已有吸头
                    //针头数据检测(针头序号+装针个数不能大于通道个数)
                    int inject_count = inject.GetSeleteced().Count();
                    if (head_index + tipdata.Count <= inject_count)
                    {
                        step++;
                    }
                    else
                    {
                        istimeout = true;
                        isdelete = true;
                        errmsg = "针头号超出";
                    }
                    break;
                case 1:
                    act_movexy.run(dt);
                    if (act_movexy.getIsFinish()) step++;
                    break;
                case 2:
                    int[] z_tem = { -1, -1, -1, -1 };
                    foreach (var ent in injects)
                        z_tem[ent.Index] = z;
                    resultz = device.MoveZ(injects, 0,IMask.Gen(-1), z_tem);
                    if (resultz) step++;
                    break;
                case 3:
                    if (device.DoneZ(injects,0)) step++;
                    break;
                case 4:
                    act_movexyf.run(dt);
                    if (act_movexyf.getIsFinish()) step++;
                    break;
                case 5:
                    act_movezt.run(dt);
                    if (act_movezt.getIsFinish()) step++;
                    break;
                case 6:
                    act_movezb.run(dt);
                    if (act_movezb.getIsFinish()) step++;
                    break;
                case 7:
                    isfinish = true;
                    break;
            }
        }
        public static InjectPutTip getInstance()
        {
            if (instance == null) instance = new InjectPutTip();
            return instance;
        }
    }

    public class InjectDetector : ActionGroup
    {
        public int[] z { get; set; } = IMask.Gen(0);//初始位
        public int[] zl { get; set; } = IMask.Gen(0);//探测最大位
        public int[] zd { get; set; } = IMask.Gen(0);//探测后加深距离
        public int[] zdp = IMask.Gen(0);//最后加深后的位置
        public int[] zliquid = IMask.Gen(0);//液面位置
        public int[] zdrain = IMask.Gen(0);//排空位置
        public bool[] detectorok { get; set; } = { true, true, true, true };
        public int speedf { get; set; } = 0;
        public int speeds { get; set; } = 0;
        public double detectortime { get; set; } = 0;
        public double zl_dt { get; set; } = 0;//z轴等待时间计算
        public double zl_wait { get; set; } = 0;//z轴等待时间
        public List<Enterclose> injects_detect { get; set; } = new List<Enterclose>();
        public Enterclose[] injects { get; set; } = null;
        public ActionBase act_movez { get; set; } = null;
        public InjectMoveTo act_movezl { get; set; } = null;
        public InjectMoveTo act_moveztry_again_up { get; set; } = null;//气压泵走完但z没走完
        public InjectMoveTo act_moveztry_again_down { get; set; } = null;//气压泵走完但z没走完
        public Sequence act_movezback { get; set; } = null;//跑回零点
        public InjectAbsorbMove act_absori { get; set; } = null;
        public InjectAbsorbMove act_absord { get; set; } = null;
        public ActionBase act_absordeep { get; set; } = null;
        public ActionBase act_drainz { get; set; } = null;//排空位置
        public ActionBase act_absorf { get; set; } = null;
        public static List<DetectoPoint> detector_list = new List<DetectoPoint>();
        public List<int>[] point_list = { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
        public static InjectDetector instance = null;
        public static InjectDetector create(AbstractCanDevice nodetem, double ttime, Enterclose[] iinjects,int[] zz, int[] zzl, int[] zzd, int sspeedf = 0,int sspeeds = 1)
        {
            InjectDetector acttem = InjectDetector.create(ttime, iinjects, zz,zzl, zzd, sspeedf, sspeeds);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectDetector create(double ttime, Enterclose[] iinjects, int[] zz, int[] zzl, int[] zzd, int sspeedf = 0, int sspeeds = 1)
        {
            InjectDetector acttem = new InjectDetector();
            acttem.time = ttime;
            acttem.injects = iinjects;
            for (int i = 0; i < 4; i++)
            {
                acttem.z[i] = zz[i];
                acttem.zl[i] = zzl[i];
                acttem.zd[i] = zzd[i];
            }
            acttem.speedf = sspeedf;
            acttem.speeds = sspeeds;
            for(int i=0;i<acttem.injects.Length;i++)
            {
                acttem.injects_detect.Add(acttem.injects[i]);
            }
            return acttem;
        }
        public override string getName()
        {
            return "液面探测";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            zl_wait = 100;
            zl_dt = 0;
            int[] absorbs = IMask.Gen(-1);
            var device = new ActionDevice(node);
            //初始化
            foreach (var ent in injects_detect)
            {
                absorbs[ent.Index] = (int)ent.PumpMotor.Maximum.SetValue;
                detectorok[ent.Index] = false;
                point_list[ent.Index].Clear();
            }
            act_absori = InjectAbsorbMove.create(time, injects_detect.ToArray(), 100, absorbs);
            act_absori.init();
            act_absori.node = node;
            //探测优化
            int rx = 0;
            int []ry = {0,0,0,0};
            int[] pz = { z[0], z[1], z[2], z[3] };

            if (device.GetRealX(ref rx) && device.GetRealY(injects_detect.ToArray(), ref ry))
            {
                foreach (var ent in injects_detect)
                {
                    int dx = rx;
                    int dy = ry[ent.Index] + (int)ent.TipDis;
                    foreach(var point in detector_list)
                    {
                        if(dx== point.x&&Math.Abs(dy- point.y)<100)
                        {
                            pz[ent.Index] = pz[ent.Index] + point.z;
                        }
                    }
                }
            }

            act_movez = InjectMoveTo.create(time, injects_detect.ToArray(), -1, IMask.Gen(-1), pz, speedf);
            act_movez.init();
            act_movez.node = node;

            act_movezback = Sequence.create();
            act_movezback.AddAction(InjectMoveTo.create(time, injects_detect.ToArray(), -1, IMask.Gen(-1), IMask.Gen(0), speedf));
            act_movezback.AddAction(InitXyz.create(time, injects_detect.ToArray(), false, false, true));
            act_movezback.init();
            act_movezback.node = node;

            //探测液体
            act_movezl = InjectMoveTo.create(time, injects_detect.ToArray(), -1, IMask.Gen(-1), zl, speeds);
            act_movezl.init();
            act_movezl.node = node;

            foreach (var ent in injects_detect)
            {
                absorbs[ent.Index] = 0;
            }
            act_absord = InjectAbsorbMove.create(time, injects_detect.ToArray(), 1, absorbs);
            act_absord.init();
            act_absord.node = node;
            //探测加深

            //完成
            foreach (var ent in injects)
            {
                absorbs[ent.Index] = (int)ent.PumpMotor.Maximum.SetValue;
            }
            act_absorf = InjectAbsorbMove.create(time, injects, 1, absorbs);
            act_absorf.init();
            act_absorf.node = node;
        }
        public override void run(double dt)
        {
            bool resultz = true;
            bool resultp = true;
            bool resuldz = true;
            CountTime(dt);
            var device = new ActionDevice(node);
            var inject = (InjectorDevice)node;
            switch (step)
            {
                case 100://Z轴回当前上2000
                    if (act_moveztry_again_up.getIsFinish() == false)
                        act_moveztry_again_up.run(dt);
                    istimeout = act_moveztry_again_up.istimeout;
                    isdelete = act_moveztry_again_up.getIsDelete();
                    if (act_moveztry_again_up.getIsFinish())
                        step++;
                    break;
                case 101://Z轴回上次位置
                    if (act_absori.getIsFinish() == false)
                        act_absori.run(dt);
                    istimeout = act_absori.istimeout;
                    isdelete = act_absori.getIsDelete();
                    if (act_absori.getIsFinish())
                        step++;
                    break;
                case 102://重新初始化抽液
                    if (act_moveztry_again_down.getIsFinish() == false)
                        act_moveztry_again_down.run(dt);
                    istimeout = act_moveztry_again_down.istimeout;
                    isdelete = act_moveztry_again_down.getIsDelete();
                    if (act_moveztry_again_down.getIsFinish())
                        step = 1;
                    break;
                case 0:
                    if(act_absori.getIsFinish()==false)
                    act_absori.run(dt);
                    if(act_movez.getIsFinish()==false)
                    act_movez.run(dt);
                    istimeout = act_absori.istimeout || act_movez.istimeout;
                    isdelete = act_absori.getIsDelete() || act_movez.getIsDelete();
                    if (act_absori.getIsFinish()&& act_movez.getIsFinish())
                    step++;
                    break;
                case 1:
                    zl_dt += dt;
                    if (act_absord.getIsFinish() == false)
                        act_absord.run(dt);
                    if (act_movezl.getIsFinish() == false&& zl_dt>=zl_wait)
                        act_movezl.run(dt);
                    istimeout = act_absord.istimeout || act_movezl.istimeout;
                    isdelete = act_absord.getIsDelete() || act_movezl.getIsDelete();
                    //探测代码
                    detectortime += dt;
                    if (detectortime>50)
                    {
                        detectortime = 0;
                        foreach(var ent in injects_detect)
                        {
                            if(detectorok[ent.Index]==false)
                            {
                                node.CanComm.ReadRegister(ent.PumpMotor.Pressure.Addr);
                                int pressure = inject.GetPressure(ent);//node.CanComm.GetInt(.PumpMotor.Pressure.Addr, -1);
                                if (pressure != -1) point_list[ent.Index].Add(pressure);
                                int count_tem = point_list[ent.Index].Where(item => item <= ent.Pressure.SetValue).Count();
                                if (count_tem >= 2)
                                {
                                    var ret_real = device.GetRealZ(inject, inject.Injector.Entercloses.ToList().GetRange(ent.Index, 1).ToArray(), ref zliquid);
                                    resultz = inject.CanComm.StopMotor(inject.Injector.Entercloses[ent.Index].ZMotor);
                                    resultp = inject.CanComm.StopMotor(inject.Injector.Entercloses[ent.Index].PumpMotor);
                                    zdp[ent.Index] = zliquid[ent.Index] + zd[ent.Index];
                                    zdrain[ent.Index] = zliquid[ent.Index] - 2000;
                                    if (zdrain[ent.Index] < 0) zdrain[ent.Index] = 0;
                                    resuldz = ret_real;
                                    if (resultz && resultp && resuldz)
                                    {
                                        detectorok[ent.Index] = true;
                                        act_absordeep = InjectMoveTo.create(time, injects, -1, IMask.Gen(-1), zdp);
                                        act_absordeep.init();
                                        act_absordeep.node = node;

                                        act_drainz = InjectMoveTo.create(time, injects, -1, IMask.Gen(-1), zdrain);
                                        act_drainz.init();
                                        act_drainz.node = node;
                                        //探测优化
                                        int rx = 0;
                                        int[] ry = { 0, 0, 0, 0 };
                                        int[] rz = { 0, 0, 0, 0 };
                                        if (device.GetRealX(ref rx) && device.GetRealY(injects_detect.ToArray(), ref ry) && device.GetRealZ(injects_detect.ToArray(), ref rz))
                                        {
                                            int dx = rx;
                                            int dy = ry[ent.Index] + (int)ent.TipDis;
                                            bool is_find = false;
                                            foreach (var point in detector_list)
                                            {
                                                if (dx == point.x && Math.Abs(dy - point.y) < 100)
                                                {
                                                    point.z = rz[ent.Index] - z[ent.Index] - 500;
                                                    is_find = true;
                                                    break;
                                                }
                                            }
                                            if (is_find == false)
                                            {
                                                detector_list.Add(new DetectoPoint(dx, dy, rz[ent.Index] - z[ent.Index] - 500));
                                            }
                                        }
                                        /*
                                        if (device.GetRealX(ref rx) && device.GetRealY(injects_detect.ToArray(), ref ry) && device.GetRealZ(injects_detect.ToArray(), ref rz))
                                        {
                                            foreach (var enttem in injects_detect)
                                            {
                                                string key_str = "x:" + rx + "y:" + (int)(ry[enttem.Index] + enttem.TipDis);
                                                if (detector_dic.ContainsKey(key_str))
                                                {
                                                    detector_dic[key_str] = rz[enttem.Index] - z[enttem.Index] - 500;
                                                }
                                                else
                                                {
                                                    detector_dic.Add(key_str, rz[enttem.Index] - z[enttem.Index] - 500);
                                                }
                                            }
                                        }
                                        */
                                    }
                                }
                            }
                        }
                    }
                    if(detectorok[0]&& detectorok[1]&& detectorok[2]&& detectorok[3])
                    {
                        step = 3;
                    }else if(act_movezl.getIsFinish()==false&& act_absord.getIsFinish())
                    {
                        foreach (var ent in injects_detect)
                        {
                            inject.CanComm.StopMotor(inject.Injector.Entercloses[ent.Index].ZMotor);
                        }

                        for (int i = injects_detect.Count - 1; i >= 0; i--)
                        {
                            var ent = injects_detect[i];
                            if(detectorok[ent.Index])
                            {
                                injects_detect.Remove(ent);
                            }
                        }
                        act_absord.injects = injects_detect.ToArray();
                        act_movezl.injects = injects_detect.ToArray();
                        act_absori.injects = injects_detect.ToArray();
                        for(int i=0;i< act_movezback.actionlist.Count();i++)
                        {
                           if( act_movezback.actionlist[i] is InjectMoveTo)
                           {
                                ((InjectMoveTo)act_movezback.actionlist[i]).injects = injects_detect.ToArray();
                           }else if (act_movezback.actionlist[i] is InitXyz)
                           {
                                ((InitXyz)act_movezback.actionlist[i]).injects = injects_detect.ToArray();
                           }
                        }
                        var rz = IMask.Gen(0);
                        var ztry_again_up = IMask.Gen(0);
                        var ztry_again_down = IMask.Gen(0);
                        device.GetRealZ(injects_detect.ToArray(), ref rz);
                        foreach (var ent in injects_detect)
                        {
                            ztry_again_up[ent.Index] = rz[ent.Index]-3000;
                            ztry_again_down[ent.Index] = rz[ent.Index];
                        }
                        act_moveztry_again_up = InjectMoveTo.create(time, injects_detect.ToArray(), -1, IMask.Gen(-1), ztry_again_up, speedf);
                        act_moveztry_again_up.init();
                        act_moveztry_again_up.node = node;

                        act_moveztry_again_down = InjectMoveTo.create(time, injects_detect.ToArray(), -1, IMask.Gen(-1), ztry_again_down, speedf);
                        act_moveztry_again_down.init();
                        act_moveztry_again_down.node = node;

                        act_absord.init();
                        act_movezl.init();
                        act_absori.init();
                        act_absord.isfinish = false;
                        act_movezl.isfinish = false;
                        act_absori.isfinish = false;
                        zl_dt = 0;
                        step = 100;
                    }
                    else if (act_movezl.getIsFinish())
                    {
                        foreach (var ent in injects_detect)
                        {
                            inject.CanComm.StopMotor(inject.Injector.Entercloses[ent.Index].ZMotor);
                            inject.CanComm.StopMotor(inject.Injector.Entercloses[ent.Index].PumpMotor);
                        }
                        step = 2;
                    }
                        
                    break;
                case 2:
                    resultz = ErrorSystem.WriteActError("无液体！code:"+ detectorok[0]+ detectorok[1]+ detectorok[2]+ detectorok[3]);
                    if (resultz)
                    {
                        istimeout = false;
                        isfinish = false;
                        int rx = 0;
                        int[] ry = { 0, 0, 0, 0 };
                        if (device.GetRealX(ref rx) && device.GetRealY(injects_detect.ToArray(), ref ry))
                        {
                            foreach (var enttem in injects_detect)
                            {
                                int dx = rx;
                                int dy = ry[enttem.Index] + (int)enttem.TipDis;
                                foreach (var point in detector_list)
                                {
                                    if (dx == point.x && Math.Abs(dy - point.y) < 100)
                                    {
                                        detector_list.Remove(point);
                                        break;
                                    }
                                }
                            }
                        }
                        for (int i = injects_detect.Count - 1; i >= 0; i--)
                        {
                            var ent = injects_detect[i];
                            if (detectorok[ent.Index])
                            {
                                injects_detect.Remove(ent);
                            }
                        }
                        step = 6;
                    }
                    else
                    {
                        istimeout = true;
                        isdelete = true;
                        errmsg = "无液体!";
                        return;
                    }
                    break;
                case 3:
                    act_drainz.run(dt);
                    istimeout = act_drainz.istimeout;
                    isdelete = act_drainz.getIsDelete();
                    if (act_drainz.getIsFinish())
                        step++;
                    break;
                case 4:
                    act_absorf.run(dt);
                    istimeout = act_absorf.istimeout;
                    isdelete = act_absorf.getIsDelete();
                    if (act_absorf.getIsFinish())
                        step++;
                    break;
                case 5:
                    act_absordeep.run(dt);
                    istimeout = act_absordeep.istimeout;
                    isdelete = act_absordeep.getIsDelete();
                    if (act_absordeep.getIsFinish())
                        isfinish = true;
                    break;
                case 6:
                    act_movezback.run(dt);
                    istimeout = act_movezback.istimeout;
                    isdelete = act_movezback.getIsDelete();
                    if (act_movezback.getIsFinish())
                        init();
                    break;
            }
        }
        public static InjectDetector getInstance()
        {
            if (instance == null) instance = new InjectDetector();
            return instance;
        }
    }

    public class PaperCard : ActionGroup
    {
        public ResInfoData paper_seat;
        public ActionBase act_paper { get; set; } = null;
        public static PaperCard instance = null;
        public int yb = 0;
        public static PaperCard create(AbstractCanDevice nodetem, double ttime,ResInfoData ppaper_seat,int yyb)
        {
            var acttem = create(ttime, ppaper_seat,yyb);
            acttem.node = nodetem;
            return acttem;
        }
        public static PaperCard create(double ttime, ResInfoData ppaper_seat,int yyb)
        {
            PaperCard acttem = new PaperCard();
            acttem.time = ttime;
            acttem.paper_seat = ppaper_seat;
            acttem.yb = yyb;
            return acttem;
        }
        public override string getName()
        {
            return "打孔";
        }
        public override void init()
        {
            isinit = true;
            sumdt = 0;
            step = 0;
            var piercerDevice = (PiercerDevice)node;
            int init_speed = piercerDevice.Pie.ZMotor.InitSpeed.SetValue;
            act_paper = Sequence.create(node,MoveTo.create(30000, -1, (int)(paper_seat.PiercerY), -1), MoveTo.create(30000, -1, -1, paper_seat.PiercerZ),
                 SkCallBackFun.create((ActionBase actem) => { piercerDevice.CanComm.SetRegister(piercerDevice.Pie.ZMotor.InitSpeed.Addr, 200); return true; }),
                 InitXyz.create(3000, false, false, true),
                 SkCallBackFun.create((ActionBase actem) => { piercerDevice.CanComm.SetRegister(piercerDevice.Pie.ZMotor.InitSpeed.Addr, init_speed); return true; }),
                 MoveTo.create(30000, -1, yb, -1),
                 InitXyz.create(3000,false, yb!=-1, false));
        }
        public override void run(double dt)
        {
            var device = new ActionDevice(node);
            switch (step)
            {
                case 0:
                    act_paper.run(dt);
                    istimeout = act_paper.istimeout;
                    isdelete = act_paper.getIsDelete();
                    errmsg = act_paper.errmsg;
                    if (act_paper.getIsFinish()) step++;
                    break;
                case 1:
                    isfinish = true;
                    break;
            }
        }
        public static PaperCard getInstance()
        {
            if (instance == null) instance = new PaperCard();
            return instance;
        }
    }

    //加样器移动到指定位
    public class InjectMoveActs : ActionGroup
    {
        public ActionPoint[] tagers { get; set; } = null;
        public bool is_asc { get; set; } = true;
        public ActionBase act_move { get; set; } = null;
        public ActionGenerater actiongen { get; set; } = ActionGenerater.getInstance();
        public static InjectMoveActs instance = null;
        public static InjectMoveActs create(AbstractCanDevice nodetem, double ttime, ActionPoint[] ttagers, bool iis_asc)
        {
            InjectMoveActs acttem = InjectMoveActs.create(ttime, ttagers, iis_asc);
            acttem.node = nodetem;
            return acttem;
        }
        public static InjectMoveActs create(double ttime, ActionPoint[] ttagers, bool iis_asc)
        {
            int maxinj_count = Engine.getInstance().injectorDevice.GetMaxInjCount();
            ttagers = ttagers.Take(maxinj_count).ToArray();
            var nomove = IMask.Gen(new ActionPoint(-1, -1, -1));
            bool is_nomove = IMask.IsEquate(nomove, ttagers);
            InjectMoveActs acttem = new InjectMoveActs();
            acttem.tagers = ttagers;
            acttem.is_asc = iis_asc;
            acttem.time = ttime;
            System.Diagnostics.Debug.Assert(ttagers!=null);
            return is_nomove?null:acttem;
        }
        public override string getName()
        {
            if(act_move!=null)
            {
                return act_move.getActState();
            }
            return "加样器动作组";
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
        }
        public override void start()
        {
            isstop = false;
            istimeout = false;
            sumdt = 0;
            act_move.start();
        }
        public override void run(double dt)
        {
            CountTime(dt);
            var device = new ActionDevice(node);
            var inject = (InjectorDevice)node;
            switch (step)
            {
                case 0:
                    {
                        System.Diagnostics.Debug.Assert(tagers != null);
                        act_move = actiongen.GenerateInjectActGroup(tagers.ToList().ToArray(), is_asc);
                        if (act_move != null)
                        {
                            act_move.father = this;
                            act_move.node = node;
                            act_move.init();
                            step++;
                        }
                    }
                    break;
                case 1:
                    act_move.run(dt);
                    istimeout = act_move.istimeout;
                    isdelete = act_move.getIsDelete();
                    errmsg = act_move.errmsg;
                    if (act_move.getIsFinish()) step++;
                    break;
                case 2:
                    isfinish = true;
                    break;
            }
        }
        public static InjectMoveActs getInstance()
        {
            if (instance == null) instance = new InjectMoveActs();
            return instance;
        }
    }

    public class HandOpenCloseDoor : ActionGroup
    {
        public bool is_open { get; set; } = false;
        public string cen_code { get; set; } = "";
        public ActionBase act_opcl { get; set; } = null;
        public static HandOpenCloseDoor instance = null;
        public static HandOpenCloseDoor create(AbstractCanDevice nodetem, double ttime, string ccen_code ,bool iis_open)
        {
            HandOpenCloseDoor acttem = HandOpenCloseDoor.create(ttime,ccen_code,iis_open);
            acttem.node = nodetem;
            return acttem;
        }
        public static HandOpenCloseDoor create(double ttime, string ccen_code, bool iis_open)
        {
            HandOpenCloseDoor acttem = new HandOpenCloseDoor();
            acttem.is_open = iis_open;
            acttem.cen_code = ccen_code;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            return "离心机:" + cen_code + " " + (is_open ? "开门" : "关门");
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            var handdevice = (MachineHandDevice)node;
            var cenbj = ResManager.getInstance().GetCenBj(cen_code);
            var cendevice = Engine.getInstance().cenMrg.GetCentrifugeByCode(cen_code);
            int zfordoor = handdevice.CheckGel()? (int)(cenbj.ZForDoor-2000) : (int)cenbj.ZForDoor;
            if (is_open)
            {
                var open_act = Spawn.create();
                open_act.AddAction(MoveTo.create(3000, (int)cenbj.X2ForDoorOpen, -1, -1, (int)cenbj.XForDoorOpenSpeed));
                open_act.AddAction(MoveTo.create(3000, -1, (int)cenbj.Y2ForOpen, -1, (int)cenbj.YForDoorOpenSpeed));
                act_opcl = Sequence.create(MoveTo.create(3000, -1, -1, 0),
                MoveTo.create(3000, (int)cenbj.X1ForDoorOpen, (int)cenbj.Y1ForOpen, -1),
                MoveTo.create(3000, -1, -1, (int)zfordoor),
                open_act,
                MoveTo.create(3000, -1, -1, 0),
                SkCallBackFun.create((ActionBase acttem)=> {
                    cendevice.IsOpen = true;
                    return true;
                })
                );
            }
            else
            {
                var close_act = Spawn.create();
                close_act.AddAction(MoveTo.create(3000, (int)cenbj.X2ForDoorClose, -1, -1, (int)cenbj.XForDoorCloseSpeed));
                close_act.AddAction(MoveTo.create(3000, -1, (int)cenbj.Y2ForClose, -1, (int)cenbj.YForDoorCloseSpeed));
                act_opcl = Sequence.create(MoveTo.create(3000, -1, -1, 0),
                MoveTo.create(3000, (int)cenbj.X1ForDoorClose, (int)cenbj.Y1ForClose, -1),
                MoveTo.create(3000, -1, -1, (int)zfordoor),
                close_act,
                MoveTo.create(3000, -1, -1, 0),
                SkCallBackFun.create((ActionBase acttem) => {
                    cendevice.IsOpen = false;
                    return true;
                })
                );
            }
            act_opcl.node = node;
            act_opcl.init();
        }
        public override void run(double dt)
        {
            var device = new ActionDevice(node);
            switch (step)
            {
                case 0:
                    {
                        var cendevice = Engine.getInstance().cenMrg.GetCentrifugeByCode(cen_code);
                        if (cendevice != null)
                        {
                            if (cendevice.IsOpen == is_open) step = 2;
                            else step++;
                        }
                        else
                        {
                            ErrorSystem.WriteActError("离心机不存在",true,false);
                            istimeout = true;
                            isdelete = true;
                        }
                    }
                    break;
                case 1:
                    act_opcl.run(dt);
                    istimeout = act_opcl.istimeout;
                    isdelete = act_opcl.getIsDelete();
                    if (act_opcl.getIsFinish()) step++;
                    break;
                case 2:
                    isfinish = true;
                    break;
            }
        }
        public static HandOpenCloseDoor getInstance()
        {
            if (instance == null) instance = new HandOpenCloseDoor();
            return instance;
        }
    }

    public class CentrifugeStart : ActionGroup
    {
        public int c_speed { get; set; } = 1;
        public int h_speed { get; set; } = 0;
        public int l_speed { get; set; } = 0;
        public double h_time { get; set; } = 0;
        public double l_time { get; set; } = 0;
        public double uph_time { get; set; } = 0;
        public double upl_time { get; set; } = 0;
        public double s_time { get; set; } = 0;
        public double c_time { get; set; } = 0;

        public ActionBase act_init { get; set; } = null;
        public ActionBase act_start { get; set; } = null;

        public static CentrifugeStart instance = null;
        public static CentrifugeStart create(AbstractCanDevice nodetem, double ttime,int hspeed,int lspeed,int htime,int ltime,int uphtime,int upltime,int stime)
        {
            CentrifugeStart acttem = CentrifugeStart.create(ttime, hspeed, lspeed, htime, ltime, uphtime, upltime, stime);
            acttem.node = nodetem;
            return acttem;
        }
        public static CentrifugeStart create(double ttime, int hspeed, int lspeed, int htime, int ltime, int uphtime, int upltime, int stime)
        {
            CentrifugeStart acttem = new CentrifugeStart();
            acttem.h_speed = hspeed;
            acttem.l_speed = lspeed;
            acttem.h_time = htime;
            acttem.l_time = ltime;
            acttem.uph_time = uphtime;
            acttem.upl_time = upltime;
            acttem.s_time = stime;
            acttem.time = ttime;
            return acttem;
        }
        public override string getName()
        {
            string[] names = {
            "离心初始化", "离心初始化", "离心转动", "加低速", "平低速",
            "加高速", "平高速", "停止", "离心完成","离心初始化","","","",""};
            return names[step]+" S:"+c_speed+" T:"+ sumdt;
        }
        public override void init()
        {
            isinit = true;
            step = 0;
            sumdt = 0;
            c_time = 0;
            c_speed = 1;
            var centrifuge = (CentrifugeMDevice)node;
            centrifuge.LowSpeedTime = 0;
            centrifuge.HightSpeedTime = 0;
            act_start = MoveTo.create(centrifuge, 6000000, -1, -1,9999999, c_speed);
            act_start.init();

            act_init = Sequence.create(InitXyz.create(centrifuge, 50000, false, false, true), InitXyz.create(centrifuge, 50000, false, false, true));
            act_init.init();
        }
        public override void run(double dt)
        {
            CountTime(dt);
            bool resultz = false;
            var device = new ActionDevice(node);
            var centrifuge = (CentrifugeMDevice)node;
            switch (step)
            {
                case 0:
                    step =2;
                    break;
                case 1:
                    step++;
                    break;
                case 2:
                    act_start.run(dt);
                    istimeout = act_start.istimeout;
                    isdelete = act_start.getIsDelete();
                    if (act_start.step==2)
                    step++;
                    break;
                case 3:
                    //加低速
                    c_time += dt;
                    if (c_time > (upl_time / l_speed))
                    {
                        c_time = 0;
                        c_speed++;
                        centrifuge.CanComm.SetRegister(centrifuge.Centrifugem.Motor.Speed.Addr, c_speed+1);
                        if (c_speed >= l_speed)
                        {
                            step++;
                        }
                    }
                    break;
                case 4:
                    //平低速
                    c_time += dt;
                    centrifuge.LowSpeedTime = (int)c_time / 1000;
                    if (c_time > l_time)
                    {
                        c_time = 0;
                        step++;
                    }
                    break;
                case 5:
                    //加高速
                    c_time += dt;
                    if (c_time >= (uph_time / (h_speed-l_speed)))
                    {
                        c_time = 0;
                        c_speed++;
                        centrifuge.CanComm.SetRegister(centrifuge.Centrifugem.Motor.Speed.Addr,c_speed+1);
                        if (c_speed >= h_speed)
                        {
                            step++;
                        }
                    }
                    break;
                case 6:
                    //平高速
                    c_time += dt;
                    centrifuge.HightSpeedTime = (int)c_time / 1000;
                    if (c_time >= h_time)
                    {
                        c_time = 0;
                        step++;
                    }
                    break;
                case 7:
                    //停止
                    c_time += dt;
                    if (c_time >= (s_time / h_speed))
                    {
                        c_time = 0;
                        c_speed-=1;
                        if (c_speed <= 0) c_speed = 0;
                        centrifuge.CanComm.SetRegister(centrifuge.Centrifugem.Motor.Speed.Addr,c_speed+1);
                        if (c_speed <= 0)
                        {
                            step++;
                        }
                    }
                    break;
                case 8:
                    if (centrifuge.CanComm.StopMotor(centrifuge.Centrifugem.Motor))
                        step++;
                    break;
                case 9:
                    act_init.run(dt);
                    istimeout = act_init.istimeout;
                    isdelete = act_init.getIsDelete();
                    isfinish = act_init.getIsFinish();
                    if (isfinish)
                    {
                        centrifuge.LowSpeedTime = 0;
                        centrifuge.HightSpeedTime = 0;
                        var cen_seat = ResManager.getInstance().GetCenBj(centrifuge.Centrifugem.Code.SetValue);
                        if(cen_seat!=null)cen_seat.LastUseTime = 0;
                    }
                    break;
            }
            if(TimeOutMsg())
            {
                step=9;
                istimeout = false;
            }
        }
        public static CentrifugeStart getInstance()
        {
            if (instance == null) instance = new CentrifugeStart();
            return instance;
        }
    }

}