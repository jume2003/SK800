using SK_ABO.Pages.SetStep;
using SKABO.BLL.IServices.IGel;
using SKABO.Common.Models.GEL;
using SKABO.Common.Utils;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using SKABO.Common;
using System.Windows.Controls;
using SKABO.Common.Enums;

namespace SK_ABO.Views.GEL
{
    public class EditStepViewModel:Screen
    {
        private T_GelStep t_GSBackup;
        private ViewManager viewManager;
        private IGelService gelService;
        
        public EditStepViewModel(ViewManager viewManager,IGelService gelService)
        {
            this.viewManager = viewManager;
            this.gelService = gelService;
            if (StepList == null)
            {
                StepList = gelService.QueryAllStepDefine();
            }
        }
        protected override void OnViewLoaded()
        {
            t_GSBackup=TransExpUtil<T_GelStep, T_GelStep>.Trans(t_GelStep);

            if (t_GelStep.StepID != 0)
            {
                var obj = StepList.Where(c => c.ID == t_GelStep.StepID);
                if (obj != null && obj.Count()>0)
                {
                    SwitchPage(obj.First());
                }
            }
        }
        public T_GelStep t_GelStep { get; set; }
        public IList<T_StepDefine> StepList { get; set; }

        public void ClickOK()
        {
            var ts = StepList.Where(c => c.ID == t_GelStep.StepID).First();
            t_GelStep.StepClass = ts.StepClass;
            t_GelStep.StepName = ts.StepName;
            if (StepViewMocel==null)
                t_GelStep.StepParamters = null;
            else
            {
                t_GelStep.StepParamters =(String) StepViewMocel.GetType().InvokeMember("StepParameter", System.Reflection.BindingFlags.GetProperty, null, StepViewMocel, new object[] { }); 
            }
            this.RequestClose();
        }
        /// <summary>
        /// 取消按钮，并撤消更改
        /// </summary>
        public void Close()
        {
            TransExpUtil<T_GelStep, T_GelStep>.CopyValue(t_GSBackup, t_GelStep);
            this.RequestClose();
        }
        private Screen StepViewMocel = null;
        private IDictionary<TestStepEnum, Frame> FrameDict = new Dictionary<TestStepEnum, Frame>();
        public void CmbSteps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.t_GSBackup == null)
            {
                return; //没有完全加载，有些属性为null
            }
            if (e.AddedItems.Count == 0) return;
            var t_StepDefine = e.AddedItems[0] as T_StepDefine;
            SwitchPage(t_StepDefine);
        }
        public void SwitchPage(T_StepDefine t_StepDefine)
        {
            Frame frame = null;
            if (FrameDict.ContainsKey(t_StepDefine.StepClass))
            {
                frame = FrameDict[t_StepDefine.StepClass];
            }
            if (frame == null)
            {
                StepViewMocel = null;
                switch (t_StepDefine.StepClass)
                {
                    case TestStepEnum.LoadGel:
                    case TestStepEnum.KaiKongGel:
                    case TestStepEnum.JYJS:
                    case TestStepEnum.ZKDLXJ:
                    case TestStepEnum.XJPD:
                    case TestStepEnum.ZKDCW:
                        {
                            break;
                        }
                    case TestStepEnum.FPSJ:
                        {
                            StepViewMocel = IoC.Get<FPSJViewModel>();
                            break;
                        }
                    case TestStepEnum.FPBRXQ:
                        {
                            StepViewMocel = IoC.Get<FPBRXQViewModel>();
                            break;
                        }
                    case TestStepEnum.FPBRXSHXB:
                    case TestStepEnum.FPXXYXSHXB:
                        {
                            StepViewMocel = IoC.Get<FPBRXSHXBViewModel>();
                            break;
                        }
                    case TestStepEnum.FPXXYXQ:
                        {
                            StepViewMocel = IoC.Get<FPXXYXQViewModel>();
                            break;
                        }
                    case TestStepEnum.FPYT:
                        {
                            StepViewMocel = IoC.Get<FPYTViewModel>();
                            break;
                        }
                    case TestStepEnum.YS:
                        {
                            StepViewMocel = IoC.Get<YSViewModel>();
                            break;
                        }
                    case TestStepEnum.LXJDZ:
                        {
                            StepViewMocel = IoC.Get<LXJDZViewModel>();
                            break;
                        }
                    case TestStepEnum.ZKDFY:
                        {
                            StepViewMocel = IoC.Get<ZKDFYViewModel>();
                            break;
                        }
                }
                if (StepViewMocel != null)
                {
                    StepViewMocel.GetType().InvokeMember("StepParameter", System.Reflection.BindingFlags.SetProperty, null, StepViewMocel, new object[] { this.t_GSBackup.StepParamters });
                    var pv = viewManager.CreateViewForModel(StepViewMocel);
                    pv.SetValue(System.Windows.Controls.Page.DataContextProperty, StepViewMocel);
                    
                    frame = new Frame();
                    frame.Content = pv;
                    FrameDict.Add(t_StepDefine.StepClass, frame);
                }
            }
            EditStepView v = this.View as EditStepView;
            if (v != null)
                v.ContentControl.Content = frame;
        }
    }
}
