using Emgu.CV;
using Emgu.CV.Structure;

namespace Algorithms.Sections
{
    public class MorphologicalOperations
    {
        // For all operations: the input is a binary image (0 / 255), the structuring element is an
        // h x w rectangle centred on the current pixel (even sizes are incremented by 1), and
        // option = 1 processes the white objects, while option = 0 processes the black objects.

        #region Dilation
        public static Image<Gray, byte> Dilation(Image<Gray, byte> inputImage, int h, int w, int option = 1)
        {
            // White objects: a pixel becomes white if there is at least one white pixel under the mask.
            // Black objects: a pixel becomes black if there is at least one black pixel under the mask.
            byte objectColor = (byte)(option == 1 ? 255 : 0);
            return SetIfAnyNeighbor(inputImage, h, w, objectColor);
        }
        #endregion

        #region Erosion
        public static Image<Gray, byte> Erosion(Image<Gray, byte> inputImage, int h, int w, int option = 1)
        {
            // White objects: a pixel becomes black if there is at least one black pixel under the mask.
            // Black objects: a pixel becomes white if there is at least one white pixel under the mask.
            byte backgroundColor = (byte)(option == 1 ? 0 : 255);
            return SetIfAnyNeighbor(inputImage, h, w, backgroundColor);
        }
        #endregion

        #region Opening
        public static Image<Gray, byte> Opening(Image<Gray, byte> inputImage, int h, int w, int option = 1)
        {
            using (Image<Gray, byte> erodedImage = Erosion(inputImage, h, w, option))
                return Dilation(erodedImage, h, w, option);
        }
        #endregion

        #region Closing
        public static Image<Gray, byte> Closing(Image<Gray, byte> inputImage, int h, int w, int option = 1)
        {
            using (Image<Gray, byte> dilatedImage = Dilation(inputImage, h, w, option))
                return Erosion(dilatedImage, h, w, option);
        }
        #endregion

        #region Helpers
        // The pixel (x, y) becomes 'color' if at least one pixel under the mask centred on (x, y) has
        // that color, otherwise it gets the opposite color. Mask positions outside the image are ignored.
        private static Image<Gray, byte> SetIfAnyNeighbor(Image<Gray, byte> inputImage, int h, int w, byte color)
        {
            if (h % 2 == 0) ++h;
            if (w % 2 == 0) ++w;
            int h_offset = h / 2, w_offset = w / 2;
            byte oppositeColor = (byte)(255 - color);

            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    bool found = false;
                    for (int i = -h_offset; i <= h_offset && !found; ++i)
                    {
                        int yy = y + i;
                        if (yy < 0 || yy >= inputImage.Height)
                            continue;

                        for (int j = -w_offset; j <= w_offset; ++j)
                        {
                            int xx = x + j;
                            if (xx < 0 || xx >= inputImage.Width)
                                continue;

                            if (inputImage.Data[yy, xx, 0] == color)
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    result.Data[y, x, 0] = found ? color : oppositeColor;
                }
            }

            return result;
        }
        #endregion
    }
}
