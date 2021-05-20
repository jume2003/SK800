using System;
using System.Collections.Generic;
using System.Linq;

namespace SKABO.Common
{
    public static class IoC
    {
        public static Func<Type, string, object> GetInstance = (service, key) => { throw new InvalidOperationException("IoC is not initialized"); };

        public static Func<Type, String, IEnumerable<object>> GetAllInstances = (service, key) => { throw new InvalidOperationException("IoC is not initialized"); };

        public static Action<object> BuildUp = instance => { throw new InvalidOperationException("IoC is not initialized"); };

        public static T Get<T>(string key = null)
        {
            try
            {
                return (T)GetInstance(typeof(T), key);
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public static IEnumerable<T> GetAll<T>()
        {
            return GetAllInstances(typeof(T), null).Cast<T>();
        }
    }
}
