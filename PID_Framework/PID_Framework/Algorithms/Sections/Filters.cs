using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace Algorithms.Sections
{
    public class Filters
    {
        #region Apply filter
        // The image is bordered by mirroring, without doubling the first/last row/column (Reflect101),
        // so the result has the same size as the input image and the edge pixels are filtered too
        public static Image<TColor, TDepth> ApplyFilter<TColor, TDepth>(Image<TColor, TDepth> inputImage, double[,] mask)
            where TColor : struct, IColor
            where TDepth : new()
        {
            Image<TColor, TDepth> resultImage = new Image<TColor, TDepth>(inputImage.Size);

            int h = mask.GetLength(0);
            int w = mask.GetLength(1);
            int h_offset = h / 2, w_offset = w / 2;

            var borderImage = new Image<TColor, TDepth>(inputImage.Width + 2 * w_offset, inputImage.Height + 2 * h_offset);
            CvInvoke.CopyMakeBorder(inputImage, borderImage, h_offset, h_offset, w_offset, w_offset,
                Emgu.CV.CvEnum.BorderType.Reflect101);

            for (int y = h_offset; y < borderImage.Height - h_offset; ++y)
            {
                for (int x = w_offset; x < borderImage.Width - w_offset; ++x)
                {
                    for (int channel = 0; channel < borderImage.NumberOfChannels; ++channel)
                    {
                        double sum = 0.0;
                        for (int i = -h_offset; i <= h_offset; i++)
                        {
                            for (int j = -w_offset; j <= w_offset; j++)
                            {
                                double pixel = Convert.ToDouble(borderImage.Data[y + i, x + j, channel]);
                                sum += mask[i + h_offset, j + w_offset] * pixel;
                            }
                        }

                        if (typeof(TDepth) == typeof(byte))
                        {
                            byte clampedSum = (byte)Math.Max(0, Math.Min(255, sum + 0.5));
                            resultImage.Data[y - h_offset, x - w_offset, channel] =
                                (TDepth)Convert.ChangeType(clampedSum, typeof(TDepth));
                        }
                        else
                        {
                            resultImage.Data[y - h_offset, x - w_offset, channel] =
                                (TDepth)Convert.ChangeType(sum, typeof(TDepth));
                        }
                    }
                }
            }

            return resultImage;
        }
        #endregion

        #region Apply separable filter
        // The image is filtered with v (horizontally), then the result is filtered with v^T (vertically).
        // The intermediate result is kept as double, so it is not rounded between the two passes.
        public static Image<TColor, byte> ApplySeparableFilter<TColor>(Image<TColor, byte> inputImage, double[] v)
            where TColor : struct, IColor
        {
            int l = v.Length;
            double[,] rowMask = new double[1, l];
            double[,] columnMask = new double[l, 1];
            for (int k = 0; k < l; ++k)
            {
                rowMask[0, k] = v[k];
                columnMask[k, 0] = v[k];
            }

            using (Image<TColor, double> image = inputImage.Convert<TColor, double>())
            using (Image<TColor, double> rez = ApplyFilter(image, rowMask))
            using (Image<TColor, double> rez2 = ApplyFilter(rez, columnMask))
            {
                Image<TColor, byte> result = new Image<TColor, byte>(inputImage.Size);
                for (int y = 0; y < result.Height; ++y)
                {
                    for (int x = 0; x < result.Width; ++x)
                    {
                        for (int channel = 0; channel < result.NumberOfChannels; ++channel)
                        {
                            result.Data[y, x, channel] =
                                (byte)Math.Max(0, Math.Min(255, rez2.Data[y, x, channel] + 0.5));
                        }
                    }
                }
                return result;
            }
        }
        #endregion
    }
}
