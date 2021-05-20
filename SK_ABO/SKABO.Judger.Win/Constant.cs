
using IBatisNet.DataMapper;
using SKABO.Common.Utils;
using System;

namespace SKABO.Judger.Win
{
    public class Constant
    {
        private static ISqlMapper _EntityMapper;
        public static ISqlMapper EntityMapper
        {
            get
            {
                try
                {
                    if (_EntityMapper == null) {
                        _EntityMapper = Mapper.Instance();
                        //String ddd = _EntityMapper.DataSource.Name;
                        //_EntityMapper.DataSource.ConnectionString = @"Password=MA85332389;Persist Security Info=True;User ID=sa;Initial Catalog=SKABO;Data Source=.\SQLExpress";
                        _EntityMapper.DataSource.ConnectionString = @"Host=localhost;Port=5432;Username=abo;Password=85332389;Database=skpcb";
                        //_EntityMapper
                    }
                    return _EntityMapper;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        private static String _MSN;
        public static String MSN
        {
            get { if(String.IsNullOrEmpty(_MSN))
                    _MSN= Tool.getAppSetting("MSN"); 
                return _MSN;
            }
        }
        
    }
}
