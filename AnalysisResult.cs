using System;

namespace SpectralAnalysis
{
    public class AnalysisResult
    {
        public required string Source { get; set; }
        public required double[] XYZ { get; set; }
        public required (double x, double y) xy { get; set; }
        public required (double u, double v) uv { get; set; }
        public required (double L, double a, double b) Lab { get; set; }
        public double DeltaE { get; set; }
        public double Pearson { get; set; }
        public double CCT { get; set; }
        public required DensityResult Densities { get; set; }
    }
}
