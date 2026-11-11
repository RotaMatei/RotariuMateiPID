using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Runtime.Remoting.Messaging;

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

        public static Image<TColor, byte> ApplySeparableFilter_X<TColor>(Image<TColor, byte> inputImage, double[] v)
            where TColor : struct, IColor
        {
            int l = v.Length;
            double[,] rowMask = new double[1, l];
            for (int k = 0; k < l; ++k)
            {
                rowMask[0, k] = v[k];
            }

            using (Image<TColor, double> image = inputImage.Convert<TColor, double>())
            using (Image<TColor, double> rez = ApplyFilter(image, rowMask))
            {
                Image<TColor, byte> result = new Image<TColor, byte>(inputImage.Size);
                for (int y = 0; y < result.Height; ++y)
                {
                    for (int x = 0; x < result.Width; ++x)
                    {
                        for (int channel = 0; channel < result.NumberOfChannels; ++channel)
                        {
                            result.Data[y, x, channel] =
                                (byte)Math.Max(0, Math.Min(255, rez.Data[y, x, channel] + 0.5));
                        }
                    }
                }
                return result;
            }
        }

        public static Image<TColor, byte> ApplySeparableFilter_Y<TColor>(Image<TColor, byte> inputImage, double[] v)
            where TColor : struct, IColor
        {
            int l = v.Length;
            double[,] columnMask = new double[l, 1];
            for (int k = 0; k < l; ++k)
            {
                columnMask[k, 0] = v[k];
            }

            using (Image<TColor, double> image = inputImage.Convert<TColor, double>())
            using (Image<TColor, double> rez = ApplyFilter(image, columnMask))
            {
                Image<TColor, byte> result = new Image<TColor, byte>(inputImage.Size);
                for (int y = 0; y < result.Height; ++y)
                {
                    for (int x = 0; x < result.Width; ++x)
                    {
                        for (int channel = 0; channel < result.NumberOfChannels; ++channel)
                        {
                            result.Data[y, x, channel] =
                                (byte)Math.Max(0, Math.Min(255, rez.Data[y, x, channel] + 0.5));
                        }
                    }
                }
                return result;
            }
        }
        #endregion

        #region Gauss Mask
        public static double[] GaussMask (double sigma)
        {
            int l = (int)Math.Ceiling(4 * sigma);
            double[] mask = new double[l + 1];
            if (l % 2 == 0) l++;
            double sum = 0;
            for (int r = -l/2; r<= l/2; ++r)
            {
                double value = 1/ (Math.Sqrt(2 * Math.PI) * sigma) * Math.Exp(-r * r / (2 * sigma * sigma));
                mask[r + l/2] = value;
                sum += value;
            }
            if(sum!=0)
            {
                for (int i = 0; i < l; ++i)
                {
                    mask[i] /= sum;
                }
            }
            return mask;
        }


        #endregion

        #region Gauss Filtering
        public static Image<Gray, byte> GaussFiltering(Image<Gray, byte> inputImage, double sigma_x, double sigma_y)
        {
            Image<Gray, byte> resultImage = inputImage.CopyBlank();
            double[] mask_x = GaussMask(sigma_x);
            resultImage = ApplySeparableFilter(inputImage, mask_x);
            double[] mask_y = GaussMask(sigma_y);
            resultImage = ApplySeparableFilter(resultImage, mask_y);
            return resultImage;
        }

        public static Image<Bgr, byte> GaussFiltering(Image<Bgr, byte> inputImage, double sigma_x, double sigma_y)
        {
            Image<Bgr, byte> resultImage = inputImage.CopyBlank();
            double[] mask_x = GaussMask(sigma_x);
            resultImage = ApplySeparableFilter_X(inputImage, mask_x);
            double[] mask_y = GaussMask(sigma_y);
            resultImage = ApplySeparableFilter_Y(resultImage, mask_y);
            return resultImage;
        }
        #endregion

        #region Median
        public static byte Median(int[] histogram, int l)
        {
            double n = Math.Round(Math.Pow(l, 2));
            int sum = 0;
            for(int i = 0; i < histogram.Length; i++)
            {
                sum += histogram[i];
                if (sum >= n / 2)
                {
                    return (byte)i;
                }
            }
            return 0;
        }

        public static Image<Gray, byte> MedianFiltering(Image<Gray, byte> inputImage, int l)
        {
            if(l % 2 == 0) l++;
            int r = l/2;
            Image<Gray, byte> resultImage = inputImage.CopyBlank();
            int[] histogram = new int[256];
            Image<Gray, byte> borderImage = new Image<Gray, byte>(inputImage.Width + 2 * r, inputImage.Height + 2 * r);
            CvInvoke.CopyMakeBorder(inputImage, borderImage, r, r, r, r,
                Emgu.CV.CvEnum.BorderType.Reflect101);

            for(int y = r; y < borderImage.Height - r; y++)
            {
                histogram = new int[256];
                int x = r;
                for (int i = -r; i <= r; i++)
                {
                    for (int j = -r; j <= r; j++)
                    {
                        histogram[borderImage.Data[y + i, x + j, 0]]++;
                    }
                }

                resultImage[y-r, x-r] = new Gray(Median(histogram, l));
                for(x = r + 1; x < borderImage.Width - r; x++)
                {
                    for (int i = -r; i <= r; i++)
                    {
                        histogram[borderImage.Data[y + i, x - r - 1, 0]]--;
                        histogram[borderImage.Data[y + i, x + r, 0]]++;
                    }
                    resultImage[y - r, x - r] = new Gray(Median(histogram, l));
                }
            }
            return resultImage;
        }
        #endregion

        #region Laplace
        public static Image<Gray, byte> LaplaceClamp(Image<Gray, byte> inputImage, double[,] filter)
        {
            Image<Gray, byte> borderImage = new Image<Gray, byte>(inputImage.Width + 2 * 1, inputImage.Height + 2 * 1);
            CvInvoke.CopyMakeBorder(inputImage, borderImage, 1, 1, 1, 1,
                Emgu.CV.CvEnum.BorderType.Reflect101);
            Image<Gray, double> result = GaussFiltering(borderImage, 1, 1).Convert<Gray, double>();
            result = ApplyFilter(result, filter);
            for(int y = 0; y < result.Height; y++)
            {
                for (int x = 0; x < result.Width; x++)
                {
                    result.Data[y, x, 0] += 128;
                    result.Data[y, x, 0] = Math.Max(0, Math.Min(255, result.Data[y, x, 0]));
                }
            }
            return result.Convert<Gray, byte>();
        }

        public static Image<Gray, byte> LaplaceZeroCrossing(Image<Gray, byte> inputImage, double[,] filter, int T)
        {
            int[,] zeroCrossingMaskA1 =
            {
                { 0, -1 },
                { 1, 0 },
                { -1, 0 },
                { 0, - 1 }
            };
            int[,] zeroCrossingMaskA2 =
            {
                { -1, -1 },
                { 1, 1 },
                { 1, -1 },
                { -1, 1 }
            };
            int[,] zeroCrossingMaskA3 =
            {
                { -1, 0 },
                { 0, 1 },
                { 0, -1 },
                { -1, 0 }
            };

            Image<Gray, byte> borderImage = new Image<Gray, byte>(inputImage.Width + 2 * 1, inputImage.Height + 2 * 1);
            CvInvoke.CopyMakeBorder(inputImage, borderImage, 1, 1, 1, 1,
                Emgu.CV.CvEnum.BorderType.Reflect101);
            Image<Gray, double> result = GaussFiltering(borderImage, 1, 1).Convert<Gray, double>();
            result = ApplyFilter(result, filter);
            for (int y = 1; y < result.Height -1; y++)
            {
                for (int x = 1; x < result.Width -1; x++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if ((((result.Data[y + zeroCrossingMaskA1[i, 0], x + zeroCrossingMaskA1[i, 1], 0] > T) &&
                                (result.Data[y - zeroCrossingMaskA1[i, 0], x - zeroCrossingMaskA1[i, 1], 0] < T))) ||
                                (((result.Data[y + zeroCrossingMaskA2[i, 0], x + zeroCrossingMaskA2[i, 1], 0] > T) &&
                                (result.Data[y - zeroCrossingMaskA2[i, 0], x - zeroCrossingMaskA2[i, 1], 0] < T))) ||
                                (((result.Data[y + zeroCrossingMaskA3[i, 0], x + zeroCrossingMaskA3[i, 1], 0] > T) &&
                                (result.Data[y - zeroCrossingMaskA3[i, 0], x - zeroCrossingMaskA3[i, 1], 0] < T))))
                        {
                            result.Data[y, x, 0] = 255;
                        }
                    }
                }
            }
            return result.Convert<Gray, byte>();
        }
        #endregion
    }
}
