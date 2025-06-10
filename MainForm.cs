using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace SpectralAnalysis
{
    public class MainForm : Form
    {
        private TextBox txtSamplePath, txtTelefonPath, txtResults;
        private Button btnBrowseSample, btnBrowseTelefon, btnCalculate;

        public MainForm()
        {
            Text = "Spectral Analysis";
            Width = 820; Height = 620; StartPosition = FormStartPosition.CenterScreen;

            Label lbl1 = new Label { Text = "Sample (.txt):", Left = 10, Top = 20, Width = 100 };
            txtSamplePath = new TextBox { Left = 120, Top = 18, Width = 550 };
            btnBrowseSample = new Button { Text = "Browse...", Left = 680, Top = 16, Width = 100 };
            btnBrowseSample.Click += (_, __) => BrowseFile(txtSamplePath);

            Label lbl2 = new Label { Text = "Telefon (.txt):", Left = 10, Top = 60, Width = 100 };
            txtTelefonPath = new TextBox { Left = 120, Top = 58, Width = 550 };
            btnBrowseTelefon = new Button { Text = "Browse...", Left = 680, Top = 56, Width = 100 };
            btnBrowseTelefon.Click += (_, __) => BrowseFile(txtTelefonPath);

            btnCalculate = new Button { Text = "Calculate", Left = 350, Top = 100, Width = 100 };
            btnCalculate.Click += (_, __) => RunCalculation();

            txtResults = new TextBox
            {
                Left = 10, Top = 140,
                Width = 770, Height = 430,
                Multiline = true, ScrollBars = ScrollBars.Both,
                Font = new System.Drawing.Font("Consolas", 10), ReadOnly = true
            };

            Controls.AddRange(new Control[] { lbl1, txtSamplePath, btnBrowseSample,
                                              lbl2, txtTelefonPath, btnBrowseTelefon,
                                              btnCalculate, txtResults });
        }

        private void BrowseFile(TextBox target)
        {
            using var dlg = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files|*.*" };
            if (dlg.ShowDialog() == DialogResult.OK)
                target.Text = dlg.FileName;
        }

        private void RunCalculation()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSamplePath.Text) || string.IsNullOrWhiteSpace(txtTelefonPath.Text))
                {
                    MessageBox.Show("Please select both files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var sample = FileParser.ParseSpectrum(txtSamplePath.Text);
                var telefon = FileParser.ParseSpectrum(txtTelefonPath.Text);
                var wavelengths = Enumerable.Range(0, (730 - 380) / 5 + 1)
                                            .Select(i => 380.0 + i * 5).ToArray();

                var R_samp = SpectralCalculator.Interpolate(sample.Wavelengths, sample.Values, wavelengths);
                var SPD_tel = SpectralCalculator.Interpolate(telefon.Wavelengths, telefon.Values, wavelengths);
                var SPD_BB = SpectralCalculator.LoadStandardBlackbodySPD(wavelengths);
                var cie = SpectralCalculator.LoadCIE1931Functions(wavelengths);

                var results = new List<AnalysisResult>();
                foreach (var source in new[] { (Name: "Blackbody", SPD: SPD_BB), (Name: "Telefon", SPD: SPD_tel) })
                {
                    var Xs = Colorimetry.ComputeCIEXYZ(wavelengths, R_samp, source.SPD, cie);
                    var Xref = Colorimetry.ComputeCIEXYZ(wavelengths, Enumerable.Repeat(1.0, wavelengths.Length).ToArray(), source.SPD, cie);
                    Colorimetry.NormalizeToY100(ref Xs);
                    Colorimetry.NormalizeToY100(ref Xref);

                    var (x, y) = Colorimetry.ComputeChromaticity(Xs);
                    var (u, v) = Colorimetry.ComputeUCSuv(Xs);
                    var labS = Colorimetry.ComputeCIELab(Xs);
                    var labR = Colorimetry.ComputeCIELab(Xref);
                    var deltaE = Colorimetry.ComputeDeltaE(labS, labR);
                    var p = SpectralCalculator.ComputePearsonCorrelation(R_samp, source.SPD);
                    var cct = Colorimetry.ComputeCCT(Xref);
                    var dens = Densitometry.ComputeDensities(wavelengths, R_samp);

                    results.Add(new AnalysisResult
                    {
                        Source = source.Name,
                        XYZ = Xs,
                        xy = (x, y),
                        uv = (u, v),
                        Lab = labS,
                        DeltaE = deltaE,
                        Pearson = p,
                        CCT = cct,
                        Densities = dens
                    });
                }

                txtResults.Text = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Calculation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}