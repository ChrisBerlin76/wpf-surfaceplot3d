﻿using System;
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
        enum FunctionOptions { Sinc, Ripple, Gaussian, Funnel, Origami, Simple, DataPlot, DataPlotUnequalXY, BigDataPlot };



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

            int sizeX = 10;
            int sizeY = 20;
            double r = sizeX / 2;
            double s = sizeY / 2;

            switch (currentOption)
            {
                case FunctionOptions.Sinc:
                    function = (x, y) => 10 * Math.Sin(Math.Sqrt(x * x + y * y)) / Math.Sqrt(x * x + y * y);
                    viewModel.PlotFunction(function, -10, 10);
                    break;

                case FunctionOptions.Ripple:
                    function = (x, y) => 0.25 * Math.Sin(Math.PI * Math.PI * x * y);
                    viewModel.PlotFunction(function, 0, 2, 300);
                    break;

                case FunctionOptions.Gaussian:
                    function = (x, y) => 5 * Math.Exp(-1 * Math.Pow(x, 2) / 4 - Math.Pow(y, 2) / 4) / (Math.Sqrt(2 * Math.PI));
                    viewModel.PlotFunction(function, -5, 5, 200);
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

                case FunctionOptions.DataPlot:
                    sizeX = 10;
                    sizeY = 20;
                    r = sizeX / 2;
                    s = sizeY / 2;
                    double[,] arrayOfPoints = new double[sizeX, sizeY];
                    for (int i = 0; i < sizeX; i++)
                    {
                        for (int j = 0; j < sizeY; j++)
                        {
                            arrayOfPoints[i, j] = 10 * Math.Sin(Math.Sqrt((i - r) * (i - r) + (j - s) * (j - s)) * 0.7) / Math.Sqrt((i - r) * (i - r) + (j - s) * (j - s) + 0.0001);
                        }
                    }
                    //arrayOfPoints[5,5] = double.NaN;
                    viewModel.PlotData(arrayOfPoints);
                    break;

                case FunctionOptions.DataPlotUnequalXY:
                    sizeX = 10;
                    sizeY = 15;
                    r = sizeX / 2;
                    s = sizeY / 2;
                    double[,] arrayOfPoints1 = new double[sizeX, sizeY];
                    for (int i = 0; i < sizeX; i++)
                    {
                        for (int j = 0; j < sizeY; j++)
                        {
                            arrayOfPoints1[i, j] = 10 * Math.Sin(Math.Sqrt((i - r) * (i - r) + (j - s) * (j - s)) * 0.7) / Math.Sqrt((i - r) * (i - r) + (j - s) * (j - s) + 0.0001);
                        }
                    }

                    var xArray = new double[sizeX];
                    var yArray = new double[sizeY];

                    for (int i = 0; i < sizeX; i++) xArray[i] = (i + Math.Sin(i*1.2) * 0.4) * 3;
                    for (int i = 0; i < sizeY; i++) yArray[i] = Math.Pow(i, 1.2) * 3;

                    //arrayOfPoints[5,5] = double.NaN;
                    viewModel.PlotData(arrayOfPoints1, xArray, yArray);
                    break;

                case FunctionOptions.BigDataPlot:
                    sizeX = 201;
                    sizeY = 301;
                    r = sizeX / 2;
                    s = sizeY / 2;
                    double[,] arrayOfPoints2 = new double[sizeX, sizeY];
                    for (int i = 0; i < sizeX; i++)
                    {
                        for (int j = 0; j < sizeY; j++)
                        {
                            arrayOfPoints2[i, j] = 1000 * Math.Sin(Math.Sqrt((i - r) * (i - r) + (j-s) * (j - s)) * 0.1) / Math.Sqrt((i - r) * (i - r) + (j - s) * (j - s) + 0.0001);
                        }
                    }
                    viewModel.PlotData(arrayOfPoints2);
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
