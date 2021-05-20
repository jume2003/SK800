using SKABO.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Ihardware.Common.IModel
{
    public interface IHardwareModel
    {
        DeviceTypeEnum TypeEnum { get; set; }
        /// <summary>
        /// 移动设备到指定的坐标
        /// </summary>
        /// <param name="X">横坐标</param>
        /// <param name="Y">纵坐标</param>
        /// <param name="Z">Z坐标</param>
        /// <returns>返回成功与否</returns>
        bool MoveTo(float? X, float? Y, float? Z);
        /// <summary>
        /// 初始化设备
        /// </summary>
        /// <returns>返回是否初始化成功</returns>
        bool Init();
        bool MoveZ(float? z);
        bool MoveX(float? z);
        bool MoveY(float? z);
        bool MoveXY(float? x, float? y);
    }
}
