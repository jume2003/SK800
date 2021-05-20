using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.ObjectModel;
using SKABO.MAI.ErrorSystem;
using SKABO.ResourcesManager;
using SKABO.Common.Models.GEL;
using SKABO.Common;
using SKABO.BLL.IServices.IGel;
using SKABO.Common.Enums;
using SKABO.ActionEngine;
using SKABO.Common.Models.NotDuplex;
using SKABO.Hardware.Model;
using SKABO.Hardware.RunBJ;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Models.BJ;

namespace SKABO.ActionGeneraterEngine
{
    public class ActionGenerater
    {
        public static ActionGenerater instance = null;
        public IGelService gelService = IoC.Get<IGelService>();
        public T_StepDefine defhxb = null;
        public T_StepDefine defxsy = null;
        public InjectorDevice injectorDevice = null;
        public MachineHandDevice handDevice = null;
        public CentrifugeMrg cenMrg = null;

        public static ActionGenerater getInstance()
        {
            if (instance == null)
            {
                instance = new ActionGenerater();
                instance.defhxb = instance.gelService.QueryStepDefineByClass((int) SKABO.Common.Enums.TestStepEnum.FPHXB)[0];
                instance.defxsy = instance.gelService.QueryStepDefineByClass((int) SKABO.Common.Enums.TestStepEnum.FPXSY)[0];
            }
            return instance;
        }

        public ActionGenerater getActionGenerater()
        {
            return instance;
        }

        //动作解析(把分配红细胞分裂)
        public List<T_GelStep> ResolveActions(T_Gel gel)
        {
            List<T_GelStep> gelstep_list = new List<T_GelStep>();
            foreach (var gelstep in gel.GelSteps)
            {
                if (gelstep.StepClass == TestStepEnum.FPBRXSHXB || gelstep.StepClass == TestStepEnum.FPXXYXSHXB)
                {
                    var gelfphxb = gelstep.clone();
                    var gelfpxsy = gelstep.clone();
                    gelfphxb.StepClass = defhxb.StepClass;
                    gelfpxsy.StepClass = defxsy.StepClass;
                    gelfphxb.StepName = defhxb.StepName;
                    gelfpxsy.StepName = defxsy.StepName;
                    gelfphxb.InjectCount = 1;
                    gelfpxsy.InjectCount = 1;
                    gelfphxb.StepID = defhxb.ID;
                    gelfpxsy.StepID = defxsy.ID;
                    gelstep_list.Add(gelfphxb);
                    gelstep_list.Add(gelfpxsy);
                }
                else
                    gelstep_list.Add(gelstep);
            }
            gelstep_list.Sort((a, b) => a.ID.CompareTo(b.ID));
            return gelstep_list;
        }
        //动作分组
        public List<List<T_GelStep>> DivideIntoGroups(List<T_GelStep> gelstep_list, int inject_count)
        {
            int count = 0;
            List<List<T_GelStep>> action_tree = new List<List<T_GelStep>>();
            foreach (var act in gelstep_list)
            {
                if (count % inject_count == 0)
                {
                    var group = new List<T_GelStep>();
                    group.Add(act);
                    action_tree.Add(group);
                }
                else if (action_tree.Count != 0)
                {
                    action_tree[action_tree.Count - 1].Add(act);
                }
                count += act.InjectCount;
            }
            return action_tree;
        }
        //按X轴坐标分组
        public List<List<T_GelStep>> DivideXIntoGroups(List<T_GelStep> act_group)
        {
            List<List<T_GelStep>> action_tree = new List<List<T_GelStep>>();
            //以X轴从小到大排序
            act_group.Sort((a, b) => { return a.Abs_X > b.Abs_X ? 1 : -1; });
            int x = -1;
            foreach (var act in act_group)
            {
                if(act.Abs_X!=x)
                {
                    var group = new List<T_GelStep>();
                    group.Add(act);
                    action_tree.Add(group);
                    x = act.Abs_X;
                }
                else if (action_tree.Count != 0)
                {
                    action_tree[action_tree.Count - 1].Add(act);
                }
            }
            return action_tree;
        }
        //按小组生成动作
        public Sequence GenerateAction(List<T_GelStep> act_group,SampleInfo sample_info)
        {
            //更新坐标计算针头个数
            int zt_count = 0;
            foreach (var act in act_group)
            {
                zt_count += act.InjectCount;
                act.UpdataPoints(sample_info);
            }
            var resmanager = ResManager.getInstance();
            Enterclose[] enters = new Enterclose[zt_count];//使用加样器个数
            for(int i=0;i< zt_count;i++)
            {
                enters[i] = injectorDevice.Injector.Entercloses[i];
            }
            //生成装帧动作
            var tip_seat = resmanager.GetFreeTip(zt_count);
            int []absorbs = {
                (int)injectorDevice.Injector.Entercloses[0].PumpMotor.Maximum.SetValue,
                (int)injectorDevice.Injector.Entercloses[1].PumpMotor.Maximum.SetValue,
                (int)injectorDevice.Injector.Entercloses[2].PumpMotor.Maximum.SetValue,
                (int)injectorDevice.Injector.Entercloses[3].PumpMotor.Maximum.SetValue
            };
            var sequ_taketip = Sequence.create(
                InjectTakeTip.create(30000, tip_seat.Z, 0, 0, tip_seat),
                InjectAbsorbMove.create(30000, enters, 100, absorbs));
            //生成加样动作
            var sequ_takesample = Sequence.create();
            var act_tree = DivideXIntoGroups(act_group);
            int inject_beg = 0;
            int[] point = { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                point[i] = (int)injectorDevice.Injector.Entercloses[i].YMotor.CurrentDistance;
                if (point[i] == 0) point[i] += (int)injectorDevice.Injector.Entercloses[i].YZero;
            }
            foreach (var act_g in act_tree)
            {
                var sque_movey = Sequence.create();
                int index = 0;
                bool is_ok = false;
                int[] tager = { -1, -1, -1, -1};
                int[] width = { 0, 0, 0, 0 };
                for (int i = 0; i < 4; i++)
                {
                    width[i] = (int)injectorDevice.Injector.Entercloses[1].YZero;
                    if(i<act_g.Count())tager[inject_beg+i] = act_g[i].Abs_Y;
                }
                GenerateSampleAddAct(ref sque_movey, ref index, ref tager, ref point, ref width, ref is_ok);
                inject_beg += act_g.Count();
                if (is_ok)
                {
                    sequ_takesample.AddAction(MoveTo.create(30000, act_g[0].Abs_X, -1, -1));//移动X 
                    sequ_takesample.AddAction(sque_movey);
                }
            }
            //生成混合液体
            var sequ_mixsample = Sequence.create();
            foreach (var act in act_group)
            {
                if (act.IsMix())
                {
                    sequ_mixsample.AddAction(MoveTo.create(30000, act.Mix_X, act.Mix_Y, -1));
                    sequ_mixsample.AddAction(InjectMoveTo.create(30000,enters,-1,IMask.Gen(-1),IMask.Gen(act.Mix_Z)));
                    if (act.StepClass == TestStepEnum.FPXSY)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            absorbs[0] = 1000;
                            sequ_mixsample.AddAction(InjectAbsorb.create(30000, enters, 100, absorbs));
                            absorbs[0] = -1000;
                            sequ_mixsample.AddAction(InjectAbsorb.create(30000, enters, 100, absorbs));

                        }
                    }
                    if (act.IsSpu() == false)
                    {
                        absorbs[0] = 1000;
                        sequ_mixsample.AddAction(InjectAbsorb.create(30000, enters, 100, absorbs));
                    }
                    sequ_mixsample.AddAction(InjectMoveTo.create(30000, enters, -1, IMask.Gen(-1), IMask.Gen(0)));
                }
            }
            //生成分配动作
            var sequ_putsample = Sequence.create();
            foreach (var act in act_group)
            {
                if (act.IsSpu())
                {
                    sequ_putsample.AddAction(MoveTo.create(30000, act.Spu_X, act.Spu_Y, -1));
                    sequ_putsample.AddAction(InjectMoveTo.create(30000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(act.Spu_Z)));
                    absorbs[0] = 1000;
                    sequ_putsample.AddAction(InjectAbsorb.create(30000, injectorDevice.GetSeleteced(), 100, absorbs));
                    sequ_putsample.AddAction(InjectMoveTo.create(30000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(0)));
                }
            }

            var action_list = Sequence.create();
            //生成脱针动作
            var sequ_puttip = Sequence.create();
            var inject_unload = resmanager.unload_list;
            if (inject_unload.Count() == 1)
            {
                sequ_puttip.AddAction(InjectPutTip.create(30000, (int)inject_unload[0].Z, 800, (int)inject_unload[0].FirstX, 0, new TakeTipData((int)inject_unload[0].X, (int)inject_unload[0].Y, 0, 1)));
                sequ_puttip.AddAction(InitXyz.create(30000, injectorDevice.GetSeleteced(), false, false, true));
            }
            action_list.AddAction(sequ_taketip);
            action_list.AddAction(sequ_takesample);
            action_list.AddAction(sequ_mixsample);
            action_list.AddAction(sequ_putsample);
            action_list.AddAction(sequ_puttip);
            return action_list;
        }
        //生成加样移动
        public void GenerateSampleAddAct(ref Sequence seque, ref int index, ref int[] tager, ref int[] point, ref int[] width, ref bool is_ok)
        {
            bool isforward = false;
            bool isback = false;
            if (index == 4)
            {
                is_ok = true;
                return;
            }
            else
            {
                isforward = index - 1 >= 0 ? tager[index] >= point[index - 1] + width[index - 1] : true;
                isback = index + 1 <= 3 ? tager[index] + width[index] <= point[index + 1] : true;
            }
            if (tager[index] < 0)
            {
                index++;
                GenerateSampleAddAct(ref seque, ref index, ref tager, ref point, ref width, ref is_ok);
            }
            else if (isforward && isback)
            {
                //单移
                point[index] = tager[index];
                int[] y = { -1, -1, -1, -1 };
                int[] z = { -1, -1, -1, -1 };
                y[index] = point[index] - width[index] * index;
                z[index] = -1;
                var move_act = Sequence.create(InjectMoveTo.create(300000, injectorDevice.GetSeleteced(), -1, y, z), SKSleep.create(index * 500), MoveTo.create(handDevice, 300000, -1, -1, 100), MoveTo.create(handDevice, 300000, -1, -1, 0));
                //查找上一个sp
                if (seque.actionlist[seque.actionlist.Count() - 1] is Spawn)
                {
                    var spawn = (Spawn)seque.actionlist[seque.actionlist.Count() - 1];
                    spawn.AddAction(move_act);
                }
                else
                {
                    var spawn = Spawn.create();
                    spawn.AddAction(move_act);
                    seque.AddAction(spawn);
                }
                index++;
                GenerateSampleAddAct(ref seque, ref index, ref tager, ref point, ref width, ref is_ok);
            }
            else if (isforward && isback == false)
            {
                //全移
                int movew = tager[index] - point[index];
                for (int i = index; i < 4; i++)
                {
                    point[i] = point[i] + movew;
                }
                int indexx = index;
                int y = point[index] - width[index] * index;
                var move_act = Sequence.create(SkCallBackFun.create((ActionBase act) =>
                {
                    for (int i = 0; i < 4; i++)
                        injectorDevice.Injector.Entercloses[i].Selected = i >= indexx;
                    return true;
                }), MoveTo.create(injectorDevice, 300000, -1, y, -1));
                seque.AddAction(move_act);
                GenerateSampleAddAct(ref seque, ref index, ref tager, ref point, ref width, ref is_ok);
            }
            else if (isforward == false && isback)
            {
                //等待前面完成
                int movew = tager[index] - point[index];
                for (int i = 0; i < 4; i++)
                {
                    point[i] = point[i] + movew;
                    if (point[i] < 0)
                    {
                        is_ok = false;
                        return;
                    }
                }
                int y = point[0];
                var move_act = Sequence.create(SkCallBackFun.create((ActionBase act) => {
                    for (int i = 0; i < 4; i++)
                        injectorDevice.Injector.Entercloses[i].Selected = true;
                    return true;
                }), MoveTo.create(injectorDevice, 300000, -1, y, -1));
                seque.AddAction(move_act);
                GenerateSampleAddAct(ref seque, ref index, ref tager, ref point, ref width, ref is_ok);
            }
        }
        //生成在普通位抓卡
        public bool GenerateTakeGelFromNormal(T_Gel gel, ref Sequence seque, string code = "", int seatindex = -1)
        {
            var resmanager = ResManager.getInstance();
            T_BJ_GelSeat gelselect = (T_BJ_GelSeat)resmanager.SearchGelCard(typeof(T_BJ_GelSeat).Name, gel, true, code, seatindex);
            if (gelselect!=null)
            {
                //抓手移动
                seque.AddAction(MoveTo.create(300000, (int)gelselect.X, (int)(gelselect.Y + gelselect.Gap * seatindex), 0));
                //抓手抓卡
                seque.AddAction(HandTakeCard.create(700000, (int)gelselect.Z, (int)gelselect.ZLimit, (int)gelselect.ZCatch, 0));
                //把测试卡放在Gel位中
                gelselect.Values[seatindex, 0] = null;
                seque.destroyfun = (ActionBase act) =>
                {
                    gelselect.Values[seatindex, 0] = gel;
                    return true;
                };
            }
            else
            {
                ErrorSystem.WriteActError(gelselect.Code + "位已满");
            }
            return gelselect!=null;
        }
        //生成放卡在普通位
        public bool GeneratePutGelToNormal(T_Gel gel,ref Sequence seque, string code = "",int seatindex = -1)
        {
            var resmanager = ResManager.getInstance();
            T_BJ_GelSeat gelselect = (T_BJ_GelSeat)resmanager.SearchGelCard(typeof(T_BJ_GelSeat).Name, gel, false, code, seatindex);
            if (gelselect!=null)
            {
                //抓手移动
                seque.AddAction(MoveTo.create(300000, (int)gelselect.X, (int)(gelselect.Y + gelselect.Gap * seatindex), 0));
                //抓手放卡
                seque.AddAction(HandPutCard.create(500000, (int)gelselect.ZPut, 0));
                //把测试卡放在Gel位中
                gelselect.Values[seatindex, 0] = gel;
                seque.destroyfun = (ActionBase act) =>
                {
                    gelselect.Values[seatindex, 0] = null;
                    return true;
                };
            }
            else
            {
                ErrorSystem.WriteActError(gelselect.Code+"位已满");
            }
            return gelselect!=null;
        }
        //生成离心机抓卡动作
        public bool GenerateTakeGelFromCent(T_Gel gel, ref Sequence seque, string code = "", int seatindex = -1)
        {
            var resmanager = ResManager.getInstance();
            T_BJ_Centrifuge gelselect = (T_BJ_Centrifuge)resmanager.SearchGelCard(typeof(T_BJ_Centrifuge).Name, gel, true, code, seatindex);
            var centrifuge = cenMrg.GetCentrifugeByCode(code);
            if (centrifuge == null) centrifuge = cenMrg.GetFreeCentrifuge();
            if (gelselect!=null)
            {
                //离心机位移动
                //抓手移动到离心机位
                var move_act = Spawn.create(
                    MoveTo.create(centrifuge, 30000, -1, -1, (int)(seatindex * (double)gelselect.Gel0), 5),
                    MoveTo.create(handDevice, 30000, (int)gelselect.HandX, (int)gelselect.HandY));
                seque.AddAction(move_act);
                //抓手抓卡
                seque.AddAction(HandTakeCard.create(700000, (int)gelselect.HandZ, (int)gelselect.ZLimit, (int)gelselect.ZCatch, 0));
                //把测试卡放在离心Gel位中
                gelselect.Values[seatindex, 0] = null;
                seque.destroyfun = (ActionBase act) =>
                {
                    gelselect.Values[seatindex, 0] = gel;
                    return true;
                };
            }
            else
            {
                ErrorSystem.WriteActError("离心机位打不到卡");
            }
            return gelselect!=null;
        }
        //生成离心机放卡动作
        public bool GeneratePutGelToCent(T_Gel gel,ref Sequence seque, string code="",int seatindex=-1)
        {
            int[] gelseatindex = { 0, 6, 1, 7, 2, 8, 3, 9, 4, 10, 5, 11 };
            var centrifuge = cenMrg.GetCentrifugeByCode(code);
            if(centrifuge==null)centrifuge = cenMrg.GetFreeCentrifuge();
            var gelseat = centrifuge.GetGelSeatsInfo();
            var gelseatsetting = centrifuge.GetGelSeatSetting();
            bool isfind = seatindex!=-1;
            if(seatindex==-1)
            {
                for (int i = 0; i < 12; i++)
                {
                    int index = gelseatindex[i];
                    if (gelseat[index] == false)
                    {
                        seatindex = index;
                        isfind = true;
                        break;
                    }
                }
            }
            if (isfind)
            {
                //离心机位移动
                //抓手移动到离心机位
                var move_act = Spawn.create(
                   MoveTo.create(centrifuge, 30000, -1, -1, (int)(seatindex * (double)gelseatsetting.Gel0), 5),
                   MoveTo.create(handDevice, 30000, (int)gelseatsetting.HandX, (int)gelseatsetting.HandY));
                seque.AddAction(move_act);
                //抓手放卡
                seque.AddAction(HandPutCard.create(handDevice, 30000, (int)gelseatsetting.ZPut));
                //把测试卡放在离心Gel位中
                gelseatsetting.Values[seatindex, 0] = gel;
                seque.destroyfun = (ActionBase act) =>
                {
                    gelseatsetting.Values[seatindex, 0] = null;
                    return true;
                };
            }
            else
            {
                ErrorSystem.WriteActError("离心机位已满");
            }
            return isfind;
        }


    }
}