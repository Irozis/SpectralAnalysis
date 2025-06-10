using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SpectralAnalysis
{
    public static class FileParser
    {
        public static Spectrum ParseSpectrum(string path)
        {
            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 2)
                throw new InvalidDataException($"File '{path}' must contain at least 2 lines.");

            var headerParts = lines[0].Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            var dataParts = lines[1].Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            if (headerParts.Length <= 7 || dataParts.Length <= 7)
                throw new InvalidDataException($"File '{path}' does not have enough columns. Found {headerParts.Length} headers and {dataParts.Length} data points.");

            var headerWl = headerParts.Skip(7).ToArray();
            var dataVal = dataParts.Skip(7).ToArray();

            int len = Math.Min(headerWl.Length, dataVal.Length);
            if (headerWl.Length != dataVal.Length)
            {
                Console.WriteLine($"Warning: '{path}' header count {headerWl.Length} != data count {dataVal.Length}. Using min length {len}.");
            }

            var wavelengths = headerWl.Take(len).Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            var values = dataVal.Take(len).Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();

            if (wavelengths.Length > 0) // Only sort if there's data
            {
                // Combine wavelengths and values into pairs
                var spectralPairs = new (double Wavelength, double Value)[wavelengths.Length];
                for (int k = 0; k < wavelengths.Length; k++)
                {
                    spectralPairs[k] = (wavelengths[k], values[k]);
                }

                // Sort the pairs based on wavelength
                Array.Sort(spectralPairs, (pair1, pair2) => pair1.Wavelength.CompareTo(pair2.Wavelength));

                // Separate them back into sorted arrays
                for (int k = 0; k < wavelengths.Length; k++)
                {
                    wavelengths[k] = spectralPairs[k].Wavelength;
                    values[k] = spectralPairs[k].Value;
                }
            }

            return new Spectrum { Wavelengths = wavelengths, Values = values };
        }
    }
}
