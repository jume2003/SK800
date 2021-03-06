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
        public Sequence GenerateAction(List<T_GelStep> act_group,SampleInfo sample_info,ref int[] point)
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
            int[] width = { 0, 0, 0, 0 };
            int[] tipgp = { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                width[i] = (int)injectorDevice.Injector.Entercloses[1].YZero;
                tipgp[i] = (int)injectorDevice.Injector.Entercloses[i].MinDistance;
            }
            //生成装帧动作
            var tip_seat = resmanager.GetFreeTipActPoint(zt_count,2);
            var sequ_taketip = GenerateInjectActGroup(tip_seat, ref point, width, tipgp);

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
                int[] tager = { -1, -1, -1, -1 };
                int[] width = { 0, 0, 0, 0 };
                for (int i = 0; i < 4; i++)
                {
                    width[i] = (int)injectorDevice.Injector.Entercloses[1].YZero;
                    if (i < act_g.Count()) tager[inject_beg + i] = act_g[i].Abs_Y;
                }
                GenerateInjectActGroup(ref sque_movey, ref index, ref tager, ref point, ref width, ref is_ok);
                inject_beg += act_g.Count();
                if (is_ok)
                {
                    sequ_takesample.AddAction(MoveTo.create(30000, act_g[0].Abs_X, -1, -1));//移动X 
                    sequ_takesample.AddAction(sque_movey);
                }
            }
            ////生成混合液体
            //var sequ_mixsample = Sequence.create();
            //foreach (var act in act_group)
            //{
            //    if (act.IsMix())
            //    {
            //        sequ_mixsample.AddAction(MoveTo.create(30000, act.Mix_X, act.Mix_Y, -1));
            //        sequ_mixsample.AddAction(InjectMoveTo.create(30000,enters,-1,IMask.Gen(-1),IMask.Gen(act.Mix_Z)));
            //        if (act.StepClass == TestStepEnum.FPXSY)
            //        {
            //            for (int i = 0; i < 3; i++)
            //            {
            //                absorbs[0] = 1000;
            //                sequ_mixsample.AddAction(InjectAbsorb.create(30000, enters, 100, absorbs));
            //                absorbs[0] = -1000;
            //                sequ_mixsample.AddAction(InjectAbsorb.create(30000, enters, 100, absorbs));

            //            }
            //        }
            //        if (act.IsSpu() == false)
            //        {
            //            absorbs[0] = 1000;
            //            sequ_mixsample.AddAction(InjectAbsorb.create(30000, enters, 100, absorbs));
            //        }
            //        sequ_mixsample.AddAction(InjectMoveTo.create(30000, enters, -1, IMask.Gen(-1), IMask.Gen(0)));
            //    }
            //}
            ////生成分配动作
            //var sequ_putsample = Sequence.create();
            //foreach (var act in act_group)
            //{
            //    if (act.IsSpu())
            //    {
            //        sequ_putsample.AddAction(MoveTo.create(30000, act.Spu_X, act.Spu_Y, -1));
            //        sequ_putsample.AddAction(InjectMoveTo.create(30000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(act.Spu_Z)));
            //        absorbs[0] = 1000;
            //        sequ_putsample.AddAction(InjectAbsorb.create(30000, injectorDevice.GetSeleteced(), 100, absorbs));
            //        sequ_putsample.AddAction(InjectMoveTo.create(30000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(0)));
            //    }
            //}

            var action_list = Sequence.create();
            ////生成脱针动作
            //var sequ_puttip = Sequence.create();
            //var inject_unload = resmanager.unload_list;
            //if (inject_unload.Count() == 1)
            //{
            //    sequ_puttip.AddAction(InjectPutTip.create(30000, (int)inject_unload[0].Z, 800, (int)inject_unload[0].FirstX, 0, new TakeTipData((int)inject_unload[0].X, (int)inject_unload[0].Y, 0, 1)));
            //    sequ_puttip.AddAction(InitXyz.create(30000, injectorDevice.GetSeleteced(), false, false, true));
            //}
            action_list.AddAction(sequ_taketip);
            //action_list.AddAction(sequ_takesample);
            //action_list.AddAction(sequ_mixsample);
            //action_list.AddAction(sequ_putsample);
            //action_list.AddAction(sequ_puttip);
            return action_list;
        }
        //
        public Sequence GenerateInjectActGroup(ActionPoint[] tagers, ref int[] point, bool is_asc=true)
        {
            //目标要根据针头偏移
            int[] width = { 0, 0, 0, 0 };
            int[] tipgp = { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                width[i] = (int)injectorDevice.Injector.Entercloses[1].YZero;
                tipgp[i] = ;
            }
            for (int i=0;i< tagers.Length;i++)
            {
                tagers[i].y = tagers[i].y + (int)injectorDevice.Injector.Entercloses[i].MinDistance;
            }
            //接X分组
            List<ActionPoint> tager_list = tagers.ToList();
            List<List<ActionPoint>> tager_group = new List<List<ActionPoint>>();
            if(is_asc)
            tager_list.Sort((a, b) => { return a.x > b.x ? 1 : -1; });
            else
            tager_list.Sort((a, b) => { return a.x< b.x ? 1 : -1; });
            int x = -1;
            foreach (var tagerp in tager_list)
            {
                if (tagerp.x != x)
                {
                    var group = new List<ActionPoint>();
                    group.Add(tagerp);
                    tager_group.Add(group);
                    x = tagerp.x;
                }
                else if (tager_group.Count != 0)
                {
                    tager_group[tager_group.Count - 1].Add(tagerp);
                }
            }
            //接Y排序
            foreach(var group in tager_group)
            {
                group.Sort((a, b) => { return a.y > b.y ? 1 : -1; });
            }
            //得到当前各别样Y轴坐标
           
            int[] point = { 0, 0, 0, 0 };
            int[] pcbpoint = { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                bool is_timeout = false;
                injectorDevice.CanComm.ReadRegister(injectorDevice.Injector.Entercloses[i].YMotor.RealDistance.Addr);
                pcbpoint[i] = injectorDevice.CanComm.GetIntBlock(injectorDevice.Injector.Entercloses[i].YMotor.RealDistance.Addr, 1000, out is_timeout);
                point[i] = pcbpoint[i] + (int)injectorDevice.Injector.Entercloses[i].YZero;
            }
            //生成动作组合
            int index = 0;
            int inject_beg = 0;
            bool is_ok = false;
            var seque = Sequence.create();
            foreach (var group in tager_group)
            {
                var sque_movey = Sequence.create();
                int[] tager = { -1, -1, -1, -1 };
                for (int i = 0; i < group.Count(); i++)
                {
                    tager[inject_beg + i] = group[i].y;
                }
                index = 0;
                GenerateInjectActGroup(ref sque_movey, ref index, ref tager, ref point, ref width, ref is_ok);
                inject_beg += group.Count();
                if (is_ok)
                {
                    var spawn = Spawn.create();
                    spawn.AddAction(MoveTo.create(30000, group[0].x, -1, -1));
                    spawn.AddAction(sque_movey);
                    seque.AddAction(spawn);
                }
                else
                {
                    break;
                }
            }
            return seque;
        }
        //算出距离
        int GetLoss(int[] tager, int[] point)
        {
            int disc = 0;
            for (int i = 0; i < 4; i++)
            {
                if(tager[i]!=-1)
                disc += Math.Abs(tager[i] - point[i]);
            }
            return disc;
        }
        //生成加样移动组合
        public void GenerateInjectActGroup(ref Sequence seque, ref int index, ref int[] tager, ref int[] point, ref int[] width, ref bool is_ok)
        {
            if (index == 4)
            {
                is_ok = true;
                return;
            }
            else if (tager[index] == point[index] || tager[index] == -1)
            {
                if (tager[index] == point[index])
                {
                    List<Enterclose> ents = new List<Enterclose>();
                    if (index == 0)
                    {
                        var move_act = Sequence.create();
                        ents.Add(injectorDevice.Injector.Entercloses[index]);
                        move_act.AddAction(InjectMoveTo.create(300000, ents.ToArray(), -1, IMask.Gen(-1), IMask.Gen(400, -1, -1, -1)));
                        move_act.AddAction(InjectMoveTo.create(300000, ents.ToArray(), -1, IMask.Gen(-1), IMask.Gen(0, -1, -1, -1)));
                        seque.AddAction(move_act);
                    }
                    else
                    {
                        var move_act = Sequence.create();
                        move_act.AddAction(MoveTo.create(handDevice, 300000, -1, -1, 100));
                        move_act.AddAction(MoveTo.create(handDevice, 300000, -1, -1, 0));
                        seque.AddAction(move_act);
                    }
                }
                index++;
                GenerateInjectActGroup(ref seque,ref index, ref tager, ref point, ref width,ref is_ok);
            }
            else
            {
                List<int[]> node_list = new List<int[]>();
                for (int i = 0; i < 4; i++)
                {
                    for (int j = i; j < 4; j++)
                    {
                        for (int k = j; k < 4; k++)
                        {
                            for (int l = k; l < 4; l++)
                            {
                                int[] point_tem = { 0, 0, 0, 0 };
                                point_tem[i] = tager[i];
                                point_tem[j] = tager[j];
                                point_tem[k] = tager[k];
                                point_tem[l] = tager[l];
                                for (int n = 0; n < 4; n++)
                                    if (point_tem[n] < 0) point_tem[n] = 0;
                                bool ispass = true;
                                int frist = 0;
                                for (int n = 0; n < 4; n++)
                                {
                                    if (point_tem[n] != 0)
                                    {
                                        frist = n;
                                        break;
                                    }
                                }
                                for (int n = frist; n < 4; n++)
                                {
                                    if (point_tem[n] == 0 && n - 1 >= 0) point_tem[n] = point_tem[n - 1] + width[n];
                                }
                                for (int n = 0; n < frist; n++)
                                {
                                    if (point_tem[n] == 0 && n + 1 <= 3) point_tem[n] = point_tem[frist] - width[n] * (frist - n);
                                }
                                for (int n = 0; n < 4; n++)
                                {
                                    bool isforward = n - 1 >= 0 ? point_tem[n] >= point_tem[n - 1] + width[n - 1] : true;
                                    bool isback = n + 1 <= 3 ? point_tem[n] + width[n] <= point_tem[n + 1] : true;

                                    ispass = isforward && isforward;
                                    if (!ispass) break;
                                }
                                if (ispass && point_tem[index] == tager[index])
                                    node_list.Add(point_tem);
                            }
                        }
                    }
                }
                int[] minnode = node_list[0];
                foreach (var node in node_list)
                {
                    if (GetLoss(tager, node) < GetLoss(tager, minnode))
                        minnode = node;
                }
                List<int> left = new List<int>();
                List<int> right = new List<int>();
                for (int i = 0; i < 4; i++)
                {
                    if (point[i] - minnode[i] < 0)
                        left.Add(i);
                    else
                        right.Add(i);
                }
                left.Sort((a, b) => { return a < b ? 1 : -1; });
                right.Sort((a, b) => { return a > b ? 1 : -1; });
                var movesp_act = Spawn.create();
                var move_act = Sequence.create();
                string msg = "";

                int[] yy = { -1, -1, -1, -1 };
                int[] zz = { -1, -1, -1, -1 };
                List<Enterclose> ents = new List<Enterclose>();
                for (int i = 0; i < left.Count; i++)
                {
                    int indextem = left[i];
                    if (point[indextem] != minnode[indextem])
                    {
                        ents.Add(injectorDevice.Injector.Entercloses[indextem]);
                        yy[indextem] = minnode[indextem];
                    }
                }
                movesp_act.AddAction(InjectMoveTo.create(300000, ents.ToArray(), -1, yy, zz));
                ents.Clear();
                for (int i = 0; i < right.Count; i++)
                {
                    int indextem = right[i];
                    if (point[indextem] != minnode[indextem])
                    {
                        ents.Add(injectorDevice.Injector.Entercloses[indextem]);
                        yy[indextem] = minnode[indextem];
                    }
                }
                movesp_act.AddAction(InjectMoveTo.create(300000, ents.ToArray(), -1, yy, zz));



                //for (int i = 0; i < 4; i++)
                //{
                //    for (int j = 0; j < 4; j++)
                //    injectorDevice.Injector.Entercloses[j].Selected = false;
                //    int[] yy = { -1, -1, -1, -1 };
                //    int[] zz = { -1, -1, -1, -1 };
                //    if (i < left.Count)
                //    {
                //        //msg = msg + "left " + left[i] + ":" + point[left[i]] + "->" + (minnode[left[i]] + " ");
                //        int indextem = left[i];
                //        if (point[indextem] !=minnode[indextem])
                //        {
                //            yy[indextem] = minnode[indextem];
                //            injectorDevice.Injector.Entercloses[indextem].Selected = true;
                //            movesp_act.AddAction(InjectMoveTo.create(300000, injectorDevice.GetSeleteced(), -1, yy, zz));
                //        }
                //    }
                //    for (int j = 0; j < 4; j++)
                //    injectorDevice.Injector.Entercloses[j].Selected = false;
                //    if (i < right.Count)
                //    {
                //        //msg = msg + "right " + right[i] + ":" + point[right[i]] + "->" + (minnode[right[i]] + " ");
                //        int indextem = right[i];
                //        if (point[indextem] != minnode[indextem])
                //        {
                //            yy[indextem] = minnode[indextem];
                //            injectorDevice.Injector.Entercloses[indextem].Selected = true;
                //            movesp_act.AddAction(InjectMoveTo.create(300000, injectorDevice.GetSeleteced(), -1, yy, zz));
                //        }
                //    }
                //}
                for (int i = 0; i < 4; i++)
                point[i] = minnode[i];
                move_act.AddAction(movesp_act);
                move_act.AddAction(SKSleep.create(1));
                seque.AddAction(move_act);
                GenerateInjectActGroup(ref seque, ref index, ref tager, ref point, ref width, ref is_ok);
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