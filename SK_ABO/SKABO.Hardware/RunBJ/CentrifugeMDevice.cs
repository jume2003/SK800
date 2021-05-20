using SKABO.ActionEngine;
using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.Common.Models;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Utils;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.MAI.ErrorSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SKABO.Hardware.RunBJ
{
    public class CentrifugeMDevice : AbstractCanDevice
    {
        public CentrifugeM Centrifugem { get; set; }
        public int LowSpeedTime = 0;
        public int HightSpeedTime = 0;
        public bool IsOpen = false;
        public CentrifugeMDevice(AbstractCanComm CanComm, CentrifugeM Centrifugestem)
        {
            this.CanComm = CanComm;
            this.Centrifugem = Centrifugestem;
        }
        public override void LoadPLCValue()
        {
            CanComm.ReadMotor(Centrifugem.Motor);
        }
        public override void Update2Plc()
        {
            CanComm.SetMotor(Centrifugem.Motor);
        }
        public override ActionBase InitAll()
        {
            return null;
        }
        /// <summary>
        /// 初始化离心机
        /// </summary>
        /// <param name="Code">离心机代号</param>
        /// <param name="onlyStart">True:仅开始初始化，False:等待初始化结束</param>
        /// <returns></returns>
        public bool Init(bool onlyStart = false)
        {
            return CanComm.InitMotor(Centrifugem.Motor, onlyStart);
        }

        /// <summary>
        /// 移动扫码器电机，不等待反馈信号
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool MoveZ(decimal Distance, bool OnlyStart = false)
        {
            var result = false;
            result = CanComm.MoveMotor(Centrifugem.Motor, Distance,OnlyStart);
            return result;
        }
        /// <summary>
        /// 得到离机卡位设置
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public T_BJ_Centrifuge GetGelSeatSetting()
        {
            var gelseats = (T_BJ_Centrifuge)Constants.BJDict[typeof(T_BJ_Centrifuge).Name].Where(item => (item as T_BJ_Centrifuge).Status == 1).Where(item => (item as T_BJ_Centrifuge).Code == Centrifugem.Code.SetValue).ToList()[0];
            return gelseats;
        }
        /// <summary>
        /// 得到离机卡位信息列表
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool[] GetGelSeatsInfo()
        {
            bool []seatinfo = new bool[12];
            var gelseats = GetGelSeatSetting();
            for (int i=0;i< gelseats.Values.Length;i++)
            {
                seatinfo[i] = gelseats.Values[i,0] != null;
            }
            return seatinfo;
        }
    }
}
