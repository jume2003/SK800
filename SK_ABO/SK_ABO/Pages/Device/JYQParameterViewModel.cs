using SKABO.BLL.IServices.IDevice;
using SKABO.Common;
using SKABO.Common.Models.Communication;
using Stylet;
using System;
using SKABO.Common.Utils;
using SK_ABO.Views;
using System.Windows.Input;
using System.Windows;
using SKABO.Hardware.RunBJ;
using SKABO.Hardware.Core;
using System.Linq;

namespace SK_ABO.Pages.Device
{
    public class JYQParameterViewModel : Screen
    {
        [StyletIoC.Inject]
        private IWindowManager windowManager;
        [StyletIoC.Inject]
        private IPlcBjParamService BjParamService;
        [StyletIoC.Inject]
        private AbstractCanComm CanComm;
        private InjectorDevice injectorDevice;
        public JYQParameterViewModel() : base()
        {
            
        }
        private bool Loaded;

        public Injector injector { get; set; }
        protected override void OnViewLoaded()
        {
            if (Loaded) return;
            Loaded = true;
            base.OnViewLoaded();
            this.injector = this.BjParamService.LoadFromJson<Injector>();
            if (injector != null)
            {
                injector.checkNull();
            }
            this.injector = this.injector?? new Injector(Constants.EntercloseCount);
            injectorDevice = new InjectorDevice(CanComm, injector);
        }
        /// <summary>
        /// 保存参数设置
        /// </summary>
        public void SaveInjector()
        {
            var ents = this.injectorDevice.Injector.Entercloses;
            String msg = null;
            if (ents.Length > 0)
            {
                int maxIndex = ents.Max(et => et.Index);
                int minIndex = ents.Min(et => et.Index);
                if (maxIndex - minIndex + 1 != ents.Length)
                {
                    msg = "禁用通道不连续，请重新设置";
                }
                else if(maxIndex!=Constants.EntercloseCount-1 && minIndex!=0)
                {
                    msg = "不能单独禁用中间的通道，请重新设置";
                }
            }
            if (!String.IsNullOrEmpty(msg))
            {
                windowManager.ShowMessageBox(msg, "系统提示");
                return;
            }
            if (Keyboard.FocusedElement is UIElement ele)
            {
                ele.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                ele.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            }
            try
            {
                var result = BjParamService.SaveAsJson<Injector>(this.injector);
                var newInjector = this.BjParamService.LoadFromJson<Injector>();
                var iocInj=IoC.Get<InjectorDevice>().Injector;
                newInjector.XMotor.IsBack = iocInj.XMotor.IsBack;
                newInjector.XMotor.CurrentDistance = iocInj.XMotor.CurrentDistance;

                newInjector.TMotor.IsBack = iocInj.TMotor.IsBack;
                newInjector.TMotor.CurrentDistance = iocInj.TMotor.CurrentDistance;

                for (byte i = 0; i < Constants.EntercloseCount; i++)
                {
                    newInjector.Entercloses[i].Selected = iocInj.Entercloses[i].Selected;

                    newInjector.Entercloses[i].YMotor.IsBack = iocInj.Entercloses[i].YMotor.IsBack;
                    newInjector.Entercloses[i].YMotor.CurrentDistance = iocInj.Entercloses[i].YMotor.CurrentDistance;

                    newInjector.Entercloses[i].ZMotor.IsBack = iocInj.Entercloses[i].ZMotor.IsBack;
                    newInjector.Entercloses[i].ZMotor.CurrentDistance = iocInj.Entercloses[i].ZMotor.CurrentDistance;

                    newInjector.Entercloses[i].PumpMotor.IsBack = iocInj.Entercloses[i].PumpMotor.IsBack;
                    newInjector.Entercloses[i].PumpMotor.CurrentDistance = iocInj.Entercloses[i].PumpMotor.CurrentDistance;
                }
                IoC.Get<InjectorDevice>().Injector = newInjector;
                
                this.View.ShowHint(new MessageWin(result?"保存成功":"保存失败"));
            }
            catch(Exception ex)
            {
                Tool.AppLogError(ex);
                this.View.ShowHint(new MessageWin(ex.Message));
            }
        }
        /// <summary>
        /// 从PLC加载数据
        /// </summary>
        public void LoadInjector()
        {
            injectorDevice.LoadPLCValue();
        }
        public void UpdateInjector2PLC()
        {
            injectorDevice.Update2Plc();
        }
    }
}
