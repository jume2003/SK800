using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Attributes
{
    public class GridColumnAttribute:Attribute
    {
        private readonly string _Header;
        private readonly double? _width;
        private readonly int _OrderIndex;
        private readonly String _StringFormat;

        public string Header { get => _Header; }
        public double? Width { get => _width; }
        public int OrderIndex { get => _OrderIndex; }
        public String StringFormat { get => _StringFormat; }
        public String BindingPath { get; set; }
        public GridColumnAttribute(string Header):this(Header,-1)
         {
         }
        public GridColumnAttribute(string Header,Double width) : this(Header, width, null,0)
        {
        }
        public GridColumnAttribute(string Header, Double width,String StringFormat) : this(Header, width, StringFormat, 0)
        {
        }
        public GridColumnAttribute(string Header, String StringFormat) : this(Header, -1, StringFormat, 0)
        {
        }
        public GridColumnAttribute(string Header, Double width, int OrderIndex) : this(Header, width, null, 0)
        {
            
        }
        public GridColumnAttribute(string Header, Double width, String StringFormat, int OrderIndex)
        {
            _Header = Header;
            if (width >= 0)
                _width = width;
            _OrderIndex = OrderIndex;
            _StringFormat = StringFormat;
        }
    }
}
