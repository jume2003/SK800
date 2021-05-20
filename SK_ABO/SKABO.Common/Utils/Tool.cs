using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using log4net;

namespace SKABO.Common.Utils
{
    public class Tool
    {
        static ILog log = log4net.LogManager.GetLogger("AppLog");
        public static string getAppSetting(string itemName)
        {
            System.Collections.Specialized.NameValueCollection setting =
                System.Configuration.ConfigurationManager.AppSettings;
            object obj = setting.Get(itemName);
            if (obj == null)
            {
                return null;
            }
            else
            {
                return obj.ToString();
            }
        }
        public static void AppLogError( object message)
        {
            StackFrame frame = new StackFrame(1);       //偏移一个函数位,也即是获取当前函数的前一个调用函数
            MethodBase methodBase = frame.GetMethod();
            message = String.Format("{0}.{1} {2}", methodBase.DeclaringType, methodBase.Name, message);
            AppLogError(message, null);
        }
        public static void AppLogError(object message, Exception exception)
        {
            
            if (exception == null)
            {
                log.Error(message);
            }
            else
            {
                StackFrame frame = new StackFrame(1);       //偏移一个函数位,也即是获取当前函数的前一个调用函数
                MethodBase methodBase = frame.GetMethod();
                log.Error(String.Format("{0}.{1} {2}", methodBase.DeclaringType, methodBase.Name, message), exception);
            }
        }
        public static void AppLogInfo( object message)
        {
            StackFrame frame = new StackFrame(1);       //偏移一个函数位,也即是获取当前函数的前一个调用函数
            MethodBase methodBase = frame.GetMethod();
            message = String.Format("{0}.{1} {2}", methodBase.DeclaringType, methodBase.Name, message);
            AppLogInfo( message, null);
        }
        public static void AppLogInfo( object message, Exception exception)
        {
            
            if (exception == null)
            {
                log.Info(message);
            }
            else
            {
                StackFrame frame = new StackFrame(1);       //偏移一个函数位,也即是获取当前函数的前一个调用函数
                MethodBase methodBase = frame.GetMethod();
                log.Info(String.Format("{0}.{1} {2}", methodBase.DeclaringType, methodBase.Name, message), exception);
            }
        }
        public static void AppLogDebug( object message)
        {
            StackFrame frame = new StackFrame(1);       //偏移一个函数位,也即是获取当前函数的前一个调用函数
            MethodBase methodBase = frame.GetMethod();
            message = String.Format("{0}.{1} {2}", methodBase.DeclaringType, methodBase.Name, message);
            AppLogDebug( message, null);
        }
        public static void AppLogDebug( object message, Exception exception)
        {
            
            if (exception == null)
            {
                log.Debug(message);
            }
            else
            {
                StackFrame frame = new StackFrame(1);       //偏移一个函数位,也即是获取当前函数的前一个调用函数
                MethodBase methodBase = frame.GetMethod();
                log.Debug(String.Format("{0}.{1} {2}", methodBase.DeclaringType, methodBase.Name, message), exception);
            }
        }
        public static void AppLogFatal( object message)
        {
            StackFrame frame = new StackFrame(1);       //偏移一个函数位,也即是获取当前函数的前一个调用函数
            MethodBase methodBase = frame.GetMethod();
            message = String.Format("{0}.{1} {2}", methodBase.DeclaringType, methodBase.Name, message);
            AppLogFatal( message, null);
        }
        public static void AppLogFatal( object message, Exception exception)
        {
            
            if (exception == null)
            {
                log.Fatal(message);
            }
            else
            {
                StackFrame frame = new StackFrame(1);       //偏移一个函数位,也即是获取当前函数的前一个调用函数
                MethodBase methodBase = frame.GetMethod();
                log.Fatal(String.Format("{0}.{1} {2}", methodBase.DeclaringType, methodBase.Name, message), exception);
            }
        }
        public static void AppLogWarn( object message)
        {
            StackFrame frame = new StackFrame(1);       //偏移一个函数位,也即是获取当前函数的前一个调用函数
            MethodBase methodBase = frame.GetMethod();
            message = String.Format("{0}.{1} {2}", methodBase.DeclaringType, methodBase.Name, message);
            AppLogWarn( message, null);
        }
        public static void AppLogWarn( object message, Exception exception)
        {
            
            if (exception == null)
            {
                log.Warn(message);
            }
            else
            {
                StackFrame frame = new StackFrame(1);       //偏移一个函数位,也即是获取当前函数的前一个调用函数
                MethodBase methodBase = frame.GetMethod();
                log.Warn(String.Format("{0}.{1} {2}", methodBase.DeclaringType, methodBase.Name, message), exception);
            }
        }
        public static void PaintLogicE(Panel entPanel)
        {
            //<CheckBox Content="1#" IsChecked="{Binding injector.Logic0.Valid,Mode=TwoWay}" Style="{DynamicResource chkLogic}" />
            for (byte i = 0; i < Constants.EntercloseCount; i++)
            {
                CheckBox selectedChk = new CheckBox() { Content = String.Format("{0}#", i + 1), Margin = new Thickness(20, 0, 0, 0) };

                BindingValue(String.Format("injector.Logic{0}.Valid", i), CheckBox.IsEnabledProperty, selectedChk, BindingMode.OneTime);
                BindingValue(String.Format("injector.Logic{0}.Selected", i), CheckBox.IsCheckedProperty, selectedChk);
                entPanel.Children.Add(selectedChk);
            }
        }
        public static void BindingValue(String Path, DependencyProperty TargetProperty, FrameworkElement Target, BindingMode bindingMode = BindingMode.TwoWay)
        {
            Binding bindingValue = new Binding(Path) { Mode = bindingMode };

            bindingValue.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Target.SetBinding(TargetProperty, bindingValue);
        }
        public static IList<int> ParseTubeNo(int TubeVal)
        {
            IList<int> list = new List<int>();
            for(int i = 0; i < 8; i++)
            {
                var v = 0X01 << i;
                if((TubeVal & v) == v)
                {
                    list.Add(i);
                }
            }
            return list;
        }
    }
}
