using SKABO.ActionEngine;
using SKABO.BLL.IServices.IJudger;
using SKABO.Camera;
using SKABO.Camera.Enums;
using SKABO.Common;
using SKABO.Common.Models.GEL;
using SKABO.Common.Models.Judger;
using SKABO.Common.Utils;
using SKABO.Hardware.Model;
using SKABO.Judger.Core;
using SKABO.ResourcesManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Hardware.RunBJ
{
    public class CameraDevice
    {
        public bool IsOpen { get; set; }
        private Judger.Core.Judger judger;
        private T_ParseTubeParameter _PTP;
        private T_ParseTubeParameter PTP
        {
            get
            {
                if (_PTP == null)
                {
                    _PTP = judgerParamerService.LoadPTP();
                }
                return _PTP;
            }
        }
        private T_ParseLEDParameter _PLP;
        private T_ParseLEDParameter PLP
        {
            get
            {
                if (_PLP == null)
                {
                    _PLP = judgerParamerService.LoadPLP();
                }
                return _PLP;
            }
        }
        public T_Camera t_Camera
        {
            get
            {
                if (_t_Camera == null)
                {
                    _t_Camera = judgerParamerService.QueryCamera(Constants.MSN);

                }
                return _t_Camera;
            }
            set => _t_Camera = value;
        }

        private T_Camera _t_Camera;
        private ICameraDevice _camera;
        private IJudgerParamerService _judgerParamerService;
        private IJudgerParamerService judgerParamerService
        {
            get
            {
                if (_judgerParamerService == null)
                {
                    _judgerParamerService = IoC.Get< IJudgerParamerService>();
                }
                return _judgerParamerService;
            }
        }
        private IResultService _ResultService;
        private IResultService ResultService
        {
            get
            {
                if (_ResultService == null)
                {
                    _ResultService = IoC.Get<IResultService>();
                }
                return _ResultService;
            }
        }
        public ICameraDevice Camera
        {
            get
            {
                if (_camera == null)
                {
                    _camera = CameraFactory.CreateCamera();
                }
                return _camera;
            }
        }

        private IList<T_JudgeParamer> _TJList;
        private IList<T_JudgeParamer> TJList
        {
            get
            {
                if (_TJList == null)
                {
                    _TJList= judgerParamerService.QueryALlParamerByMSN(Constants.MSN);
                }
                return _TJList;
            }
        }
        public Bitmap CaptureImage()
        {
            if (!IsOpen)
            {
                Open();
            }

           return Camera.GetBitmap();
        }
        public void Close()
        {
            this.Camera.Close();
        }
        public bool Open()
        {
            var result = true;
            try
            {
                if (Camera == null)
                {
                    return false;
                }
                if (Camera.Open())
                {
                    if (t_Camera != null)
                    {
                        Camera.SetBalanceRatio(BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_BLUE, Convert.ToDouble(t_Camera.BB));
                        Camera.SetBalanceRatio(BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_GREEN, Convert.ToDouble(t_Camera.GB));
                        Camera.SetBalanceRatio(BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_RED, Convert.ToDouble(t_Camera.RB));
                        Camera.SetExposureTime(t_Camera.ExposureTime);
                        Camera.SetGain(t_Camera.Gain);
                    }
                    Camera.Play();
                    this.IsOpen = true;
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception exception)
            {
                Tool.AppLogError(exception);
                result = false;
            }
            return result;
        }

        public bool Save(Bitmap img,ExperimentPackage exp_pack,ref List<T_Result> result_list)
        {
            var result = true;
            try
            {
                using (judger = new Judger.Core.Judger(img, true, true))
                {
                    
                    var path = $"pictures\\{exp_pack.start_time}";
                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }
                    path = path + @"\" + exp_pack.GetGelMask() + $"_{DateTime.Now.ToString("HHmmss")}.jpg";
                    if(System.IO.File.Exists(path))
                    {
                        System.Windows.Forms.MessageBox.Show(path+"已存在");
                    }
                    img.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                    var list = TJList.Where(c => c.AreaType == "T").ToList();
                    var resArray = judger.Judge(list, PTP);

                    var EndTime = DateTime.Now;
                    int[] vals;
                    var led = ImgUtil.AnalsyAreaDigitalTube(img, TJList.Where(c => c.AreaType == "D"), PLP.LEDThreshold, PLP.LEDBrushWidth, out vals);
                    byte tubeCount = (byte)(exp_pack.gel_type / exp_pack.ren_fen);
                    //生成Picture对象
                    T_Picture picture = new T_Picture();
                    picture.LED = "888";
                    picture.RawFile = path;
                    picture.MD5 = GetMD5HashFromFile(path);
                    var tp = typeof(T_Picture);
                    for (byte x = 0; x < resArray.Length; x++)
                    {
                        tp.GetProperty($"T{x + 1}").SetValue(picture, (int)resArray[x]);
                        tp.GetProperty($"Tube{x + 1}").SetValue(picture, ImgUtil.BitmapToBytes(judger.Tubes[x].Bm));
                    }
                    ////保存测试结果

                    for (byte b = 0; b < exp_pack.samples_barcode.Count; b++)
                    {
                        var sample_code = exp_pack.GetSampleCode(b);
                        if (sample_code.IndexOf("used") != -1) continue;
                        var sample_info = ResManager.getInstance().GetSampleInfo(sample_code);
                        var exp_result = new T_Result();
                        var geltype = ResManager.getInstance().GetGelTestByTestId(exp_pack.gel_test_id);
                        exp_result.SmpBarcode = exp_pack.GetSampleCode(b);
                        exp_result.DonorBarcode = exp_pack.GetAddvolunteerCode(b);
                        exp_result.TestUser = Constants.Login.LoginName;
                        exp_result.TubeCount = tubeCount;
                        exp_result.TubeStartNo = (byte)(b * tubeCount);
                        exp_result.RackIndex = $"{sample_info.RackIndex},{sample_info.Index}";
                        exp_result.Outed = false;
                        exp_result.LED = picture.LED;
                        exp_result.GelBarcode = exp_pack.GetGelMask();
                        exp_result.GelID = geltype.ID;
                        exp_result.GelName = geltype.GelName;
                        exp_result.TestName = geltype.TestName;
                        exp_result.EndTime = EndTime;
                        exp_result.StartTime = exp_pack.start_time_data;
                        exp_result.Outed = Constants.TakedOutRack(sample_info.RackIndex, exp_result.StartTime, EndTime);
                        exp_result.Picture = picture;
                        String color = null;
                        var finResult = ResultService.FinishResult(geltype, out color, ResultService.GetResultEnums(picture, exp_result.TubeStartNo, tubeCount));
                        exp_result.Result = finResult;
                        exp_result.Color = color;
                        result_list.Add(exp_result);
                        ResultService.SaveT_Result(exp_result);
                    }

                }

            }
            catch (Exception ex)
            {
                Tool.AppLogError(ex);
                result = false;
            }
            finally
            {

            }
            //
            //bag.SamplesInfo.Where(item => { item.Item1=gel.})
            return result;
        }
        /// <summary>
        /// 获取文件的MD5码
        /// </summary>
        /// <param name="fileName">传入的文件名（含路径及后缀名）</param>
        /// <returns></returns>
        public string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
    }
}
