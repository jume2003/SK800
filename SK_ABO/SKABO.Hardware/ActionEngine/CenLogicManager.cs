using SKABO.Hardware.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKABO.MAI.ErrorSystem;
using SKABO.ResourcesManager;
using SKABO.Hardware.RunBJ;
using SKABO.ActionGeneraterEngine;
using static SKABO.ResourcesManager.ActionPoint;
using SKABO.Common.Models.GEL;

namespace SKABO.ActionEngine
{
    public class CenLogicManager
    {
        public CentrifugeMrg cenMrg = null;
        public MachineHandDevice handDevice = null;
        public PiercerDevice piercerDevice = null;
        public CameraDevice cameraDevice = null;
        public ResManager resmanager = ResManager.getInstance();
        public ActionManager actionmanager = ActionManager.getInstance();
        public ActionGenerater generater = ActionGenerater.getInstance();
        public double lasttime = 0;
        public double hand_wait_time = 0;
        public static CenLogicManager cenlogicmanager = null;
        List<ResInfoData> resinfo_list = new List<ResInfoData>();
        public static CenLogicManager getInstance()
        {
            if (cenlogicmanager == null) cenlogicmanager = new CenLogicManager();
            return cenlogicmanager;
        }
        //是否在离心机内的所有卡当前动作是离心
        public bool IsCanPutCen(CentrifugeMDevice cendev)
        {
            if (actionmanager.getAllActionsCount(cendev) != 0) return false;
            foreach (var seat in resmanager.centrifuge_list)
            {
                if (seat.Code == cendev.Centrifugem.Code.SetValue)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                        if (resinfo != null && resinfo.InjectFinish && resinfo.PutOk)
                        {
                            if (resinfo.GetActionAt(0) != ActionPoint.ActionPointType.Centrifugal)
                                return false;
                        }
                    }
                    break;
                }
            }
            return true;
        }
        //离心开始逻辑
        public bool CenRunLogic(CentrifugeMDevice cendev)
        {
            
            bool is_allcenact = true;//是否全是离心动作
            bool is_allputok = true;//是否全是已放好
            bool is_empty = true;//是否为空
            bool is_gelfull = false;//是否已满
            bool is_gelsoon = false;//是否有卡快要离心
            int gel_count = 0;
            List<ResInfoData> resinfo_list = new List<ResInfoData>();
            foreach (var seat in resmanager.centrifuge_list)
            {
                if (seat.Code == cendev.Centrifugem.Code.SetValue)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                        if (resinfo != null && resinfo.InjectFinish && resinfo.PutOk)
                        {
                            is_empty = false;
                            is_allcenact = is_allcenact && resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Centrifugal;
                            is_allputok = is_allputok && resinfo.PutOk;
                            resinfo_list.Add(resinfo);
                            gel_count++;
                        }
                    }
                    is_gelfull = gel_count >= seat.Values.Length;
                }
            }
            if ((is_gelfull == false && actionmanager.getAllActionsCount(handDevice) != 0) || actionmanager.getAllActionsCount(cendev) != 0) return false;
            //普通卡位是否有卡快要离心
            foreach (var seat in resmanager.gelseat_list)
            {
                for (int i = 0; i < seat.Values.Length; i++)
                {
                    ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                    if (resinfo != null && resinfo.InjectFinish && resinfo.PutOk)
                    {
                        is_gelsoon = (resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Centrifugal) ||
                        (resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Hatch &&
                        resinfo.GetActionAt(1) == ActionPoint.ActionPointType.Centrifugal &&
                        resinfo.HatchTime - resinfo.HatchCurTime <= 20000);
                        if (seat.Purpose == 4&&resinfo.FindAct(ActionPoint.ActionPointType.Centrifugal) && resinfo.FindAct(ActionPoint.ActionPointType.Hatch)==false)
                            is_gelsoon = true;
                        if (is_gelsoon) break;
                    }
                }
                if (is_gelsoon) break;
            }
            //检测是否满足运行条件
            if (is_allcenact && is_allputok && !is_empty && (!is_gelsoon || is_gelfull))
            {
                int hspeed = (int)cendev.Centrifugem.HightSpeed.SetValue;
                int lspeed = (int)cendev.Centrifugem.LowSpeed.SetValue;
                int htime = (int)cendev.Centrifugem.HightSpeedTime.SetValue;
                int ltime = (int)cendev.Centrifugem.LowSpeedTime.SetValue;
                int uphtime = (int)cendev.Centrifugem.AddHSpeedTime.SetValue;
                int upltime = (int)cendev.Centrifugem.AddLSpeedTime.SetValue;
                int stime = (int)cendev.Centrifugem.StopSpeedTime.SetValue;
                //配平卡动作(如果离心机内卡是单数就再放一张配平卡)
                var seque_pei = Sequence.create();
                var seque = Sequence.create();
                if (resinfo_list.Count()%2!=0)
                {
                    var spaw = Spawn.create();
                    var put_seque = Sequence.create();
                    //得到配平卡
                    var pei_gel = resmanager.GetResByCode("pei" + cendev.Centrifugem.Code.SetValue, "T_BJ_GelSeat");
                    var put_gel = generater.GenerateTakeGelFromNormal(pei_gel, ref put_seque);
                    put_gel.ActionList.Add(ActionPointType.Centrifugal);
                    put_gel.ActionList.Add(ActionPointType.PutPeiGelBack);
                    put_gel.InjectFinish = true;
                    resinfo_list.Add(put_gel);
                    var put_seat = resmanager.GetResByCode("null", "T_BJ_Centrifuge", cendev.Centrifugem.Code.SetValue);
                    spaw.AddAction(put_seque);
                    spaw.AddAction(MoveTo.create(cendev, 30001, -1, -1, put_seat.CenGelP[put_seat.CountX]));
                    seque_pei.AddAction(spaw);
                    generater.GeneratePutGelToCent(cendev.Centrifugem.Code.SetValue, put_seat, put_gel, ref seque_pei);
                }
                seque_pei.AddAction(HandOpenCloseDoor.create(handDevice,5000, cendev.Centrifugem.Code.SetValue, false));
                seque.AddAction(SkWaitForAction.create(handDevice, seque_pei));
                seque.AddAction(CentrifugeStart.create(cendev, 300000, hspeed, lspeed, htime, ltime, uphtime, upltime, stime));
                seque.AddAction(SkCallBackFun.create((ActionBase acttem) =>
                {
                    foreach (var resinfo in resinfo_list)
                    {
                        if (resinfo.InjectFinish)
                            resinfo.RemoveActionAt(0);
                    }
                    return true;
                }));
                seque_pei.runAction(handDevice);
                seque.runAction(cendev);
                return true;
            }
            return false;
        }
        public void UpdataResInfoList()
        {
            resinfo_list.Clear();
            //普通卡位
            foreach (var seat in resmanager.gelseat_list)
            {
                for (int i = 0; i < seat.Values.Length; i++)
                {
                    ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                    if (resinfo != null && resinfo.InjectFinish && resinfo.PutOk&& resinfo.ActionList.Count()!=0)
                    {
                        resinfo_list.Add(resinfo);
                    }
                }
            }
            if (actionmanager.getAllActionsCount(handDevice) != 0) return;
            //检测离心机只的卡
            foreach (var seat in resmanager.centrifuge_list)
            {
                for (int i = 0; i < seat.Values.Length; i++)
                {
                    ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                    if (resinfo != null && resinfo.InjectFinish && resinfo.PutOk && resinfo.ActionList.Count() != 0)
                    {
                        resinfo_list.Add(resinfo);
                    }
                }
            }
        }

        public void Logic(ResInfoData resinfo,double dt)
        {
            switch (resinfo.GetActionAt(0))
            {
                case ActionPointType.Hatch:
                    {
                        if (resinfo.Purpose == "1")
                        {
                            resinfo.HatchCurTime += dt;
                            if (resinfo.HatchCurTime >= resinfo.HatchTime)
                            {
                                resinfo.RemoveActionAt(0);
                            }
                        }
                        else if (!(resinfo.Purpose == "4" && actionmanager.getAllActionsCount(piercerDevice) != 0))
                        {
                            var seque = Sequence.create();
                            var put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "1");
                            if (put_seat == null && resinfo.Purpose != "3") put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "3");
                            if (put_seat != null)
                            {
                                ResInfoData put_gel = null;
                                if (resinfo.Purpose == "lxj")
                                {
                                    //CentrifugeMDevice cendev = cenMrg.GetCentrifugeByCode(resinfo.CenCode);
                                    //seque.AddAction(MoveTo.create(cendev, 30000, -1, -1, 0));
                                    //seque.AddAction(InitXyz.create(cendev, 30000, false, false, true));
                                    put_gel = generater.GenerateTakeGelFromCent(resinfo, resinfo.CenCode, ref seque);
                                }
                                else
                                    put_gel = generater.GenerateTakeGelFromNormal(resinfo, ref seque);
                                put_gel.HatchCurTime = 0;
                                generater.GeneratePutGelToNormal(put_seat, put_gel, ref seque);
                                seque.runAction(handDevice);
                            }
                        }
                    }
                    break;
                case ActionPointType.Centrifugal:
                    {
                        if(resinfo.Purpose!="lxj"&& !(resinfo.Purpose == "4" && actionmanager.getAllActionsCount(piercerDevice) != 0))
                        {
                            foreach (var seat in resmanager.centrifuge_list)
                            {
                                CentrifugeMDevice cendev = cenMrg.GetCentrifugeByCode(seat.Code);
                                if (seat.Status == 1 && cendev != null)
                                {
                                    bool iscanputcen = IsCanPutCen(cendev);
                                    var seque = Sequence.create();
                                    var put_seat = resmanager.GetResByCode("null", "T_BJ_Centrifuge", cendev.Centrifugem.Code.SetValue);
                                    if (iscanputcen == false) put_seat = null;
                                    if (put_seat == null && resinfo.Purpose != "3") put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "3");
                                    if (put_seat != null)
                                    {
                                        var spaw = Spawn.create();
                                        var put_seque = Sequence.create();
                                        var put_gel = generater.GenerateTakeGelFromNormal(resinfo, ref put_seque);
                                        spaw.AddAction(put_seque);
                                        seque.AddAction(spaw);
                                        if (put_seat.Purpose == "lxj")
                                        {
                                            spaw.AddAction(MoveTo.create(cendev, 30001, -1, -1, put_seat.CenGelP[put_seat.CountX]));
                                            generater.GeneratePutGelToCent(cendev.Centrifugem.Code.SetValue, put_seat, put_gel, ref seque);
                                        }
                                        else
                                            generater.GeneratePutGelToNormal(put_seat, put_gel, ref seque);
                                        seque.runAction(handDevice);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case ActionPointType.PutPeiGelBack:
                    {
                        var seque = Sequence.create();
                        var put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "2");
                        if (put_seat != null&& resinfo.Purpose == "lxj")
                        {
                            //CentrifugeMDevice cendev = cenMrg.GetCentrifugeByCode(resinfo.CenCode);
                            //seque.AddAction(MoveTo.create(cendev, 30000, -1, -1, 0));
                            //seque.AddAction(InitXyz.create(cendev, 30000, false, false, true));
                            var put_gel = generater.GenerateTakeGelFromCent(resinfo, resinfo.CenCode, ref seque);
                            generater.GeneratePutGelToNormal(put_seat, put_gel, ref seque);
                            seque.AddAction(SkCallBackFun.create((ActionBase acttem) => {
                                resinfo.RemoveActionAt(0);
                                return true;
                            }));
                            seque.runAction(handDevice);
                            
                        }
                    }
                    break;
                case ActionPointType.Camera:
                    {
                        var seque = Sequence.create();
                        var put_seat = resmanager.GetResByCode("", "T_BJ_Camera");
                        var wasted_seat = resmanager.GetResByCode("", "T_BJ_WastedSeat");
                        if (put_seat != null&& wasted_seat!=null&& !(resinfo.Purpose == "4" && actionmanager.getAllActionsCount(piercerDevice) != 0))
                        {
                            ResInfoData put_gel = null;
                            if (resinfo.Purpose == "lxj")
                            {
                                //CentrifugeMDevice cendev = cenMrg.GetCentrifugeByCode(resinfo.CenCode);
                                //seque.AddAction(MoveTo.create(cendev, 30000, -1, -1, 0));
                                //seque.AddAction(InitXyz.create(cendev, 30000, false, false, true));
                                put_gel = generater.GenerateTakeGelFromCent(resinfo, resinfo.CenCode, ref seque);
                            }
                            else
                                put_gel = generater.GenerateTakeGelFromNormal(resinfo, ref seque);

                            seque.AddAction(MoveTo.create(handDevice, 3000,-1, -1, 0));
                            seque.AddAction(MoveTo.create(handDevice, 3000, (int)put_seat.X, (int)(put_seat.Y), -1));
                            seque.AddAction(MoveTo.create(handDevice, 3000, -1, -1, (int)(put_seat.Z)));
                            //拍照分析
                            seque.AddAction(SkCallBackFun.create((ActionBase acttem) =>
                            {
                                bool result = true;
                                if (cameraDevice.IsOpen==false)result=cameraDevice.Open();
                                var bm = cameraDevice.CaptureImage();
                                var bag = new TestBag(Common.Enums.TestLevelEnum.Normal);
                                var gel = new Gel(resinfo.GetGelMask());
                                bag.GelType = resinfo.gel;
                                bag.Add(resinfo.GetSampleBarcode(),(byte)resinfo.GetSampleRackIndex(),(byte)resinfo.CountX);
                                bag.SetStartTime(resinfo.StartTime);
                                gel.SampleBarcodes.Add(resinfo.GetSampleBarcode());
                                cameraDevice.Save(bag, gel, bm, "888");
                                //cameraDevice.Close();
                                put_gel.RemoveActionAt(0);
                                return true;
                            }));
                            seque.AddAction(MoveTo.create(handDevice, 3000, (int)wasted_seat.X, (int)(wasted_seat.Y)));
                            seque.AddAction(HandPutCard.create(handDevice, 3000, (int)wasted_seat.ZPut, 0));
                            seque.runAction(handDevice);
                        }
                    }
                    break;
            }

        }

        public void runLoop(double time)
        {
            double dt = time - lasttime;
            if (dt < 500) return;
            lasttime = time;
            //更新卡
            UpdataResInfoList();
            if (resinfo_list.Count == 0&&InjLogicManager.getInstance().action_tree.Count==0) return;
            foreach(var resinfo in resinfo_list)
            {
                Logic(resinfo,dt);
            }
            ////离心逻辑
            foreach (var cent in cenMrg.CentrifugeMDevices)
            {
                //如果当前离心机在跑或已满卡就添加到下一个离心机中
                if (CenRunLogic(cent)) return;// 离心启动逻辑
                break;
            }
            //机器手空闲回零
            if (actionmanager.getAllActionsCount(handDevice) == 0&& handDevice.Hand.XMotor.CurrentDistance!=0)
            {
                hand_wait_time += dt;
                if (hand_wait_time > 1000)
                {
                    var act = Sequence.create(MoveTo.create(handDevice, 10000, 0, 0, 0), InitXyz.create(20000, true, true, true));
                    act.runAction(handDevice);
                    hand_wait_time = 0;
                }
            }
        }
    }
}
