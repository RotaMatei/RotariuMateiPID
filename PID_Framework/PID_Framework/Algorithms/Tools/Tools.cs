using Emgu.CV;
using Emgu.CV.Structure;

namespace Algorithms.Tools
{
    public class Tools
    {
        #region Copy
        public static Image<Gray, byte> Copy(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = inputImage.Clone();
            return result;
        }

        public static Image<Bgr, byte> Copy(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = inputImage.Clone();
            return result;
        }
        #endregion

        #region Invert
        public static Image<Gray, byte> Invert(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[y, x, 0] = (byte)(255 - inputImage.Data[y, x, 0]);
                }
            }
            return result;
        }

        public static Image<Bgr, byte> Invert(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[y, x, 0] = (byte)(255 - inputImage.Data[y, x, 0]);
                    result.Data[y, x, 1] = (byte)(255 - inputImage.Data[y, x, 1]);
                    result.Data[y, x, 2] = (byte)(255 - inputImage.Data[y, x, 2]);
                }
            }
            return result;
        }
        #endregion

        #region Convert color image to grayscale image
        public static Image<Gray, byte> Convert(Image<Bgr, byte> inputImage)
        {
            Image<Gray, byte> result = inputImage.Convert<Gray, byte>();
            return result;
        }
        #endregion

        #region Binary
        public static Image<Gray, byte> Binary(Image<Gray, byte> inputImage, byte threshold = 128)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    if (inputImage.Data[y, x, 0] >= threshold) result.Data[y, x, 0] = 255;
                }
            }
            return result;
        }
        #endregion

        #region Mirror
        public static Image<Gray, byte> Mirror(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[y, inputImage.Width - 1 - x, 0] = inputImage.Data[y, x, 0];
                }
            }
            return result;
        }

        public static Image<Bgr, byte> Mirror(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[y, inputImage.Width - 1 - x, 0] = inputImage.Data[y, x, 0];
                    result.Data[y, inputImage.Width - 1 - x, 1] = inputImage.Data[y, x, 1];
                    result.Data[y, inputImage.Width - 1 - x, 2] = inputImage.Data[y, x, 2];
                }
            }
            return result;
        }
        #endregion

        #region Rotate
        public static Image<Gray, byte> RotateClockwise(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Height, inputImage.Width);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[x, inputImage.Height - 1 - y, 0] = inputImage.Data[y, x, 0];
                }
            }
            return result;
        }

        public static Image<Bgr, byte> RotateClockwise(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Height, inputImage.Width);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[x, inputImage.Height - 1 - y, 0] = inputImage.Data[y, x, 0];
                    result.Data[x, inputImage.Height - 1 - y, 1] = inputImage.Data[y, x, 1];
                    result.Data[x, inputImage.Height - 1 - y, 2] = inputImage.Data[y, x, 2];
                }
            }
            return result;
        }

        public static Image<Gray, byte> RotateAntiClockwise(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Height, inputImage.Width);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[inputImage.Width - 1 - x, y, 0] = inputImage.Data[y, x, 0];
                }
            }
            return result;
        }

        public static Image<Bgr, byte> RotateAntiClockwise(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Height, inputImage.Width);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    result.Data[inputImage.Width - 1 - x, y, 0] = inputImage.Data[y, x, 0];
                    result.Data[inputImage.Width - 1 - x, y, 1] = inputImage.Data[y, x, 1];
                    result.Data[inputImage.Width - 1 - x, y, 2] = inputImage.Data[y, x, 2];
                }
            }
            return result;
        }
        #endregion

        #region Crop
        // The corners are inclusive: (left, top) and (right, bottom) are both part of the result
        public static Image<Gray, byte> Crop(Image<Gray, byte> inputImage, int left, int top, int right, int bottom)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(right - left + 1, bottom - top + 1);

            for (int y = top; y <= bottom; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    result.Data[y - top, x - left, 0] = inputImage.Data[y, x, 0];
                }
            }
            return result;
        }

        public static Image<Bgr, byte> Crop(Image<Bgr, byte> inputImage, int left, int top, int right, int bottom)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(right - left + 1, bottom - top + 1);

            for (int y = top; y <= bottom; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    result.Data[y - top, x - left, 0] = inputImage.Data[y, x, 0];
                    result.Data[y - top, x - left, 1] = inputImage.Data[y, x, 1];
                    result.Data[y - top, x - left, 2] = inputImage.Data[y, x, 2];
                }
            }
            return result;
        }
        #endregion

        #region Mean and standard deviation
        // One value per channel (Gray: [0]; Bgr: [0] = B, [1] = G, [2] = R)
        public static void MeanAndStandardDeviation<TColor>(
            Image<TColor, byte> inputImage, out double[] mean, out double[] standardDeviation)
            where TColor : struct, IColor
        {
            int channels = inputImage.NumberOfChannels;
            double pixels = (double)inputImage.Width * inputImage.Height;

            mean = new double[channels];
            standardDeviation = new double[channels];

            for (int c = 0; c < channels; ++c)
            {
                double sum = 0;
                for (int y = 0; y < inputImage.Height; ++y)
                {
                    for (int x = 0; x < inputImage.Width; ++x)
                    {
                        sum += inputImage.Data[y, x, c];
                    }
                }
                mean[c] = sum / pixels;

                double squaredDifferences = 0;
                for (int y = 0; y < inputImage.Height; ++y)
                {
                    for (int x = 0; x < inputImage.Width; ++x)
                    {
                        double difference = inputImage.Data[y, x, c] - mean[c];
                        squaredDifferences += difference * difference;
                    }
                }
                standardDeviation[c] = System.Math.Sqrt(squaredDifferences / pixels);
            }
        }
        #endregion
    }
}