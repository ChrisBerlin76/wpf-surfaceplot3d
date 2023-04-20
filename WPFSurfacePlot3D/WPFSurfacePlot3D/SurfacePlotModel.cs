using HelixToolkit.Wpf;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace WPFSurfacePlot3D
{
    public enum ColorCoding
    {
        /// <summary>
        /// Color code by Z-value using a gradient brush with white ambient light
        /// </summary>
        ByValueZ,

        /// <summary>
        /// No color coding, use coloured lights
        /// </summary>
        ByLights,

        /// <summary>
        /// Color code by gradient in y-direction using a gradient brush with white ambient light
        /// </summary>
        ByGradientX,

        /// <summary>
        /// Color code by gradient in y-direction using a gradient brush with white ambient light
        /// </summary>
        ByGradientY,

        /// <summary>
        /// Color coding switched off
        /// </summary>
        NoColorCoding
    }

    public enum BrushPreset
    {
        BlueWhiteRed,

        GreenYellowRed,

        BlackWhite,

        Custom
    }

    public class SurfacePlotModel : INotifyPropertyChanged
    {
        private int defaultFunctionSampleSize = 100;

        // Brush presets
        private readonly Brush BlueWhiteRedBrush = BrushHelper.CreateGradientBrush(Colors.DarkBlue, Colors.LightBlue, Colors.White, Colors.Pink, Colors.Red);
        private readonly Brush GreenYellowRedBrush = BrushHelper.CreateGradientBrush(Colors.Green, Colors.Yellow, Colors.Red);
        private readonly Brush BlackWhiteBrush = BrushHelper.CreateGradientBrush(Color.FromRgb(20,20,20), Colors.White);

        private double _xMax, _xMin, _yMax, _yMin, _zMax, _zMin;


        // So the overall goal of this section is to output the appropriate values to SurfacePlotVisual3D - namely,
        // - DataPoints as Point3D, plus xAxisTicks (and y, z) as double[]
        // - plus all the appropriate properties, which can be directly edited/bindable by the user

        public SurfacePlotModel()
        {
            SupressUpdates = true;
            
            Title = "New Surface Plot";
            XAxisLabel = "X";
            YAxisLabel = "Y";
            ZAxisLabel = "Z";


            BrushPreset = BrushPreset.BlueWhiteRed;
            CustomSurfaceBrush = BrushHelper.CreateGradientBrush(Colors.DarkViolet, Colors.Red, Colors.Goldenrod, Colors.ForestGreen, Colors.Lime, Colors.White);
            ShowGrid = true;
            ShowAxes = true;
            ShowSurfaceMesh = true;
            ShowOrthographic = true;
            ColorCoding = ColorCoding.ByValueZ;

            SupressUpdates = false;

            // Initialize the DataPoints collection
            //Func<double, double, double> sampleFunction = (x, y) => 10 * Math.Sin(Math.Sqrt(x * x + y * y)) / Math.Sqrt(x * x + y * y);
            //PlotFunction(sampleFunction, -10, 10);
        }

        public event EventHandler VisualUpdateRequested;

        public event EventHandler ZoomToContentRequested;


        #region === Public Methods ===


        public void PlotData(double[,] zData2DArray)
        {
            XAxisTicks = null;
            YAxisTicks = null;
            PlotData2DArray(zData2DArray);
        }

        public void PlotData(double[,] zData2DArray, double xMinimum, double xMaximum, double yMinimum, double yMaximum)
        {
            int n = zData2DArray.GetLength(0);
            int m = zData2DArray.GetLength(1);
            XAxisTicks = CreateLinearlySpacedArray2(xMinimum, xMaximum, n);
            YAxisTicks = CreateLinearlySpacedArray2(yMinimum, yMaximum, m);
            PlotData(zData2DArray, XAxisTicks, YAxisTicks);
        }

        public void PlotData(double[,] zData2DArray, double[] xArray, double[] yArray)
        {
            int n = zData2DArray.GetLength(0);
            int m = zData2DArray.GetLength(1);
            XAxisTicks = xArray;
            YAxisTicks = yArray;
            PlotData2DArray(zData2DArray, xArray, yArray);
        }

        public void PlotData(Point3D[,] point3DArray)
        {
            // Directly plot from a Point3D array
        }

        public void PlotFunction(Func<double, double, double> function)
        {
            PlotFunction(function, -1, 1, -1, 1, defaultFunctionSampleSize, defaultFunctionSampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double minimumXY, double maximumXY)
        {
            PlotFunction(function, minimumXY, maximumXY, minimumXY, maximumXY, defaultFunctionSampleSize, defaultFunctionSampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double minimumXY, double maximumXY, int sampleSize)
        {
            PlotFunction(function, minimumXY, maximumXY, minimumXY, maximumXY, sampleSize, sampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double xMinimum, double xMaximum, double yMinimum, double yMaximum)
        {
            PlotFunction(function, xMinimum, xMaximum, yMinimum, yMaximum, defaultFunctionSampleSize, defaultFunctionSampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double xMinimum, double xMaximum, double yMinimum, double yMaximum, int sampleSize)
        {
            PlotFunction(function, xMinimum, xMaximum, yMinimum, yMaximum, sampleSize, sampleSize);
        }

        public void PlotFunction(Func<double, double, double> function, double xMinimum, double xMaximum, double yMinimum, double yMaximum, int xSampleSize, int ySampleSize)
        {
            // todo - implement checks to ensure the input parameters make sense. Maybe a SetXYRange internal method?
            _xMin = xMinimum;
            _xMax = xMaximum;
            _yMin = yMinimum;
            _yMax = yMaximum;

            double[] xArray = CreateLinearlySpacedArray(xMinimum, xMaximum, xSampleSize);
            double[] yArray = CreateLinearlySpacedArray(yMinimum, yMaximum, ySampleSize);

            DataPoints = CreateDataArrayFromFunction(function, xArray, yArray);
            CreateColorValues();
            RequestUpdateVisual(true);
        }


        public static double[] CreateLinearlySpacedArray2(double minValue, double maxValue, int numberOfPoints)
        {
            double[] array = new double[numberOfPoints];
            double intervalSize = (maxValue - minValue) / (numberOfPoints);
            for (int i = 0; i < numberOfPoints; i++)
            {
                array[i] = minValue + i * intervalSize;
            }
            return array;
        }

        #endregion

        #region === Private Methods ===


        private void PlotData2DArray(double[,] zData2DArray, double[] xArray=null, double[] yArray=null)
        {
            int n = zData2DArray.GetLength(0);
            int m = zData2DArray.GetLength(1);

            if (xArray != null && xArray.Length != n) throw new Exception("SurfacePlotModel: PlotData2DArray() zData2DArray size x not equal to xArray size");
            if (yArray != null && yArray.Length != m) throw new Exception("SurfacePlotModel: PlotData2DArray() zData2DArray size y not equal to yArray size");

            Point3D[,] newDataArray = new Point3D[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    double x = i;
                    double y = j;
                    if (xArray != null) x = xArray[i];
                    if (yArray != null) y = yArray[j];
                    Point3D point = new Point3D(x, y, zData2DArray[i, j]);
                    newDataArray[i, j] = point;
                }
            }
            DataPoints = newDataArray;
            CreateColorValues();
            RequestUpdateVisual(true);
        }

        private Point3D[,] CreateDataArrayFromFunction(Func<double, double, double> f, double[] xArray, double[] yArray)
        {
            Point3D[,] newDataArray = new Point3D[xArray.Length, yArray.Length];
            for (int i = 0; i < xArray.Length; i++)
            {
                double x = xArray[i];
                for (int j = 0; j < yArray.Length; j++)
                {
                    double y = yArray[j];
                    newDataArray[i, j] = new Point3D(x, y, f(x, y));
                }
            }
            return newDataArray;
        }

        private double[] CreateLinearlySpacedArray(double minValue, double maxValue, int numberOfPoints)
        {
            double[] array = new double[numberOfPoints];
            double intervalSize = (_xMax - _xMin) / (numberOfPoints - 1);
            for (int i = 0; i < numberOfPoints; i++)
            {
                array[i] = minValue + i * intervalSize;
            }
            return array;
        }

        private void CreateColorValues()
        {
            switch (ColorCoding)
            {
                case ColorCoding.ByValueZ:
                    ColorValues = GetZData(DataPoints);
                    break;
                case ColorCoding.ByGradientX:
                    ColorValues = FindGradientX(DataPoints);
                    break;
                case ColorCoding.ByGradientY:
                    ColorValues = FindGradientY(DataPoints);
                    break;
                case ColorCoding.ByLights:
                case ColorCoding.NoColorCoding:
                    ColorValues = null;
                    break;
            }



            // ZMax / ZCenter / ZMin bestimmen

            if (ColorValues == null)
            {
                ZMax = string.Empty;
                ZCenter = string.Empty;
                ZMin = string.Empty;
                ZMaxC = string.Empty;
                ZMinC = string.Empty;
                return;
            }

            int n = ColorValues.GetLength(0);
            int m = ColorValues.GetLength(1);
            double zMax = double.MinValue; 
            double zMin = double.MaxValue;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    var v = ColorValues[i, j];
                    if (double.IsNaN(v) || double.IsInfinity(v)) continue;
                    if(v > zMax) zMax = v;
                    if(v < zMin) zMin = v;
                }
            }

            double zCenter = (zMax + zMin) / 2;
            double zMaxC = (zCenter + zMax) / 2;
            double zMinC = (zCenter + zMin) / 2;

            //ZMax = string.Format("F2", zMax);
            //ZCenter = string.Format("F2", zCenter);
            //ZMin = string.Format("F2", zMin);

            ZMax = $"{zMax:F2}";
            ZCenter = $"{zCenter:F2}";
            ZMin = $"{zMin:F2}";
            ZMaxC = $"{zMaxC:F2}";
            ZMinC = $"{zMinC:F2}";
        }

        private void RequestUpdateVisual(bool zoomToContent=false)
        {
            if (SupressUpdates) return;

            RaisePropertyChanged(nameof(Lights));
            RaisePropertyChanged(nameof(CurrentSurfaceBrush));
            RaisePropertyChanged(nameof(DataPoints));
            RaisePropertyChanged(nameof(ColorValues));

            VisualUpdateRequested?.Invoke(this, EventArgs.Empty);

            if(zoomToContent)
            {
                ZoomToContentRequested?.Invoke(this, EventArgs.Empty);
            }
        }



        /*
        private void SetTicksAutomatically()
        {
            xTickMin = xMin;
            xTickMax = xMax;
            xNumberOfTicks = 10;
            xTickInterval = (xTickMax - xTickMin) / (xNumberOfTicks - 1);
            for (int i = 0; i < xNumberOfTicks; i++)
            {
                //xTickMin
            }
        } */

        #endregion

        #region === Exposed Properties to SurfacePlotVisual3D ===

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        public Point3D[,] DataPoints { get; private set; }

        public double[,] ColorValues { get; private set; }

        public Model3DGroup Lights
        {
            get
            {
                var group = new Model3DGroup();
                switch (ColorCoding)
                {
                    case ColorCoding.ByValueZ:
                        group.Children.Add(new AmbientLight(Colors.White));
                        break;
                    case ColorCoding.ByGradientX:
                    case ColorCoding.ByGradientY:
                    case ColorCoding.NoColorCoding:
                        group.Children.Add(new AmbientLight(Colors.White));
                        break;
                    
                    case ColorCoding.ByLights:
                        group.Children.Add(new AmbientLight(Colors.Gray));
                        group.Children.Add(new PointLight(Colors.Red, new Point3D(0, -1000, 0)));
                        group.Children.Add(new PointLight(Colors.Blue, new Point3D(0, 0, 1000)));
                        group.Children.Add(new PointLight(Colors.Green, new Point3D(1000, 1000, 0)));
                        break;
                }
                return group;
            }
        }





        public Brush CurrentSurfaceBrush
        {
            get
            {
                // Brush = BrushHelper.CreateGradientBrush(Colors.White, Colors.Blue);
                // Brush = GradientBrushes.RainbowStripes;
                // Brush = GradientBrushes.BlueWhiteRed;
                switch (ColorCoding)
                {
                    case ColorCoding.ByValueZ:
                        return GetBrush();
                    case ColorCoding.ByGradientX:
                    case ColorCoding.ByGradientY:
                        return GetBrush();
                    case ColorCoding.ByLights:
                        return Brushes.White;
                    case ColorCoding.NoColorCoding:
                        return Brushes.Transparent;
                }
                return null;
            }
        }


        private Brush GetBrush()
        {
            switch (BrushPreset)
            {
                case BrushPreset.BlueWhiteRed: return BlueWhiteRedBrush;
                case BrushPreset.GreenYellowRed: return GreenYellowRedBrush;
                case BrushPreset.BlackWhite: return BlackWhiteBrush;
                case BrushPreset.Custom: return CustomSurfaceBrush ?? BlackWhiteBrush;
                default: return BlackWhiteBrush;
            }
        }


        private string zMax;
        public string ZMax
        {
            get { return zMax; }
            private set
            {
                zMax = value;
                RaisePropertyChanged(nameof(ZMax));
            }
        }

        private string zMaxC;
        public string ZMaxC
        {
            get { return zMaxC; }
            private set
            {
                zMaxC = value;
                RaisePropertyChanged(nameof(ZMaxC));
            }
        }

        private string zCenter;
        public string ZCenter
        {
            get { return zCenter; }
            private set
            {
                zCenter = value;
                RaisePropertyChanged(nameof(ZCenter));
            }
        }



        private string zMinC;
        public string ZMinC
        {
            get { return zMinC; }
            private set
            {
                zMinC = value;
                RaisePropertyChanged(nameof(ZMinC));
            }
        }

        private string zMin;
        public string ZMin
        {
            get { return zMin; }
            private set
            {
                zMin = value;
                RaisePropertyChanged(nameof(ZMin));
            }
        }


        #endregion

        #region === Exposed Properties ===

        private bool supressUpdates;
        public bool SupressUpdates
        {
            get { return supressUpdates; }
            set
            {
                supressUpdates = value;
                RaisePropertyChanged("SupressUpdates");
            }
        }


        public BrushPreset brushPreset;
        public BrushPreset BrushPreset
        {
            get { return brushPreset; }
            set
            {
                brushPreset = value;
                RaisePropertyChanged(nameof(BrushPreset));
                RequestUpdateVisual();
            }
        }


        public ColorCoding colorCoding;
        public ColorCoding ColorCoding
        {
            get { return colorCoding; }
            set
            {
                colorCoding = value;
                RaisePropertyChanged(nameof(ColorCoding));
                CreateColorValues();
                RequestUpdateVisual();
            }
        }


        public Brush customSurfaceBrush;

        public Brush CustomSurfaceBrush
        {
            get { return customSurfaceBrush; }
            set
            {
                customSurfaceBrush = value;
                RaisePropertyChanged(nameof(CustomSurfaceBrush));
                //CreateColorValues();
                RequestUpdateVisual();
            }
        }

        private double[] xAxisTicks;
        public double[] XAxisTicks
        {
            get { return xAxisTicks; }
            private set
            {
                xAxisTicks = value;
                //RequestUpdateVisual();
            }
        }



        private double[] yAxisTicks;
        public double[] YAxisTicks
        {
            get { return yAxisTicks; }
            private set
            {
                yAxisTicks = value;
                //RequestUpdateVisual();
            }
        }

        private double[] zAxisTicks;
        public double[] ZAxisTicks
        {
            get { return zAxisTicks; }
            private set
            {
                zAxisTicks = value;
                //RequestUpdateVisual();
            }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChanged(nameof(Title));
                // Only to View, not to Visual3D
            }
        }

        private string xAxisLabel;
        public string XAxisLabel
        {
            get { return xAxisLabel; }
            set
            {
                xAxisLabel = value;
                RaisePropertyChanged(nameof(XAxisLabel));
                RequestUpdateVisual();
            }
        }

        private string yAxisLabel;
        public string YAxisLabel
        {
            get { return yAxisLabel; }
            set
            {
                yAxisLabel = value;
                RaisePropertyChanged(nameof(YAxisLabel));
                RequestUpdateVisual();
            }
        }

        private string zAxisLabel;
        public string ZAxisLabel
        {
            get { return zAxisLabel; }
            set
            {
                zAxisLabel = value;
                RaisePropertyChanged(nameof(ZAxisLabel));
                RequestUpdateVisual();
            }
        }

        private bool showSurfaceMesh;
        public bool ShowSurfaceMesh
        {
            get { return showSurfaceMesh; }
            set
            {
                showSurfaceMesh = value;
                RaisePropertyChanged(nameof(ShowSurfaceMesh));
                RequestUpdateVisual();
            }
        }

        //private bool showContourLines;
        //public bool ShowContourLines
        //{
        //    get { return showContourLines; }
        //    set
        //    {
        //        showContourLines = value;
        //        RaisePropertyChanged(nameof(ShowContourLines));
        //        RequestUpdateVisual();
        //    }
        //}

        private bool showGrid;
        public bool ShowGrid
        {
            get { return showGrid; }
            set
            {
                showGrid = value;
                RaisePropertyChanged(nameof(ShowGrid));
                RequestUpdateVisual();
            }
        }

        private bool showAxes;
        public bool ShowAxes
        {
            get { return showAxes; }
            set
            {
                showAxes = value;
                RaisePropertyChanged(nameof(ShowAxes));
                RequestUpdateVisual();
            }
        }

        private bool showColorScale;
        public bool ShowColorScale
        {
            get { return showColorScale; }
            set
            {
                showColorScale = value;
                RaisePropertyChanged(nameof(ShowColorScale));
            }
        }

        private bool showMiniCoordinates;
        public bool ShowMiniCoordinates
        {
            get { return showMiniCoordinates; }
            set
            {
                showMiniCoordinates = value;
                RaisePropertyChanged(nameof(ShowMiniCoordinates));
                // Only to HelixViewport3D, not to Visual3D
            }
        }

        private bool showOrthographic;
        public bool ShowOrthographic
        {
            get { return showOrthographic; }
            set
            {
                showOrthographic = value;
                RaisePropertyChanged(nameof(ShowOrthographic));
                ZoomToContentRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool showXyIsometric;
        public bool ShowXyIsometric
        {
            get { return showXyIsometric; }
            set
            {
                showXyIsometric = value;
                RaisePropertyChanged(nameof(ShowXyIsometric));
                RequestUpdateVisual(true);
            }
        }

        private bool showZIsometric;
        public bool ShowZIsometric
        {
            get { return showZIsometric; }
            set
            {
                showZIsometric = value;
                RaisePropertyChanged(nameof(ShowZIsometric));
                RequestUpdateVisual(true);
            }
        }

        #endregion

        /* // Do we actually need to keep any of these persistent variables for any reason...? (binding?)

        private int xNumberOfPoints;
        private int yNumberOfPoints;

        private int xNumberOfTicks;
        private int yNumberOfTicks;
        private int zNumberOfTicks;

        private double xTickInterval, yTickInterval, zTickInterval;
        private double xTickMin, xTickMax, yTickMin, yTickMax, zTickMin, zTickMax; */
        //private double xMin, xMax, yMin, yMax, zMin, zMax;

        /* OLD STUFF */

        






        // http://en.wikipedia.org/wiki/Numerical_differentiation
        private double[,] FindGradientX(Point3D[,] data)
        {
            int n = data.GetLength(0);
            int m = data.GetLength(1);
            var K = new double[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    // Finite difference approximation
                    var p10 = data[(i + 1 < n ? i + 1 : i), (j - 1 > 0 ? j - 1 : j)];
                    var p00 = data[(i - 1 > 0 ? i - 1 : i), (j - 1 > 0 ? j - 1 : j)];

                    double dx = p10.X - p00.X;
                    double dz = p10.Z - p00.Z;

                    K[i, j] = dz / dx;
                }
            return K;
        }

        private double[,] FindGradientY(Point3D[,] data)
        {
            int n = data.GetLength(0);
            int m = data.GetLength(1);
            var K = new double[n, m];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < m; j++)
                {
                    // Finite difference approximation
                    var p10 = data[(i - 1 > 0 ? i - 1 : i), (j + 1 < n ? j + 1 : j)];
                    var p00 = data[(i - 1 > 0 ? i - 1 : i), (j - 1 > 0 ? j - 1 : j)];

                    double dy = p10.Y - p00.Y;
                    double dz = p10.Z - p00.Z;

                    K[i, j] = dz / dy;
                }
            return K;
        }

        private double[,] GetZData(Point3D[,] data)
        {
            if(data==null) return null;
            
            int n = data.GetLength(0);
            int m = data.GetLength(1);
            var K = new double[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    K[i, j] = data[i, j].Z;
                }
            }

            return K;
        }
    }
}
