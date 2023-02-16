using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplexNumbers
{
    /// <summary>
    /// Creates a complex number
    /// </summary>
    public class CN
    {
        private double real;
        private double imaginary;

        /// <summary>
        /// Creates a new complex number.
        /// </summary>
        /// <param name="real">The real part (rectangular).</param>
        /// <param name="imaginary">The imaginary part (rectangular).</param>
        public CN(double real, double imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        #region FACTORY

        /// <summary>
        /// Returns a new complex number from rectangular values.
        /// </summary>
        /// <param name="real">The real part.</param>
        /// <param name="imaginary">The imaginary part.</param>
        /// <returns></returns>
        public static CN FromRectangular(double real, double imaginary)
        {
            return new CN(real, imaginary);
        }

        /// <summary>
        /// Returns a new complex number from polar values (phase in degrees).
        /// </summary>
        /// <param name="absolute">The number magnitude.</param>
        /// <param name="phase">The number phase (degrees)</param>
        /// <returns></returns>
        public static CN FromPolarDeg(double absolute, double phase)
        {
            return new CN(absolute * Math.Cos(phase * Math.PI / 180), absolute * Math.Sin(phase * Math.PI / 180));
        }

        /// <summary>
        /// Returns a new complex number from polar values (phase in radians).
        /// </summary>
        /// <param name="absolute">The number magnitude.</param>
        /// <param name="phase">The number phase (radians)</param>
        /// <returns></returns>
        public static CN FromPolarRad(double absolute, double phase)
        {
            return new CN(absolute * Math.Cos(phase), absolute * Math.Sin(phase));
        }

        /// <summary>
        /// Returns a new complex number from FEMM 4.2 string (ex: "0.05-I*0.13").
        /// </summary>
        /// <param name="femmResultString">The resulting (1-line) string from FEMM.</param>
        /// <returns></returns>
        public static CN FromFEMM42String(string femmResultString)
        {
            List<char> chars = femmResultString.ToCharArray().ToList();
            double re = 0;
            double im = 0;
            if (chars.Contains('I'))
            {
                int IPosition = chars.IndexOf('I');
                //builds the real string
                string reS = "";
                for (int i = 0; i < IPosition - 1; i++)
                {
                    reS += chars[i];
                }
                string imS = "";
                for (int i = IPosition + 2; i < chars.Count; i++)
                {
                    imS += chars[i];
                }
                double imSign = 1;
                if (IPosition == 0)
                {
                    reS = "0";
                }
                else
                {
                    imSign = chars[IPosition - 1] == '-' ? -1 : 1;
                }
                re = Convert.ToDouble(reS);
                im = Convert.ToDouble(imS) * imSign;
            }
            else
            {
                List<string> ld = femmResultString.Split('\n').ToList();
                re = Convert.ToDouble(ld[0]);
            }
            return new CN(re, im);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets or sets the real part (rectangular).
        /// </summary>
        public double Real
        {
            get { return real; }
            set { real = value; }
        }

        /// <summary>
        /// Gets or sets the imaginary part (rectangular).
        /// </summary>
        public double Imaginary
        {
            get { return imaginary; }
            set { imaginary = value; }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Returns the absolute value.
        /// </summary>
        public double GetAbsolute()
        {
            return Math.Pow(Math.Pow(real, 2) + Math.Pow(imaginary, 2), 0.5);
        }

        /// <summary>
        /// Returns the angle (in radian).
        /// </summary>
        public double GetAngleRadian()
        {
            double angleRadian;
            if (real == 0)
            {
                if (imaginary > 0)
                {
                    angleRadian = 0;
                }
                else if (imaginary < 0)
                {
                    angleRadian = Math.PI;
                }
                else
                {
                    angleRadian = 0;
                }
            }
            else
            {
                angleRadian = Math.Atan2(imaginary, real);
            }
            return angleRadian;
        }

        /// <summary>
        /// Returns the "|c|∠arg(c)" string (8 decimal digits precision).
        /// </summary>
        /// <param name="polarDegree">True for degrees, False for radian.</param>
        /// <param name="absoluteDecimalPlaces">Number of decimal places for the absolute value.</param>
        /// <param name="angleDecinalPlaces">Number of decimal places for the angle value.</param>
        /// <returns></returns>
        public string GetString(bool polarDegree, int absoluteDecimalPlaces, int angleDecinalPlaces)
        {
            if (polarDegree)
            {
                return Math.Round(this.GetAbsolute(), absoluteDecimalPlaces).ToString() + "∠" + Math.Round(this.GetAngleDegree(), angleDecinalPlaces).ToString() + "°";
            }
            else
            {
                return Math.Round(this.GetAbsolute(), absoluteDecimalPlaces).ToString() + "∠" + Math.Round(this.GetAngleRadian(), angleDecinalPlaces).ToString();
            }
        }

        /// <summary>
        /// Returns the angle (in degrees).
        /// </summary>
        public double GetAngleDegree()
        {
            return this.GetAngleRadian() * 180 / Math.PI;
        }

        /// <summary>
        /// Returns the "a+jb" string (8 decimal digits precision).
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string sign = "-";
            if (this.Imaginary >= 0)
                sign = "+";
            return string.Format("{0}{1}j{2}", Math.Round(this.Real, 8), sign, Math.Round(Math.Abs(this.Imaginary), 8));
        }

        /// <summary>
        /// Returns the "a+I*b" string.
        /// </summary>
        /// <returns></returns>
        public string ToFEMM42String()
        {
            string sign = "-";
            if (this.Imaginary >= 0)
                sign = "+";
            return string.Format("{0}{1}I*{2}", this.Real, sign, Math.Abs(this.Imaginary));
        }

        #endregion

        #region METHOD_OF_SYMMETRICAL_COMPONENTS

        /// <summary>
        /// Performs the fortescue inverse transform.
        /// </summary>
        /// <param name="I0">The zero sequence current.</param>
        /// <param name="Ipos">The positive sequence current.</param>
        /// <param name="Ineg">The negative sequence current.</param>
        /// <returns></returns>
        public static List<CN> FortescueIT270(double I0, double Ipos, double Ineg)
        {
            List<CN> list = new List<CN>();
            CN a1 = CN.FromPolarDeg(1, 120);
            CN a2 = a1 * a1;
            CN I0c = new CN(I0, 0);
            CN I1c = new CN(Ipos, 0);
            CN I2c = new CN(Ineg, 0);
            list.Add(I0c + I1c + I2c);
            list.Add(I0c + (I1c * a2 + I2c * a1));
            list.Add((I0c + (I1c * a1 + I2c * a2)));
            return list;
        }

        #endregion

        #region SUM

        /// <summary>
        /// Makes a + b.
        /// </summary>
        /// <param name="a">The first ComplexNumber.</param>
        /// <param name="b">The second ComplexNumber.</param>
        /// <returns></returns>
        public static CN operator +(CN a, CN b)
        {
            return new CN(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }

        /// <summary>
        /// Makes a + b.
        /// </summary>
        /// <param name="a">A double literal number.</param>
        /// <param name="b">A ComplexNumber.</param>
        /// <returns></returns>
        public static CN operator +(double a, CN b)
        {
            return new CN(a + b.Real, b.Imaginary);
        }

        /// <summary>
        /// Makes a + b.
        /// </summary>
        /// <param name="a">A ComplexNumber.</param>
        /// <param name="b">A double literal number.</param>
        /// <returns></returns>
        public static CN operator +(CN a, double b)
        {
            return new CN(a.Real + b, a.Imaginary);
        }

        #endregion

        #region SUBTRACT

        /// <summary>
        /// Makes a - b.
        /// </summary>
        /// <param name="a">The first ComplexNumber.</param>
        /// <param name="b">The second ComplexNumber.</param>
        /// <returns></returns>
        public static CN operator -(CN a, CN b)
        {
            return new CN(a.Real - b.Real, a.Imaginary - b.Imaginary);
        }

        /// <summary>
        /// Makes a - b.
        /// </summary>
        /// <param name="a">A double literal number.</param>
        /// <param name="b">A ComplexNumber.</param>
        /// <returns></returns>
        public static CN operator -(double a, CN b)
        {
            return new CN(a - b.Real, -b.Imaginary);
        }

        /// <summary>
        /// Makes a - b.
        /// </summary>
        /// <param name="a">A ComplexNumber.</param>
        /// <param name="b">A double literal number.</param>
        /// <returns></returns>
        public static CN operator -(CN a, double b)
        {
            return new CN(a.Real - b, a.Imaginary);
        }

        #endregion

        #region MULTIPLY

        /// <summary>
        /// Makes a * b.
        /// </summary>
        /// <param name="a">The first ComplexNumber.</param>
        /// <param name="b">The second ComplexNumber.</param>
        /// <returns></returns>
        public static CN operator *(CN a, CN b)
        {
            return new CN(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);
        }

        /// <summary>
        /// Makes a * b.
        /// </summary>
        /// <param name="a">A double literal number.</param>
        /// <param name="b">A ComplexNumber.</param>
        /// <returns></returns>
        public static CN operator *(double a, CN b)
        {
            return new CN(a * b.Real, a * b.Imaginary);
        }

        /// <summary>
        /// Makes a * b.
        /// </summary>
        /// <param name="a">A ComplexNumber.</param>
        /// <param name="b">A double literal number.</param>
        /// <returns></returns>
        public static CN operator *(CN a, double b)
        {
            return new CN(b * a.Real, b * a.Imaginary);
        }

        #endregion

        #region DIVIDE

        /// <summary>
        /// Makes a / b.
        /// </summary>
        /// <param name="a">The first ComplexNumber.</param>
        /// <param name="b">The second ComplexNumber.</param>
        /// <returns></returns>
        public static CN operator /(CN a, CN b)
        {
            return new CN((a.Real * b.Real + a.Imaginary * b.Imaginary) / (b.Real * b.Real + b.Imaginary * b.Imaginary), (a.Imaginary * b.Real - a.Real * b.Imaginary) / (b.Real * b.Real + b.Imaginary * b.Imaginary));
        }

        /// <summary>
        /// Makes a / b.
        /// </summary>
        /// <param name="a">A double literal number.</param>
        /// <param name="b">A ComplexNumber.</param>
        /// <returns></returns>
        public static CN operator /(double a, CN b)
        {
            return new CN(a * b.Real / (b.Real * b.Real + b.Imaginary * b.Imaginary), -(a * b.Imaginary) / (b.Real * b.Real + b.Imaginary * b.Imaginary));
        }

        /// <summary>
        /// Makes a / b.
        /// </summary>
        /// <param name="a">A ComplexNumber.</param>
        /// <param name="b">A double literal number.</param>
        /// <returns></returns>
        public static CN operator /(CN a, double b)
        {
            return new CN(a.Real * b / (b * b), (a.Imaginary * b) / (b * b));
        }

        #endregion

        #region EQUALS

        /// <summary>
        /// Makes a == b.
        /// </summary>
        /// <param name="a">The first ComplexNumber.</param>
        /// <param name="b">The second ComplexNumber.</param>
        /// <returns></returns>
        public static bool operator ==(CN a, CN b)
        {
            return (a.Real == b.Real) && (a.Imaginary == b.Imaginary);
        }

        /// <summary>
        /// Makes a == b.
        /// </summary>
        /// <param name="a">The first ComplexNumber.</param>
        /// <param name="b">The second ComplexNumber.</param>
        /// <returns></returns>
        public static bool operator !=(CN a, CN b)
        {
            return !(a == b);
        }

        #endregion

    }
}
