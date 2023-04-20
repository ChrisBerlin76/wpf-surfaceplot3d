using System;
using System.Windows;
using System.Windows.Documents;

namespace WPFSurfacePlot3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml - all of the application logic lives here
    /// </summary>
    public partial class MainWindow : Window
    {
        private SurfacePlotModel viewModel;

        /// <summary>
        /// Initialize the main window (hence, this function runs on application start).
        /// You should initialize your SurfacePlotViewModel here, and set it as the
        /// DataContext for your SurfacePlotView (which is defined in MainWindow.xaml).
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Initialize surface plot objects
            viewModel = new SurfacePlotModel();
            propertyGrid.DataContext = viewModel;
            surfacePlotView.DataContext = viewModel;

            // Populate the functionSelectorComboBox
            functionSelectorComboBox.ItemsSource = Enum.GetValues(typeof(FunctionOptions));
        }

        /// <summary>
        /// Used to control which demo function the user has chosen to display.
        /// </summary>
        enum FunctionOptions { Sinc, Ripple, Gaussian, Funnel, Origami, Simple, DataPlot, BigDataPlot };



        /// <summary>
        /// This function is called whenever the user selects a different demo function to plot.
        /// </summary>
        private void FunctionSelectionWasChanged(object sender, RoutedEventArgs e)
        {
            UpdateFunction();
        }

        private void buttonUpdateFunction_Click(object sender, RoutedEventArgs e)
        {
            UpdateFunction();
        }


        private void UpdateFunction()
        {
            FunctionOptions currentOption = FunctionOptions.Simple;
            Func<double, double, double> function;

            if (functionSelectorComboBox.SelectedItem == null)
            {
                Console.WriteLine("No function selected");
            }
            else
            {
                currentOption = (FunctionOptions)functionSelectorComboBox.SelectedItem;
            }

            switch (currentOption)
            {
                case FunctionOptions.Gaussian:
                    function = (x, y) => 5 * Math.Exp(-1 * Math.Pow(x, 2) / 4 - Math.Pow(y, 2) / 4) / (Math.Sqrt(2 * Math.PI));
                    viewModel.PlotFunction(function, -5, 5, 200);
                    break;

                case FunctionOptions.Sinc:
                    function = (x, y) => 10 * Math.Sin(Math.Sqrt(x * x + y * y)) / Math.Sqrt(x * x + y * y);
                    viewModel.PlotFunction(function, -10, 10);
                    break;
                    
                case FunctionOptions.Funnel:
                    function = (x, y) => -1 / (x * x + y * y);
                    viewModel.PlotFunction(function, -1, 1);
                    break;
                    
                case FunctionOptions.Origami:
                    function = (x, y) => Math.Cos(Math.Abs(x) + Math.Abs(y)) * (Math.Abs(x) + Math.Abs(y));
                    viewModel.PlotFunction(function, -1, 1);
                    break;

                case FunctionOptions.Simple:
                    function = (x, y) => x * y;
                    viewModel.PlotFunction(function, -1, 1);
                    break;

                case FunctionOptions.Ripple:
                    function = (x, y) => 0.25 * Math.Sin(Math.PI * Math.PI * x * y);
                    viewModel.PlotFunction(function, 0, 2, 300);
                    break;

                case FunctionOptions.DataPlot:
                    const int sizeX = 10;
                    const int sizeY = 20;
                    double[,] arrayOfPoints = new double[sizeX, sizeY];
                    for (int i = 0; i < sizeX; i++)
                    {
                        for (int j = 0; j < sizeY; j++)
                        {
                            arrayOfPoints[i, j] = 10 * Math.Sin(Math.Sqrt(i * i + j * j)) / Math.Sqrt(i * i + j * j + 0.0001);
                        }
                    }
                    //arrayOfPoints[5,5] = double.NaN;
                    viewModel.PlotData(arrayOfPoints);
                    break;

                case FunctionOptions.BigDataPlot:
                    const int sizeX1 = 201;
                    const int sizeY1 = 301;
                    double r = sizeX1 / 2;
                    double s = sizeY1 / 2;
                    double[,] arrayOfPoints1 = new double[sizeX1, sizeY1];
                    for (int i = 0; i < sizeX1; i++)
                    {
                        for (int j = 0; j < sizeY1; j++)
                        {
                            arrayOfPoints1[i, j] = 1000 * Math.Sin(Math.Sqrt((i - r) * (i - r) + (j-s) * (j - s)) * 0.1) / Math.Sqrt((i - r) * (i - r) + (j - s) * (j - s) + 0.0001);
                        }
                    }
                    viewModel.PlotData(arrayOfPoints1);
                    break;

                default:
                    function = (x, y) => 0;
                    viewModel.PlotFunction(function, -1, 1);
                    break;
            }
        }

        private void FileExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


    }
}
