using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Duplex
{
    public class SampleInfo
    {
        public byte RackIndex { get; set; }
        public byte Index { get; set; }
        public String Barcode1 { get; set; }
        public String Barcode2 { get; set; }
        public String Barcode3 { get; set; }
        public String Barcode4 { get; set; }

        public String Barcode5 { get; set; }
        public String Barcode6 { get; set; }
        public String Barcode7 { get; set; }
        public String Barcode8 { get; set; }
        public String Barcode9 { get; set; }
        public void SetBarcode(int RackIndex, String Barcode)
        {
            switch (RackIndex)
            {
                case 1:
                    {
                        Barcode1 = Barcode;
                        break;
                    }
                case 2:
                    {
                        Barcode2 = Barcode;
                        break;
                    }
                case 3:
                    {
                        Barcode3 = Barcode;
                        break;
                    }
                case 4:
                    {
                        Barcode4 = Barcode;
                        break;
                    }
                case 5:
                    {
                        Barcode5 = Barcode;
                        break;
                    }
                case 6:
                    {
                        Barcode6 = Barcode;
                        break;
                    }
                case 7:
                    {
                        Barcode7 = Barcode;
                        break;
                    }
                case 8:
                    {
                        Barcode8 = Barcode;
                        break;
                    }
                case 9:
                    {
                        Barcode9 = Barcode;
                        break;
                    }
            }
        }
        public String GetBarcode(int RackIndex)
        {
            String Barcode = null;
            switch (RackIndex)
            {
                case 1:
                    {
                        Barcode = Barcode1;
                        break;
                    }
                case 2:
                    {
                        Barcode = Barcode2;
                        break;
                    }
                case 3:
                    {
                        Barcode = Barcode3;
                        break;
                    }
                case 4:
                    {
                        Barcode = Barcode4;
                        break;
                    }
                case 5:
                    {
                        Barcode = Barcode5;
                        break;
                    }
                case 6:
                    {
                        Barcode = Barcode6;
                        break;
                    }
                case 7:
                    {
                        Barcode = Barcode7;
                        break;
                    }
                case 8:
                    {
                        Barcode = Barcode8;
                        break;
                    }
                case 9:
                    {
                        Barcode = Barcode9;
                        break;
                    }
            }
            return Barcode;
        }
    }
}
