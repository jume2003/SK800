using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKABO.Common.Models.BJ;
using SKABO.Common;
using SKABO.Common.Models.GEL;
using SKABO.Common.Models.NotDuplex;
using SKABO.Common.Enums;
using SKABO.MAI.ErrorSystem;
using SKABO.ActionEngine;

namespace SKABO.ResourcesManager
{
    public class ResManager
    {
        public static object ui_lockObj = new object();//UI更新锁
        public List<T_BJ_DeepPlate> deepplate_list = new List<T_BJ_DeepPlate>();
        public List<T_BJ_AgentiaWarehouse> agentiawa_list = new List<T_BJ_AgentiaWarehouse>();
        public List<T_BJ_Camera> camera_list = new List<T_BJ_Camera>();
        public List<T_BJ_Centrifuge> centrifuge_list = new List<T_BJ_Centrifuge>();
        public List<T_BJ_GelSeat> gelseat_list = new List<T_BJ_GelSeat>();
        public List<T_BJ_GelWarehouse> gelwarehouse_list = new List<T_BJ_GelWarehouse>();
        public List<T_BJ_Piercer> piercer_list = new List<T_BJ_Piercer>();
        public List<T_BJ_SampleRack> samplerack_list = new List<T_BJ_SampleRack>();
        public List<T_BJ_Tip> tip_list = new List<T_BJ_Tip>();
        public List<T_BJ_Unload> unload_list = new List<T_BJ_Unload>();
        public List<T_BJ_WastedSeat> wastedseat_list = new List<T_BJ_WastedSeat>();
        public List<T_BJ_Scaner> scaner_list = new List<T_BJ_Scaner>();
        public List<T_Gel> gel_list = new List<T_Gel>();
        public Dictionary<int, string> gelmask_list = new Dictionary<int, string>();
        public int gel_count = 0;//废卡
        public int tip_count = 0;//废针
        public int gelmaskid_count = 0;//检测卡id
        public int last_zt_count = 0;//上次取针个数
        public ResInfoData handseat_resinfo = null;
        public static ResManager resmanager = null;
        public static ResManager getInstance()
        {
            if (resmanager == null) resmanager = new ResManager();
            return resmanager;
        }
        public void Init()
        {
            deepplate_list = Constants.BJDict[typeof(T_BJ_DeepPlate).Name].Select(item => item as T_BJ_DeepPlate).ToList();
            agentiawa_list = Constants.BJDict[typeof(T_BJ_AgentiaWarehouse).Name].Select(item => item as T_BJ_AgentiaWarehouse).ToList();
            camera_list = Constants.BJDict[typeof(T_BJ_Camera).Name].Select(item => item as T_BJ_Camera).ToList();
            centrifuge_list = Constants.BJDict[typeof(T_BJ_Centrifuge).Name].Select(item => item as T_BJ_Centrifuge).ToList();
            gelseat_list = Constants.BJDict[typeof(T_BJ_GelSeat).Name].Select(item => item as T_BJ_GelSeat).ToList();
            gelwarehouse_list = Constants.BJDict[typeof(T_BJ_GelWarehouse).Name].Select(item => item as T_BJ_GelWarehouse).ToList();
            piercer_list = Constants.BJDict[typeof(T_BJ_Piercer).Name].Select(item => item as T_BJ_Piercer).ToList();
            samplerack_list = Constants.BJDict[typeof(T_BJ_SampleRack).Name].Select(item => item as T_BJ_SampleRack).ToList();
            tip_list = Constants.BJDict[typeof(T_BJ_Tip).Name].Select(item => item as T_BJ_Tip).ToList();
            unload_list = Constants.BJDict[typeof(T_BJ_Unload).Name].Select(item => item as T_BJ_Unload).ToList();
            wastedseat_list = Constants.BJDict[typeof(T_BJ_WastedSeat).Name].Select(item => item as T_BJ_WastedSeat).ToList();
            scaner_list = Constants.BJDict[typeof(T_BJ_Scaner).Name].Select(item => item as T_BJ_Scaner).ToList();
        }
        public void ClsRes()
        {
            foreach(var seat in deepplate_list)
            {
                seat.FillAll(null);
            }
            foreach (var seat in centrifuge_list)
            {
                seat.FillAll(null);
            }

            foreach (var seat in gelseat_list)
            {
                seat.FillAll(null);
            }

            foreach (var seat in gelwarehouse_list)
            {
                seat.FillAll(null);
            }

            foreach (var seat in samplerack_list)
            {
                seat.FillAll(null);
            }

            foreach (var seat in tip_list)
            {
                seat.FillAll(null);
            }
           
        }
        public string GetGelMaskByID(int id)
        {
            if (gelmask_list.ContainsKey(id))
            {
                return gelmask_list[id];
            }
            return "";
        }
        public bool SetGelMaskByID(int id,string mask)
        {
            if (gelmask_list.ContainsKey(id))
            {
                gelmask_list[id] = mask;
                return true;
            }
            return false;
        }
        public int AddGelMaskByID(string mask)
        {
            gelmaskid_count++;
            int id = gelmaskid_count;
            gelmask_list.Add(id, mask);
            return id;
        }
        //得到扫码器信息
        public T_BJ_Scaner GetScaner(string purpose)
        {
            foreach(var scaner in scaner_list)
            {
                if (scaner.Purpose.ToString() == purpose)
                    return scaner;
            }
            return null;
        }
        //得到样本扫描架信息
        public T_BJ_SampleRack GetSampleRack(int rack_index)
        {
            foreach (var seat in samplerack_list)
            {
                if (seat.Index == rack_index)
                    return seat;
            }
            return null;
        }
        //得到测试卡类
        public T_Gel GetGelTestByMask(string mask)
        {
            foreach(var gel in gel_list)
            {
                if(mask.IndexOf(gel.GelMask)!=-1)
                {
                    return gel;
                }
            }
            return null;
        }
        //得到测试卡类
        public T_Gel GetGelTestByTestId(int testid)
        {
            foreach (var gel in gel_list)
            {
                if (gel.ID == testid)
                {
                    return gel;
                }
            }
            return null;
        }
        //得到样本信息
        public SampleInfo GetSampleInfo(string sample_code)
        {
            foreach (var seat in samplerack_list)
            {
                for (int i = 0; i < seat.Values.Length; i++)
                {
                    var resinfo = (ResInfoData)seat.Values[i, 0];
                    if (resinfo != null && resinfo.sampleinfo != null && resinfo.sampleinfo.Barcode == sample_code)
                        return resinfo.sampleinfo;
                }
            }
            return null;
        }
        //查看离心机是否启用
        public bool GetCenStatus(string device_code)
        {
            foreach (var seat in centrifuge_list)
            {
                if (seat.Code == device_code)
                {
                    return seat.Status == 1;
                }
            }
            return false;
        }
        //查看离心机BJ
        public T_BJ_Centrifuge GetCenBj(string device_code)
        {
            foreach (var seat in centrifuge_list)
            {
                if (seat.Code == device_code)
                {
                    return seat;
                }
            }
            return null;
        }
        //查找可用破孔位
        public int GetPaperFreeCount()
        {
            int paper_count = 0;
            foreach (var seat in gelseat_list)
            {
                if (seat.Purpose==4)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        var resinfo = (ResInfoData)seat.Values[i, 0];
                        if (resinfo == null)
                        {
                            paper_count++;
                        }
                    }
                }
            }
            return paper_count;
        }
        //查找可用针头
        public ActionPoint[] GetFreeTipActPoint(int zt_count, int step,string name = "", List<T_GelStep> act_group=null)
        {
            var injects = Engine.getInstance().injectorDevice.Injector.Entercloses.Where(item => item.InjEnable).ToArray();
            List<ActionPoint> point_list = new List<ActionPoint>();
            int beg_index = -1;
            zt_count = act_group != null ? act_group.Count : zt_count;
            last_zt_count = zt_count;
            resmanager.tip_count += zt_count;
            step = injects.Count() > 2 ? 2 : 1;
            foreach (var tip_seat in tip_list)
            {
                if(tip_seat.Name == name || name == "")
                {
                    for (int i = 0; i < tip_seat.CountX; i++)
                    {
                        for (int j = 0; j < tip_seat.CountY; j++)
                        {
                            if (tip_seat.Values[i, j] != null)//&&j%2== jl)
                            {
                                if (beg_index < 0) beg_index = j;
                                if ((beg_index + j) % step == 0)
                                {
                                    var point = new ActionPoint();
                                    point.x = (int)tip_seat.X - i * (int)tip_seat.GapX;
                                    point.y = (int)tip_seat.Y + j * (int)tip_seat.GapY;
                                    point.z = (int)tip_seat.Limit;
                                    point.type = TestStepEnum.JXZT;
                                    //第1，2加样器不能装第4个盒子大于Y为5的吸头,
                                    int entindex = point_list.Count;
                                    var inject = Engine.getInstance().injectorDevice.Injector.Entercloses[entindex];
                                    bool is_lager = (point.y - (int)inject.TipDis)>0&& (point.y - (int)inject.TipDis)< inject.YMotor.Maximum.SetValue;
                                    if(is_lager)
                                    {
                                        if(act_group == null||( act_group != null&&act_group[entindex].is_skip_zjt==false))
                                        tip_seat.Values[i, j] = null;
                                        if (act_group != null && act_group[entindex].is_skip_zjt)
                                        {
                                            point.x = -1;
                                            point.y = -1;
                                            point.z = -1;
                                            point.type = TestStepEnum.Define;
                                        }
                                        point_list.Add(point);
                                        if (point_list.Count == zt_count)
                                        {
                                            return point_list.ToArray();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        //得到试剂位
        public T_BJ_AgentiaWarehouse GetAgentiaWarehouseSeat(string code)
        {
            int ag_index = 0;
            foreach (var agent in agentiawa_list)
            {
                if (agent.FindAgByCode(code, ref ag_index))
                {
                    return agent;
                }
            }
            return null;
        }      
        //查找可用深盘
        public ResInfoData[] GetFreeDeepPlate(int zt_count, int step, string name = "")
        {
            int beg_index = -1;
            List<ResInfoData> seat_list = new List<ResInfoData>();
            var injects = Engine.getInstance().injectorDevice.Injector.Entercloses.Where(item => item.InjEnable).ToArray();
            step = injects.Count() > 2 ? 2 : 1;
            foreach (var deep_seat in deepplate_list)
            {
                if (deep_seat.Name == name || name == "")
                {
                    for (int i = 0; i < deep_seat.CountX; i++)
                    {
                        for (int j = 0; j < deep_seat.CountY; j++)
                        {
                            if (deep_seat.Values[i, j] == null)//&&j%2== jl)
                            {
                                if (beg_index < 0) beg_index = j;
                                if ((beg_index + j) % step == 0)
                                {
                                    var resinfo = new ResInfoData();
                                    resinfo.X = (int)deep_seat.X + i * (int)deep_seat.GapX;
                                    resinfo.Y = (int)deep_seat.Y + j * (int)deep_seat.GapY;
                                    resinfo.Z = (int)deep_seat.Z;
                                    resinfo.Gap = (int)deep_seat.GapX;

                                    resinfo.InjectorX = (int)deep_seat.X + i * (int)deep_seat.GapX;
                                    resinfo.InjectorY = (int)deep_seat.Y + j * (int)deep_seat.GapY;
                                    resinfo.InjectorZ = (int)deep_seat.Z;
                                    resinfo.InjectorGap = (int)deep_seat.GapX;

                                    resinfo.Values = deep_seat.Values;
                                    resinfo.Purpose = "pla";

                                    resinfo.CountX = i;
                                    resinfo.CountY = j;
                                    seat_list.Add(resinfo);
                                    if (seat_list.Count == zt_count)
                                    {
                                        return seat_list.ToArray();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
        //查找资源
        public ResInfoData GetResByCode(string code,string class_name="", string device_code = "",string device_purpose="", List<ResInfoData> except =null,bool descending=false)
        {
            //样本架 
            if (class_name == "" || class_name == "T_BJ_SampleRack")
            {
                foreach (var seat in samplerack_list)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        var resinfo = (ResInfoData)seat.Values[i, 0];
                        
                        if ((resinfo != null&& resinfo.PutOk && resinfo.FindCode(code, false) && (except == null|| except.Find((ResInfoData infotem) => infotem == resinfo)==null)) || (code == "null" && resinfo == null))
                        {
                            if (resinfo == null)
                            {
                                resinfo = new ResInfoData();
                            }
                         
                            resinfo.X = (int)seat.X;
                            resinfo.Y = seat.GetY(i);
                            resinfo.Z = (int)seat.Z;
                            resinfo.MinWidth = (double)seat.MinWidth;
                            resinfo.Gap = (int)seat.Gap;

                            resinfo.InjectorX = resinfo.X;
                            resinfo.InjectorY = resinfo.Y;
                            resinfo.InjectorZ = resinfo.Z;
                            resinfo.InjectorGap = resinfo.Gap;

                            resinfo.PiercerY = resinfo.X;
                            resinfo.PiercerZ = resinfo.Y;
                            resinfo.PiercerGap = resinfo.Gap;

                            resinfo.CountX = i;
                            resinfo.Values = seat.Values;
                            resinfo.Purpose = "sample";
                            return resinfo;
                        }
                    }
                }
            }
            //稀释板
            if (class_name == ""|| class_name == "T_BJ_DeepPlate")
            {
                foreach (var plate in deepplate_list)
                {
                    for (int i = 0; i < plate.CountX; i++)
                    {
                        for (int j = 0; j < plate.CountY; j++)
                        {
                            var resinfo = (ResInfoData)plate.Values[i, j];

                            if ((resinfo != null && resinfo.PutOk && resinfo.FindCode(code, true) && (except == null || except.Find((ResInfoData infotem) => infotem == resinfo) == null)) || (code == "null" && resinfo == null))
                            {
                                if (resinfo == null)
                                {
                                    resinfo = new ResInfoData();
                                }

                                resinfo.X = (int)plate.X + i * (int)plate.GapX;
                                resinfo.Y = plate.GetY(j);
                                resinfo.Z = (int)plate.Z;
                                resinfo.Gap = (int)plate.GapX;

                                resinfo.InjectorX = (int)plate.X + i * (int)plate.GapX;
                                resinfo.InjectorY = plate.GetY(j);
                                resinfo.InjectorZ = (int)plate.Z;
                                resinfo.MinWidth = (double)plate.MinWidth;
                                resinfo.InjectorGap = (int)plate.GapX;

                                resinfo.Values = plate.Values;
                                resinfo.Purpose = "pla";

                                resinfo.CountX = i;
                                resinfo.CountY = j;
                                return resinfo;
                            }
                        }
                    }
                }
            }
            //试剂
            if (class_name == "" || class_name == "T_BJ_AgentiaWarehouse")
            {
                int ag_index = 0;
                foreach (var agent in agentiawa_list)
                {
                    if (agent.FindAgByCode(code, ref ag_index))
                    {
                        var resinfo = new ResInfoData();

                        resinfo.X = (int)agent.X;
                        resinfo.Y = agent.GetY(ag_index);
                        resinfo.Z = (int)agent.Z;
                        resinfo.Gap = (int)agent.Gap;

                        resinfo.InjectorX = (int)agent.X;
                        resinfo.InjectorY = agent.GetY(ag_index);
                        resinfo.InjectorZ = (int)agent.Z;
                        resinfo.MinWidth = (double)agent.MinWidth;
                        resinfo.InjectorGap = (int)agent.Gap;
                        resinfo.Purpose = "sj";
                        

                        resinfo.Values = agent.Values;
                        resinfo.CountX = ag_index;
                        return resinfo;
                    }
                }
            }
            //普通卡位
            if (class_name == "" || class_name == "T_BJ_GelSeat")
            {
                foreach (var seat in gelseat_list)
                {
                    if ((seat.Code == device_code|| device_code=="")&& (seat.Purpose.ToString() == device_purpose || device_purpose == ""))
                    {
                        for (int i = 0; i < seat.Values.Length; i++)
                        {
                            int index_tem = i;
                            if (descending) index_tem = (seat.Values.Length-1) - i;
                             var resinfo = (ResInfoData)seat.Values[index_tem, 0];
                            if ((resinfo != null && resinfo.PutOk && resinfo.FindCode(code, false) && (except == null || except.Find((ResInfoData infotem) => infotem == resinfo) == null)) || (code == "null" && resinfo == null))
                            {
                                if (resinfo == null)
                                {
                                    resinfo = new ResInfoData();
                                }

                                resinfo.X = (int)seat.X;
                                resinfo.Y = (int)seat.Y + index_tem * (int)seat.Gap;
                                resinfo.Z = (int)seat.Z;
                                resinfo.MinWidth = (double)seat.MinWidth;
                                resinfo.ZLimit = (int)seat.ZLimit;
                                resinfo.ZCatch = (int)seat.ZCatch;
                                resinfo.ZPut = (int)seat.ZPut;
                                resinfo.Gap = (int)seat.Gap;

                                resinfo.InjectorX = (int)seat.InjectorX;
                                resinfo.InjectorY = seat.GetInjectorY(index_tem);
                                resinfo.InjectorZ = (int)seat.InjectorZ;
                                resinfo.InjectorGap = (int)seat.InjectorGapX;

                                resinfo.PiercerY = (int)seat.YForPie + index_tem * (int)seat.GapForPie;
                                resinfo.PiercerZ = (int)seat.ZForPie;
                                resinfo.PiercerGap = (int)seat.GapForPie;

                                resinfo.CountX = index_tem;
                                resinfo.Values = seat.Values;
                                resinfo.Purpose = seat.Purpose.ToString();

                                return resinfo;
                            }
                        }
                    }
                }
            }
            //离心机卡位
            if (class_name == "" || class_name == "T_BJ_Centrifuge")
            {
                foreach (var seat in centrifuge_list)
                {
                    if (seat.Code == device_code || device_code == "")
                    {
                        int[] seatindex = { 0, 6, 1, 7, 2, 8, 3, 9, 4, 10, 5, 11 };
                        for (int i = 0; i < seat.Values.Length; i++)
                        {
                            if(seat.Status==1)
                            {
                                int index = seatindex[i];
                                var resinfo = (ResInfoData)seat.Values[index, 0];
                                if ((resinfo != null && resinfo.PutOk && resinfo.FindCode(code, false) && (except == null || except.Find((ResInfoData infotem) => infotem == resinfo) == null)) || (code == "null" && resinfo == null))
                                {
                                    if (resinfo == null)
                                    {
                                        resinfo = new ResInfoData();
                                    }
                                    int[] CenGelP = { (int)seat.Gel0, (int)seat.Gel1, (int)seat.Gel2, (int)seat.Gel3, (int)seat.Gel4, (int)seat.Gel5,
                                                 (int)seat.Gel6, (int)seat.Gel7, (int)seat.Gel8, (int)seat.Gel9, (int)seat.Gel10, (int)seat.Gel11 };
                                    int[] CenHandYP = { (int)seat.HandY0, (int)seat.HandY1, (int)seat.HandY2, (int)seat.HandY3, (int)seat.HandY4, (int)seat.HandY5,
                                                 (int)seat.HandY6, (int)seat.HandY7, (int)seat.HandY8, (int)seat.HandY9, (int)seat.HandY10, (int)seat.HandY11 };
                                    resinfo.X = (int)seat.HandX;
                                    resinfo.Y = (int)seat.HandY;
                                    resinfo.Z = (int)seat.HandZ;
                                    resinfo.ZLimit = (int)seat.ZLimit;
                                    resinfo.ZCatch = (int)seat.ZCatch;
                                    resinfo.ZPut = (int)seat.ZPut;
                                    resinfo.Gap = (double)seat.Gel0;
                                    resinfo.CenGelP = CenGelP;
                                    resinfo.CenHandYP = CenHandYP;
                                    resinfo.CountX = index;
                                    resinfo.Values = seat.Values;
                                    resinfo.Purpose = "lxj";
                                    resinfo.CenCode = seat.Code;
                                    return resinfo;
                                }
                            }
                        }
                    }  
                }
            }
            //卡仓卡位
            if (class_name == "" || class_name == "T_BJ_GelWarehouse")
            {
                foreach (var seat in gelwarehouse_list)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        var resinfo = (ResInfoData)seat.Values[i, 0];
                        if (resinfo != null && resinfo.PutOk && (resinfo.FindCode(code, false)||code=="any")&& (except == null || except.Find((ResInfoData infotem) => infotem == resinfo) == null))
                        {
                            resinfo.CountX = i;
                            resinfo.X = (int)seat.HandX;
                            resinfo.Y = (int)seat.HandY + i * (int)seat.Gap;
                            resinfo.Z = (int)seat.HandZ;
                            resinfo.ZLimit = (int)seat.ZLimit;
                            resinfo.ZCatch = (int)seat.ZCatch;
                            resinfo.ZPut = (int)seat.ZPut;
                            resinfo.Gap = (int)seat.Gap;
                            resinfo.StoreX = (int)seat.StoreX;
                            resinfo.Values = seat.Values;
                            resinfo.Purpose = "kc";
                            resinfo.gel =(T_Gel)GetGelTestByMask(code)?.Clone();
                            return resinfo;
                        }
                    }
                }
            }
            //拍照位
            if (class_name == "T_BJ_Camera")
            {
                foreach (var seat in camera_list)
                {
                    var resinfo = new ResInfoData();
                    resinfo.X = (int)seat.HandX;
                    resinfo.Y = (int)seat.HandY;
                    resinfo.Z = (int)seat.HandZ;
                    resinfo.ZLimit = (int)seat.HandZ;
                    resinfo.ZCatch = (int)seat.HandZ;
                    resinfo.ZPut = (int)seat.HandZ;
                    resinfo.Purpose = "cam";
                    return resinfo;
                }
            }
            //废卡箱
            if (class_name == "T_BJ_WastedSeat")
            {
                foreach (var seat in wastedseat_list)
                {
                    if (device_code == "" && (seat.Purpose.ToString() == device_purpose || device_purpose == ""))
                    {
                        var resinfo = new ResInfoData();
                        resinfo.X = (int)seat.HandX;
                        resinfo.Y = (int)seat.HandY;
                        resinfo.Z = (int)seat.HandZ;
                        resinfo.ZLimit = (int)seat.HandZ;
                        resinfo.ZCatch = (int)seat.HandZ;
                        resinfo.ZPut = (int)seat.HandZ;
                        resinfo.Purpose = "rb";
                        return resinfo;
                    }

                }
            }
            //抓手中的卡
            if (class_name == "T_BJ_HandSeat")
            {
                if (handseat_resinfo != null) handseat_resinfo.Purpose = "hand";
                return handseat_resinfo;
            }
            return null;
        }
        //全局Gel位搜索
        public ResInfoData SearchGelCard(string class_name = "", string device_code = "", string device_purpose = "", int seatindex = 0,int seatrackindex = 0,bool is_not_null=true,string name = "")
        {
            //样本架
            if (class_name == "" || class_name == "T_BJ_SampleRack")
            {
                for(int i=0;i<samplerack_list.Count();i++)
                {
                    if(i== seatrackindex)
                    {
                        var seat = samplerack_list[i];
                        var resinfo = (ResInfoData)seat.Values[seatindex, 0];
                        if (resinfo == null) resinfo = new ResInfoData();
                        resinfo.X = (int)seat.X;
                        resinfo.Y = (int)((double)seat.Y + seatindex * (double)seat.Gap);
                        resinfo.Z = (int)seat.Z;
                        resinfo.Gap = (double)seat.Gap;

                        resinfo.InjectorX = (int)seat.X;
                        resinfo.InjectorY = seat.GetY(seatindex);
                        resinfo.InjectorZ = (int)seat.Z;
                        resinfo.CountX = seatindex;
                        resinfo.Values = seat.Values;
                        resinfo.Purpose = "sample";
                        return resinfo;
                    }
                }
            }
            //普通卡位
            if (class_name == "" || class_name == "T_BJ_GelSeat")
            {
                foreach (var seat in gelseat_list)
                {
                    if ((seat.Code == device_code || device_code == "") && (seat.Purpose.ToString() == device_purpose || device_purpose == "")&&(seat.Name== name|| name==""))
                    {
                        if (seatindex >= seat.Values.Length)
                        {
                            ErrorSystem.WriteActError(seat.Name + "超出索引" + seatindex + "/" + seat.Values.Length,true,false);
                            seatindex = seat.Values.Length - 1;
                        }
                        var resinfo = (ResInfoData)seat.Values[seatindex, 0];
                        if (resinfo == null) resinfo = new ResInfoData();
                        resinfo.X = (int)seat.X;
                        resinfo.Y = (int)((double)seat.Y + seatindex * (double)seat.Gap);
                        resinfo.Z = (int)seat.Z;
                        resinfo.ZLimit = (int)seat.ZLimit;
                        resinfo.ZCatch = (int)seat.ZCatch;
                        resinfo.ZPut = (int)seat.ZPut;
                        resinfo.Gap = (double)seat.Gap;

                        resinfo.InjectorX = (int)seat.InjectorX;
                        resinfo.InjectorY = seat.GetInjectorY(seatindex);
                        resinfo.InjectorZ = (int)seat.InjectorZ;
                        resinfo.CountX = seatindex;

                        resinfo.Values = seat.Values;
                        resinfo.Purpose = seat.Purpose.ToString();

                        return resinfo;
                    }
                }
            }
            //离心机卡位
            if (class_name == "" || class_name == "T_BJ_Centrifuge")
            {
                foreach (var seat in centrifuge_list)
                {
                    if ((seat.Code == device_code || device_code == "") && (seat.Name == name || name == ""))
                    {
                        var resinfo = (ResInfoData)seat.Values[seatindex, 0];
                        if(resinfo==null) resinfo = new ResInfoData();
                        int[] CenGelP = { (int)seat.Gel0, (int)seat.Gel1, (int)seat.Gel2, (int)seat.Gel3, (int)seat.Gel4, (int)seat.Gel5,
                                                 (int)seat.Gel6, (int)seat.Gel7, (int)seat.Gel8, (int)seat.Gel9, (int)seat.Gel10, (int)seat.Gel11 };
                        int[] CenHandYP = { (int)seat.HandY0, (int)seat.HandY1, (int)seat.HandY2, (int)seat.HandY3, (int)seat.HandY4, (int)seat.HandY5,
                                                 (int)seat.HandY6, (int)seat.HandY7, (int)seat.HandY8, (int)seat.HandY9, (int)seat.HandY10, (int)seat.HandY11 };
                        resinfo.X = (int)seat.HandX;
                        resinfo.Y = (int)seat.HandY;
                        resinfo.Z = (int)seat.HandZ;
                        resinfo.ZLimit = (int)seat.ZLimit;
                        resinfo.ZCatch = (int)seat.ZCatch;
                        resinfo.ZPut = (int)seat.ZPut;
                        resinfo.Gap = (double)seat.Gel0;
                        resinfo.CenGelP = CenGelP;
                        resinfo.CenHandYP = CenHandYP;
                        resinfo.CountX = seatindex;
                        resinfo.Values = seat.Values;
                        resinfo.Purpose = "lxj";
                        resinfo.CenCode = seat.Code;
                        return resinfo;
                    }
                }
            }
            //卡仓卡位
            if (class_name == "" || class_name == "T_BJ_GelWarehouse")
            {
                int rack_index = 0;
                foreach (var seat in gelwarehouse_list)
                {
                    if(rack_index== seatrackindex|| seatrackindex==0)
                    {
                        if ((seat.Code == device_code || device_code == "") && (seat.Name == name || name == ""))
                        {
                            if (seatindex >= seat.Count) seatindex = seat.Count - 1;
                            var resinfo = (ResInfoData)seat.Values[seatindex, 0];
                            if (resinfo == null)
                            {
                                if (is_not_null)
                                    resinfo = new ResInfoData();
                                else
                                    return null;
                            }
                            resinfo.CountX = seatindex;
                            resinfo.X = (int)seat.HandX;
                            resinfo.Y = (int)seat.HandY + seatindex * (int)seat.Gap;
                            resinfo.Z = (int)seat.HandZ;
                            resinfo.ZLimit = (int)seat.ZLimit;
                            resinfo.ZCatch = (int)seat.ZCatch;
                            resinfo.ZPut = (int)seat.ZPut;
                            resinfo.Gap = (double)seat.Gap;
                            resinfo.StoreX = (int)seat.StoreX;
                            resinfo.Values = seat.Values;
                            resinfo.Purpose = "kc";
                            return resinfo;
                        }
                    }
                    rack_index++;
                }
            }
            //拍照位
            if (class_name == "T_BJ_Camera")
            {
                foreach (var seat in camera_list)
                {
                    var resinfo = new ResInfoData();
                    resinfo.X = (int)seat.HandX;
                    resinfo.Y = (int)seat.HandY;
                    resinfo.Z = (int)seat.HandZ;
                    resinfo.ZLimit = (int)seat.HandZ;
                    resinfo.ZCatch = (int)seat.HandZ;
                    resinfo.ZPut = (int)seat.HandZ;
                    resinfo.Purpose = "cam";
                    return resinfo;
                }
            }
            //废卡位
            if (class_name == "" || class_name == "T_BJ_WastedSeat")
            {
                foreach (var seat in wastedseat_list)
                {
                    if (device_code == "" && (seat.Purpose.ToString() == device_purpose || device_purpose == ""))
                    {
                        var resinfo = new ResInfoData();
                        resinfo.X = (int)seat.HandX;
                        resinfo.Y = (int)seat.HandY;
                        resinfo.Z = (int)seat.HandZ;
                        resinfo.ZLimit = (int)seat.HandZ;
                        resinfo.ZCatch = (int)seat.HandZ;
                        resinfo.ZPut = (int)seat.HandZ;
                        resinfo.Purpose = "rb";
                        return resinfo;
                    }
                }
            }
            //扫描器位
            if (class_name == "" || class_name == "T_BJ_Scaner")
            {
                foreach (var seat in gelwarehouse_list)
                {
                    if ((seat.Code == device_code || device_code == "")&& (seat.Name == name || name == ""))
                    {
                        var resinfo = (ResInfoData)seat.Values[seatindex, 0];
                        if (resinfo == null) resinfo = new ResInfoData();
                        resinfo.CountX = seatindex;
                        resinfo.X = (int)seat.HandX;
                        resinfo.Y = (int)seat.HandY + seatindex * (int)seat.Gap;
                        resinfo.Z = (int)seat.HandZ;
                        resinfo.ZLimit = (int)seat.ZLimit;
                        resinfo.ZCatch = (int)seat.ZCatch;
                        resinfo.ZPut = (int)seat.ZPut;
                        resinfo.Gap = (double)seat.Gap;
                        resinfo.StoreX = (int)seat.StoreX;
                        resinfo.Values = seat.Values;
                        resinfo.Purpose = "scaner";
                        return resinfo;
                    }
                }
            }
            return null;
        }
        //
        public void SetRand(int zt_count)
        {
            List<ActionPoint> point_list = new List<ActionPoint>();
            Random rd = new Random();
            foreach (var tip_seat in tip_list)
            {
                for (int i = 0; i < tip_seat.CountX; i++)
                {
                    for (int j = 0; j < tip_seat.CountY; j++)
                    {
                        tip_seat.Values[i, j] = null;
                    }
                }

                for (int i = 0; i < zt_count; i++)
                {
                    int x = (rd.Next() % tip_list[0].CountX);
                    int y = (rd.Next() % tip_list[0].CountY);
                    tip_seat.Values[x, y] = 1;
                    tip_seat.Values[11, i + 4] = 1;
                }
            }
        }
    }

    public class ResInfoData
    {
        public T_Gel gel = null;
        public SampleInfo sampleinfo = null;
        public Object[,] Values = null;
        public List<string> codes = new List<string>();
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Z { get; set; } = 0;
        public double Gap { get; set; } = 0;
        public int []CenGelP { get; set; } = new int[12];
        public int[] CenHandYP { get; set; } = new int[12];
        public int InjectorX { get; set; } = 0;
        public int InjectorY { get; set; } = 0;
        public int InjectorZ { get; set; } = 0;
        public double InjectorGap { get; set; } = 0;
        public int PiercerY { get; set; } = 0;
        public int PiercerZ { get; set; } = 0;
        public double PiercerGap { get; set; } = 0;
        public int ZLimit { get; set; } = 0;
        public int ZCatch { get; set; } = 0;
        public int ZPut { get; set; } = 0;
        public int CountX { get; set; } = 0;//在values 中的序号
        public int CountY { get; set; } = 0;//在values 中的序号
        public int StoreX { get; set; } = 0;//卡仓取卡时移动位
        public double MinWidth { get; set; } = 1.0f;//最小间距
        public DateTime StartTime { get; set; } = DateTime.Now;//开始时间
        //离心逻辑相关
        public string CenCode { get; set; } = "";
        public string Purpose { get; set; } = "";
        public bool PutOk { get; set; } = true;
        public void SetSeatInfo(ResInfoData info)
        {
            Values = info.Values;
            X = info.X;
            Y = info.Y;
            Z = info.Z;
            Gap = info.Gap;
            CenGelP = info.CenGelP;
            CenHandYP = info.CenHandYP;
            InjectorX = info.InjectorX;
            InjectorY = info.InjectorY;
            InjectorZ = info.InjectorZ;
            InjectorGap = info.InjectorGap;
            PiercerY = info.PiercerY;
            PiercerZ = info.PiercerZ;
            PiercerGap = info.PiercerGap;
            ZLimit = info.ZLimit;
            ZCatch = info.ZCatch;
            ZPut = info.ZPut;
            CountX = info.CountX;
            CountY = info.CountY;
            StoreX = info.StoreX;
            Purpose = info.Purpose;
            CenCode = info.CenCode;
        }
        public ResInfoData(int xx,int yy,int zz)
        {
            X = xx;
            Y = yy;
            Z = zz;
        }
        public ResInfoData()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
        public bool FindCode(string code,bool is_deep_plane)
        {
            if (is_deep_plane==false||codes.Count() == 1)
            {
                foreach (var code_tem in codes)
                {
                    if(code.IndexOf("*")==-1)
                    {
                        if (code_tem == code)
                            return true;
                    }
                    else
                    {
                        var ncode = code.Replace("*", "");
                        if (code_tem.IndexOf(ncode)!=-1)
                            return true;
                    }
                }
            }
            return false;
        }
        public bool AddCode(string code)
        {
            codes.Add(code);
            return true;
        }
        public bool SetCode(string code)
        {
            codes.Clear();
            codes.Add(code);
            return true;
        }
        public string GetCodeAt(int index)
        {
            if(index< codes.Count)
            return codes[index];
            return "";
        }
        public string GetCode(string code)
        {
            foreach (var code_tem in codes)
            {
                if (code.IndexOf("*") == -1)
                {
                    if (code_tem == code)
                        return code_tem;
                }
                else
                {
                    var ncode = code.Replace("*", "");
                    if (code_tem.IndexOf(ncode) != -1)
                        return code_tem;
                }
            }
            return "";
        }
        public string GetGelMask()
        {
            if(gel!=null)
            {
                return GetCode(gel.GelMask + "*");
            }
            return "";
        }
        public string GetSampleBarcode()
        {
            if (sampleinfo != null)
            {
                return sampleinfo.Barcode;
            }
            return "";
        }
    }

    public class TakeTipData
    {
        public int X;
        public int Y;
        public int Z;
        public int Count;
        public TakeTipData(int xx, int yy,int zz, int ccount)
        {
            X = xx;
            Y = yy;
            Z = zz;
            Count = ccount;
        }
        public TakeTipData()
        {
            X = 0;
            Y = 0;
            Z = 0;
            Count = 0;
        }
    }

    public class DetectoPoint
    {
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int z { get; set; } = 0;
        public DetectoPoint(int xx,int yy,int zz)
        {
            x = xx;
            y = yy;
            z = zz;
        }
    }

    public class ActionPoint : System.ICloneable
    {
        public T_GelStep action { get; set; } = null;
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int z { get; set; } = 0;
        public int zb { get; set; } = 0;
        public int puttip_x { get; set; } = 0;//脱针第二坐标
        public int index { get; set; } = 0;//加样器序号
        public int capacity { get; set; } = 0;//容量
        public int follow_ratio { get; set; } = 0;//跟随系数
        public int mixtimes { get; set; } = 0;//混合次数
        public int mixdeep { get; set; } = 0;//混合深度
        public int mixcapacity { get; set; } = 0;//混合容量
        public int spucapacity { get; set; } = 0;//分配容量
        public int tube { get; set; } = 0;//微胶柱选位
        public double tube_gap { get; set; } = 0;//微胶柱选位
        public int deep { get; set; } = 0;//加样加深
        public int deepspeed { get; set; } = 0;//加样速度
        public int detectordeep { get; set; } = 0;//液面探测加深
        public int absbspeed { get; set; } = 0;//吸液速度
        public int spuspeed { get; set; } = 0;//分液速度
        public int backcapacity { get; set; } = 0;//回量
        public int backabsspeed { get; set; } = 0;//吸液回量速度
        public int backabstime { get; set; } = 0;//吸液回量间隔
        public int backspuspeed { get; set; } = 0;//分液回量速度
        public int backsputime { get; set; } = 0;//分液回量间隔
        public int abspressure { get; set; } = 0;//吸液压力值
        public int hatchtime { get; set; } = 0;//孵育时间
        public int hitsort { get; set; } = 0;//Y轴优先级
        public string liquid_type { get; set; } = "";//吸液类型
        public bool isdone { get; set; } = false;//移动计算
        public int after_mix_spucapacity { get; set; } = 1;//混合后的吸液倍数
        public double minwidth { get; set; } = 1.0f;//最小间距
        public TestStepEnum type { get; set; } = TestStepEnum.Define;
        public ActionPoint()
        {
            x = 0;
            y = 0;
            z = 0;
            zb = 0;
            index = 0;
        }
        public ActionPoint(int xx, int yy, int zz)
        {
            x = xx;
            y = yy;
            z = zz;
            zb = 0;
            index = 0;
        }
        public ActionPoint(int xx, int yy, int zz, TestStepEnum ttype)
        {
            x = xx;
            y = yy;
            z = zz;
            zb = 0;
            type = ttype;
            index = 0;
        }
        public ActionPoint(int xx, int yy)
        {
            x = xx;
            y = yy;
            zb = 0;
            index = 0;
        }
        public List<int> GetTubeList()
        {
            List<int> tubelist = new List<int>();
            for(int i=0;i<8;i++)
            {
                int aa = (tube & (0x01 << i));
                if ((tube & (0x01 << i)) != 0x00)
                {
                    tubelist.Add(i);
                }
            }
            return tubelist;
        }
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class IMask
    {
       public static int [] Gen(params int[] values)
       {
            return values;
       }

        public static int[] Gen(int value)
        {
            int[] values = { value, value, value, value };
            return values;
        }

        public static double[] Gen(double value)
        {
            double[] values = { value, value, value, value };
            return values;
        }

        public static ActionPoint[] Gen(params ActionPoint[] values)
        {
            return values;
        }

        public static ActionPoint[] Gen(ActionPoint value)
        {
            ActionPoint[] values = new ActionPoint[4];
            for (int i = 0; i < values.Count(); i++)
            {
                values[i] = (ActionPoint)value.Clone();
                values[i].index = i;
            }
            return values;
        }

        public static bool IsEquate(ActionPoint[] valuesa, ActionPoint[] valuesb)
        {
            if (valuesa.Count() != valuesb.Count()) return false;
            bool is_the_same = true;
            for(int i=0;i<valuesa.Count();i++)
            {
                is_the_same = valuesa[i].x == valuesb[i].x&& valuesa[i].y == valuesb[i].y&& valuesa[i].z == valuesb[i].z;
                if (is_the_same == false) break;
            }
            return is_the_same;
        }

        public static bool IsEquate(int[] valuesa, int[] valuesb)
        {
            if (valuesa.Count() != valuesb.Count()) return false;
            bool is_the_same = true;
            for (int i = 0; i < valuesa.Count(); i++)
            {
                is_the_same = valuesa[i] == valuesb[i];
                if (is_the_same == false) break;
            }
            return is_the_same;
        }

        public static bool IsLessThan(int[] valuesa, int[] valuesb)
        {
            if (valuesa.Count() != valuesb.Count()) return false;
            bool is_the_same = true;
            for (int i = 0; i < valuesa.Count(); i++)
            {
                is_the_same = valuesa[i] > valuesb[i];
                if (is_the_same == false) break;
            }
            return is_the_same;
        }

        public static bool IsLargeThan(int[] valuesa, int[] valuesb)
        {
            if (valuesa.Count() != valuesb.Count()) return false;
            bool is_the_same = true;
            for (int i = 0; i < valuesa.Count(); i++)
            {
                is_the_same = valuesa[i] < valuesb[i];
                if (is_the_same == false) break;
            }
            return is_the_same;
        }
        
        public static bool Copy(int[] valuesa, int[] valuesb)
        {
            if (valuesa.Count() != valuesb.Count()) return false;
            bool is_the_same = true;
            for (int i = 0; i < valuesa.Count(); i++)
            {
                valuesb[i]=valuesa[i];
            }
            return is_the_same;
        }
    }


}
