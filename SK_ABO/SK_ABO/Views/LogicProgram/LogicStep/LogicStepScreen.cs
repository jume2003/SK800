using SKABO.Common.Enums;
using SKABO.Common.Models.Logic;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    public abstract class LogicStepScreen:Screen
    {
        public LogicStepEnum StepEnum { get; set; }
        private String _Name;
        public String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                this.DisplayName = value;
            }
        }
        public int StepID { get; set; }
        public String Parameters { get; set; }
        public delegate void ConfirmHandler(object sender, T_LogicStep Step);
        public event ConfirmHandler ConfirmEvent;
        public void Close()
        {
            this.RequestClose();
        }
        public void Confirm()
        {
            var s = GetStep();
            this.ConfirmEvent?.Invoke(this, s);
            this.RequestClose(true);
        }
        public virtual T_LogicStep GetStep()
        {
            if (Step == null)
            {
                return new T_LogicStep() { StepID = StepID, Name = Name, Parameters = Parameters , StepEnum = StepEnum };
            }
            else
            {
                this.Step.StepEnum = StepEnum;
                Step.Parameters = Parameters;
                Step.Name = Name;
                return Step;
            }
        }
        private T_LogicStep Step { get; set; }
        public virtual void ParseLogicStep(T_LogicStep Step)
        {
            this.Step = Step;
            this.Parameters = Step.Parameters;
            this.Name = Step.Name;
            this.StepID = Step.StepID;
        }
    }
}
