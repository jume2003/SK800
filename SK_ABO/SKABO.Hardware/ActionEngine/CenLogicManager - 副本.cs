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

namespace SKABO.ActionEngine
{
    public class CenLogicManager
    {
        public CentrifugeMrg cenMrg = null;
        public MachineHandDevice handDevice = null;
        public PiercerDevice piercerDevice = null;
        public ResManager resmanager = ResManager.getInstance();
        public ActionManager actionmanager = ActionManager.getInstance();
        public ActionGenerater generater = ActionGenerater.getInstance();
        public double lasttime = 0;
        public static CenLogicManager cenlogicmanager = null;
        public int setp = 0;
        public static CenLogicManager getInstance()
        {
            if (cenlogicmanager == null) cenlogicmanager = new CenLogicManager();
            return cenlogicmanager;
        }
        //放卡到离心逻辑
        public bool CenPutLogic(CentrifugeMDevice cendev)
        {
            if (resmanager.handseat_resinfo!=null || 
                actionmanager.getAllActionsCount(piercerDevice) != 0||
                actionmanager.getAllActionsCount(handDevice) != 0 || 
                actionmanager.getAllActionsCount(cendev) != 0) return false;
            bool isallcen = true;//是否在离心机内的所有卡当前动作是离心
            foreach (var seat in resmanager.centrifuge_list)
            {
                if (seat.Code == cendev.Centrifugem.Code.SetValue)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                        if (resinfo != null && resinfo.InjectFinish && resinfo.PutOk)
                        {
                            isallcen = resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Centrifugal;
                            if (isallcen == false) break;
                        }
                    }
                    break;
                }
            }
            //普通卡位
            List<ResInfoData> resinfo_list = new List<ResInfoData>();
            foreach (var seat in resmanager.gelseat_list)
            {
                for (int i = 0; i < seat.Values.Length; i++)
                {
                    ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                    if (resinfo != null && resinfo.InjectFinish&& resinfo.PutOk)
                    {
                        if (resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Centrifugal)
                        {
                            resinfo_list.Add(resinfo);
                        }
                    }
                }
            }

            //把卡转到离心位
            if (resinfo_list.Count != 0)
            {
                var seque = Sequence.create();
                bool is_put = false;
                seque.AddAction(InitXyz.create(cendev,30000, false, false, true));
                foreach (var resinfo in resinfo_list)
                {
                    var put_seat = resmanager.GetResByCode("null", "T_BJ_Centrifuge", cendev.Centrifugem.Code.SetValue);
                    if (isallcen == false) put_seat = null;
                    if (put_seat == null && resinfo.Purpose != "3") put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "3");
                    if (put_seat != null)
                    {
                        var put_gel = generater.GenerateTakeGelFromNormal(resinfo, ref seque);
                        if(put_seat.Purpose=="lxj")
                            generater.GeneratePutGelToCent(cendev.Centrifugem.Code.SetValue,put_seat, put_gel, ref seque);
                        else
                            generater.GeneratePutGelToNormal(put_seat, put_gel, ref seque);
                        is_put = true;
                    }
                }
                if (is_put)
                {
                    seque.runAction(handDevice);
                    return true;
                }
            }
            return false;
        }
        //离心开始逻辑
        public bool CenRunLogic(CentrifugeMDevice cendev)
        {
            if (actionmanager.getAllActionsCount(handDevice) != 0|| actionmanager.getAllActionsCount(cendev) != 0) return false;
            bool is_allcenact = true;//是否全是离心动作
            bool is_allputok = true;//是否全是已放好
            bool is_empty = true;//是否为空
            bool is_gelfull = false;//是否已满
            bool is_gelsoon = false;//是否有卡快要离心
            int gel_count = 0;
            List<ResInfoData> resinfo_list = new List<ResInfoData>();
            foreach (var seat in resmanager.centrifuge_list)
            {
                if(seat.Code == cendev.Centrifugem.Code.SetValue)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                        if(resinfo!=null&& resinfo.InjectFinish&& resinfo.PutOk)
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
            //普通卡位是否有卡快要离心
            foreach (var seat in resmanager.gelseat_list)
            {
                for (int i = 0; i < seat.Values.Length; i++)
                {
                    ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                    if (resinfo != null&& resinfo.InjectFinish&& resinfo.PutOk)
                    {
                        is_gelsoon = (resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Centrifugal) ||
                        (resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Hatch &&
                        resinfo.GetActionAt(1) == ActionPoint.ActionPointType.Centrifugal &&
                        resinfo.HatchTime-resinfo.HatchCurTime <=10);
                        if (is_gelsoon) break;
                    }
                }
                if (is_gelsoon) break;
            }
            //检测是否满足运行条件
            if(is_allcenact&& is_allputok&&!is_empty&&(!is_gelsoon|| is_gelfull))
            {
                int hspeed = (int)cendev.Centrifugem.HightSpeed.SetValue;
                int lspeed = (int)cendev.Centrifugem.LowSpeed.SetValue;
                int htime = (int)cendev.Centrifugem.HightSpeedTime.SetValue;
                int ltime = (int)cendev.Centrifugem.LowSpeedTime.SetValue;
                int uphtime = (int)cendev.Centrifugem.AddHSpeedTime.SetValue;
                int upltime = (int)cendev.Centrifugem.AddLSpeedTime.SetValue;
                int stime = (int)cendev.Centrifugem.StopSpeedTime.SetValue;
                var act = Sequence.create(CentrifugeStart.create(cendev, 300000, hspeed, lspeed, htime, ltime, uphtime, upltime, stime), SkCallBackFun.create((ActionBase acttem) =>
                {
                    foreach (var resinfo in resinfo_list)
                    {
                        if (resinfo.InjectFinish)
                            resinfo.RemoveActionAt(0);
                    }
                    return true;
                }));
                act.runAction(cendev);
                return true;
            }
            return false;
        }
        //孵育逻辑(孵育位计数不在孵育位就添加抓卡)
        public bool HatchLogic(double dt)
        {
            //普通卡位
            List<ResInfoData> resinfo_list = new List<ResInfoData>();
            foreach (var seat in resmanager.gelseat_list)
            {
                for (int i = 0; i < seat.Values.Length; i++)
                {
                    ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                    if (resinfo != null && resinfo.InjectFinish&& resinfo.PutOk)
                    {
                        if (seat.Purpose != 1)
                        {
                            if (resinfo.GetActionAt(0)==ActionPoint.ActionPointType.Hatch)
                            {
                                resinfo_list.Add(resinfo);
                            }
                        }
                        else if (seat.Purpose == 1)
                        {
                            if (resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Hatch)
                            {
                                resinfo.HatchCurTime += dt;
                                if (resinfo.HatchCurTime >= resinfo.HatchTime)
                                {
                                    resinfo.RemoveActionAt(0);
                                }
                            }
                        }
                    }
                }
            }
            //检测离心机只的卡
            foreach (var seat in resmanager.centrifuge_list)
            {
                for (int i = 0; i < seat.Values.Length; i++)
                {
                    ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                    if (seat.Status == 1 && resinfo != null && resinfo.InjectFinish && resinfo.PutOk)
                    {
                        if (resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Hatch)
                        {
                            resinfo.Purpose = "lxj";
                            resinfo.CenCode = seat.Code;
                            resinfo_list.Add(resinfo);
                        }
                    }
                }
            }
            if (actionmanager.getAllActionsCount(piercerDevice)!=0 ||actionmanager.getAllActionsCount(handDevice) != 0 || resmanager.handseat_resinfo != null) return false;
            //把卡转到孵育位
            if (resinfo_list.Count!=0)
            {
                var seque = Sequence.create();
                bool is_put = false;
                foreach (var resinfo in resinfo_list)
                {
                    var put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "1");
                    if(put_seat==null&& resinfo.Purpose!="3") put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "3");
                    if (put_seat!=null)
                    {
                        ResInfoData put_gel = null;
                        if(resinfo.Purpose=="lxj")
                            put_gel = generater.GenerateTakeGelFromCent(resinfo, resinfo.CenCode, ref seque);
                        else
                            put_gel = generater.GenerateTakeGelFromNormal(resinfo, ref seque);
                        put_gel.HatchCurTime = 0;
                        generater.GeneratePutGelToNormal(put_seat, put_gel,ref seque);
                        is_put = true;
                    }
                }
                if (is_put)
                {
                    seque.AddAction(MoveTo.create(handDevice, 3000, 0, -1, -1));
                    seque.runAction(handDevice);
                    return true;
                }
            }
            return false;
        }
        //拍照逻辑
        public bool CameraLogic(double dt)
        {
            if (actionmanager.getAllActionsCount(handDevice) != 0 || resmanager.handseat_resinfo != null) return false;
            //普通卡位
            ResInfoData camer_gel = null;
            foreach (var seat in resmanager.gelseat_list)
            {
                for (int i = 0; i < seat.Values.Length; i++)
                {
                    ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                    if (resinfo != null && resinfo.InjectFinish && resinfo.PutOk)
                    {
                        if(resinfo.GetActionAt(0)==ActionPoint.ActionPointType.Camera)
                        camer_gel = resinfo;
                        break;
                    }
                }
                if (camer_gel != null) break;
            }
            //检测离心机只的卡
            if(camer_gel==null)
            {
                foreach (var seat in resmanager.centrifuge_list)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                        if (seat.Status==1&&resinfo != null && resinfo.InjectFinish && resinfo.PutOk)
                        {
                            if (resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Camera)
                            {
                                resinfo.Purpose = "lxj";
                                resinfo.CenCode = seat.Code;
                                camer_gel = resinfo;
                                break;
                            }
                        }
                    }
                    if (camer_gel != null) break;
                }
            }
            //把卡转到拍照位
            if (camer_gel != null)
            {
                var seque = Sequence.create();
                bool is_put = false;
                var put_seat = resmanager.GetResByCode("", "T_BJ_Camera");
                if (put_seat != null)
                {
                    ResInfoData put_gel = null;
                    if (camer_gel.Purpose == "lxj")
                        put_gel = generater.GenerateTakeGelFromCent(camer_gel, camer_gel.CenCode, ref seque);
                    else
                        put_gel = generater.GenerateTakeGelFromNormal(camer_gel, ref seque);

                    seque.AddAction(MoveTo.create(handDevice,3000, (int)put_seat.X, (int)(put_seat.Y), -1));
                    seque.AddAction(MoveTo.create(handDevice,3000, -1, -1, (int)(put_seat.Z)));
                    //拍照分析
                    seque.AddAction(SkCallBackFun.create((ActionBase acttem) =>
                    {
                        put_gel.RemoveActionAt(0);
                        return true;
                    }));
                    is_put = true;
                }
                if (is_put)
                {
                    seque.runAction(handDevice);
                    return true;
                }
            }
            return false;
        }
        //垃圾箱逻辑
        public bool RubbishLogic(double dt)
        {
            if (actionmanager.getAllActionsCount(handDevice) != 0) return false;
            //普通卡位
            ResInfoData camer_gel = null;
            if(resmanager.handseat_resinfo != null&& resmanager.handseat_resinfo.GetActionAt(0)==ActionPoint.ActionPointType.Rubbish)
            {
                camer_gel = resmanager.handseat_resinfo;
                camer_gel.Purpose = "hand";
            }
            if (camer_gel==null)
            {
                foreach (var seat in resmanager.gelseat_list)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                        if (resinfo != null && resinfo.InjectFinish && resinfo.PutOk)
                        {
                            if (resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Rubbish)
                                camer_gel = resinfo;
                            break;
                        }
                    }
                    if (camer_gel != null) break;
                }
            }
            //检测离心机只的卡
            if (camer_gel == null)
            {
                foreach (var seat in resmanager.centrifuge_list)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                        if (seat.Status == 1 && resinfo != null && resinfo.InjectFinish && resinfo.PutOk)
                        {
                            if (resinfo.GetActionAt(0) == ActionPoint.ActionPointType.Rubbish)
                            {
                                resinfo.Purpose = "lxj";
                                resinfo.CenCode = seat.Code;
                                camer_gel = resinfo;
                                break;
                            }
                        }
                    }
                    if (camer_gel != null) break;
                }
            }
            //把卡转到垃圾位
            if (camer_gel != null&& camer_gel.GetActionAt(0)==ActionPoint.ActionPointType.Rubbish)
            {
                var seque = Sequence.create();
                bool is_put = false;
                var put_seat = resmanager.GetResByCode("", "T_BJ_WastedSeat");
                if (put_seat != null)
                {
                    ResInfoData put_gel = null;
                    if (camer_gel.Purpose == "lxj")
                        put_gel = generater.GenerateTakeGelFromCent(camer_gel, camer_gel.CenCode, ref seque);
                    else if (camer_gel.Purpose == "hand")
                        put_gel = camer_gel;
                    else
                        put_gel = generater.GenerateTakeGelFromNormal(camer_gel, ref seque);

                    seque.AddAction(MoveTo.create(handDevice,3000, (int)put_seat.X, (int)(put_seat.Y)));
                    seque.AddAction(HandPutCard.create(handDevice,3000, (int)put_seat.ZPut, 0));
                    seque.AddAction(MoveTo.create(handDevice,3000, -1, -1,0));
                    seque.AddAction(SkCallBackFun.create((ActionBase acttem) =>
                    {
                        if(camer_gel.Purpose == "hand")
                        resmanager.handseat_resinfo = null;
                        put_gel.RemoveActionAt(0);
                        return true;
                    }));
                    is_put = true;
                }
                if (is_put)
                {
                    seque.runAction(handDevice);
                    return true;
                }
            }
            return false;
        }

        public void runLoop(double time)
        {
            double dt = time - lasttime;
            if (dt < 1000) return;
            lasttime = time;
            //拍照逻辑
            if (CameraLogic(dt)) return;
            //垃圾箱逻辑
            if (RubbishLogic(dt)) return;
            //离心逻辑
            foreach (var cent in cenMrg.CentrifugeMDevices)
            {
                //如果当前离心机在跑或已满卡就添加到下一个离心机中
                if (CenPutLogic(cent)) return; //离心放卡
                break;
                //bool is_check_next_cen = resmanager.GetCenStatus(cent.Centrifugem.Code.SetValue) && (actionmanager.getAllActionsCount(cent) != 0 || resmanager.GetResByCode("null", "T_BJ_Centrifuge", cent.Centrifugem.Code.SetValue) == null);
                //if (is_check_next_cen == false) break;
            }
            //离心逻辑
            foreach (var cent in cenMrg.CentrifugeMDevices)
            {
                //如果当前离心机在跑或已满卡就添加到下一个离心机中
                if (CenRunLogic(cent)) return;// 离心启动逻辑
                break;
                //bool is_check_next_cen = resmanager.GetCenStatus(cent.Centrifugem.Code.SetValue) && (actionmanager.getAllActionsCount(cent) != 0 || resmanager.GetResByCode("null", "T_BJ_Centrifuge", cent.Centrifugem.Code.SetValue) == null);
                //if (is_check_next_cen == false) break;
            }
            //孵育逻辑
            if (HatchLogic(dt)) return;
        }
    }
}
