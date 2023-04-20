using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace WPFSurfacePlot3D
{
    /// <summary>
    /// Interaction logic for SurfacePlotView.xaml
    /// </summary>
    public partial class SurfacePlotView : UserControl
    {
        private SurfacePlotModel _model;

        private DispatcherTimer _zoomExtentsTimer;

        public SurfacePlotView()
        {
            InitializeComponent();
            DataContext = LayoutRoot.DataContext;
            
            hViewport.ZoomExtentsGesture = new KeyGesture(Key.Space);
            hViewport.TopViewGesture = new KeyGesture(Key.Space);

            hViewport.ResetCameraGesture= new KeyGesture(Key.Space, ModifierKeys.Control);

            _zoomExtentsTimer = new DispatcherTimer();
            _zoomExtentsTimer.Interval = TimeSpan.FromMilliseconds(200);
            _zoomExtentsTimer.Tick += _zoomExtentsTimer_Tick;
        }



        public Point3D[,] DataPoints
        {
            get { return (Point3D[,])GetValue(DataPointsProperty); }
            set { SetValue(DataPointsProperty, value); }
        }

        public static readonly DependencyProperty DataPointsProperty = DependencyProperty.Register("DataPoints", typeof(Point3D[,]), typeof(SurfacePlotView), new FrameworkPropertyMetadata(SurfacePlotVisual3D.SamplePoints));
        
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(SurfacePlotView), new FrameworkPropertyMetadata("Surface Plot Title"));

        public string XAxisLabel
        {
            get { return (string)GetValue(XAxisLabelProperty); }
            set { SetValue(XAxisLabelProperty, value); }
        }

        public static readonly DependencyProperty XAxisLabelProperty = DependencyProperty.Register("XAxisLabel", typeof(string), typeof(SurfacePlotView), new FrameworkPropertyMetadata("X Axis Label"));

        public string YAxisLabel
        {
            get { return (string)GetValue(YAxisLabelProperty); }
            set { SetValue(YAxisLabelProperty, value); }
        }

        public static readonly DependencyProperty YAxisLabelProperty = DependencyProperty.Register("YAxisLabel", typeof(string), typeof(SurfacePlotView), new FrameworkPropertyMetadata("Y Axis Label"));

        public string ZAxisLabel
        {
            get { return (string)GetValue(ZAxisLabelProperty); }
            set { SetValue(ZAxisLabelProperty, value); }
        }

        public static readonly DependencyProperty ZAxisLabelProperty = DependencyProperty.Register("ZAxisLabel", typeof(string), typeof(SurfacePlotView), new FrameworkPropertyMetadata("Z Axis Label"));

        public bool ShowSurfaceMesh
        {
            get { return (bool)GetValue(ShowSurfaceMeshProperty); }
            set { SetValue(ShowSurfaceMeshProperty, value); }
        }

        public static readonly DependencyProperty ShowSurfaceMeshProperty = DependencyProperty.Register("ShowSurfaceMesh", typeof(bool), typeof(SurfacePlotView), new FrameworkPropertyMetadata(true));

        public bool ShowContourLines
        {
            get { return (bool)GetValue(ShowContourLinesProperty); }
            set { SetValue(ShowContourLinesProperty, value); }
        }

        public static readonly DependencyProperty ShowContourLinesProperty = DependencyProperty.Register("ShowContourLines", typeof(bool), typeof(SurfacePlotView), new FrameworkPropertyMetadata(true));

        public bool ShowMiniCoordinates
        {
            get { return (bool)GetValue(ShowMiniCoordinatesProperty); }
            set { SetValue(ShowMiniCoordinatesProperty, value); }
        }

        public static readonly DependencyProperty ShowMiniCoordinatesProperty = DependencyProperty.Register("ShowMiniCoordinates", typeof(bool), typeof(SurfacePlotView), new FrameworkPropertyMetadata(true));

        //public bool ShowOrthographic
        //{
        //    get { return (bool)GetValue(ShowOrthographicProperty); }
        //    set { SetValue(ShowOrthographicProperty, value); }
        //}

        //public static readonly DependencyProperty ShowOrthographicProperty = DependencyProperty.Register("ShowOrthographic", typeof(bool), typeof(SurfacePlotView), new FrameworkPropertyMetadata(true));

        public void ZoomToExtents()
        {
            // FieldOfView resp. NearPlaneDistance are set to default again
            // because sometimes these values change to unreasonable values
            if (hViewport.Camera is PerspectiveCamera cam)
            {
                cam.FieldOfView = 30;
            }

            if (hViewport.Camera is OrthographicCamera ocam)
            {
                ocam.NearPlaneDistance = -10000000;
            }

            hViewport.ZoomExtents(400);
        }


        private void hViewport_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ZoomToExtents();
        }



        private void hViewport_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_model != null)
            {
                _model.ZoomToContentRequested -= _model_ZoomToContentRequested;
            }

            if (DataContext is SurfacePlotModel model)
            {
                surfacePlotVisual3D.RegisterModel(model);

                _model = model;
                _model.ZoomToContentRequested += _model_ZoomToContentRequested;
            }
        }

        private void _model_ZoomToContentRequested(object sender, System.EventArgs e)
        {
            _zoomExtentsTimer.Start();
        }

        private void _zoomExtentsTimer_Tick(object sender, EventArgs e)
        {
            _zoomExtentsTimer.Stop();
            ZoomToExtents();
        }


    }
}
