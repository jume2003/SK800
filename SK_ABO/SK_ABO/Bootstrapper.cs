using System;
using Stylet;
using StyletIoC;
using SK_ABO.Pages;
using SK_ABO.Views;
using SKABO.BLL.IServices.IJudger;
using SKABO.BLL.Services.Judger;
using IBatisNet.DataMapper;
using SKABO.Common.Utils;
using SKABO.BLL.Services.User;
using SKABO.BLL.IServices.IUser;
using SKABO.DAL.DAO.User;
using SKABO.DAL.IDAO.IUser;
using SKABO.BLL.IServices.IGel;
using SKABO.BLL.Services.Gel;
using SKABO.DAL.IDAO.IGEL;
using SKABO.DAL.DAO.GEL;
using SKABO.DAL.IDAO.IDevice;
using SKABO.DAL.DAO.Device;
using SKABO.BLL.IServices.IDevice;
using SKABO.BLL.Services.Device;
using SKABO.DAL.IDAO.ITrace;
using SKABO.DAL.DAO.Trace;
using SKABO.BLL.Services.Trace;
using SKABO.BLL.IServices.ITrace;
using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.DAL.DAO.Judger;
using SKABO.DAL.IDAO.IJudger;
using SKABO.BLL.Services.Logic;
using SKABO.BLL.IServices.ILogic;
using SKABO.DAL.DAO.Logic;
using SKABO.DAL.IDAO.ILogic;
using SKABO.Hardware.Core;
using SKABO.Hardware.RunBJ;
using SKABO.Common.Models.Communication;
using SKABO.Ihardware.Core;
using SKABO.Hardware.Scaner;
using SKABO.Hardware.Core.ZLG;
using System.Collections.Generic;

namespace SK_ABO
{
    public class Bootstrapper : Bootstrapper<MainViewModel>
    {
        private  ISqlMapper _EntityMapper;
        public  ISqlMapper EntityMapper
        {
            get
            {
                try
                {
                    if (_EntityMapper == null)
                    {
                        _EntityMapper = Mapper.Instance();
                        //String ddd = _EntityMapper.DataSource.Name; 192.168.0.102
                        //_EntityMapper.DataSource.ConnectionString = @"Password=85332389;Persist Security Info=True;User ID=sa;Initial Catalog=SKABO;Data Source=.";
                        _EntityMapper.DataSource.ConnectionString = @"Host=localhost;Port=5432;Username=abo;Password=85332389;Database=skpcb";
                    }
                    return _EntityMapper;
                }
                catch (Exception ex)
                {
                    Tool.AppLogError(ex);
                    throw ex;
                }
            }
        }
        protected override void OnStart()
        {
            base.OnStart();
            // This is called just after the application is started, but before the IoC container is set up.
            // Set up things like logging, etc
        }
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);
            
            builder.Bind<ISqlMapper>().ToInstance(EntityMapper);
            builder.Bind<IJudgerParamerService>().To<JudgerParamerService>().InSingletonScope();
            builder.Bind<IUserService>().To<UserService>().InSingletonScope();
            builder.Bind<IUserDAO>().To<UserDAO>().InSingletonScope();

            builder.Bind<IGelService>().To<GelService>().InSingletonScope();
            builder.Bind<IGELDAO>().To<GELDAO>().InSingletonScope();

            builder.Bind<IBJService>().To<BJService>().InSingletonScope();
            builder.Bind<IBJDAO>().To<BJDAO>().InSingletonScope();

            builder.Bind<ITraceService>().To<TraceService>().InSingletonScope();
            builder.Bind<ITraceDAO>().To<TraceDAO>().InSingletonScope();

            builder.Bind<IResultService>().To<ResultService>().InSingletonScope();
            builder.Bind<IResultDAO>().To<ResultDAO>().InSingletonScope();

            builder.Bind<ILogicService>().To<LogicService>().InSingletonScope();
            builder.Bind<ILogicDAO>().To<LogicDAO>().InSingletonScope();

            builder.Bind<IPlcBjParamService>().To<PlcBjParamService>().InSingletonScope();
            builder.Bind<IPlcBjParamDAO>().To<PlcBjParamDAO>().InSingletonScope();

            builder.Bind<AbstractScaner>().WithKey("FX8090").To<FX8090>().InSingletonScope(); 
            builder.Bind<AbstractScaner>().WithKey("BL1300").To<BL1300>().InSingletonScope();
            builder.Bind<AbstractScaner>().WithKey("FM316").To<FM316>().InSingletonScope();
            builder.Bind<ScanDevice>().To<ScanDevice>().InSingletonScope();

            builder.Bind<AbstractCanComm>().To<ZLGCanComm>().InSingletonScope();
            builder.Bind<OtherPart>().ToFactory<OtherPart>(c =>
            {
                var bjService = c.Get<IPlcBjParamService>();
                OtherPart op = bjService.LoadFromJson<OtherPart>();
                op = op ?? new OtherPart();
                return op;
            }).InSingletonScope();

            builder.Bind<AbstractComm>().WithKey("PLC").ToFactory<Communicater>(c=>
            {
                var op = c.Get<OtherPart>();
                var comm= new Communicater(op.IpAddress, op.Port);
                return comm;
            }).InSingletonScope();
            //builder.Bind<AbstractComm>().WithKey("PCB").ToFactory<PcbComm>(c =>
            //{
            //    var op = c.Get<OtherPart>();
            //    var comm = new PcbComm(op.SencondIpAddress, op.SecondPort);
            //    return comm;
            //}).InSingletonScope();

            builder.Bind<PiercerDevice>().ToFactory<PiercerDevice>(c=>
            {
                var bjService = c.Get<IPlcBjParamService>();
                Piercer Pie = bjService.LoadFromJson<Piercer>();
                Pie = Pie ?? new Piercer(true);
                return new PiercerDevice(c.Get<AbstractCanComm>(), Pie);
            }).InSingletonScope(); 
            builder.Bind<CentrifugeDevice>().ToFactory<CentrifugeDevice>(c =>
            {
                var bjService = c.Get<IPlcBjParamService>();
                CentrifugeSystem CentSys = bjService.LoadFromJson<CentrifugeSystem>();
                CentSys = CentSys ?? new CentrifugeSystem(true);
                return new CentrifugeDevice(c.Get<AbstractCanComm>(), CentSys);
            }).InSingletonScope();
            builder.Bind<CentrifugeMrg>().ToFactory<CentrifugeMrg>(c =>
            {
                var bjService = c.Get<IPlcBjParamService>();
                var cen_datas = bjService.LoadFromJson<CentrifugeData>();
                cen_datas = cen_datas ?? CentrifugeData.Create();
                return new CentrifugeMrg(c.Get<AbstractCanComm>(), cen_datas); 
            }).InSingletonScope();

            builder.Bind<GelWarehouseDevice>().ToFactory<GelWarehouseDevice>(c =>
            {
                var bjService = c.Get<IPlcBjParamService>();
                GelWarehouse GelWare = bjService.LoadFromJson<GelWarehouse>();
                GelWare = GelWare ?? new GelWarehouse(true);
                return new GelWarehouseDevice(c.Get<AbstractCanComm>(), GelWare);
            }).InSingletonScope();
            builder.Bind<MachineHandDevice>().ToFactory<MachineHandDevice>(c =>
            {
                var bjService = c.Get<IPlcBjParamService>();
                MachineHand Hand = bjService.LoadFromJson<MachineHand>();
                Hand = Hand ?? new MachineHand(true);
                Hand.CheckNull();
                return new MachineHandDevice(c.Get<AbstractCanComm>(), Hand);
            }).InSingletonScope();
            builder.Bind<CouveuseMixerDevice>().ToFactory<CouveuseMixerDevice>(c =>
            {
                var bjService = c.Get<IPlcBjParamService>();
                CouveuseMixer CM = bjService.LoadFromJson<CouveuseMixer>();
                CM = CM ?? new CouveuseMixer(true);
                CM.checkNull();
                return new CouveuseMixerDevice(c.Get<AbstractCanComm>(), CM);
            }).InSingletonScope();
            builder.Bind<OtherPartDevice>().ToFactory<OtherPartDevice>(c =>
            {
                var op = c.Get<OtherPart>();
                return new OtherPartDevice(c.Get<AbstractCanComm>(),op);
            }).InSingletonScope();
            builder.Bind<InjectorDevice>().ToFactory<InjectorDevice>(c =>
            {
                var bjService = c.Get<IPlcBjParamService>();
                Injector Injector = bjService.LoadFromJson<Injector>();
                Injector = Injector ?? new Injector(Constants.EntercloseCount);
                Injector.checkNull();
                return new InjectorDevice(c.Get < AbstractCanComm >(), Injector);
            }).InSingletonScope();
            builder.Bind<CameraDevice>().To<CameraDevice>().InSingletonScope();
        }

        protected override void Configure()
        {
            base.Configure();
            IoC.GetInstance = this.Container.Get;
            IoC.GetAllInstances = this.Container.GetAll;
            IoC.BuildUp = this.Container.BuildUp;
            InitSystemConfig();
            System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
            var loginModel = IoC.Get<LoginViewModel>();
            bool? dialogResult = IoC.Get<IWindowManager>().ShowDialog(loginModel);
            if (dialogResult.HasValue && dialogResult.Value)
            {
                System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
            }
            else
            {
                System.Windows.Application.Current.Shutdown(0);
            }
        }
        private void InitSystemConfig()
        {
            Constants.TraceLevel = TraceLevelEnum.Info;
        }
        protected override void OnLaunch()
        {
            base.OnLaunch();
        }
    }
}
