using System;
using System.Linq;

namespace SpectralAnalysis
{
    public static class Colorimetry
    {
        public static double[] ComputeCIEXYZ(double[] wl, double[] R, double[] SPD, (double[] xBar, double[] yBar, double[] zBar) cie)
        {
            double[] xBar = cie.xBar;
            double[] yBar = cie.yBar;
            double[] zBar = cie.zBar;

            double denom = SpectralCalculator.IntegrateTrapezoidal(wl, SPD.Zip(yBar, (s, y) => s * y).ToArray());
            if (denom == 0) throw new ArgumentException("SPD integration result is zero.");
            double k = 100 / denom;

            double X = k * SpectralCalculator.IntegrateTrapezoidal(wl, R.Zip(SPD, (r, s) => r * s).Zip(xBar, (rs, xb) => rs * xb).ToArray());
            double Y = k * SpectralCalculator.IntegrateTrapezoidal(wl, R.Zip(SPD, (r, s) => r * s).Zip(yBar, (rs, yv) => rs * yv).ToArray());
            double Z = k * SpectralCalculator.IntegrateTrapezoidal(wl, R.Zip(SPD, (r, s) => r * s).Zip(zBar, (rs, zb) => rs * zb).ToArray());
            return new double[] { X, Y, Z };
        }

        public static void NormalizeToY100(ref double[] XYZ)
        {
            if (XYZ == null || XYZ.Length < 2) throw new ArgumentException("XYZ too short.");
            double factor = 100 / XYZ[1];
            for (int i = 0; i < XYZ.Length; i++) XYZ[i] *= factor;
        }

        public static (double x, double y) ComputeChromaticity(double[] XYZ)
        {
            if (XYZ == null || XYZ.Length < 2) throw new ArgumentException("XYZ too short.");
            double sum = XYZ.Sum(); if (sum == 0) throw new ArgumentException("XYZ sum zero.");
            return (XYZ[0] / sum, XYZ[1] / sum);
        }

        public static (double u, double v) ComputeUCSuv(double[] XYZ)
        {
            if (XYZ == null || XYZ.Length < 3) throw new ArgumentException("XYZ too short.");
            double denom = XYZ[0] + 15 * XYZ[1] + 3 * XYZ[2]; if (denom == 0) throw new ArgumentException("Denom zero.");
            return (4 * XYZ[0] / denom, 9 * XYZ[1] / denom);
        }

        public static (double L, double a, double b) ComputeCIELab(double[] XYZ)
        {
            const double Xn = 95.047, Yn = 100, Zn = 108.883;
            static double f(double t) => t > Math.Pow(6.0/29, 3) ? Math.Pow(t,1.0/3) : t/(3*Math.Pow(6.0/29,2)) + 4.0/29;
            double fx = f(XYZ[0]/Xn), fy = f(XYZ[1]/Yn), fz = f(XYZ[2]/Zn);
            double L = 116 * fy - 16;
            double a = 500 * (fx - fy);
            double b = 200 * (fy - fz);
            return (L, a, b);
        }

        public static double ComputeDeltaE((double L,double a,double b) c1, (double L,double a,double b) c2)
            => Math.Sqrt(Math.Pow(c1.L-c2.L,2) + Math.Pow(c1.a-c2.a,2) + Math.Pow(c1.b-c2.b,2));

        public static double ComputeCCT(double[] XYZ)
        {
            var (u, v) = ComputeUCSuv(XYZ);
            double n = (4 * u - 0.1548) / (v - 0.306);
            return -449 * n*n*n + 3525 * n*n - 6823.3 * n + 5520.33;
        }
    }
}