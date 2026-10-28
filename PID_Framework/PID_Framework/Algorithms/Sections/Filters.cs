using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace Algorithms.Sections
{
    public class Filters
    {
        #region Rectangular Filter
        public static Image<Gray, byte> RectangularFilter(Image<Gray, byte> input, double[,] mask)
        {
            Image<Gray, byte> result = input.Clone();
            int h = mask.GetLength(0);
            int w = mask.GetLength(1);
            for (int y = 0 + h/2; y < result.Height - h/2; y++)
            {
                for (int x = 0 + w/2; x < result.Width - w/2; x++)
                {
                    double sum = 0;
                    for (int i = -h / 2; i <= h / 2; i++)
                    {
                        for (int j = -w / 2; j <= w / 2; j++)
                        {
                            sum += mask[i + h / 2, j + w / 2] * input.Data[y + i, x + j, 0];
                        }
                    }
                    result.Data[y, x, 0] = (byte)Math.Min(Math.Max(Math.Round(sum), 0), 255);
                }
            }
            return result;
        }

        #endregion

    }
}