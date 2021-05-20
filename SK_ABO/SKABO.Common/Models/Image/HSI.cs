using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Image
{
    /// <summary>
    /// HSI colorspace
    /// </summary>
    /// <remarks>All components normalized</remarks>
    public sealed class HSI
    {
        /// <summary>
        /// Hue component
        /// </summary>
        /// <remarks>Hue ranges [0,360]</remarks>
        public double Hue;

        /// <summary>
        /// Saturation component
        /// </summary>
        /// <remarks>Saturation ranges [0,1]</remarks>
        public double Saturation;

        /// <summary>
        /// Intensity component
        /// </summary>
        /// <remarks>Intensity ranges [0,1]</remarks>
        public double Intensity;

        /// <summary>
        /// Initializes a new instance of the HSI class
        /// </summary>
        public HSI() { }

        /// <summary>
        /// Initializes a new instance of the HSI class
        /// </summary>
        /// <param name="hue">Hue component</param>
        /// <param name="saturation">Saturation component</param>
        /// <param name="intensity">Intensity component</param>
        public HSI(double hue, double saturation, double intensity)
        {
            this.Hue = hue;
            this.Saturation = saturation;
            this.Intensity = intensity;
        }

        public override string ToString()
        {
            return String.Format("HSI: ({0},{1},{2})", Hue, Saturation, Intensity);
        }
    }
}
