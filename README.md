# Warning: this library is not actively maintained

This project is free to use as you see fit (for example, as a reference or starter point for a 3D plotting library in WPF). However, please be aware that it is NOT actively maintained, and has not been updated in years. I do not have time to respond to requests or questions; sorry.

# WPF-SurfacePlot3D
[![Documentation Status](http://readthedocs.org/projects/wpf-surfaceplot3d/badge/?version=latest)](http://wpf-surfaceplot3d.readthedocs.org/en/latest/?badge=latest)

WPF-SurfacePlot3D is a tiny C# library containing easy-to-use 3D surface plotting components for WPF (.NET) applications.

You can get started in as few as four lines:

```csharp
var myPlot = new SurfacePlotViewModel();
myPlotView.DataContext = myPlot;
function = (x, y) => x * y;
viewModel.PlotFunction(function, -1, 1);
```

[Check out the documentation](http://wpf-surfaceplot3d.readthedocs.org) for more information!
