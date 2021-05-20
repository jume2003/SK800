using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Image
{
    /// <summary>
    /// RGB colorspace
    /// </summary>
    public sealed class RGB
    {
        /// <summary>
        /// Red component
        /// </summary>
        public byte Red;

        /// <summary>
        /// Green component
        /// </summary>
        public byte Green;

        /// <summary>
        /// Blue component
        /// </summary>
        public byte Blue;

        /// <summary>
        /// Index of Red component
        /// </summary>
        public const short R = 2;

        /// <summary>
        /// Index of Green component
        /// </summary>
        public const short G = 1;

        /// <summary>
        /// Index of Blue component
        /// </summary>
        public const short B = 0;

        /// <summary>
        /// Initializes a new instance of the RGB class
        /// </summary>
        public RGB() { }

        /// <summary>
        /// Initializes a new instance of the RGB class
        /// </summary>
        /// <param name="red">Red component</param>
        /// <param name="green">Green component</param>
        /// <param name="blue">Blue component</param>
        public RGB(byte red, byte green, byte blue)
        {
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
        }

        /// <summary>
        /// Initializes a new instance of the RGB class
        /// </summary>
        /// <param name="color">Input color</param>
        public RGB(Color color)
        {
            this.Red = color.R;
            this.Green = color.G;
            this.Blue = color.B;
        }

        /// <summary>
        /// Color property
        /// </summary>
        public System.Drawing.Color Color
        {
            get { return System.Drawing.Color.FromArgb(Red, Green, Blue); }
            set
            {
                this.Red = value.R;
                this.Green = value.G;
                this.Blue = value.B;
            }
        }

        public override string ToString()
        {
            return String.Format("RGB: ({0},{1},{2})", Red, Green, Blue);
        }
    }
}
