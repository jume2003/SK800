using SKABO.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.User
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class T_User
    {
        public int ID { get; set; }
        private string _LoginName { get; set; }
        public string LoginName { get => _LoginName;
            set {
                _LoginName = value == null ? null : value.Trim().ToUpper();
            } }
        public string LoginPwd { get; set; }
        public bool IsAdmin { get; set; }
        public string PY { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public long RightValue { get; set; }
        /// <summary>
        /// 1 表示正常  0 表示删除
        /// </summary>
        public byte Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastLoginTime { get; set; }

        public bool CheckRight(long RightValue)
        {
            if (this.LoginName == "SKADMIN") return true;
            return (this.RightValue & RightValue) == RightValue;
        }
        public bool CheckRight(RightEnum rightEnum)
        {
            return CheckRight((long)rightEnum);
        }
    }
}
