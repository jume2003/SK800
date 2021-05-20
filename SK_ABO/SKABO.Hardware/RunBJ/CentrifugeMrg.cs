using SKABO.ActionEngine;
using SKABO.Common.Enums;
using SKABO.Common.Models;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Utils;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.ResourcesManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SKABO.Hardware.RunBJ
{
    public class CentrifugeMrg: AbstractCanDevice
    {
        public CentrifugeMDevice[] CentrifugeMDevices { get; set; }
        public CentrifugeMrg(AbstractCanComm CanComm,CentrifugeData CentrifugeDatas)
        {
            CentrifugeMDevice[] cents = { new CentrifugeMDevice(CanComm, CentrifugeDatas.Centrifuges[0]), new CentrifugeMDevice(CanComm, CentrifugeDatas.Centrifuges[1]) };
            this.CentrifugeMDevices = cents;
        }
        public override void LoadPLCValue()
        {
            foreach (var cent in CentrifugeMDevices)
            {
                cent.LoadPLCValue();
            }
        }
        public override void Update2Plc()
        {
            foreach(var cent in CentrifugeMDevices)
            {
                cent.Update2Plc();
            }
        }

        public CentrifugeMrg()
        {

        }

        public void UpdateCentrifuge2PLC(Centrifuge centrifuge)
        {
            
        }
        public void LoadCentrifugePLCValue(Centrifuge centrifuge)
        {
        }
        private void LoadAction(CentrifugeAction action,float SpeedFactor)
        {   
            
        }
        public override ActionBase InitAll()
        {
            var spaw = Spawn.create();
            foreach (var cent in CentrifugeMDevices)
            {
                if(ResManager.getInstance().GetCenStatus(cent.Centrifugem.Code.SetValue))
                {
                    cent.IsOpen = false;
                    var act = InitXyz.create(cent, 30000, false, false, true);
                    spaw.AddAction(act);
                }
            }
            
            return spaw;
        }

        public CentrifugeMDevice GetFreeCentrifuge()
        {
            foreach(var centr in CentrifugeMDevices)
            {
                if (ActionManager.getInstance().getAllRuningActions(centr).Count() == 0)
                    return centr;
            }
            return null;
        }

        public CentrifugeMDevice GetCentrifugeByCode(string code)
        {
            foreach (var centr in CentrifugeMDevices)
            {
                if (centr.Centrifugem.Code.SetValue == code)
                    return centr;
            }
            return null;
        }

        public void SetCentrifugeDatas(CentrifugeData cent_datas)
        {
            for (int i=0;i< cent_datas.Centrifuges.Count();i++)
            {
                CentrifugeMDevices[i].Centrifugem = cent_datas.Centrifuges[i];
            }
        }

        public CentrifugeData GetCentrifugeDatas()
        {
            CentrifugeData cent_datas = new CentrifugeData();
            List<CentrifugeM> cent_list = new List<CentrifugeM>();
            foreach(var cent in CentrifugeMDevices)
            {
                cent_list.Add(cent.Centrifugem);
            }
            cent_datas.Centrifuges = cent_list.ToArray();
            return cent_datas;
        }
    }
    public class CentrifugeData
    {
        public CentrifugeM[] Centrifuges { get; set; }
        public static CentrifugeData Create()
        {
            CentrifugeData centrdata = new CentrifugeData();
            centrdata.Centrifuges = new CentrifugeM[2];
            centrdata.Centrifuges[0] = new CentrifugeM(true);
            centrdata.Centrifuges[1] = new CentrifugeM(true);
            return centrdata;
        }
    }
}
