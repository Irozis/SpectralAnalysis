using System;
using System.Linq;

namespace SpectralAnalysis
{
    public static class SpectralCalculator
    {
        public static double[] Interpolate(double[] x, double[] y, double[] xi)
        {
            double[] result = new double[xi.Length];
            for (int i = 0; i < xi.Length; i++)
            {
                double val = xi[i];
                int idx = Array.BinarySearch(x, val);
                if (idx >= 0) result[i] = y[idx];
                else
                {
                    idx = ~idx;
                    if (idx == 0 || idx >= x.Length) result[i] = 0;
                    else result[i] = y[idx - 1] + (y[idx] - y[idx - 1]) * (val - x[idx - 1]) / (x[idx] - x[idx - 1]);
                }
            }
            return result;
        }

        public static double IntegrateTrapezoidal(double[] x, double[] y)
        {
            double sum = 0;
            for (int i = 1; i < x.Length; i++) sum += (y[i - 1] + y[i]) * (x[i] - x[i - 1]) / 2;
            return sum;
        }

        public static double[] LoadStandardBlackbodySPD(double[] wl)
        {
            const double T = 2856, c2 = 1.4388e7;
            double[] spd = new double[wl.Length];
            for (int i = 0; i < wl.Length; i++)
                spd[i] = Math.Pow(560.0 / wl[i], 5) * (Math.Exp(c2 / (560.0 * T)) - 1) / (Math.Exp(c2 / (wl[i] * T)) - 1);
            int idx = Array.IndexOf(wl, 560.0);
            if (idx < 0)
            {
                throw new InvalidOperationException("Wavelength 560.0nm not found in the provided wavelength array. Cannot normalize Blackbody SPD as intended.");
            }
            if (idx >= 0)
                for (int i = 0; i < spd.Length; i++) spd[i] *= 100 / spd[idx];
            return spd;
        }

        public static (double[] xBar, double[] yBar, double[] zBar) LoadCIE1931Functions(double[] wl)
        {
            // CIE 1931 2° standard observer functions at 380-730nm step 5nm
            double[] xBar = new double[]
            {
                0.001368,0.002236,0.004243,0.007650,0.014310,0.023190,0.043510,0.077630,0.134380,0.214770,
                0.283900,0.328500,0.348280,0.348060,0.336200,0.318700,0.290800,0.251100,0.195360,0.142100,
                0.095640,0.057950,0.032010,0.014700,0.004900,0.002400,0.009300,0.029100,0.063270,0.109600,
                0.165500,0.225750,0.290400,0.359700,0.433450,0.512050,0.594500,0.678400,0.762100,0.842500,
                0.916300,0.978600,1.026300,1.056700,1.062200,1.045600,1.002600,0.938400,0.854450,0.751400,
                0.642400,0.541900,0.447900,0.360800,0.283500,0.218700,0.164900,0.121200,0.087400,0.063600,
                0.046770,0.032900,0.022700,0.015840,0.011359,0.008111,0.005790,0.004109,0.002899,0.002049,0.001440
            };
            double[] yBar = new double[]
            {
                0.000039,0.000064,0.000120,0.000217,0.000396,0.000640,0.001210,0.002180,0.004000,0.007300,
                0.011600,0.016840,0.023000,0.029800,0.038000,0.048000,0.060000,0.073900,0.090980,0.112600,
                0.139020,0.169300,0.208020,0.258600,0.323000,0.407300,0.503000,0.608200,0.710000,0.793200,
                0.862000,0.914850,0.954000,0.980300,0.994950,1.000000,0.995000,0.978600,0.952000,0.915400,
                0.870000,0.816300,0.757000,0.694900,0.631000,0.566800,0.503000,0.441200,0.381000,0.321000,
                0.265000,0.217000,0.175000,0.138200,0.107000,0.081600,0.061000,0.044580,0.032000,0.023200,
                0.017000,0.011920,0.008210,0.005723,0.004102,0.002929,0.002091,0.001484,0.001047,0.000740,0.000520
            };
            double[] zBar = new double[]
            {
                0.006450,0.010550,0.020050,0.036210,0.067850,0.110200,0.207400,0.371300,0.645600,1.039050,
                1.385600,1.622960,1.747060,1.782600,1.772110,1.744100,1.669200,1.528100,1.287640,1.041900,
                0.813000,0.616200,0.465180,0.353300,0.272000,0.212300,0.158200,0.111700,0.078250,0.057250,
                0.042160,0.029840,0.020300,0.013400,0.008750,0.005750,0.003900,0.002749,0.002100,0.001800,
                0.001650,0.001400,0.001100,0.001000,0.000800,0.000600,0.000340,0.000240,0.000190,0.000100,
                0.000049,0.000030,0.000020,0.000010,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,
                0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000,0.000000
            };
            return (xBar, yBar, zBar);
        }

        public static double ComputePearsonCorrelation(double[] a, double[] b)
        {
            double meanA = a.Average(), meanB = b.Average();
            double num = a.Zip(b, (u, v) => (u - meanA) * (v - meanB)).Sum();
            double den = Math.Sqrt(a.Sum(u => (u - meanA) * (u - meanA))) * Math.Sqrt(b.Sum(v => (v - meanB) * (v - meanB)));
            return den == 0 ? 0 : num / den;
        }
    }
}