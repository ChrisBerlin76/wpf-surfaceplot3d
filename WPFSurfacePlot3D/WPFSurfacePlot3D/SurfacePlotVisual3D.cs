using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace WPFSurfacePlot3D
{
    public class SurfacePlotVisual3D : ModelVisual3D
    {
        private SurfacePlotModel _model;        //  = ViewModel
        private readonly Model3DGroup _modelContainer;
        private Model3DGroup _cursorModel;
        private Model3DGroup _SurfacePlotModel;
        private double _stretchX = 1;
        private double _stretchZ = 1;
        private double _lineThickness = 0.01;
        private Material _lineMaterial = MaterialHelper.CreateMaterial(Colors.Black);
        private double _sizeFactor = 1.0;

        /// <summary>
        /// The constructor for a new SurfacePlotVisual3D object.
        /// </summary>
        public SurfacePlotVisual3D()
        {
            _modelContainer = new Model3DGroup();
            this.Content = _modelContainer;

            IntervalX = 1;
            IntervalY = 1;
            IntervalZ = 0.25;
            FontSize = 0.06;
            LineThickness = 5;
        }

        public void RegisterModel(SurfacePlotModel model)
        {
            if (_model != null)
            {
                _model.VisualUpdateRequested -= _model_VisualUpdateRequested;
                _model.UpdateCursorRequested -= _model_UpdateCursorRequested;
            }

            if (model is SurfacePlotModel)
            {
                _model = model;
                _model.VisualUpdateRequested += _model_VisualUpdateRequested;
                _model.UpdateCursorRequested += _model_UpdateCursorRequested;

                UpdateSurfacePlotModel();
            }
        }

        private void _model_UpdateCursorRequested(object sender, EventArgs e)
        {
            UpdateCursor();
        }

        private void _model_VisualUpdateRequested(object sender, EventArgs e)
        {
            UpdateSurfacePlotModel();
        }

        /// <summary>
        /// Gets or sets the points defining the 3D surface plot, as a 2D-array of Point3D objects.
        /// </summary>
        public Point3D[,] DataPoints
        {
            get { return (Point3D[,])GetValue(DataPointsProperty); }
            set { SetValue(DataPointsProperty, value); }
        }

        public static readonly DependencyProperty DataPointsProperty = DependencyProperty.Register("DataPoints", typeof(Point3D[,]), typeof(SurfacePlotVisual3D), new UIPropertyMetadata(SamplePoints));
        //public static readonly DependencyProperty DataPointsProperty = DependencyProperty.Register("DataPoints", typeof(Point3D[,]), typeof(SurfacePlotVisual3D), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the color values corresponding to the Points array, as a 2D-array of doubles.
        /// The color values are used as Texture coordinates for the surface.
        /// Remember to set the SurfaceBrush, e.g. by using the BrushHelper.CreateGradientBrush method.
        /// If this property is not set, the z-value of the Points will be used as color value.
        /// </summary>
        public double[,] ColorValues
        {
            get { return (double[,])GetValue(ColorValuesProperty); }
            set { SetValue(ColorValuesProperty, value); }
        }

        public static readonly DependencyProperty ColorValuesProperty = DependencyProperty.Register("ColorValues", typeof(double[,]), typeof(SurfacePlotVisual3D), new UIPropertyMetadata(null));

        /// <summary>
        /// Sets X and Y axis to equal length
        /// </summary>
        public bool ShowXyIsometric
        {
            get { return (bool)GetValue(ShowXyIsometricProperty); }
            set { SetValue(ShowXyIsometricProperty, value); }
        }

        public static readonly DependencyProperty ShowXyIsometricProperty = DependencyProperty.Register("ShowXyIsometric", typeof(bool), typeof(SurfacePlotVisual3D), new UIPropertyMetadata(false));

        /// <summary>
        /// Sets X and Y axis to equal length
        /// </summary>
        public bool ShowZIsometric
        {
            get { return (bool)GetValue(ShowZIsometricProperty); }
            set { SetValue(ShowZIsometricProperty, value); }
        }

        public static readonly DependencyProperty ShowZIsometricProperty = DependencyProperty.Register(nameof(ShowZIsometric), typeof(bool), typeof(SurfacePlotVisual3D), new UIPropertyMetadata(false));


        /// <summary>
        /// Gets or sets the brush used for the surface.
        /// </summary>
        public Brush SurfaceBrush
        {
            get { return (Brush)GetValue(SurfaceBrushProperty); }
            set { SetValue(SurfaceBrushProperty, value); }
        }

        public static readonly DependencyProperty SurfaceBrushProperty = DependencyProperty.Register(nameof(SurfaceBrush), typeof(Brush), typeof(SurfacePlotVisual3D), new UIPropertyMetadata(null));


        public double[] XAxisTicks { get; set; }
        public double[] YAxisTicks { get; set; }

        // todo: make Dependency properties
        public double IntervalX { get; set; }
        public double IntervalY { get; set; }
        public double IntervalZ { get; set; }
        public double FontSize { get; set; }
        public double LineThickness { get; set; }

        /// <summary>
        /// This is called whenever a property of the SurfacePlotVisual3D is changed; it updates the 3D model.
        /// </summary>
        private static void ModelWasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SurfacePlotVisual3D)d).UpdateSurfacePlotModel();
            //((SurfacePlotVisual3D)d).RaiseModelUpdatedEvent();
        }

        private ContentControl controlObject = new ContentControl();

        /* Add event handler to push events up via an exposed property */
        // (thanks, http://stackoverflow.com/questions/24870539/custom-events-from-user-control-to-parent-control-in-wpf)

        /*
    public static readonly RoutedEvent ModelUpdatedEvent = EventManager.RegisterRoutedEvent("ModelUpdated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ContentControl));

    public event RoutedEventHandler ModelUpdated
    {
        add { controlObject.AddHandler(ModelUpdatedEvent, value); }
        remove { controlObject.RemoveHandler(ModelUpdatedEvent, value); }
    }

    private void RaiseModelUpdatedEvent()
    {
        RoutedEventArgs newEventArgs = new RoutedEventArgs(SurfacePlotVisual3D.ModelUpdatedEvent);
        controlObject.RaiseEvent(newEventArgs);
    }

    private void ValueWasChanged(object sender, RoutedEventArgs e)
    {
        RaiseModelUpdatedEvent();
    } */



        /// <summary>
        /// This function updates the 3D visual model. It is called whenever a DependencyProperty of the SurfacePlotVisual3D object is called.
        /// </summary>
        private void UpdateSurfacePlotModel()
        {
            this.Children.Clear(); // Necessary to remove BillboardTextVisual3D objects (?)
            CreateSurfacePlotModel();

            UpdateCursor();
            //UpdateTopLevelModel();  // not neccessary because already in UpdateCursor();
        }

        private void UpdateCursor()
        {
            if (_model == null) return;
            var p = new Point3D(_model.CursorX * _stretchX, _model.CursorY, _model.CursorZ * _stretchZ);
            CreateCursorModel(p);

            UpdateTopLevelModel();
        }

        private void UpdateTopLevelModel()
        {
            _modelContainer.Children.Clear();
            if (_SurfacePlotModel != null)
            {
                _modelContainer.Children.Add(_SurfacePlotModel);
            }

            if (_cursorModel != null && _model?.ShowCursorBall == true)
            {
                _modelContainer.Children.Add(_cursorModel);
            }
        }


        /// <summary>
        /// This function contains all the "business logic" for constructing a SurfacePlot 3D. 
        /// </summary>
        /// <returns>A Model3DGroup containing all the component models (mesh, surface definition, grid objects, etc).</returns>
        private void CreateSurfacePlotModel()
        {
            var newModelGroup = new Model3DGroup();

            try
            {
                _lineThickness = 0.01;
                _sizeFactor = 1.0;

                if (_model != null)
                {
                    DataPoints = _model.DataPoints;
                    ColorValues = _model.ColorValues;
                    SurfaceBrush = _model.CurrentSurfaceBrush;
                    ShowXyIsometric = _model.ShowXyIsometric;
                    ShowZIsometric = _model.ShowZIsometric;
                    XAxisTicks = _model.XAxisTicks;
                    YAxisTicks = _model.YAxisTicks;
                    LineThickness = _model.LineThickness;

                    _lineMaterial = MaterialHelper.CreateMaterial(_model.LineColor);
                }


                if (DataPoints == null || DataPoints.GetLength(0) < 2 || DataPoints.GetLength(1) < 2)
                {
                    return;
                }

                // Get relevant constaints from the DataPoints object
                int numberOfRows = DataPoints.GetLength(0);
                int numberOfColumns = DataPoints.GetLength(1);

                // Determine the x, y, and z ranges of the DataPoints collection
                double minX = double.MaxValue;
                double maxX = double.MinValue;
                double minY = double.MaxValue;
                double maxY = double.MinValue;
                double minZ = double.MaxValue;
                double maxZ = double.MinValue;



                for (int i = 0; i < numberOfRows; i++)
                {
                    for (int j = 0; j < numberOfColumns; j++)
                    {
                        double x = DataPoints[i, j].X;
                        double y = DataPoints[i, j].Y;
                        double z = DataPoints[i, j].Z;
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                        maxZ = Math.Max(maxZ, z);
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        minZ = Math.Min(minZ, z);
                    }
                }


                GetMinMaxColorValues(ColorValues, out double minColorValue, out double maxColorValue);

                //double maxDiff = Math.Max(Math.Max(maxX - minX, maxY - minY), maxZ - minZ);
                //double minDiff = Math.Min(Math.Min(maxX - minX, maxY - minY), maxZ - minZ);
                double factorY = (maxY - minY) / (maxX - minX);
                double factorZ = (maxZ - minZ) / Math.Max((maxX - minX), (maxY - minY));
                double maxDiff = Math.Max(maxX - minX, maxY - minY);
                double minDiff = Math.Min(maxX - minX, maxY - minY);
                _sizeFactor = maxDiff * 0.04;

                double lt = LineThickness;
                if (lt < 1) lt = 1;
                _lineThickness = (minDiff + maxDiff) / 2 * lt * 0.0005;


                /* TEMP */
                int numberOfXAxisTicks = 10;
                int numberOfYAxisTicks = 10;
                int numberOfZAxisTicks = 5;

                if (numberOfRows <= 20) numberOfXAxisTicks = numberOfRows - 1;
                if (numberOfColumns <= 20) numberOfYAxisTicks = numberOfColumns - 1;

                double XAxisInterval = (maxX - minX) / numberOfXAxisTicks;
                double YAxisInterval = (maxY - minY) / numberOfYAxisTicks;
                double ZAxisInterval = (maxZ - minZ) / numberOfZAxisTicks;
                /* /TEMP */

                // Set color value to 0 at texture coordinate 0.5, with an even spread in either direction
                if (Math.Abs(minColorValue) < Math.Abs(maxColorValue)) { minColorValue = -maxColorValue; }
                else { maxColorValue = -minColorValue; }

                // Set the texture coordinates by either z-value or ColorValue
                var textureCoordinates = new Point[numberOfRows, numberOfColumns];
                bool colorValuesValid = ColorValues != null && ColorValues.GetLength(0) == numberOfRows && ColorValues.GetLength(1) == numberOfColumns;
                for (int i = 0; i < numberOfRows; i++)
                {
                    for (int j = 0; j < numberOfColumns; j++)
                    {
                        double tc;
                        if (colorValuesValid) { tc = (ColorValues[i, j] - minColorValue) / (maxColorValue - minColorValue); }
                        else { tc = (DataPoints[i, j].Z - minZ) / (maxZ - minZ); }
                        textureCoordinates[i, j] = new Point(tc, tc);
                    }
                }


                Point3D[,] datapoints = DataPoints;

                // Optional: Stretch X coordinate to get equal Size as Y
                _stretchX = 1;
                _stretchZ = 1;

                if (ShowXyIsometric) _stretchX = factorY;
                if (ShowZIsometric) _stretchZ = 1 / factorZ;

                Point3D p;

                if (_stretchX != 1 || _stretchZ != 1)
                {
                    datapoints = new Point3D[numberOfRows, numberOfColumns];
                    for (int i = 0; i < numberOfRows; i++)
                    {
                        for (int j = 0; j < numberOfColumns; j++)
                        {
                            p = DataPoints[i, j];
                            p.X = p.X * _stretchX;
                            p.Z = p.Z * _stretchZ;
                            datapoints[i, j] = p;
                        }
                    }
                }

                double maxXs = maxX * _stretchX;
                double minXs = minX * _stretchX;
                double maxZs = maxZ * _stretchZ;
                double minZs = minZ * _stretchZ;
                double ZAxisIntervalS = ZAxisInterval * _stretchZ;

                bool showSurfaceMesh = _model != null ? _model.ShowSurfaceMesh : true;
                bool showSurfaceMeshZValues = _model != null ? _model.ShowSurfaceMeshZValues : true;
                bool showAxes = _model != null ? _model.ShowAxes : true;
                bool showGrid = showAxes;

                // Build the surface model (i.e. the coloured surface model)
                MeshBuilder surfaceModelBuilder = new MeshBuilder();
                surfaceModelBuilder.AddRectangularMesh(datapoints, textureCoordinates);

                GeometryModel3D surfaceModel = new GeometryModel3D(surfaceModelBuilder.ToMesh(), MaterialHelper.CreateMaterial(SurfaceBrush, null, null, 1, 0));
                surfaceModel.BackMaterial = surfaceModel.Material;

                // Instantiate MeshBuilder objects for the Grid and SurfaceMeshLines meshes
                MeshBuilder surfaceMeshLinesBuilder = new MeshBuilder();
                MeshBuilder surfaceContourLinesBuilder = new MeshBuilder();
                MeshBuilder gridBuilder = new MeshBuilder();

                // Build the axes labels model (i.e. the object that holds the axes labels and ticks)
                ModelVisual3D axesLabelsModel = new ModelVisual3D();

                List<double> xValues = new List<double>();
                List<double> yValues = new List<double>();
                List<double> zValues = new List<double>();

                double px, py;

                ModelVisual3D zValueLabelsModel = new ModelVisual3D();
                List<Tuple<Point3D, double>> zValuePoints = new List<Tuple<Point3D, double>>();

                if (XAxisTicks != null && XAxisTicks.Length > 1)
                {
                    xValues = XAxisTicks.ToList();
                }
                else
                {
                    // Loop through x intervals - for the surface meshlines, the grid, and X axes ticks
                    for (double x = minX; x <= maxX + 0.0001; x += XAxisInterval)
                    {
                        xValues.Add(x);
                    }
                }

                if (YAxisTicks != null && YAxisTicks.Length > 1)
                {
                    yValues = YAxisTicks.ToList();
                }
                else
                {
                    // Loop through y intervals - for the surface meshlines, the grid, and Y axes ticks
                    for (double y = minY; y <= maxY + 0.0001; y += YAxisInterval)
                    {
                        yValues.Add(y);
                    }
                }


                


                foreach (double x in xValues)
                {
                    double xs = x * _stretchX;

                    // Add surface mesh lines which denote intervals along the x-axis
                    if (showSurfaceMesh || showSurfaceMeshZValues)
                    {
                        var surfacePath = new List<Point3D>();
                        double i = (x - minX) / (maxX - minX) * (numberOfRows - 1);
                        for (int j = 0; j < numberOfColumns; j++)
                        {
                            if (YAxisTicks != null && YAxisTicks.Length == numberOfColumns)
                            {
                                px = xs;
                                py = YAxisTicks[j];
                                p = DoBilinearInterpolation(datapoints, px, py);                               
                            }
                            else
                            {
                                p = DoBilinearInterpolation2(datapoints, i, j);
                            }

                            if (showSurfaceMeshZValues)
                            {
                                zValuePoints.Add(new Tuple<Point3D, double>(p, p.Z / _stretchZ));
                            }
                            if (showSurfaceMesh)
                            {
                                surfacePath.Add(p);
                            }
                        }

                        if (showSurfaceMesh)
                        {
                            surfaceMeshLinesBuilder.AddTube(surfacePath, _lineThickness, 9, false);
                        } 
                    }

                    // Axes labels
                    if (showAxes)
                    {
                        BillboardTextVisual3D label = new BillboardTextVisual3D();
                        label.Text = string.Format("{0:F2}", x);
                        label.Position = new Point3D(xs, minY - _sizeFactor, minZs - _sizeFactor);
                        axesLabelsModel.Children.Add(label);
                    }

                    // Grid lines
                    if (showGrid)
                    {
                        var gridPath = new List<Point3D>();
                        gridPath.Add(new Point3D(xs, minY, minZs));
                        gridPath.Add(new Point3D(xs, maxY, minZs));
                        gridPath.Add(new Point3D(xs, maxY, maxZs));
                        gridBuilder.AddTube(gridPath, _lineThickness, 9, false);
                    }
                }




                foreach (double y in yValues)
                {
                    // Add surface mesh lines which denote intervals along the y-axis
                    if (showSurfaceMesh)
                    {
                        var surfacePath = new List<Point3D>();
                        double j = (y - minY) / (maxY - minY) * (numberOfColumns - 1);
                        for (int i = 0; i < numberOfRows; i++)
                        {
                            if (XAxisTicks != null && XAxisTicks.Length == numberOfRows)
                            {
                                px = XAxisTicks[i] * _stretchX;
                                py = y;
                                surfacePath.Add(DoBilinearInterpolation(datapoints, px, py));
                            }
                            else
                            {
                                surfacePath.Add(DoBilinearInterpolation2(datapoints, i, j));
                            }
                        }
                        surfaceMeshLinesBuilder.AddTube(surfacePath, _lineThickness, 9, false);
                    }

                    // Axes labels
                    if (showAxes)
                    {
                        BillboardTextVisual3D label = new BillboardTextVisual3D();
                        label.Text = string.Format("{0:F2}", y);
                        label.Position = new Point3D(minXs - _sizeFactor, y, minZs - _sizeFactor);
                        axesLabelsModel.Children.Add(label);
                    }

                    // Grid lines
                    if (showGrid)
                    {
                        var gridPath = new List<Point3D>();
                        gridPath.Add(new Point3D(minXs, y, minZs));
                        gridPath.Add(new Point3D(maxXs, y, minZs));
                        gridPath.Add(new Point3D(maxXs, y, maxZs));
                        gridBuilder.AddTube(gridPath, _lineThickness, 9, false);
                    }
                }

                // Loop through z intervals - for the grid, and Z axes ticks
                for (double z = minZ; z <= maxZ + 0.0001; z += ZAxisInterval)
                {
                    //    yValues.Add(z);
                    //}

                    //foreach (double z in zValues)
                    //{
                    double zs = z * _stretchZ;

                    // Grid lines
                    if (showGrid)
                    {
                        var path = new List<Point3D>();
                        path.Add(new Point3D(minXs, maxY, zs));
                        path.Add(new Point3D(maxXs, maxY, zs));
                        path.Add(new Point3D(maxXs, minY, zs));
                        gridBuilder.AddTube(path, _lineThickness, 9, false);
                    }


                    // Axes labels
                    if (showAxes)
                    {
                        BillboardTextVisual3D label = new BillboardTextVisual3D();
                        label.Text = string.Format("{0:F2}", z);
                        label.Position = new Point3D(minXs - _sizeFactor, maxY + _sizeFactor, zs);
                        axesLabelsModel.Children.Add(label);
                    }
                }

                // Add axes labels
                if (showAxes)
                {
                    BillboardTextVisual3D xLabel = new BillboardTextVisual3D();
                    xLabel.Text = _model?.XAxisLabel ?? "X";
                    xLabel.Position = new Point3D((maxX + minX) * _stretchX / 2, minY - 2.5 * _sizeFactor, minZs - 2.5 * _sizeFactor);
                    axesLabelsModel.Children.Add(xLabel);
                    BillboardTextVisual3D yLabel = new BillboardTextVisual3D();
                    yLabel.Text = _model?.YAxisLabel ?? "Y";
                    yLabel.Position = new Point3D(minXs - 2.5 * _sizeFactor, (maxY + minY) / 2, minZs - 2.5 * _sizeFactor);
                    axesLabelsModel.Children.Add(yLabel);
                    BillboardTextVisual3D zLabel = new BillboardTextVisual3D();
                    zLabel.Text = _model?.ZAxisLabel ?? "Z";
                    zLabel.Position = new Point3D(minXs - 2.5 * _sizeFactor, maxY + 2.5 * _sizeFactor, (maxZs + minZs) / 2); // Note: trying to find the midpoint of minZ, maxZ doesn't work when minZ = -0.5 and maxZ = 0.5...
                    axesLabelsModel.Children.Add(zLabel);
                }

                if (showSurfaceMeshZValues)
                {
                    try
                    {
                        foreach (var pt in zValuePoints)
                        {
                            BillboardTextVisual3D lbl = new BillboardTextVisual3D();
                            lbl.Text = $"{pt.Item2:F2}";
                            p = pt.Item1;
                            p.Z += _sizeFactor;
                            lbl.Position = p;
                            zValueLabelsModel.Children.Add(lbl);
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Exception occured for showSurfaceMeshZValues");
                    }
                }

                // Create models from MeshBuilders
                GeometryModel3D surfaceMeshLinesModel = new GeometryModel3D(surfaceMeshLinesBuilder.ToMesh(), _lineMaterial);
                GeometryModel3D gridModel = new GeometryModel3D(gridBuilder.ToMesh(), _lineMaterial);


                // Update model group
                this.Children.Add(axesLabelsModel);
                newModelGroup.Children.Add(surfaceModel);
                newModelGroup.Children.Add(surfaceMeshLinesModel);
                newModelGroup.Children.Add(gridModel);

                if (showSurfaceMeshZValues)
                {
                    this.Children.Add(zValueLabelsModel);
                }

                //ScaleTransform3D surfaceTransform = new ScaleTransform3D(20, 20, 20, 0, 0, 0);
                //newModelGroup.Transform = surfaceTransform;



            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateModel. {ex.Message}");
            }

            _SurfacePlotModel = newModelGroup;
        }

        private void CreateCursorModel(Point3D p)
        {
            Model3DGroup modelGroup = new Model3DGroup();

            // remove old label
            var oldLbl = this.Children.SingleOrDefault(x => x.GetName() == "CursorLabel");
            if (oldLbl != null) this.Children.Remove(oldLbl);

            if (_model?.ShowCursorBall == true)
            {
                MeshBuilder cursorBallBuilder = new MeshBuilder();
                cursorBallBuilder.AddSphere(p, _sizeFactor * 0.5);
                modelGroup.Children.Add(new GeometryModel3D(cursorBallBuilder.ToMesh(), Materials.Blue));

                // create new label
                BillboardTextVisual3D lbl = new BillboardTextVisual3D();
                double zValue = p.Z / _stretchZ;
                lbl.Text = $"{zValue:F2}";
                lbl.Background = new SolidColorBrush(Colors.White);
                var p2 = p;
                p2.Z += 2 * _sizeFactor;
                lbl.Position = p2;
                lbl.SetName("CursorLabel");
                this.Children.Add(lbl);
            }

            _cursorModel = modelGroup;
        }

        private void GetMinMaxColorValues(double[,] colorValues, out double minColorValue, out double maxColorValue)
        {
            minColorValue = double.MaxValue;
            maxColorValue = double.MinValue;

            if (colorValues == null) return;

            int numberOfRows = colorValues.GetLength(0);
            int numberOfColumns = colorValues.GetLength(1);

            minColorValue = double.MaxValue;
            maxColorValue = double.MinValue;

            for (int i = 0; i < numberOfRows; i++)
            {
                for (int j = 0; j < numberOfColumns; j++)
                {
                    var v = ColorValues[i, j];
                    if (double.IsNaN(v) || double.IsInfinity(v)) continue;
                    maxColorValue = Math.Max(maxColorValue, v);
                    minColorValue = Math.Min(minColorValue, v);
                }
            }
        }

        // <summary>
        /// The bilinear interpolation method calculates a weighted "average" between four points on a discrete grid, allowing us to build a "smooth" path between consecutive points along a grid.
        /// </summary>
        /// <param name="points">Points array - containing the data to be interpolated</param>
        /// <param name="i">First index: i.e., points[i, j]</param>
        /// <param name="j">Second index: i.e., points[i, j]</param>
        /// <returns></returns>
        private static Point3D DoBilinearInterpolation(Point3D[,] points, double x, double y)
        {
            int n = points.GetUpperBound(0);
            int m = points.GetUpperBound(1);
            int x0 = n;
            int y0 = m;
            double xu = 0;
            double yu = 0;

            for (int i = 0; i < n; i++)
            {
                var p0 = points[i, 0];
                var p1 = points[i + 1, 0];
                if (p1.X > x)
                {
                    x0 = i;
                    xu = (x - p0.X) / (p1.X - p0.X);
                    break;
                }
            }

            for (int i = 0; i < m; i++)
            {
                var p0 = points[0, i];
                var p1 = points[0, i + 1];
                if (p1.Y > y)
                {
                    y0 = i;
                    yu = (y - p0.Y) / (p1.Y - p0.Y);
                    break;
                }
            }

            if (x0 >= n)
            {
                x0 = n - 1;
                xu = 1;
            }
            if (y0 >= m)
            {
                y0 = m - 1;
                yu = 1;
            }

            if (xu < 0) xu = 0;
            if (yu < 0) yu = 0;

            Vector3D v00 = points[x0, y0].ToVector3D();
            Vector3D v01 = points[x0, y0 + 1].ToVector3D();
            Vector3D v10 = points[x0 + 1, y0].ToVector3D();
            Vector3D v11 = points[x0 + 1, y0 + 1].ToVector3D();
            Vector3D v0 = v00 * (1 - xu) + v10 * xu;
            Vector3D v1 = v01 * (1 - xu) + v11 * xu;
            return (v0 * (1 - yu) + v1 * yu).ToPoint3D();
        }


        // <summary>
        /// The bilinear interpolation method calculates a weighted "average" between four points on a discrete grid, allowing us to build a "smooth" path between consecutive points along a grid.
        /// </summary>
        /// <param name="points">Points array - containing the data to be interpolated</param>
        /// <param name="i">First index: i.e., points[i, j]</param>
        /// <param name="j">Second index: i.e., points[i, j]</param>
        /// <returns></returns>
        private static Point3D DoBilinearInterpolation2(Point3D[,] points, double i, double j)
        {
            int n = points.GetUpperBound(0);
            int m = points.GetUpperBound(1);
            var i0 = (int)i;
            var j0 = (int)j;
            if (i0 + 1 >= n) i0 = n - 1;
            if (j0 + 1 >= m) j0 = m - 1;

            if (i < 0) i = 0;
            if (j < 0) j = 0;
            double u = i - i0;
            double v = j - j0;
            Vector3D v00 = points[i0, j0].ToVector3D();
            Vector3D v01 = points[i0, j0 + 1].ToVector3D();
            Vector3D v10 = points[i0 + 1, j0].ToVector3D();
            Vector3D v11 = points[i0 + 1, j0 + 1].ToVector3D();
            Vector3D v0 = v00 * (1 - u) + v10 * u;
            Vector3D v1 = v01 * (1 - u) + v11 * u;
            return (v0 * (1 - v) + v1 * v).ToPoint3D();
        }


        /// <summary>
        /// This Point3D data set is used to populate the DataPoints dependency properties when they are initialized.
        /// </summary>
        public static Point3D[,] SamplePoints
        {
            get
            {
                int n = 50;
                Point3D[,] points = new Point3D[n, n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        double x = i * (Math.PI / n);
                        double y = j * (Math.PI / n);
                        double z = 0.5 * Math.Sin(x * y);
                        points[i, j] = new Point3D(x, y, z);
                    }
                }
                return points;
            }
        }


    }
}
