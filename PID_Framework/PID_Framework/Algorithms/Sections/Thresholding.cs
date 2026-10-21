using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Security.Cryptography;

namespace Algorithms.Sections
{
    public class Thresholding
    {
        #region Intermeans
        public static byte IntermeansThresholding(Image<Gray, byte> inputImage)
        {
            var histogram = Utilities.Utils.ComputeHistogram(inputImage); 
            double n = inputImage.Width * inputImage.Height;

            double[] p = new double[256];
            double miu = 0.0;
            for (int k = 0; k < 256; k++)
            {
                p[k] = histogram[k] / n;
                miu += k * p[k];
            }

            byte T = (byte)Math.Round(miu);
            byte newT;

            while (true)
            {
                double P1 = 0, miu1 = 0, miu2 = 0;
                for (int k = 0; k <= T; k++)
                {
                    P1 += p[k];
                    miu1 += k * p[k];
                }
                for (int k = T + 1; k < 256; k++)
                    miu2 += k * p[k];

                double P2 = 1 - P1;
                if (P1 <= 0 || P2 <= 0) break;

                miu1 /= P1;
                miu2 /= P2;
                newT = (byte)Math.Round((miu1 + miu2) / 2);

                if (newT == T) break; 
                T = newT;
            }
            return T;
        }
        #endregion
    }
}