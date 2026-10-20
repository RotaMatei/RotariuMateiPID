using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;

namespace Algorithms.Sections
{
    public class PointwiseOperations
    {
        #region Contrast and Brightness
        public static byte[] ContrastAndBrightness(double alpha, double beta)
        {
            byte[] LUT = new byte[256];
            for(int r = 0; r< 256; r++)
            {
                double s = (int)(alpha * r + beta);
                LUT[r] = (byte)Math.Max(0, Math.Min(255, Math.Round(s)));
            }
            return LUT;
        }
        #endregion

        #region Apply LUT
        public static Image<Gray, byte> ApplyLUT(Image<Gray, byte> inputImage, byte[] LUT)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);
            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    byte pixelValue = inputImage.Data[y, x, 0];
                    result.Data[y, x, 0] = LUT[pixelValue];
                }
            }
            return result;
        }

        public static Image<Bgr, byte> ApplyLUT(Image<Bgr, byte> inputImage, byte[] LUT)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);
            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    byte pixelValue = inputImage.Data[y, x, 0];
                    byte pixelValue1 = inputImage.Data[y, x, 1];
                    byte pixelValue2 = inputImage.Data[y, x, 2];
                    result.Data[y, x, 0] = LUT[pixelValue];
                    result.Data[y, x, 1] = LUT[pixelValue1];
                    result.Data[y, x, 2] = LUT[pixelValue2];
                }
            }
            return result;
        }
        #endregion

        #region Gamma operator
        // s = c * r^gamma, with c = 255^(1 - gamma) so that 0 and 255 are left unchanged
        public static byte[] Gamma(double gamma)
        {
            byte[] LUT = new byte[256];
            double c = Math.Pow(255, 1 - gamma);
            for (int r = 0; r < 256; r++)
            {
                double s = c * Math.Pow(r, gamma);
                LUT[r] = (byte)Math.Max(0, Math.Min(255, Math.Round(s)));
            }
            return LUT;
        }
        #endregion
    }
}