using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SKABO.Common.Models.Attributes;
using SKABO.Common.Utils;

namespace SKABO.Common.Models.BJ
{
    public abstract class VBJ
    {
        public delegate void ChangeValueHandler(ChangeBJEventArgs eventArgs);
        public ChangeValueHandler _ChangedValueMap = null;
        public ChangeValueHandler ChangedValueMap
        {
            get
            {
                return  _ChangedValueMap;
            }
            set
            {
                _ChangedValueMap = value;
            }
        }
        public virtual int ID { get; set; }
        public virtual string Name { get; set; }
        public Object[,] _Values;
        public virtual Object[,] Values { get; }
        public String _Code { get; set; }
        /// <summary>
        /// 设置部件对应位置的数据
        /// </summary>
        /// <param name="X">X索引，从0开始</param>
        /// <param name="Y">Y索引，从0开始</param>
        /// <param name="val">对应的数据，如条码，或是否存在</param>
        public virtual void SetValue(int X,int Y,Object val)
        {
            if ("".Equals(val))
                val = null;
            var e = new ChangeBJEventArgs()
            {
                X = X,
                Y = Y,
                OldVal = Values[X, Y],
                NewVal = val,
                Code = _Code
            };
            Values[X, Y] = val;
            System.Diagnostics.Debug.Assert(ChangedValueMap != null);
            ChangedValueMap?.Invoke(e);
        }
        /// <summary>
        /// 设置为完成状态
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        public virtual void SetFinishStatus(int X, int Y)
        {
            var e = new ChangeBJEventArgs()
            {
                X = X,
                Y = Y,
                OldVal = Values[X, Y],
                NewVal = Values[X, Y],
                IsComplete = true
            };
            if(e.OldVal!=null && e.OldVal is String v)
            {
                e.NewVal = v + ",F";
                Values[X, Y] = e.NewVal;
            }
            ChangedValueMap?.Invoke(e);
        }
        public virtual int GetUnUsedCount()
        {
            int count= 0;
            foreach(var item in Values)
            {
                if (item == null)
                {
                    count++;
                }
            }
            return count;
        }
        public virtual int GetUsedCount()
        {
            int count = 0;
            foreach (var item in Values)
            {
                if (item != null)
                {
                    count++;
                }
            }
            return count;
        }
        public virtual (byte X, byte Y)? GetNext()
        {
            return GetNext(true);
        }
        /// <summary>
        /// 获取下一个空位
        /// </summary>
        /// <returns></returns>
        public virtual (byte X,byte Y)? GetNext(bool CheckNull)
        {
            int row = Values.GetLength(0);
            int col = Values.GetLength(1);
            for(byte x = 0; x < row; x++)
            {
                for (byte y = 0; y < col; y++)
                {
                    if (CheckNull)
                    {
                        if (Values[x, y] == null)
                        {
                            return (x, y);
                        }
                    }
                    else
                    {
                        if (Values[x, y] != null)
                        {
                            return (x, y);
                        }
                    }
                }
            }
            return null;
        }

        public virtual void CopyValues(Object[,] values_tem)
        {
            if(values_tem!=null)
            {
                if (values_tem.GetLength(0) == Values.GetLength(0) && values_tem.GetLength(1) == Values.GetLength(1))
                {
                    int row = Values.GetLength(0);
                    int col = Values.GetLength(1);
                    for (int x = 0; x < row; x++)
                    {
                        for (int y = 0; y < col; y++)
                        {
                            SetValue(x, y, values_tem[x, y]);
                        }
                    }
                }
            }
        }

        public virtual void Refresh()
        {
            int row = Values.GetLength(0);
            int col = Values.GetLength(1);
            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < col; y++)
                {
                    SetValue(x, y, Values[x, y]);
                }
            }
        }
        public virtual void FillAll(object val)
        {
            int MaxX = this.Values.GetLength(0);
            int MaxY = Values.GetLength(1);
            for (int i = 0; i < MaxX; i++)
            {
                for (int j = 0; j < MaxY; j++)
                {
                    SetValue(i, j, val);
                }
            }
        }
        public static void SaveConfig(IList<VBJ> list)
        {
            int index = 0;
            foreach (var item in list)
            {
                if (item is VBJ vbj)
                {
                    String jsonStr = vbj.Values.ToJsonStr<object[,]>();
                    String f = System.AppDomain.CurrentDomain.BaseDirectory + @"Config\BJParameters\" + item.GetType().Name + "_" + index + ".json";
                    File.WriteAllText(f, jsonStr,Encoding.Default);
                }
                index++;
            }
        }

        public int GetFixPoint(int index,decimal[] FixPoints,decimal []FixIndexs)
        {
            int retindex = -1;
            for(int i=0;i< FixPoints.Length;i++)
            {
                if(index>=FixIndexs[i]&& FixPoints[i]!=0)
                {
                    retindex= i;
                }
            }
            return retindex;
        }
    }
}
