using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Utils
{
    public static class JsonUtil
    {
        static JsonUtil()
        {
            Newtonsoft.Json.JsonSerializerSettings setting = new Newtonsoft.Json.JsonSerializerSettings();
            JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
            {
                //日期类型默认格式化处理
                setting.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                setting.DateFormatString = "yyyy-MM-dd HH:mm:ss";


                //空值处理
                setting.NullValueHandling = NullValueHandling.Ignore;

                if (setting.Converters.FirstOrDefault(p => p.GetType() == typeof(JsonCustomDoubleConvert)) == null)
                {
                    setting.Converters.Add(new JsonCustomDoubleConvert(1));
                }
                return setting;
            });
        }
        public static String ToJsonStr<T>(this T obj) where T : class
        {
            if (obj == null)
                return string.Empty;
            return JsonConvert.SerializeObject(obj);

        }
        public static T ToInstance<T>(this String jsonStr) where T : class
        {
            if (string.IsNullOrEmpty(jsonStr))
                return null;
            
            var instance = JsonConvert.DeserializeObject<T>(jsonStr);

            return instance;

        }
    }
    public class JsonCustomDoubleConvert : CustomCreationConverter<double>
    {
        /// <summary>
        /// 序列化后保留小数位数
        /// </summary>
        public virtual int Digits { get; private set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public JsonCustomDoubleConvert()
        {
            this.Digits = 3;
        }
        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="digits">序列化后保留小数位数</param>
        public JsonCustomDoubleConvert(int digits)
        {
            this.Digits = digits;
        }
        /// <summary>
        /// 重载是否可写
        /// </summary>
        public override bool CanWrite { get { return true; } }

        public override double Create(Type objectType)
        {
            return 0.0;
        }
        /// <summary>
        /// 重载序列化方法
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var formatter = ((double)value).ToString("N" + Digits.ToString());
                writer.WriteValue(formatter);
            }

        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            double val = 0;
            var valObj = reader.Value;
            if (reader.TokenType == JsonToken.Null)
            {
                val= 0;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                string varStr = valObj.ToString();
                double.TryParse(varStr, out val);
            }
            else
            {
                try
                {
                    val = Convert.ToDouble(valObj);
                }
                catch
                {

                }
            }
            return val;
        }
    }
    }
