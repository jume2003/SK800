using SK_ABO.Views;
using SKABO.Hardware.RunBJ;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using SKABO.Common.Utils;
using System.Threading.Tasks;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common;
using SKABO.Hardware.Core;
using SKABO.Hardware.Core.ZLG;
using System.Threading;
using System.IO;
using OxyPlot;
using OxyPlot.Axes;
using System.Collections.ObjectModel;
using SKABO.ActionEngine;
using SKABO.Common.Models.BJ;
using SK_ABO.Views.Start;
using SKABO.ResourcesManager;
using SKABO.ActionGeneraterEngine;
using SKABO.Common.Enums;
using SKABO.MAI.ErrorSystem;
using SKABO.Hardware.Model;
using SKABO.Common.Models.GEL;

namespace SK_ABO.Pages.Device
{
    //[PropertyChanged.AddINotifyPropertyChangedInterface]
    public class JYQViewModel:Screen
    {
        public JYQViewModel()
        {
            Pre1Points = new ObservableCollection<DataPoint>();
            Pre2Points = new ObservableCollection<DataPoint>();
            Pre3Points = new ObservableCollection<DataPoint>();
            Pre4Points = new ObservableCollection<DataPoint>();
            ObservableCollection<DataPoint>[] prepoints = { Pre1Points, Pre2Points, Pre3Points, Pre4Points };
            myTimer = new System.Windows.Forms.Timer();
            myTimer.Tick += new EventHandler(UpDataPrePoints);
            myTimer.Enabled = true;
            myTimer.Interval = 100;
            myTimer.Start();

        }
        private bool loaded;
        [StyletIoC.Inject]
        private InjectorDevice injectorDevice;
        [StyletIoC.Inject]
        private IWindowManager windowManager;
        [StyletIoC.Inject]
        private MachineHandDevice handDevice;
        [StyletIoC.Inject]
        private OtherPartDevice opDevice;
        


        public Injector injector { get
            {
                return injectorDevice.Injector;
            } }
        /// <summary>
        /// 液面参数
        /// </summary>
        public decimal DeepForUl { get; set; } = 0.0125m;
        /// <summary>
        /// Z轴追随液面速度系数
        /// </summary>
        public decimal ZSFactor { get; set; } = 0m;
        public decimal DistanceX { get; set; }
        public decimal DistanceY { get; set; }
        public decimal DistanceZ { get; set; }
        public decimal StepXValue { get; set; }
        public decimal StepYValue { get; set; }
        public decimal StepZValue { get; set; }
        /// <summary>
        /// 气压曲线图
        /// </summary>
        public static System.Windows.Forms.Timer myTimer = null;
        private ObservableCollection<DataPoint>[] _PrePoints = { new ObservableCollection<DataPoint>(), new ObservableCollection<DataPoint>(), new ObservableCollection<DataPoint>(), new ObservableCollection<DataPoint>() };
        public ObservableCollection<DataPoint> Pre1Points{get {return _PrePoints[0];}set {value = _PrePoints[0];}}
        public ObservableCollection<DataPoint> Pre2Points { get { return _PrePoints[1]; } set { value = _PrePoints[1]; } }
        public ObservableCollection<DataPoint> Pre3Points { get { return _PrePoints[2]; } set { value = _PrePoints[2]; } }
        public ObservableCollection<DataPoint> Pre4Points { get { return _PrePoints[3]; } set { value = _PrePoints[3]; } }
        //装针
        public ResManager resmanager = ResManager.getInstance();
        public byte SeatXIndex { get; set; }
        public byte SeatYIndex { get; set; }
        public Stylet.BindableCollection<VBJ> _TargetBJList;
        public Stylet.BindableCollection<VBJ> TargetBJList
        {
            get
            {
                if (_TargetBJList == null) _TargetBJList = new Stylet.BindableCollection<VBJ>();
                UpdataBjList();
                return _TargetBJList;
            }
        }

        public void UpdataBjList()
        {
            _TargetBJList.Clear();
            foreach (var bj in Constants.BJDict[typeof(T_BJ_Tip).Name])
            {
                _TargetBJList.Add(bj);
            }
            foreach (var bj in Constants.BJDict[typeof(T_BJ_SampleRack).Name])
            {
                _TargetBJList.Add(bj);
            }
            foreach (var bj in Constants.BJDict[typeof(T_BJ_DeepPlate).Name])
            {
                _TargetBJList.Add(bj);
            }
            foreach (var bj in Constants.BJDict[typeof(T_BJ_AgentiaWarehouse).Name])
            {
                _TargetBJList.Add(bj);
            }
            foreach (var bj in Constants.BJDict[typeof(T_BJ_GelSeat).Name])
            {
                var bjj = (T_BJ_GelSeat)bj;
                if (bjj.Purpose == 4)
                    _TargetBJList.Add(bj);
            }
            foreach (var bj in Constants.BJDict[typeof(T_BJ_Unload).Name])
            {
                _TargetBJList.Add(bj);
            }
        }
        public VBJ SelectedBJ { get; set; }
        /// <summary>
        /// 吸液速度
        /// </summary>
        public int AbsorbSpeed { get; set; }
        /// <summary>
        /// 吸液量
        /// </summary>
        public decimal AbsorbVol { get; set; }
        /// <summary>
        /// 分液速度
        /// </summary>
        public int DistributeSpeed { get; set; }
        /// <summary>
        /// 分液量
        /// </summary>
        public decimal DistributeVol { get; set; }
        /// <summary>
        /// 分液回吸速度
        /// </summary>
        public int BackAbsorbSpeed { get; set; }
        /// <summary>
        /// 分液回吸量
        /// </summary>
        public decimal BackAbsorbVol { get; set; }
        /// <summary>
        /// 液面探测时Z轴极限位置
        /// </summary>
        public decimal ZLimitWhenDetecting { get; set; }
        /// <summary>
        /// 液面探测加深
        /// </summary>
        public decimal ZDeepWhenDetecting { get; set; }
        /// <summary>
        /// 是否Z自动归零，在移动x|y之前
        /// </summary>
        public bool AutoSetZeroForZ { get; set; }
        public bool IsUseTMotor { get; set; }
        public bool IsUpDataPrePoint { get; set; }
        public bool IsUpLoadAD { get; set; }
        public decimal SplitDistance { get; set; }
        protected override void OnViewLoaded()
        {
            if (loaded)
            {
                return;
            }
            base.OnViewLoaded();
            loaded = true;
            this.AutoSetZeroForZ = true;
            this.IsUseTMotor = false;
        }
        /// <summary>
        /// 更新气压曲线
        /// </summary>
        public void UpDataPrePoints(object sender, EventArgs e)
        {
            if(IsUpDataPrePoint)
            {
                var date = DateTime.Now;
                var injects = injectorDevice.GetSeleteced();
                int pressure = 0;
                ObservableCollection<DataPoint>[] prepoints = { Pre1Points, Pre2Points, Pre3Points, Pre4Points };
                Random rd = new Random();
                foreach (var inject in injects)
                {
                    pressure = injectorDevice.GetPressure(inject.Index);
                    prepoints[inject.Index].Add(DateTimeAxis.CreateDataPoint(date, (double)pressure));
                    if (prepoints[inject.Index].Count > 50)
                    {
                        prepoints[inject.Index].RemoveAt(0);
                    }
                }
            }
        }

        public void ClsPerPoints()
        {
            var date = DateTime.Now;
            ObservableCollection<DataPoint>[] prepoints = { Pre1Points, Pre2Points, Pre3Points, Pre4Points };
            foreach(var points in prepoints)
            {
                points.Clear();
                //points.Add(DateTimeAxis.CreateDataPoint(date, (double)4096));
            }
        }
        /// <summary>
        /// 初始化X轴初始化
        /// </summary>
        public void InitX()
        {
            var act = Sequence.create(InitXyz.create(30000, true, false, false));
            act.runAction(injectorDevice);
        }
        /// <summary>
        /// 初始化Y轴初始化
        /// </summary>
        public void InitY()
        {
            var by = IMask.Gen(0);
            foreach(var ent in injectorDevice.Injector.Entercloses)
            {
                by[ent.Index] = (int)ent.YZero;
            }
            var act = Sequence.create(InitXyz.create(30000, injectorDevice.GetSeleteced(), false,true,false, by));
            act.runAction(injectorDevice);
        }
        /// <summary>
        /// 初始化全部
        /// </summary>
        public void InitAll()
        {
            //var res = this.injectorDevice.InitAll();
            //this.View.ShowHint(new MessageWin(res));
            InitZ();
            InitY();
            InitX();
        }
        /// <summary>
        /// 得到选中通道
        /// </summary>
        public Enterclose[] GetSelectedEnt()
        {
            return this.injectorDevice.GetSeleteced();
        }
        /// <summary>
        /// 初始化Z轴初始化
        /// </summary>
        public void InitZ()
        {
            var act = InitXyz.create(30000, injectorDevice.GetSeleteced(), false, false, true);
            act.runAction(injectorDevice);
        }
        /// <summary>
        /// 移动X
        /// </summary>
        public void MoveX()
        {
            var act = Sequence.create(MoveTo.create(3000, (int)DistanceX));
            act.runAction(injectorDevice);
        }
        /// <summary>
        /// 移动Y
        /// </summary>
        public void MoveY()
        {

            var move_act = Sequence.create();
            var device = new ActionDevice(injectorDevice);
            move_act.AddAction(InjectMoveTo.create(10000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(0)));
            int x = 0;
            int y = (int)DistanceY;
            double min_width = 1.0f;
            var points = IMask.Gen(new ActionPoint(-1, -1, -1));
            if (device.GetRealX(ref x))
            {
                var ens = injectorDevice.Injector.Entercloses;
                int[] py = { 0, (int)(ens[1].TipDis), (int)(ens[2].TipDis + ens[2].InjWidth), (int)(ens[3].TipDis + ens[3].InjWidth) };
                foreach (var ent in injectorDevice.GetSeleteced())
                {
                    points[ent.Index].x = x;
                    points[ent.Index].y = y+ py[ent.Index];
                    points[ent.Index].minwidth = min_width;
                }
                move_act.AddAction(InjectMoveActs.create(3001, points.ToArray(), true));
                move_act.runAction(injectorDevice);
            }
            //var canComm = IoC.Get<AbstractCanComm>();
            //var injects = injectorDevice.GetSeleteced();
            //if(IsUseTMotor)
            //{
            //    var act = InjectMoveTo.create(5001, injects.ToArray(), -1, IMask.Gen((int)DistanceY), IMask.Gen(0), 2);
            //    act.runAction(injectorDevice);
            //}
            //else
            //{
            //    var points = IMask.Gen(new ActionPoint(-1, -1, -1));
            //    var device = new ActionDevice(injectorDevice);
            //    int x = 0;
            //    if(device.GetRealX(ref x))
            //    {
            //        var min_index = injects.Min(et => et.Index);
            //        points[min_index].x = x;
            //        points[min_index].y = (int)DistanceY;
            //        points[min_index].minwidth = 1.0f;
            //        var act = InjectMoveActs.create(3001, points.ToArray(), true);
            //        act.runAction(injectorDevice);
            //    }
            //}
        }

        public void MoveZ()
        {
            int[] z = { (int)DistanceZ, (int)DistanceZ, (int)DistanceZ, (int)DistanceZ };
            var act = Sequence.create(InjectMoveTo.create(300000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), z));
            act.runAction(injectorDevice);
        }
        /// <summary>
        /// 初始化吸液泵
        /// </summary>
        public void InitPump()
        {
            var act = Sequence.create(InitInjectPump.create(30000,injectorDevice.GetSeleteced()));
            act.runAction(injectorDevice);
        }
        /// <summary>
        /// 吸液
        /// </summary>
        public void Absorb()
        {
            int[] absorbs = { -(int)AbsorbVol, -(int)AbsorbVol, -(int)AbsorbVol, -(int)AbsorbVol };
            var act = Sequence.create(InjectAbsorb.create(30000, injectorDevice.GetSeleteced(), AbsorbSpeed, absorbs, IMask.Gen(0)));
            act.runAction(injectorDevice);
        }
        /// <summary>
        /// 分液
        /// </summary>
        public void Distribute()
        {
            int[] absorbs = {(int)DistributeVol, (int)DistributeVol, (int)DistributeVol, (int)DistributeVol };
            var act = Sequence.create(InjectAbsorb.create(3000, injectorDevice.GetSeleteced(), DistributeSpeed, absorbs, IMask.Gen(0)));
            act.runAction(injectorDevice);

        }

        public bool IsDetect = false;
        public String ErrorMsg { get; set; } = "";
        /// <summary>
        /// 探测液面
        /// </summary>
        public void Detect()
        {
            if (this.SelectedBJ == null) return;
            var move_act = Sequence.create();
            move_act.AddAction(InjectMoveTo.create(10000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(0)));
            int x = 0;
            int y = 0;
            int z = 0;
            int zl = (int)ZDeepWhenDetecting;
            double gap = 0;
            if (SelectedBJ is T_BJ_SampleRack sampleseat)
            {
                x = (int)(sampleseat.X);
                y = (int)(sampleseat.Y + SeatYIndex * sampleseat.Gap);
                z = (int)sampleseat.Z;
                if (zl == 0) zl = (int)sampleseat.Z + 1000;
                else zl = (int)sampleseat.Z+(int)ZDeepWhenDetecting;
                gap = (double)sampleseat.Gap;
            }
            else if (SelectedBJ is T_BJ_DeepPlate deepplate)
            {
                x = (int)(deepplate.X + SeatXIndex * deepplate.GapX);
                y = (int)(deepplate.Y + SeatYIndex * deepplate.GapY);
                z = (int)deepplate.Z;
                if (zl == 0) zl = (int)deepplate.Z;
                else zl = (int)deepplate.Z + (int)ZDeepWhenDetecting;
                gap = (double)deepplate.GapX;
            }
            else if (SelectedBJ is T_BJ_AgentiaWarehouse agentiaware)
            {
                x = (int)(agentiaware.X);
                y = (int)(agentiaware.Y + SeatYIndex * agentiaware.Gap);
                z = (int)agentiaware.Z;
                if (zl == 0) zl = (int)agentiaware.Z+1000;
                else zl = (int)agentiaware.Z + (int)ZDeepWhenDetecting;
                gap = (double)agentiaware.Gap;
            }
            var points = IMask.Gen(new ActionPoint(-1, -1, -1));
            foreach (var ent in injectorDevice.GetSeleteced())
            {
                points[ent.Index].x = x;
                points[ent.Index].y = y+ (int)(ent.Index* gap);
                points[ent.Index].z = z;
                points[ent.Index].zb = z;
            }
            move_act.AddAction(InjectMoveActs.create(3001, points.ToArray(), true));
            move_act.AddAction(InjectDetector.create(30000, injectorDevice.GetSeleteced(), IMask.Gen(z), IMask.Gen((int)zl), IMask.Gen((int)0)));
            move_act.runAction(injectorDevice);
        }
        public void AdUpload()
        {
            var canComm=IoC.Get<AbstractCanComm>();
            var injects = injectorDevice.GetSeleteced();
            foreach (var inject in injects)
            {
                canComm.SetCoil(inject.PumpMotor.PressureSwitch.Addr, IsUpLoadAD);
            }
        }
        public void MoveXY()
        {
            if (this.SelectedBJ == null) return;
            var move_act = Sequence.create();
            move_act.AddAction(InjectMoveTo.create(10000, injectorDevice.GetSeleteced(), - 1, IMask.Gen(-1),IMask.Gen(0)));
            int x = 0;
            int y = 0;
            double min_width = 1.0f;
            if (SelectedBJ is T_BJ_Tip tipseat)
            {
                x = (int)(tipseat.X - SeatXIndex * tipseat.GapX);
                y = tipseat.GetY(SeatYIndex);
                min_width = (double)tipseat.MinWidth;
            }
            else if (SelectedBJ is T_BJ_SampleRack sampleseat)
            {
                x = (int)(sampleseat.X);
                y = (int)sampleseat.GetY(SeatYIndex);
                min_width = (double)sampleseat.MinWidth;
            }
            else if (SelectedBJ is T_BJ_DeepPlate deepplate)
            {
                x = (int)(deepplate.X + SeatXIndex * deepplate.GapX);
                y = deepplate.GetY(SeatYIndex);
                min_width = (double)deepplate.MinWidth;
            }
            else if (SelectedBJ is T_BJ_AgentiaWarehouse agentiaware)
            {
                x = (int)(agentiaware.X);
                y = agentiaware.GetY(SeatYIndex);
                min_width = (double)agentiaware.MinWidth;
            }
            else if(SelectedBJ is T_BJ_GelSeat gelseat)
            {
                x = (int)(gelseat.InjectorX + SeatXIndex * gelseat.InjectorGapX);
                y = gelseat.GetInjectorY(SeatYIndex);
                min_width = (double)gelseat.MinWidth;
            }
            else if(SelectedBJ is T_BJ_Unload unload)
            {
                x = (int)(unload.X);
                y = (int)(unload.Y + SeatYIndex * unload.FZ);
            }
            var points = IMask.Gen(new ActionPoint(-1, -1, -1));
            foreach (var ent in injectorDevice.GetSeleteced())
            {
                points[ent.Index].x = x;
                points[ent.Index].y = y;
                points[ent.Index].minwidth = min_width;
            }
            move_act.AddAction(InjectMoveActs.create(3001, points.ToArray(), true));
            move_act.runAction(injectorDevice);
        }
        public void MoveXYSort()
        {
            if (this.SelectedBJ == null) return;
            var move_act = Sequence.create();
            move_act.AddAction(InjectMoveTo.create(10000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(0)));
            int x = 0;
            int y = 0;
            int z = 0;
            double min_width = 1.0f;
            double gap = 0;
            if (SelectedBJ is T_BJ_SampleRack sampleseat)
            {
                x = (int)(sampleseat.X);
                y = (int)(sampleseat.Y + SeatYIndex * sampleseat.Gap);
                z = (int)sampleseat.Z;
                gap = (double)sampleseat.Gap;
                min_width = (double)sampleseat.MinWidth;
            }
            else if (SelectedBJ is T_BJ_DeepPlate deepplate)
            {
                x = (int)(deepplate.X + SeatXIndex * deepplate.GapX);
                y = (int)(deepplate.Y + SeatYIndex * deepplate.GapY);
                z = (int)deepplate.Z;
                gap = (double)deepplate.GapX;
                min_width = (double)deepplate.MinWidth;
            }
            else if (SelectedBJ is T_BJ_AgentiaWarehouse agentiaware)
            {
                x = (int)(agentiaware.X);
                y = (int)(agentiaware.Y + SeatYIndex * agentiaware.Gap);
                z = (int)agentiaware.Z;
                gap = (double)agentiaware.Gap;
                min_width = (double)agentiaware.MinWidth;
            }
            else if (SelectedBJ is T_BJ_Unload unload)
            {
                x = (int)(unload.X);
                y = (int)(unload.Y + SeatYIndex * unload.FZ);
                z = (int)unload.Z;
                gap = (double)unload.FZ;
                min_width = (double)1.0f;
            }
            else if (SelectedBJ is T_BJ_GelSeat gelseat)
            {
                x = (int)(gelseat.InjectorX + SeatXIndex * gelseat.InjectorGapX);
                y = (int)(gelseat.InjectorY + SeatYIndex * gelseat.InjectorGapY);
                gap = (double)gelseat.Gap;
                min_width = (double)gelseat.MinWidth;
            }
            var points = IMask.Gen(new ActionPoint(-1, -1, -1));
            foreach (var ent in injectorDevice.GetSeleteced())
            {
                points[ent.Index].x = x;
                points[ent.Index].y = y + (int)(ent.Index * gap);
                points[ent.Index].z = z;
                points[ent.Index].minwidth = min_width;
            }
            move_act.AddAction(InjectMoveActs.create(3001, points.ToArray(), true));
            move_act.runAction(injectorDevice);
        }
        public void TakeTip()
        {
            if (this.SelectedBJ == null) return;
            if (SelectedBJ is T_BJ_Tip tipseat)
            {
                var tip_seat = IMask.Gen(new ActionPoint(-1, -1, -1));
                int x = (int)(tipseat.X - SeatXIndex * tipseat.GapX);
                int y = (int)(tipseat.Y + SeatYIndex * tipseat.GapY);
                foreach (var ent in injectorDevice.GetSeleteced())
                {
                    var point = new ActionPoint();
                    point.x = x;
                    point.y = y;
                    point.z = (int)tipseat.Limit;
                    point.type = TestStepEnum.JXZT;
                    point.index = ent.Index;
                    tip_seat[ent.Index] = point;
                }
                var sequ_taketip = Sequence.create();
                var move_act = InjectMoveActs.create(3000, tip_seat.ToArray(), false);
                sequ_taketip.AddAction(InjectMoveTo.create(3000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(0)));
                sequ_taketip.AddAction(move_act);
                sequ_taketip.runAction(injectorDevice);
            }
        }
        public void TakeTipAuto()
        {
            //生成装帧动作
            if (this.SelectedBJ == null) return;
            if (SelectedBJ is T_BJ_Tip tipseat)
            {
                var sequ_taketip = Sequence.create();
                var tip_seat = resmanager.GetFreeTipActPoint(injectorDevice.GetSeleteced().Length, 2, tipseat.Name);
                if(tip_seat!=null)
                {
                    int index = 0;
                    var tip_seat_tem = IMask.Gen(new ActionPoint(-1, -1, -1));
                    foreach (var ent in injectorDevice.GetSeleteced())
                    {
                        tip_seat_tem[ent.Index] = tip_seat[index];
                        index++;
                    }
                    var move_act = InjectMoveActs.create(20000, tip_seat_tem, false);
                    //sequ_taketip.AddAction(InjectMoveTo.create(3000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(0)));
                    sequ_taketip.AddAction(move_act);
                    sequ_taketip.runAction(injectorDevice);
                }
                else
                {
                    ErrorSystem.WriteActError("吸盘盒为空!",true,false,9999);
                }

            }
        }
        public void PutTip()
        {
            var sequ_puttip = Sequence.create();
            List<ActionPoint> unload_seat = new List<ActionPoint>();
            var inject_unload = resmanager.unload_list;
            if (inject_unload.Count() == 1)
            {
                var unloader = inject_unload[0];
                for (int i = 0; i < 4; i++)
                {
                    var unload_point = new ActionPoint((int)unloader.X, (int)unloader.Y + i * (int)unloader.FZ, (int)unloader.Z,TestStepEnum.PutTip);
                    unload_point.puttip_x = (int)unloader.FirstX;
                    unload_seat.Add(unload_point);
                }
                sequ_puttip.AddAction(InjectMoveTo.create(3000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(0)));
                sequ_puttip.AddAction(InjectMoveActs.create(3000, unload_seat.ToArray(), true));
                sequ_puttip.runAction(injectorDevice);
            }
        }

        public void AbsLiquid()
        {
            if (this.SelectedBJ == null) return;
            var move_act = Sequence.create();
            bool is_find = false;
            move_act.AddAction(InjectMoveTo.create(10000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(0)));
            int x = 0;
            int y = 0;
            int z = 0;
            if (SelectedBJ is T_BJ_SampleRack sampleseat)
            {
                x = (int)(sampleseat.X);
                y = (int)(sampleseat.Y + SeatYIndex * sampleseat.Gap);
                z = (int)(sampleseat.Z);
                is_find = true;
            }
            else if (SelectedBJ is T_BJ_DeepPlate deepplate)
            {
                x = (int)(deepplate.X + SeatXIndex * deepplate.GapX);
                y = (int)(deepplate.Y + SeatYIndex * deepplate.GapY);
                z = (int)(deepplate.Z);
                is_find = true;
            }
            else if (SelectedBJ is T_BJ_AgentiaWarehouse agentiaware)
            {
                x = (int)(agentiaware.X);
                y = (int)(agentiaware.Y + SeatYIndex * agentiaware.Gap);
                z = (int)(agentiaware.Z);
                is_find = true;
            }
            else if (SelectedBJ is T_BJ_GelSeat gelseat)
            {
                x = (int)(gelseat.InjectorX + SeatXIndex * gelseat.InjectorGapX);
                y = (int)(gelseat.InjectorY + SeatYIndex * gelseat.InjectorGapY);
                z = (int)(gelseat.InjectorZ);
                is_find = true;
            }
            else if (SelectedBJ is T_BJ_Unload unload)
            {
                x = (int)(unload.X);
                y = (int)(unload.Y);
                z = (int)(unload.Z);
                is_find = true;
            }
            if(is_find)
            {
                var tip_seat = IMask.Gen(new ActionPoint(-1, -1, -1));
                foreach (var ent in injectorDevice.GetSeleteced())
                {
                    var point = new ActionPoint();
                    point.x = x;
                    point.y = y;
                    point.z = z;
                    point.zb = 0;
                    point.type = TestStepEnum.JXZT;
                    point.index = ent.Index;
                    tip_seat[ent.Index] = point;
                }
                move_act.AddAction(InjectMoveActs.create(3000, tip_seat.ToArray(), false));
                move_act.runAction(injectorDevice);
            }
        }

        public void SubLiquid()
        {
            if (SelectedBJ is T_BJ_GelSeat gelseat)
            {
                int x = (int)(gelseat.InjectorX);
                int y = gelseat.GetInjectorY(SeatYIndex);
                var sequ_putsample = Sequence.create(
                    InjectMoveTo.create(injectorDevice, 3000,injectorDevice.GetSeleteced(), -1,IMask.Gen(-1),IMask.Gen(0)),
                    InjectAbsorbMove.create(300, injectorDevice.GetSeleteced(), 100, IMask.Gen(0)));
                for (int i=0;i<8;i++)
                {
                    var points = IMask.Gen(new ActionPoint(-1, -1, -1));
                    foreach(var ent in injectorDevice.GetSeleteced())
                    {
                        points[ent.Index].x = x + i * (int)gelseat.InjectorGapX;
                        points[ent.Index].y = y + ent.Index * (int)gelseat.InjectorGapY;
                        points[ent.Index].z = (int)gelseat.InjectorZ;
                        points[ent.Index].zb = points[ent.Index].z - 1500;
                        points[ent.Index].minwidth = (double)gelseat.MinWidth;
                        points[ent.Index].type = TestStepEnum.SpuLiquid;
                        points[ent.Index].capacity = 500;
                    }
                    sequ_putsample.AddAction(InjectMoveActs.create(3000, points.ToArray(), true));
                }
                sequ_putsample.runAction(injectorDevice);
            }

        }

        public void CheckTip()
        {
            var inject_list = injector.Entercloses.Where(item => item.InjEnable).ToList();
            int[] pressure = IMask.Gen(0);
            int[] movez = IMask.Gen(100);
            var act = PressureTest.create(3000, inject_list.ToArray(), movez, 50, 50);
            act.runAction(injectorDevice);
            ////var act = InjectCheckTip.create(30000, 100, 0);
            ////act.runAction(injectorDevice);
            //Random rd = new Random();
            //T_BJ_GelSeat seat = new T_BJ_GelSeat();
            //seat.X = 2120;
            //seat.Y = 3820;
            //seat.Z = 5700;
            //seat.ZCatch = 100;
            //seat.ZPut = 100;
            //seat.ZLimit = 6000;
            //seat.Gap = 0;
            //int sleeptime = rd.Next()%10*10000;
            //var act_inject = Sequence.create(InjectMoveTo.create(300000, injectorDevice.GetSeleteced(), 3000, IMask.Gen(1000, 1000, 1000, -1), IMask.Gen(-1)));
            //var act_hand = Sequence.create(
            //    SkCallBackFun.create((ActionBase act_tem)=> {
            //        if (injectorDevice.Injector.XMotor.CurrentDistance < 2000)
            //        {
            //            return true;
            //        }
            //        return false;
            //    }),
            //    MoveTo.create(300000, -1, -1, 0), MoveTo.create(300000, (int)seat.X, (int)(seat.Y + seat.Gap), -1), HandTakeCard.create(700000, (int)seat.Z, (int)seat.ZLimit, (int)seat.ZCatch, 0),HandPutCard.create(500000, (int)seat.Z, 0), SKSleep.create(sleeptime), MoveTo.create(300000, -1, -1, 0), MoveTo.create(300000, (int)seat.X, (int)(seat.Y + seat.Gap), -1), HandTakeCard.create(700000, (int)seat.Z, (int)seat.ZLimit, (int)seat.ZCatch, 0), HandPutCard.create(500000, (int)seat.Z, 0));
            //act_inject.AddAction(SkCallBackFun.create((ActionBase act_tem) =>
            //{
            //    act_hand.runAction(handDevice);
            //    return true;
            //}));
            //act_inject.runAction(injectorDevice);
        }

        public void ShowNeedlePlate()
        {
            var vm = IoC.Get<FirstStepViewModel>();
            var result = windowManager.ShowDialog(vm);
        }
    }
}
