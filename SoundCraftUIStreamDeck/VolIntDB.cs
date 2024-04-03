using System;
using System.Linq;


namespace SoundCraftUIStreamDeck
{
    public static class DBLinearConversion
    {
        public static double LookupDBValue(double a)
        {
            a = VtoLIN(a);
            return (20 * Math.Log10(a) * 10 + .45) / 10;
        }

        public static double LookupLinearValue(double a)
        {
            return FindV(LookupDBValue, a);
        }

        public static string VtoDB(double a)
        {
            double b = VtoLIN(a);
            if (0.001 > a)
            {
                return "- \u221E ";
            }
            else
            {
                b = Math.Round((20 * Math.Log10(b) * 10 + .45) / 10, 1);
                if (b <= -20)
                {
                    b = Math.Truncate(b);
                }
                else if (!b.ToString().Contains("."))
                {
                    b = b + 0.0;
                }
                string aString = b + " dB";
                if (b <= -120)
                {
                    aString = "- \u221E ";
                }
                return aString;
            }
        }

        private static double FindV(Func<double, double> a, double b)
        {
            double c = 0, d = 1;
            for (int f = 0; f < 128; f++)
            {
                double e = 0.5 * (c + d);
                double g = a(e);
                if (Math.Abs(g - b) < 1E-10) return e;
                if (g > b)
                {
                    d = e;
                }
                else
                {
                    c = e;
                }
            }
            return 0.5 * (c + d);
        }

        private static double VtoLIN(double a)
        {
            return 2.676529517952372E-4 * Math.Exp(a * (23.90844819639692 + a * (-26.23877598214595 + (12.195249692570245 - 0.4878099877028098 * a) * a))) * (0.055 > a ? Math.Sin(28.559933214452666 * a) : 1);
        }
    }
}