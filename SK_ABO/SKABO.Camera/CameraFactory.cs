using SKABO.Common.Utils;
using System;

namespace SKABO.Camera
{
    public class CameraFactory
    {
        private static ICameraDevice camera;
        public static ICameraDevice CreateCamera()
        {
            if (camera == null)
            {
                String CameraType = Tool.getAppSetting("CameraType");
                Type type = Type.GetType(CameraType);
                object obj = null;
                try
                {
                    obj = Activator.CreateInstance(type);
                }catch(Exception ex)
                {
                    Tool.AppLogFatal( "创建相机实例失败！",ex);
                }
                if (obj != null && obj is ICameraDevice)
                {
                    camera = obj as ICameraDevice;
                }
                
            }
            return camera;
        }
    }
}
