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
using SKABO.Common.Models.TestStep;
using SKABO.Common.Utils;
using System.Diagnostics;

namespace SKABO.ActionGeneraterEngine
{
    public class ActionGenerater
    {
        public static ActionGenerater instance = null;
        public IGelService gelService = IoC.Get<IGelService>();


        public static ActionGenerater getInstance()
        {
            if (instance == null)
            {
                instance = new ActionGenerater();
            }
            return instance;
        }

        public ActionGenerater getActionGenerater()
        {
            return instance;
        }
        //把转换为action point
        public ActionPoint GelStepToActionPoint(T_GelStep gel_step,TestStepEnum type, ResInfoData mix_deep_seat)
        {
            if (gel_step.StepClass == TestStepEnum.FPYT&&gel_step.GetLiquidInfo().IsDetector&& type!=TestStepEnum.MixLiquid && type != TestStepEnum.SpuLiquid)
            type = TestStepEnum.FollowAbsLiquid;
            ActionPoint action_point = new ActionPoint(-1, -1, -1, type);
            var resmanager = ResManager.getInstance();
            var exp_pack = ((ExperimentPackage)gel_step.ExperPackage);
            if (gel_step.StepClass == TestStepEnum.FPYT)
            {
                //是否为混合
                bool is_mix = gel_step.is_mix;
                var fpytinfo = gel_step.GetFpytInfo();
                var liquidinfo = gel_step.GetLiquidInfo();
                action_point.deep = (int)liquidinfo.Deep;
                action_point.deepspeed = (int)liquidinfo.DeeSpeed;
                action_point.detectordeep = (int)liquidinfo.DetectorDeep;
                action_point.tube = (int)(fpytinfo.TubeValue<< (gel_step.SampleIndex* (exp_pack.gel_type/exp_pack.ren_fen)));
                action_point.capacity = (int)liquidinfo.Vol;
                action_point.absbspeed = (int)liquidinfo.AbsSpeed;
                action_point.spuspeed = (int)liquidinfo.SpuSpeed;
                action_point.backcapacity = (int)liquidinfo.BackCapacity;
                action_point.backspeed = (int)liquidinfo.BackCapacitySpeed;
                action_point.mixtimes = (int)(gel_step.is_spu?fpytinfo.MixTimes:0);
                action_point.mixdeep = (int)fpytinfo.MixDeep;
                action_point.spucapacity = (int)fpytinfo.Vol;
                action_point.liquid_type = liquidinfo.LiquidType;
                if (action_point.type == TestStepEnum.AbsLiquid|| action_point.type == TestStepEnum.FollowAbsLiquid)
                {
                    ResInfoData abs_tager = null;//吸取的液体
                    if (liquidinfo.LiquidType == "病人红细胞"|| liquidinfo.LiquidType == "病人血清")
                    {
                        abs_tager = resmanager.GetResByCode(exp_pack.GetSampleCode(gel_step.SampleIndex), "T_BJ_SampleRack");
                    }
                    else
                    {
                        abs_tager = resmanager.GetResByCode(liquidinfo.LiquidType+ gel_step.InjIndex, "T_BJ_AgentiaWarehouse");
                        if(abs_tager==null) abs_tager = resmanager.GetResByCode(liquidinfo.LiquidType, "T_BJ_AgentiaWarehouse");
                    }
                    System.Diagnostics.Debug.Assert(abs_tager != null);
                    action_point.x = abs_tager.InjectorX;
                    action_point.y = abs_tager.InjectorY;
                    action_point.z = abs_tager.InjectorZ;
                }
                else if (action_point.type == TestStepEnum.MixLiquid)
                {
                    //查找deep盘
                    ResInfoData deep_plane = resmanager.GetResByCode(gel_step.MixCode, "T_BJ_DeepPlate");
                    if (deep_plane == null)
                    {
                        deep_plane = mix_deep_seat;
                        //deep_plane = resmanager.GetResByCode("null", "T_BJ_DeepPlate");
                        deep_plane.Values[deep_plane.CountX, deep_plane.CountY] = deep_plane;
                    }
                    System.Diagnostics.Debug.Assert(deep_plane != null);
                    if(deep_plane.FindCode(gel_step.MixCode,false)==false)
                    deep_plane.AddCode(gel_step.MixCode);
                    action_point.x = deep_plane.InjectorX;
                    action_point.y = deep_plane.InjectorY;
                    action_point.z = deep_plane.InjectorZ;
                    action_point.hitsort = gel_step.is_spu ? 0 : 1;
                }
                else if (action_point.type == TestStepEnum.SpuLiquid)
                {
                    //查找gel卡
                    ResInfoData gel_seat = resmanager.GetResByCode(exp_pack.GetGelMask(), "T_BJ_GelSeat");
                    System.Diagnostics.Debug.Assert(gel_seat != null);
                    action_point.x = gel_seat.InjectorX;
                    action_point.y = gel_seat.InjectorY;
                    action_point.z = gel_seat.InjectorZ;
                    action_point.tube_gap = gel_seat.InjectorGap;
                }
            }
            return action_point;
        }
        //动作解析(分配液体分裂)
        public static int G_MixCode = 0;
        public List<T_GelStep> ResolveActions(T_Gel gel)
        {
            List<T_GelStep> gelstep_list = new List<T_GelStep>();
            foreach (var gelstep in gel.GelSteps)
            {
                if (gelstep.StepClass == TestStepEnum.FPYT)
                {
                    FPYTStepParameter action = gelstep.GetFpytInfo();
                    int index = 0;
                    foreach (var item in action.LiquidList)
                    {
                        var gelfpyt = gelstep.clone();
                        gelfpyt.LiquidTypeIndex = index;
                        gelfpyt.MixCode = "mix_code:"+G_MixCode;
                        gelfpyt.is_mix = action.LiquidList.Count>1;
                        //gelfpyt.is_spu = action.LiquidList.IndexOf(item) == 0;
                        gelstep_list.Add(gelfpyt);
                        index++;
                    }
                    G_MixCode++;
                }
                else
                {
                    gelstep_list.Add(gelstep);
                }

            }
            return gelstep_list;
        }
        //动作分组
        public List<List<T_GelStep>> DivideIntoGroups(List<T_GelStep> gelstep_list, int inject_count)
        {
            int count = 0;
            List<List<T_GelStep>> action_tree = new List<List<T_GelStep>>();
            foreach (var act in gelstep_list)
            {
                if(act.InjectCount == 0)
                {
                    var group = new List<T_GelStep>();
                    group.Add(act);
                    action_tree.Add(group);
                    count = 0;
                }
                else
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
            }
            return action_tree;
        }


        //按小组生成动作
        public Sequence GenerateAction(List<T_GelStep> act_group)
        {
            //更新坐标计算吸头个数
            int zt_count = 0;
            foreach (var act in act_group)
            {
                act.InjIndex = zt_count;
                zt_count += act.InjectCount;
            }
            var resmanager = ResManager.getInstance();
            Enterclose[] enters = new Enterclose[zt_count];//使用加样器个数
            for (int i = 0; i < zt_count; i++)
            {
                enters[i] = Engine.getInstance().injectorDevice.Injector.Entercloses[i];
            }
            //生成装帧动作
            var tip_seat = resmanager.GetFreeTipActPoint(zt_count, 2);
            var sequ_taketip = InjectMoveActs.create(3001, tip_seat, false);
            //生成加样动作
            var sequ_takesample = Sequence.create();
            List<ActionPoint> abs_seat = new List<ActionPoint>();
            int abs_index = 0;
            var abs_width_rate = IMask.Gen(1.0f);
            foreach (var act_abs in act_group)
            {
                var abs_point = GelStepToActionPoint(act_abs, TestStepEnum.AbsLiquid,null);
                var agentseat = resmanager.GetAgentiaWarehouseSeat(abs_point.liquid_type);
                if (agentseat != null)
                {
                    abs_width_rate[act_abs.InjIndex] = !act_abs.GetLiquidInfo().IsAgentia ? 0.7693f : 1.1f;
                    abs_point.y += (int)(agentseat.Gap * abs_index) * (agentseat.Count == 1 ? 1 : 0);
                }
                abs_seat.Add(abs_point);
                abs_index++;
            }
            sequ_takesample.AddAction(InjectMoveActs.create(3001, abs_seat.ToArray(), true, abs_width_rate));
            //计算混合是否需要分配（当前卡在第一卡位时且只有一个试验）
            var inj_exp_list =  ExperimentLogic.getInstance().GetInjExpPackageList();
            foreach (var act_mix in act_group)
            {
                var exp = (ExperimentPackage)act_mix.ExperPackage;
                var piperseat = resmanager.GetResByCode(exp.GetGelMask(), "T_BJ_GelSeat", "", "4");
                if(piperseat.CountX==0&& inj_exp_list.Count==1)
                    act_mix.is_spu = act_mix.InjIndex== 0;
                else
                    act_mix.is_spu = act_mix.LiquidTypeIndex == act_mix.GetFpytInfo().LiquidList.Count - 1;
            }
            //得到可用深盘
            var mix_list = act_group.Where(item => item.is_mix == true).ToList();
            mix_list = mix_list.Where((x, x_index) => x_index == (mix_list.FindIndex(y => y.MixCode == x.MixCode))).ToList();
            int mix_inj_count = mix_list.Count;
            var mix_deep_seat = resmanager.GetFreeDeepPlate(mix_inj_count, 2);
            //生成混合液体
            var sequ_mixsample = Sequence.create();
            var mix_seat = IMask.Gen(new ActionPoint(-1, -1, -1));
            int mix_index = 0;
            foreach (var act_mix in act_group)
            {
                if (act_mix.is_mix)
                {
                    var mix_point = GelStepToActionPoint(act_mix, TestStepEnum.MixLiquid, mix_deep_seat[mix_index% mix_inj_count]);
                    mix_seat[act_mix.InjIndex] = mix_point;
                    mix_index++;
                }
            }
            sequ_mixsample.AddAction(InjectMoveActs.create(3001, mix_seat.ToArray(), true));
            //生成分配动作
            var sequ_putsample = Sequence.create();
            //查找卡位
            List<ActionPoint> spupoint_list = new List<ActionPoint>();
            foreach (var act_spu in act_group)
            {
                if (act_spu.is_spu)
                {
                    var spu_point = GelStepToActionPoint(act_spu, TestStepEnum.SpuLiquid,null);
                    spu_point.index = act_spu.InjIndex;
                    spupoint_list.Add(spu_point);
                }
            }
            var hit_count = IMask.Gen(0);
            for (int i=0;i<8;i++)
            {
                bool is_find = false;
                var points = IMask.Gen(new ActionPoint(-1, -1, -1));
                foreach (var spuc in spupoint_list)
                {
                    bool is_hit = (spuc.tube & (0x01 << i))!=0;
                    if(is_hit)
                    {
                        var tubelist = spuc.GetTubeList();
                        points[spuc.index] = (ActionPoint)spuc.Clone();
                        points[spuc.index].x = (int)(spuc.x + i * spuc.tube_gap);
                        points[spuc.index].y = (int)spuc.y;
                        if (tubelist.Count() != 1 && hit_count[spuc.index] < tubelist.Count() - 1)
                        {
                            points[spuc.index].zb = points[spuc.index].z - 1500;
                            if (points[spuc.index].zb <= 0) points[spuc.index].zb = 0;
                        }
                        hit_count[spuc.index]++;
                        is_find = true;
                    }
                }
                if(is_find)
                sequ_putsample.AddAction(InjectMoveActs.create(3001, points.ToArray(), true));
            }
            //生成脱针动作
            var sequ_puttip = Sequence.create();
            List<ActionPoint> unload_seat = new List<ActionPoint>();
            var inject_unload = resmanager.unload_list;
            if (inject_unload.Count() == 1)
            {
                var unloader = inject_unload[0];
                for (int i = 0; i < 4; i++)
                {
                    var unload_point = new ActionPoint((int)unloader.X, (int)unloader.Y + i * (int)unloader.FZ, (int)unloader.Z, TestStepEnum.PutTip);
                    unload_point.puttip_x = (int)unloader.FirstX;
                    unload_seat.Add(unload_point);
                }
                sequ_puttip.AddAction(InjectMoveActs.create(3001, unload_seat.ToArray(), true));
            }

            var action_list = Sequence.create();
            action_list.node = Engine.getInstance().injectorDevice;
            action_list.AddAction(sequ_taketip);
            action_list.AddAction(sequ_takesample);
            action_list.AddAction(sequ_mixsample);
            action_list.AddAction(sequ_putsample);
            action_list.AddAction(sequ_puttip);
            return action_list;
        }
        //
        public Sequence GenerateInjectActGroup(ActionPoint[] tagers,bool is_asc=true,double []width_rate=null)
        {
            int[] point = { 0, 0, 0, 0 };
            int[] width = { 0, 0, 0, 0 };
            //目标要根据针头偏移
            for (int i = 0; i < tagers.Length; i++)
            {
                tagers[i].index = i;
            }
            //删除为负数的参数
            if (width_rate == null) width_rate = IMask.Gen(1.0f);
            tagers = tagers.ToList().Where(item => item.y >= 0).ToArray();
            for (int i = 0; i < 4; i++)
            {
                bool is_timeout = false;
                width[i] = (int)((double)Engine.getInstance().injectorDevice.Injector.Entercloses[i].InjWidth);
                //得到当前各别样Y轴坐标
                Engine.getInstance().injectorDevice.CanComm.ReadRegister(Engine.getInstance().injectorDevice.Injector.Entercloses[i].YMotor.RealDistance.Addr);
                int pointtem = Engine.getInstance().injectorDevice.CanComm.GetIntBlock(Engine.getInstance().injectorDevice.Injector.Entercloses[i].YMotor.RealDistance.Addr, 1000, out is_timeout);
                point[i] = pointtem + (int)Engine.getInstance().injectorDevice.Injector.Entercloses[i].TipDis;
                if (is_timeout) return null;
            }
            //接X分组
            List<ActionPoint> tager_list = tagers.ToList();
            List<List<ActionPoint>> tager_group = new List<List<ActionPoint>>();
            if(is_asc)
            tager_list.Sort((a, b) => { return a.x > b.x ? 1 : -1; });
            else
            tager_list.Sort((a, b) => { return a.x< b.x ? 1 : -1; });
            int x = -10;
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
            //生成动作组合
            int index = 0;
            bool is_ok = false;
            var seque = Sequence.create();
            foreach (var group in tager_group)
            {
                var move_y = Sequence.create();
                ActionPoint[] tager = IMask.Gen(new ActionPoint(-1, -1, -1));
                var tager_sort = IMask.Gen(new ActionPoint(-1, -1, -1)).ToList();
                for (int i = 0; i < group.Count(); i++)
                {
                    tager[group[i].index] = group[i];
                    tager_sort[group[i].index] = group[i];
                }
                index = 0;
                int[] hit_sort = { 0, 1, 2, 3 };
                //y轴顺序排列
                tager_sort.Sort((a, b) => { return a.hitsort <= b.hitsort ? 1 : -1; });
                for (int i = 0; i < tager_sort.Count(); i++)
                {
                    hit_sort[i] = tager_sort[i].index;
                }
                var spawn = Spawn.create();
                ActionBase move_x = null;
                if (group[0].x != -1)
                    move_x = (ActionBase)MoveTo.create(3001, group[0].x, -1, -1);
                else
                    move_x = SKSleep.create(0);
                GenerateInjectActGroup(ref move_y, hit_sort,ref index, ref tager, ref point, ref width,ref width_rate, ref is_ok, move_x);
                if (is_ok)
                {
                    spawn.AddAction(move_x);
                    spawn.AddAction(move_y);
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
        int GetLoss(ActionPoint[] tager, int[] point)
        {
            int disc = 0;
            for (int i = 0; i < 4; i++)
            {
                if(tager[i].y!=-1)
                disc += Math.Abs(tager[i].y - point[i]);
            }
            return disc;
        }
        //生成加样移动组合
        public void GenerateInjectActGroup(ref Sequence seque,int []hitsort,ref int index, ref ActionPoint[] tager, ref int[] point, ref int[] width,ref double[]width_rate, ref bool is_ok, ActionBase move_x=null)
        {
            int hit_index = 0;
            if (index <4) hit_index = hitsort[index];
            
            if (index == 4)
            {
                is_ok = true;
                return;
            }
            else if ((tager[hit_index].isdone&&tager[hit_index].y == point[hit_index]) || tager[hit_index].y == -1)
            {
                index++;
                GenerateInjectActGroup(ref seque, hitsort, ref index, ref tager, ref point, ref width,ref width_rate, ref is_ok, move_x);
            }
            else
            {
                List<int[]> node_list = new List<int[]>();
                int[] width_tem = IMask.Gen(0);
                for(int i=0;i<4;i++)
                {
                    width_tem[i] = (int)(width[i] * width_rate[hit_index]);
                }
                for (int i = 0; i < 4; i++)
                {
                    for (int j = i; j < 4; j++)
                    {
                        for (int k = j; k < 4; k++)
                        {
                            for (int l = k; l < 4; l++)
                            {
                                int[] point_tem = { 0, 0, 0, 0 };
                                point_tem[i] = tager[i].y;
                                point_tem[j] = tager[j].y;
                                point_tem[k] = tager[k].y;
                                point_tem[l] = tager[l].y;
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
                                    if (point_tem[n] == 0 && n - 1 >= 0) point_tem[n] = point_tem[n - 1] + width_tem[n];
                                }
                                for (int n = 0; n < frist; n++)
                                {
                                    if (point_tem[n] == 0 && n + 1 <= 3) point_tem[n] = point_tem[frist] - width_tem[n] * (frist - n);
                                }
                                for (int n = 0; n < 4; n++)
                                {
                                    bool isforward = n - 1 >= 0 ? point_tem[n] >= point_tem[n - 1] + width_tem[n - 1] : true;
                                    bool isback = n + 1 <= 3 ? point_tem[n] + width_tem[n] <= point_tem[n + 1] : true;

                                    ispass = isforward && isforward;
                                    if (!ispass) break;
                                }
                                if (ispass && point_tem[hit_index] == tager[hit_index].y)
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

                int[] yy = IMask.Gen(-1);
                int[] zz = IMask.Gen(-1);
                List<Enterclose> ents = new List<Enterclose>();
                for (int i = 0; i < left.Count; i++)
                {
                    int indextem = left[i];
                    if (point[indextem] != minnode[indextem])
                    {
                        ents.Add(Engine.getInstance().injectorDevice.Injector.Entercloses[indextem]);
                        yy[indextem] = minnode[indextem];
                    }
                }
                for (int i = 0; i < right.Count; i++)
                {
                    int indextem = right[i];
                    if (point[indextem] != minnode[indextem])
                    {
                        ents.Add(Engine.getInstance().injectorDevice.Injector.Entercloses[indextem]);
                        yy[indextem] = minnode[indextem];
                    }
                }
                movesp_act.AddAction(InjectMoveTo.create(3001, ents.ToArray(), -1, yy, zz));
                for (int i = 0; i < 4; i++)
                point[i] = minnode[i];
                move_act.AddAction(movesp_act);

                //得到到达的点
                List<ActionPoint> done_points = new List<ActionPoint>();
                List<Enterclose> entcloses = new List<Enterclose>();
                for (int i= 0; i < 4;i++)
                {
                    if (tager[i].y == point[i]&&tager[i].isdone == false)
                    {
                        done_points.Add(tager[i]);
                        entcloses.Add(Engine.getInstance().injectorDevice.Injector.Entercloses[i]);
                        tager[i].isdone = true;
                    }
                }
                var run_act = Sequence.create();
                //对到达的点进行分类生成
                int[] injz = IMask.Gen(-1);
                int[] injzl = IMask.Gen(-1);
                int[] injzd = IMask.Gen(-1);
                int[] absorbs = IMask.Gen(-1);
                run_act.AddAction(SkWaitForAction.create(move_x));
                if (done_points.Count!=0)
                {
                    if (done_points[0].type == TestStepEnum.JXZT)
                    {
                        var injact_sp = Spawn.create();
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].z;
                            absorbs[entcloses[i].Index] = (int)entcloses[0].PumpMotor.Maximum.SetValue;
                        }
                        injact_sp.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz, 2));
                        injact_sp.AddAction(InjectAbsorbMove.create(3001, entcloses.ToArray(), 100, absorbs));
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].zb;
                        }
                        run_act.AddAction(injact_sp);
                        run_act.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz));
                    }
                    else if (done_points[0].type == TestStepEnum.PutTip)
                    {
                        var sequ = Sequence.create();
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].z;
                        }
                        sequ.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz, 2));
                        sequ.AddAction(MoveTo.create(3001, done_points[0].puttip_x, -1, -1));
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].z - 1000;
                        }
                        sequ.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz, 1));
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].zb;
                        }
                        sequ.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz));
                        run_act.AddAction(sequ);
                    }
                    else if (done_points[0].type == TestStepEnum.AbsLiquid)
                    {
                        run_act.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), IMask.Gen(-1), 0));
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].z + done_points[i].deep;
                            absorbs[entcloses[i].Index] = -(done_points[i].capacity);
                        }
                        run_act.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz, done_points[0].deepspeed));
                        run_act.AddAction(InjectAbsorb.create(3001, entcloses.ToArray(), done_points[0].absbspeed, absorbs));
                        //回吸
                        var back_act = Spawn.create();
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            if (done_points[i].backcapacity != 0)
                            {
                                absorbs[entcloses[i].Index] = -(done_points[i].backcapacity);
                                List<Enterclose> ent_tem = new List<Enterclose>();
                                ent_tem.Add(entcloses[i]);
                                back_act.AddAction(Sequence.create(SKSleep.create(200), InjectAbsorb.create(3001, ent_tem.ToArray(), done_points[i].backspeed, absorbs)));
                            }
                        }
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].zb;
                        }
                        back_act.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz));
                        run_act.AddAction(back_act);
                    }
                    else if (done_points[0].type == TestStepEnum.FollowAbsLiquid)
                    {
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].z;
                            injzl[entcloses[i].Index] = done_points[i].z + done_points[i].detectordeep;
                            injzd[entcloses[i].Index] = done_points[i].deep;
                            absorbs[entcloses[i].Index] = -(done_points[i].capacity);
                        }
                        run_act.AddAction(InjectDetector.create(3001, entcloses.ToArray(), injz, injzl, injzd, 2, 1));
                        run_act.AddAction(InjectAbsorb.create(3001, entcloses.ToArray(), done_points[0].absbspeed, absorbs));
                        //回吸
                        var back_act = Spawn.create();
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            if (done_points[i].backcapacity != 0)
                            {
                                absorbs[entcloses[i].Index] = -(done_points[i].backcapacity);
                                List<Enterclose> ent_tem = new List<Enterclose>();
                                ent_tem.Add(entcloses[i]);
                                back_act.AddAction(Sequence.create(SKSleep.create(200), InjectAbsorb.create(3001, ent_tem.ToArray(), done_points[i].backspeed, absorbs)));
                            }
                        }
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].zb;
                        }
                        back_act.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz));
                        run_act.AddAction(back_act);
                    }
                    else if (done_points[0].type == TestStepEnum.SpuLiquid)
                    {
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].z;
                            absorbs[entcloses[i].Index] = done_points[i].spucapacity + done_points[i].backcapacity;
                        }
                        run_act.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz, 2));
                        run_act.AddAction(InjectAbsorb.create(3001, entcloses.ToArray(), done_points[0].spuspeed, absorbs));
                        //回吸
                        var back_act = Spawn.create();
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            if (done_points[i].backcapacity != 0)
                            {
                                absorbs[entcloses[i].Index] = -(done_points[i].backcapacity);
                                List<Enterclose> ent_tem = new List<Enterclose>();
                                ent_tem.Add(entcloses[i]);
                                back_act.AddAction(Sequence.create(SKSleep.create(200), InjectAbsorb.create(3001, ent_tem.ToArray(), done_points[i].backspeed, absorbs)));
                            }
                        }
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].zb;
                        }
                        back_act.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz));
                        run_act.AddAction(back_act);
                    }
                    else if (done_points[0].type == TestStepEnum.MixLiquid)
                    {
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].z;
                            absorbs[entcloses[i].Index] = done_points[i].capacity+ done_points[i].backcapacity;
                        }
                        run_act.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz, 2));
                        run_act.AddAction(InjectAbsorb.create(3001, entcloses.ToArray(), 100, absorbs));//把稀释液放进去
                        //混合操作
                        var mix_act = Spawn.create();
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            List<Enterclose> ent_tem = new List<Enterclose>();
                            ent_tem.Add(entcloses[i]);
                            var mix_seq_act = Sequence.create();
                            if (done_points[i].mixtimes != 0)
                            {
                                
                                for (int j = 0; j < done_points[i].mixtimes * 2; j++)
                                {
                                    int mixz = done_points[i].z - done_points[i].mixdeep;//混合高度
                                    if (mixz <= 0) mixz = done_points[i].z;
                                    absorbs[entcloses[i].Index] = j % 2 == 0 ? -done_points[i].capacity : done_points[i].capacity;
                                    injz[entcloses[i].Index] = j % 2 == 0 ? done_points[i].z : mixz;
                                    mix_seq_act.AddAction(InjectMoveTo.create(3001, ent_tem.ToArray(), -1, IMask.Gen(-1), injz, 2));
                                    mix_seq_act.AddAction(InjectAbsorb.create(30001, ent_tem.ToArray(), 100, absorbs));
                                }
                                injz[entcloses[i].Index] = done_points[i].z;
                                mix_seq_act.AddAction(InjectMoveTo.create(3001, ent_tem.ToArray(), -1, IMask.Gen(-1), injz, 2));
                                absorbs[entcloses[i].Index] = (int)ent_tem[0].PumpMotor.Maximum.SetValue;
                                mix_seq_act.AddAction(InjectAbsorbMove.create(3001, ent_tem.ToArray(), 100, absorbs));
                                absorbs[entcloses[i].Index] = -(done_points[i].GetTubeList().Count * done_points[i].spucapacity);
                                mix_seq_act.AddAction(InjectAbsorb.create(3001, ent_tem.ToArray(), 100, absorbs));
                            }

                            mix_act.AddAction(mix_seq_act);
                        }
                        run_act.AddAction(mix_act);
                        var zb_act = Spawn.create();
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            injz[entcloses[i].Index] = done_points[i].zb;
                        }
                        zb_act.AddAction(InjectMoveTo.create(3001, entcloses.ToArray(), -1, IMask.Gen(-1), injz));
                        //回吸
                        var back_act = Spawn.create();
                        for (int i = 0; i < done_points.Count; i++)
                        {
                            List<Enterclose> ent_tem = new List<Enterclose>();
                            ent_tem.Add(entcloses[i]);
                            if (done_points[i].backcapacity != 0 && done_points[i].mixtimes != 0)
                            {
                                absorbs[entcloses[i].Index] = -(done_points[i].backcapacity);
                                back_act.AddAction(Sequence.create(SKSleep.create(500), InjectAbsorb.create(3001, ent_tem.ToArray(), done_points[i].backspeed, absorbs)));
                            }
                        }
                        zb_act.AddAction(back_act);
                        run_act.AddAction(zb_act);
                    }
                }
                move_act.AddAction(run_act);
                seque.AddAction(move_act);
                GenerateInjectActGroup(ref seque,hitsort,ref index, ref tager, ref point, ref width,ref width_rate, ref is_ok, move_x);
            }
        }
        //生成在普通位抓卡
        public ResInfoData GenerateTakeGelFromNormal(ResInfoData take_seat,ref Sequence seque)
        {
            if (take_seat != null)
            {
                //抓手移动
                seque.AddAction(MoveTo.create(Engine.getInstance().handDevice,3001, (int)take_seat.X, (int)(take_seat.Y), 0));
                //抓手抓卡
                var take_act = HandTakeCard.create(Engine.getInstance().handDevice, 3001, (int)take_seat.Z, (int)take_seat.ZLimit, (int)take_seat.ZCatch, 0);
                seque.AddAction(take_act);
                //把测试卡放在Gel位中
                var geltem = take_seat.Values[take_seat.CountX, 0];
                if(take_seat.Values!=null)
                take_seat.Values[take_seat.CountX, 0] = null;
                take_act.successfun = (ActionBase act) =>
                {
                    ResManager.getInstance().handseat_resinfo = (ResInfoData)geltem;
                    return true;
                };
                take_act.destroyfun = (ActionBase act) =>
                {
                    take_seat.Values[take_seat.CountX, 0] = geltem;
                    return true;
                };
            }
            else
            {
                ErrorSystem.WriteActError("普通位找不到卡",true,false);
            }
            return take_seat;
        }
        //生成放卡在普通位
        public bool GeneratePutGelToNormal(ResInfoData put_seat, ResInfoData put_gel, ref Sequence seque)
        {
            if (put_seat != null)
            {
                //抓手移动
                seque.AddAction(MoveTo.create(Engine.getInstance().handDevice, 3001, (int)put_seat.X, (int)(put_seat.Y), 0));
                //抓手放卡
                var put_act = HandPutCard.create(Engine.getInstance().handDevice, 3001, (int)put_seat.ZPut, 0);
                seque.AddAction(put_act);
                //把测试卡放在Gel位中
                if(put_seat.Values!=null)
                put_seat.Values[put_seat.CountX, 0] = put_gel;
                if(put_gel!=null)
                put_gel.PutOk = false;
                put_act.successfun = (ActionBase act) =>
                {
                    if (put_gel != null)
                    {
                        put_gel.PutOk = true;
                        put_gel.SetSeatInfo(put_seat);
                    }
                    ResManager.getInstance().handseat_resinfo = null;
                    return true;
                };
                put_act.destroyfun = (ActionBase act) =>
                {
                    if (put_seat.Values != null)
                        put_seat.Values[put_seat.CountX, 0] = null;
                    ResManager.getInstance().handseat_resinfo = null;
                    return true;
                };
            }
            else
            {
                ErrorSystem.WriteActError("普通位已满",true,false);
            }
            return put_seat != null;
        }
        //生成离心机抓卡动作
        public ResInfoData GenerateTakeGelFromCent(ResInfoData take_seat,string cen_code, ref Sequence seque)
        {
            var centrifuge = Engine.getInstance().cenMrg.GetCentrifugeByCode(cen_code);
            if (centrifuge == null) centrifuge = Engine.getInstance().cenMrg.GetFreeCentrifuge();
            if (take_seat != null)
            {
                //打开离心机门
                var opendoor_act = HandOpenCloseDoor.create(Engine.getInstance().handDevice, 5000, cen_code, true);
                seque.AddAction(opendoor_act);
                //离心机位移动
                //抓手移动到离心机位
                var move_act = Spawn.create(
                    MoveTo.create(centrifuge, 30001, -1, -1, (int)take_seat.CenGelP[take_seat.CountX]),
                    MoveTo.create(Engine.getInstance().handDevice, 3001, (int)take_seat.X, (int)take_seat.CenHandYP[take_seat.CountX]));
                seque.AddAction(move_act);
                //抓手抓卡
                var take_act = HandTakeCard.create(Engine.getInstance().handDevice, 3001, (int)take_seat.Z, (int)take_seat.ZLimit, (int)take_seat.ZCatch, 0, centrifuge);
                seque.AddAction(take_act);
                //把测试卡放在离心Gel位中
                var geltem = take_seat.Values[take_seat.CountX, 0];
                take_seat.Values[take_seat.CountX, 0] = null;
                take_act.successfun = (ActionBase act) =>
                {
                    ResManager.getInstance().handseat_resinfo = (ResInfoData)geltem;
                    return true;
                };
                take_act.destroyfun = (ActionBase act) =>
                {
                    take_seat.Values[take_seat.CountX, 0] = geltem;
                    return true;
                };
            }
            else
            {
                ErrorSystem.WriteActError("离心机位找不到卡",true,false);
            }
            return take_seat;
        }
        //生成离心机放卡动作
        public bool GeneratePutGelToCent(string cen_code, ResInfoData put_seat, ResInfoData put_gel,ref Sequence seque)
        {
            var centrifuge = Engine.getInstance().cenMrg.GetCentrifugeByCode(cen_code);
            if (centrifuge == null) centrifuge = Engine.getInstance().cenMrg.GetFreeCentrifuge();
            if (put_seat != null)
            {
                //打开离心机门
                var opendoor_act = HandOpenCloseDoor.create(Engine.getInstance().handDevice, 5000, cen_code, true);
                seque.AddAction(opendoor_act);
                //离心机位移动
                //抓手移动到离心机位
                var move_act = Spawn.create(
                   MoveTo.create(centrifuge, 30001, -1, -1, put_seat.CenGelP[put_seat.CountX]),
                   MoveTo.create(Engine.getInstance().handDevice, 3001, (int)put_seat.X, (int)put_seat.CenHandYP[put_seat.CountX]));
                seque.AddAction(move_act);
                //抓手放卡
                var put_act = HandPutCard.create(Engine.getInstance().handDevice, 3001, (int)put_seat.ZPut);
                seque.AddAction(put_act);
                //把测试卡放在离心Gel位中
                put_seat.Values[put_seat.CountX, 0] = put_gel;
                if(put_gel!=null)
                put_gel.PutOk = false;
                put_act.successfun = (ActionBase act) =>
                {
                    if (put_gel != null)
                    {
                        put_gel.PutOk = true;
                        put_gel.SetSeatInfo(put_seat);
                    }
                    ResManager.getInstance().handseat_resinfo = null;
                    return true;
                };
                put_act.destroyfun = (ActionBase act) =>
                {
                    put_seat.Values[put_seat.CountX, 0] = null;
                    ResManager.getInstance().handseat_resinfo = null;
                    return true;
                };
            }
            else
            {
                ErrorSystem.WriteActError("离心机位已满",true,false);
            }
            return put_seat != null;
        }
        //生成卡仓抓卡动作
        public ResInfoData GenerateTakeGelFromWare(ResInfoData take_seat,ref Sequence seque,string gelmask="")
        {
            if (take_seat != null)
            {
                //卡仓位移动
                //抓手移动到卡仓机位
                var move_act = Spawn.create(
                    MoveTo.create(Engine.getInstance().gelwareDevice, 3001, take_seat.StoreX, -1,-1),
                    MoveTo.create(Engine.getInstance().handDevice, 3001, take_seat.X, take_seat.Y));
                seque.AddAction(move_act);
                //抓手抓卡
                var take_act = HandTakeCard.create(Engine.getInstance().handDevice, 3001, take_seat.Z, take_seat.ZLimit, take_seat.ZCatch, 0);
                seque.AddAction(take_act);
                //把测试卡放在卡仓Gel位中
                var geltem = take_seat.Values[take_seat.CountX, 0];
                take_seat.Values[take_seat.CountX, 0] = null;
                take_act.successfun = (ActionBase act) =>
                {
                    ResManager.getInstance().handseat_resinfo = (ResInfoData)geltem;
                    return true;
                };
                take_act.destroyfun = (ActionBase act) =>
                {
                    take_seat.Values[take_seat.CountX, 0] = geltem;
                    return true;
                };
            }
            else
            {
                ErrorSystem.WriteActError("卡仓无卡", true, false);
            }
            return take_seat;
        }
        //生成卡仓放卡动作
        public bool GeneratePutGelToWare(ResInfoData put_seat, ResInfoData put_gel, ref Sequence seque)
        {
            if (put_seat != null)
            {
                var move_act = Spawn.create(
                   MoveTo.create(Engine.getInstance().gelwareDevice, 3001, (int)(put_seat.StoreX), -1,-1),
                   MoveTo.create(Engine.getInstance().handDevice, 3001, (int)put_seat.X, (int)put_seat.Y));
                seque.AddAction(move_act);
                //抓手放卡
                var put_act = HandPutCard.create(Engine.getInstance().handDevice, 3001, (int)put_seat.ZPut);
                seque.AddAction(put_act);
                //把测试卡放在离心Gel位中
                put_seat.Values[put_seat.CountX, 0] = put_gel;
                if (put_gel != null)
                put_gel.PutOk = false;
                put_act.successfun = (ActionBase act) =>
                {
                    if(put_gel!=null)
                    {
                        put_gel.PutOk = true;
                        put_gel.SetSeatInfo(put_seat);
                    }
                    ResManager.getInstance().handseat_resinfo = null;
                    return true;
                };
                put_act.destroyfun = (ActionBase act) =>
                {
                    put_seat.Values[put_seat.CountX, 0] = null;
                    ResManager.getInstance().handseat_resinfo = null;
                    return true;
                };
            }
            else
            {
                ErrorSystem.WriteActError("卡仓位已满",true,false);
            }
            return put_seat != null;
        }

    }
}