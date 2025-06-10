using System;
using System.Linq;

namespace SpectralAnalysis
{
    public static class Densitometry
    {
        public static DensityResult ComputeDensities(double[] wl, double[] R)
        {
            double total = R.Sum();
            double D0 = Math.Log10(1.0 / total);
            double C = Math.Log10(1.0 / R.Where((_,i)=> wl[i]>=380 && wl[i]<=495).Sum());
            double M = Math.Log10(1.0 / R.Where((_,i)=> wl[i]>=500 && wl[i]<=630).Sum());
            double Y = Math.Log10(1.0 / R.Where((_,i)=> wl[i]>=580 && wl[i]<=730).Sum());
            return new DensityResult { D0 = D0, C = C, M = M, Y = Y, K = D0 };
        }
    }

    public class DensityResult
    {
        public double D0 { get; set; }
        public double C  { get; set; }
        public double M  { get; set; }
        public double Y  { get; set; }
        public double K  { get; set; }
    }
}
