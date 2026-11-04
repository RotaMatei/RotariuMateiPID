using Emgu.CV;
using Emgu.CV.Structure;

using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using PointCollection = System.Windows.Media.PointCollection;

using Framework.View;
using Framework.Model;
using Framework.Services;
using Framework.ViewModel.ColorLevels;
using static Framework.Utilities.DrawingHelper;
using static Framework.Utilities.FileHelper;

using Algorithms.Sections;
using Algorithms.Tools;
using Algorithms.Utilities;

namespace Framework.ViewModel.Commands
{
    public class MenuCommands : BaseVM
    {
        private readonly MainVM _mainVM;
        private readonly IImageStore _imageStore;
        private readonly IWindowService _windowService;

        public MenuCommands(MainVM mainVM, IImageStore imageStore, IWindowService windowService)
        {
            _mainVM = mainVM;
            _imageStore = imageStore;
            _windowService = windowService;
        }

        #region Properties
        private Image<Gray, byte> GrayInitialImage
        {
            get => _imageStore.GrayInitialImage;
            set => _imageStore.GrayInitialImage = value;
        }

        private Image<Gray, byte> GrayProcessedImage
        {
            get => _imageStore.GrayProcessedImage;
            set => _imageStore.GrayProcessedImage = value;
        }

        private Image<Bgr, byte> ColorInitialImage
        {
            get => _imageStore.ColorInitialImage;
            set => _imageStore.ColorInitialImage = value;
        }

        private Image<Bgr, byte> ColorProcessedImage
        {
            get => _imageStore.ColorProcessedImage;
            set => _imageStore.ColorProcessedImage = value;
        }

        private PointCollection MouseClickCollection
        {
            get => _imageStore.MouseClickCollection;
        }

        private Point LastMouseClick
        {
            get => _imageStore.LastMouseClick;
        }

        private double ScaleValue
        {
            get => _mainVM.ScaleValue;
            set => _mainVM.ScaleValue = value;
        }
        #endregion

        #region File

        #region Load grayscale image
        private RelayCommand _loadGrayscaleImageCommand;
        public RelayCommand LoadGrayscaleImageCommand
        {
            get
            {
                if (_loadGrayscaleImageCommand == null)
                    _loadGrayscaleImageCommand = new RelayCommand(LoadGrayscaleImage);
                return _loadGrayscaleImageCommand;
            }
        }

        private void LoadGrayscaleImage(object parameter)
        {
            var image = AskForImage<Gray>("Select a grayscale picture");
            if (image != null)
            {
                Clear(parameter);
                GrayInitialImage = image;
            }
        }
        #endregion

        #region Load color image
        private ICommand _loadColorImageCommand;
        public ICommand LoadColorImageCommand
        {
            get
            {
                if (_loadColorImageCommand == null)
                    _loadColorImageCommand = new RelayCommand(LoadColorImage);
                return _loadColorImageCommand;
            }
        }

        private void LoadColorImage(object parameter)
        {
            var image = AskForImage<Bgr>("Select a color picture");
            if (image != null)
            {
                Clear(parameter);
                ColorInitialImage = image;
            }
        }
        #endregion

        #region Save processed image
        private ICommand _saveProcessedImageCommand;
        public ICommand SaveProcessedImageCommand
        {
            get
            {
                if (_saveProcessedImageCommand == null)
                    _saveProcessedImageCommand = new RelayCommand(SaveProcessedImage);
                return _saveProcessedImageCommand;
            }
        }

        private void SaveProcessedImage(object parameter)
        {
            if (GrayProcessedImage == null && ColorProcessedImage == null)
            {
                MessageBox.Show("If you want to save your processed image, " +
                    "please load and process an image first!");
                return;
            }

            string imagePath = SaveFileDialog("image.jpg");
            if (imagePath != null)
            {
                if (GrayProcessedImage != null)
                    SaveImage(GrayProcessedImage, imagePath);
                else
                    SaveImage(ColorProcessedImage, imagePath);
                Process.Start(imagePath);
            }
        }
        #endregion

        #region Exit
        private ICommand _exitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                    _exitCommand = new RelayCommand(Exit);
                return _exitCommand;
            }
        }

        private void Exit(object parameter)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #endregion

        #region Edit

        #region Remove drawn shapes
        private ICommand _removeDrawnShapesCommand;
        public ICommand RemoveDrawnShapesCommand
        {
            get
            {
                if (_removeDrawnShapesCommand == null)
                    _removeDrawnShapesCommand = new RelayCommand(RemoveDrawnShapes);
                return _removeDrawnShapesCommand;
            }
        }

        private void RemoveDrawnShapes(object parameter)
        {
            foreach (var canvas in parameter as object[] ?? new[] { parameter })
                RemoveUiElements(canvas as Canvas);
        }
        #endregion

        #region Clear initial canvas
        private ICommand _clearInitialCanvasCommand;
        public ICommand ClearInitialCanvasCommand
        {
            get
            {
                if (_clearInitialCanvasCommand == null)
                    _clearInitialCanvasCommand = new RelayCommand(ClearInitialCanvas);
                return _clearInitialCanvasCommand;
            }
        }

        private void ClearInitialCanvas(object parameter)
        {
            RemoveUiElements(parameter as Canvas);

            _imageStore.ClearInitial();
        }
        #endregion

        #region Clear processed canvas
        private ICommand _clearProcessedCanvasCommand;
        public ICommand ClearProcessedCanvasCommand
        {
            get
            {
                if (_clearProcessedCanvasCommand == null)
                    _clearProcessedCanvasCommand = new RelayCommand(ClearProcessedCanvas);
                return _clearProcessedCanvasCommand;
            }
        }

        private void ClearProcessedCanvas(object parameter)
        {
            RemoveUiElements(parameter as Canvas);

            _imageStore.ClearProcessed();
        }
        #endregion

        #region Closing all open windows and clear both canvases
        private ICommand _clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                    _clearCommand = new RelayCommand(Clear);
                return _clearCommand;
            }
        }

        private void Clear(object parameter)
        {
            _windowService.CloseAllChildWindows();

            ScaleValue = 1;

            var canvases = (object[])parameter;
            ClearInitialCanvas(canvases[0]);
            ClearProcessedCanvas(canvases[1]);
        }
        #endregion

        #endregion

        #region Tools

        #region Magnifier
        private ICommand _magnifierCommand;
        public ICommand MagnifierCommand
        {
            get
            {
                if (_magnifierCommand == null)
                    _magnifierCommand = new RelayCommand(Magnifier);
                return _magnifierCommand;
            }
        }

        private void Magnifier(object parameter)
        {
            if (_windowService.IsMagnifierOpen || IsAreaSelected())
                _windowService.ShowMagnifier();
        }
        #endregion

        #region Visualize color levels

        #region Row color levels
        private ICommand _rowColorLevelsCommand;
        public ICommand RowColorLevelsCommand
        {
            get
            {
                if (_rowColorLevelsCommand == null)
                    _rowColorLevelsCommand = new RelayCommand(RowColorLevels);
                return _rowColorLevelsCommand;
            }
        }

        private void RowColorLevels(object parameter)
        {
            if (_windowService.IsRowColorLevelsOpen || IsAreaSelected())
                _windowService.ShowColorLevels(ColorLevelsType.Row);
        }
        #endregion

        #region Column color levels
        private ICommand _columnColorLevelsCommand;
        public ICommand ColumnColorLevelsCommand
        {
            get
            {
                if (_columnColorLevelsCommand == null)
                    _columnColorLevelsCommand = new RelayCommand(ColumnColorLevels);
                return _columnColorLevelsCommand;
            }
        }

        private void ColumnColorLevels(object parameter)
        {
            if (_windowService.IsColumnColorLevelsOpen || IsAreaSelected())
                _windowService.ShowColorLevels(ColorLevelsType.Column);
        }
        #endregion

        #endregion

        #region Visualize image histogram

        #region Initial image histogram
        private ICommand _histogramInitialImageCommand;
        public ICommand HistogramInitialImageCommand
        {
            get
            {
                if (_histogramInitialImageCommand == null)
                    _histogramInitialImageCommand = new RelayCommand(HistogramInitialImage);
                return _histogramInitialImageCommand;
            }
        }

        private void HistogramInitialImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            _windowService.ShowHistogram(ImageCanvasType.Initial);
        }
        #endregion

        #region Processed image histogram
        private ICommand _histogramProcessedImageCommand;
        public ICommand HistogramProcessedImageCommand
        {
            get
            {
                if (_histogramProcessedImageCommand == null)
                    _histogramProcessedImageCommand = new RelayCommand(HistogramProcessedImage);
                return _histogramProcessedImageCommand;
            }
        }

        private void HistogramProcessedImage(object parameter)
        {
            if (!IsImageProcessed())
                return;

            _windowService.ShowHistogram(ImageCanvasType.Processed);
        }
        #endregion

        #region Initial image normalized histogram
        private ICommand _normalizedHistogramInitialImageCommand;
        public ICommand NormalizedHistogramInitialImageCommand
        {
            get
            {
                if (_normalizedHistogramInitialImageCommand == null)
                    _normalizedHistogramInitialImageCommand = new RelayCommand(NormalizedHistogramInitialImage);
                return _normalizedHistogramInitialImageCommand;
            }
        }

        private void NormalizedHistogramInitialImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            var histograms = new List<(double[] Histogram, string PlotColor)>();
            string title;

            if (GrayInitialImage != null)
            {
                title = "Grayscale - Initial Image Normalized Histogram";
                histograms.Add((Utils.ComputeNormalizedHistogram(GrayInitialImage), Brushes.Black.ToString()));
            }
            else
            {
                title = "Color - Initial Image Normalized Histogram";
                string[] colors = { Brushes.Blue.ToString(), Brushes.Green.ToString(), Brushes.Red.ToString() };
                for (int channel = 0; channel < 3; ++channel)
                {
                    using (Image<Gray, byte> channelImage = ColorInitialImage[channel])
                        histograms.Add((Utils.ComputeNormalizedHistogram(channelImage), colors[channel]));
                }
            }

            new HistogramWindow(_mainVM, title, histograms).Show();
        }
        #endregion

        #endregion

        #region Copy image
        private ICommand _copyImageCommand;
        public ICommand CopyImageCommand
        {
            get
            {
                if (_copyImageCommand == null)
                    _copyImageCommand = new RelayCommand(CopyImage);
                return _copyImageCommand;
            }
        }

        private void CopyImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Copy(ColorInitialImage);
            }
            else if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Copy(GrayInitialImage);
            }
        }
        #endregion

        #region Invert image
        private ICommand _invertImageCommand;
        public ICommand InvertImageCommand
        {
            get
            {
                if (_invertImageCommand == null)
                    _invertImageCommand = new RelayCommand(InvertImage);
                return _invertImageCommand;
            }
        }

        private void InvertImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Invert(GrayInitialImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Invert(ColorInitialImage);
            }
        }
        #endregion

        #region Convert color image to grayscale image
        private ICommand _convertImageToGrayscaleCommand;
        public ICommand ConvertImageToGrayscaleCommand
        {
            get
            {
                if (_convertImageToGrayscaleCommand == null)
                    _convertImageToGrayscaleCommand = new RelayCommand(ConvertImageToGrayscale);
                return _convertImageToGrayscaleCommand;
            }
        }

        private void ConvertImageToGrayscale(object parameter)
        {
            if (!IsImageLoaded())
                return;

            if (GrayInitialImage != null)
            {
                MessageBox.Show("It is possible to convert only color images!");
                return;
            }

            ClearProcessedCanvas(parameter);

            GrayProcessedImage = Tools.Convert(ColorInitialImage);
        }
        #endregion

        #region Binary image
        private ICommand _binaryImageCommand;
        public ICommand BinaryImageCommand
        {
            get
            {
                if (_binaryImageCommand == null)
                    _binaryImageCommand = new RelayCommand(BinaryImage);
                return _binaryImageCommand;
            }
        }

        private void BinaryImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            List<string> labels = new List<string>
            {
                "Threshold: ",
            };
            DialogWindow window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();
            List<double> values = window.GetValues();
            if (values == null)
                return;
            byte threshold = (byte)values[0];
            if (threshold > 145) threshold = 145;
            if (threshold < 10) threshold = 10;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.ApplyLUT(GrayInitialImage, (byte)threshold);
            }
            else
            {
                var grayImage = Tools.Convert(ColorInitialImage);
                GrayProcessedImage = Tools.ApplyLUT(grayImage, (byte)threshold);
            }
        }
        #endregion

        #region Mirror image
        private ICommand _mirrorImageCommand;
        public ICommand MirrorImageCommand
        {
            get
            {
                if (_mirrorImageCommand == null)
                    _mirrorImageCommand = new RelayCommand(MirrorImage);
                return _mirrorImageCommand;
            }
        }

        private void MirrorImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Mirror(GrayInitialImage);
            }
            else
            {
                ColorProcessedImage = Tools.Mirror(ColorInitialImage);
            }
        }
        #endregion

        #region Rotate image

        #region Rotate clockwise
        private ICommand _rotateClockwiseCommand;
        public ICommand RotateClockwiseCommand
        {
            get
            {
                if (_rotateClockwiseCommand == null)
                    _rotateClockwiseCommand = new RelayCommand(RotateClockwise);
                return _rotateClockwiseCommand;
            }
        }

        private void RotateClockwise(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.RotateClockwise(GrayInitialImage);
            }
            else
            {
                ColorProcessedImage = Tools.RotateClockwise(ColorInitialImage);
            }
        }
        #endregion

        #region Rotate anti-clockwise
        private ICommand _rotateAntiClockwiseCommand;
        public ICommand RotateAntiClockwiseCommand
        {
            get
            {
                if (_rotateAntiClockwiseCommand == null)
                    _rotateAntiClockwiseCommand = new RelayCommand(RotateAntiClockwise);
                return _rotateAntiClockwiseCommand;
            }
        }

        private void RotateAntiClockwise(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.RotateAntiClockwise(GrayInitialImage);
            }
            else
            {
                ColorProcessedImage = Tools.RotateAntiClockwise(ColorInitialImage);
            }
        }
        #endregion

        #endregion

        #region Crop image
        private System.Windows.Shapes.Rectangle _cropRectangle;

        private ICommand _cropImageCommand;
        public ICommand CropImageCommand
        {
            get
            {
                if (_cropImageCommand == null)
                    _cropImageCommand = new RelayCommand(CropImage);
                return _cropImageCommand;
            }
        }

        private void CropImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            if (MouseClickCollection.Count < 2)
            {
                MessageBox.Show("Please select the area first, by clicking on two opposite corners of it!");
                return;
            }

            var canvases = (object[])parameter;
            var initialCanvas = canvases[0] as Canvas;

            int imageWidth = GrayInitialImage?.Width ?? ColorInitialImage.Width;
            int imageHeight = GrayInitialImage?.Height ?? ColorInitialImage.Height;

            // The selected area is given by the last two clicks, in any order
            Point firstClick = MouseClickCollection[MouseClickCollection.Count - 2];
            Point secondClick = MouseClickCollection[MouseClickCollection.Count - 1];

            int left = ClampToRange((int)Math.Min(firstClick.X, secondClick.X), imageWidth - 1);
            int top = ClampToRange((int)Math.Min(firstClick.Y, secondClick.Y), imageHeight - 1);
            int right = ClampToRange((int)Math.Max(firstClick.X, secondClick.X), imageWidth - 1);
            int bottom = ClampToRange((int)Math.Max(firstClick.Y, secondClick.Y), imageHeight - 1);

            RemoveUiElement(initialCanvas, _cropRectangle);
            _cropRectangle = DrawRectangle(initialCanvas, new Point(left, top), new Point(right + 1, bottom + 1),
                1, Brushes.Red, ScaleValue);

            ClearProcessedCanvas(canvases[1]);

            double[] mean, standardDeviation;
            string statistics;
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Crop(GrayInitialImage, left, top, right, bottom);
                Tools.MeanAndStandardDeviation(GrayProcessedImage, out mean, out standardDeviation);

                statistics = $"Mean: {mean[0]:F2}\nStandard deviation: {standardDeviation[0]:F2}";
            }
            else
            {
                ColorProcessedImage = Tools.Crop(ColorInitialImage, left, top, right, bottom);
                Tools.MeanAndStandardDeviation(ColorProcessedImage, out mean, out standardDeviation);

                statistics =
                    $"Mean: B = {mean[0]:F2}, G = {mean[1]:F2}, R = {mean[2]:F2}\n" +
                    $"Standard deviation: B = {standardDeviation[0]:F2}, G = {standardDeviation[1]:F2}, " +
                    $"R = {standardDeviation[2]:F2}";
            }

            MessageBox.Show(
                $"Top-left corner: ({left}, {top})\nBottom-right corner: ({right}, {bottom})\n\n{statistics}",
                "Cropped area");
        }

        private static int ClampToRange(int value, int maximum)
        {
            return Math.Max(0, Math.Min(value, maximum));
        }
        #endregion

        #endregion

        #region Pointwise operations

        #region Gamma operator
        private ICommand _gammaOperatorCommand;
        public ICommand GammaOperatorCommand
        {
            get
            {
                if (_gammaOperatorCommand == null)
                    _gammaOperatorCommand = new RelayCommand(GammaOperator);
                return _gammaOperatorCommand;
            }
        }

        private void GammaOperator(object parameter)
        {
            if (!IsImageLoaded())
                return;

            List<string> labels = new List<string>
            {
                "gamma: "
            };
            DialogWindow window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();
            List<double> values = window.GetValues();
            if (values == null)
                return;
            double gamma = values[0];
            if (gamma <= 0)
            {
                MessageBox.Show("Gamma must be a strictly positive value!");
                return;
            }

            ClearProcessedCanvas(parameter);
            byte[] LUT = PointwiseOperations.Gamma(gamma);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = PointwiseOperations.ApplyLUT(GrayInitialImage, LUT);
            }
            else
            {
                ColorProcessedImage = PointwiseOperations.ApplyLUT(ColorInitialImage, LUT);
            }
        }
        #endregion

        #endregion

        #region Contrast and brightness 

        private ICommand _brightnessAndContrastCommand;
        public ICommand BrightnessAndContrastCommand
        {
            get
            {
                if (_brightnessAndContrastCommand == null)
                    _brightnessAndContrastCommand = new RelayCommand(BrightnessAndContrast);
                return _brightnessAndContrastCommand;
            }
        }

        private void BrightnessAndContrast(object parameter)
        {
            if (!IsImageLoaded())
                return;

            List<string> labels = new List<string>
            {
                "alpha: ",
                "beta: "
            };
            DialogWindow window = new DialogWindow(_mainVM, labels);
            window.ShowDialog();
            List<double> values = window.GetValues();
            if (values == null)
                return;
            double alpha = values[0];
            double beta = values[1];
            if (alpha <= 0) return;

            ClearProcessedCanvas(parameter);
            byte[] LUT = PointwiseOperations.ContrastAndBrightness(alpha, beta);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = PointwiseOperations.ApplyLUT(GrayInitialImage, LUT);
            }
            else
            {
                ColorProcessedImage = PointwiseOperations.ApplyLUT(ColorInitialImage, LUT);
            }
        }
        #endregion

        #region Thresholding
        private ICommand _intermeansThresholdingCommand;
        public ICommand IntermeansThresholdingCommand
        {
            get
            {
                if (_intermeansThresholdingCommand == null)
                    _intermeansThresholdingCommand = new RelayCommand(IntermeansThresholding);
                return _intermeansThresholdingCommand;
            }
        }

        private void IntermeansThresholding(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                byte threshold = Thresholding.IntermeansThresholding(GrayInitialImage);
                Console.WriteLine(threshold);
                GrayProcessedImage = Tools.ApplyLUT(GrayInitialImage, (byte)threshold);
            }
            else
            {
                var grayImage = Tools.Convert(ColorInitialImage);
                byte threshold = Thresholding.IntermeansThresholding(grayImage);
                Console.WriteLine(threshold);
                GrayProcessedImage = Tools.ApplyLUT(grayImage, (byte)threshold);
            }
        }
        #endregion

        #region Filters

        #region Custom filter

        #region Separable filter
        private ICommand _separableFilterCommand;
        public ICommand SeparableFilterCommand
        {
            get
            {
                if (_separableFilterCommand == null)
                    _separableFilterCommand = new RelayCommand(SeparableFilter);
                return _separableFilterCommand;
            }
        }

        private void SeparableFilter(object parameter)
        {
            if (!IsImageLoaded())
                return;

            double[] v = { 0.25, 0.5, 0.25 };

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Filters.ApplySeparableFilter(GrayInitialImage, v);
            }
            else
            {
                ColorProcessedImage = Filters.ApplySeparableFilter(ColorInitialImage, v);
            }
        }
        #endregion

        #region Square filter 1
        private ICommand _squareFilter1Command;
        public ICommand SquareFilter1Command
        {
            get
            {
                if (_squareFilter1Command == null)
                    _squareFilter1Command = new RelayCommand(SquareFilter1);
                return _squareFilter1Command;
            }
        }

        private void SquareFilter1(object parameter)
        {
            double[,] mask = new double[3, 3]
            {
                { 0.1, 0.1, 0.1 },
                { 0.1, 0.2, 0.1 },
                { 0.1, 0.1, 0.1 }
            };

            CustomFilter(parameter, mask);
        }
        #endregion

        #region Square filter 2
        private ICommand _squareFilter2Command;
        public ICommand SquareFilter2Command
        {
            get
            {
                if (_squareFilter2Command == null)
                    _squareFilter2Command = new RelayCommand(SquareFilter2);
                return _squareFilter2Command;
            }
        }

        private void SquareFilter2(object parameter)
        {
            double[,] mask = new double[3, 3]
            {
                {  0, -1,  0 },
                { -1,  5, -1 },
                {  0, -1,  0 }
            };

            CustomFilter(parameter, mask);
        }
        #endregion

        private void CustomFilter(object parameter, double[,] mask)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Filters.ApplyFilter(GrayInitialImage, mask);
            }
            else
            {
                ColorProcessedImage = Filters.ApplyFilter(ColorInitialImage, mask);
            }
        }

        #endregion

        #region Gauss Filtering
        private ICommand _gaussFilteringCommand;
        public ICommand GaussFilteringCommand
        {
            get
            {
                if (_gaussFilteringCommand == null)
                    _gaussFilteringCommand = new RelayCommand(GaussFiltering);
                return _gaussFilteringCommand;
            }
        }

        private void GaussFiltering(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Filters.GaussFiltering(GrayInitialImage, 3, 5);
            }
            else
            {
                ColorProcessedImage = Filters.GaussFiltering(ColorInitialImage, 3, 5);
            }
        }
        #endregion

        #region Median Filtering
        private ICommand _medianFilteringCommand;
        public ICommand MedianFilteringCommand
        {
            get
            {
                if (_medianFilteringCommand == null)
                    _medianFilteringCommand = new RelayCommand(MedianFiltering);
                return _medianFilteringCommand;
            }
        }

        private void MedianFiltering(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Filters.MedianFiltering(GrayInitialImage, 3);
            }
            else
            {
                var grayInitialImage = Tools.Convert(ColorInitialImage);
                GrayProcessedImage = Filters.MedianFiltering(grayInitialImage, 3);
            }
        }
        #endregion
        #endregion

        #region Morphological operations
        #endregion

        #region Geometric transformations
        #endregion

        #region Segmentation
        #endregion

        #region Use processed image as initial image
        private ICommand _useProcessedImageAsInitialImageCommand;
        public ICommand UseProcessedImageAsInitialImageCommand
        {
            get
            {
                if (_useProcessedImageAsInitialImageCommand == null)
                    _useProcessedImageAsInitialImageCommand = new RelayCommand(UseProcessedImageAsInitialImage);
                return _useProcessedImageAsInitialImageCommand;
            }
        }

        private void UseProcessedImageAsInitialImage(object parameter)
        {
            if (!IsImageProcessed())
                return;

            var canvases = (object[])parameter;

            ClearInitialCanvas(canvases[0]);

            if (GrayProcessedImage != null)
            {
                GrayInitialImage = GrayProcessedImage;
            }
            else if (ColorProcessedImage != null)
            {
                ColorInitialImage = ColorProcessedImage;
            }

            ClearProcessedCanvas(canvases[1]);
        }
        #endregion

        #region Helpers
        private Image<TColor, byte> AskForImage<TColor>(string dialogTitle)
            where TColor : struct, IColor
        {
            _windowService.CloseAllChildWindows();

            string fileName = LoadFileDialog(dialogTitle);
            return fileName != null ? ReadImage<TColor>(fileName) : null;
        }

        private static Image<TColor, byte> ReadImage<TColor>(string fileName)
            where TColor : struct, IColor
        {
            try
            {
                return LoadImage<TColor>(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"The image could not be opened:\n{fileName}\n\n{ex.Message}");
                return null;
            }
        }

        private bool IsImageLoaded()
        {
            if (GrayInitialImage != null || ColorInitialImage != null)
                return true;

            MessageBox.Show("Please add an image!");
            return false;
        }

        private bool IsImageProcessed()
        {
            if (GrayProcessedImage != null || ColorProcessedImage != null)
                return true;

            MessageBox.Show("Please process an image first!");
            return false;
        }

        private bool IsAreaSelected()
        {
            if (!IsImageLoaded())
                return false;

            if (MouseClickCollection.Count > 0)
                return true;

            MessageBox.Show("Please select an area first!");
            return false;
        }
        #endregion
    }
}