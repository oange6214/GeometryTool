using MainGui.Mvvm.Models;
using MainGui.Mvvm.ViewModels;
using MainGui.UI.Borders;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MainGui.Services.Actions;


/// <summary>
///     用於繪製圓形，如點、直線、三角形、矩形、圓形、橢圓形
/// </summary>
public class GraphAdd
{
    MainWindowViewModel _mainWindowViewModel;


    /// <summary>
    /// 包裝 Path 的 border
    /// </summary>
    private BorderWithDrag _border;

    private BorderWithAdorner _borderWithAdorner;

    public GraphAdd(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }

    /// <summary>
    ///     新建一個 PathGeometry
    /// </summary>
    /// <param name="vGraphAppearance"></param>
    /// <param name="vRootCanvas"></param>
    /// <param name="vPath"></param>
    /// <param name="vEllipsePath"></param>
    /// <param name="vIsClose"></param>
    public void NewPathGeometry(GraphAppearance vGraphAppearance, Canvas vRootCanvas, out Path vPath, Path vEllipsePath,
        bool vIsClose)
    {
        // 設置直線的外觀
        vPath = new Path
        {
            Stroke = vGraphAppearance.Stroke,
            StrokeThickness = vGraphAppearance.StrokeThickness
        };

        if (vGraphAppearance.Fill != Brushes.Transparent)
        {
            vPath.Fill = vGraphAppearance.Fill;
        }

        var pathGeometry = new PathGeometry();
        vPath.Data = pathGeometry;
        pathGeometry.FillRule = vGraphAppearance.FillRule;
        pathGeometry.Figures = new PathFigureCollection();
        var pathFigure = new PathFigure { IsClosed = vIsClose };
        pathGeometry.Figures.Add(pathFigure);
        var segmentCollection = new PathSegmentCollection();
        pathFigure.Segments = segmentCollection;

        // 設置起點綁定該 Point
        var e = vEllipsePath.Data as EllipseGeometry;
        var binding = new Binding("Center") { Source = e };
        binding.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(pathFigure, PathFigure.StartPointProperty, binding);
    }

    /// <summary>
    ///     增加一個 LineSegment
    /// </summary>
    /// <param name="vPathFigure"></param>
    /// <param name="vEllipsePath"></param>
    public void AddLineSegment(PathFigure vPathFigure, Path vEllipsePath)
    {
        // 獲取要綁定的點
        var e = (EllipseGeometry)vEllipsePath.Data;
        var newLineSegment = new LineSegment();

        // 綁定點
        var binding = new Binding("Center") { Source = e };
        binding.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(newLineSegment, LineSegment.PointProperty, binding);
        vPathFigure.Segments.Add(newLineSegment);
    }

    /// <summary>
    ///     增加一條 ArgSegment
    /// </summary>
    public void AddArcSegment(PathFigure vPathFigure, Path vEllipsePath, Size vSize, double vRotationAngle,
        SweepDirection vSweepDirection, bool vIsLargeArc)
    {
        // 獲取要綁定的點
        var e = (EllipseGeometry)vEllipsePath.Data;
        var newArcSegment = new ArcSegment();
        newArcSegment.Size = vSize;
        newArcSegment.SweepDirection = vSweepDirection;
        newArcSegment.IsLargeArc = vIsLargeArc;
        newArcSegment.RotationAngle = vRotationAngle;

        // 綁定點
        var binding = new Binding("Center") { Source = e };
        binding.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(newArcSegment, ArcSegment.PointProperty, binding);
        vPathFigure.Segments.Add(newArcSegment);
    }

    /// <summary>
    ///     三次方貝茲曲線
    /// </summary>
    /// <param name="vPathFigure"></param>
    /// <param name="vEllipsePath1"></param>
    /// <param name="vEllipsePath2"></param>
    /// <param name="vEllipsePath3"></param>
    public void AddBezierSegment(PathFigure vPathFigure, Path vEllipsePath1, Path vEllipsePath2, Path vEllipsePath3)
    {
        var e1 = (EllipseGeometry)vEllipsePath1.Data; // 獲取要綁定的點 1
        var e2 = (EllipseGeometry)vEllipsePath2.Data; // 獲取要綁定的點 2
        var e3 = (EllipseGeometry)vEllipsePath3.Data; // 獲取要綁定的點 3
        var newLineSegment = new BezierSegment();

        var binding1 = new Binding("Center") { Source = e1 }; // 綁定點 1
        binding1.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(newLineSegment, BezierSegment.Point1Property, binding1);

        var binding2 = new Binding("Center") { Source = e2 }; // 綁定點 2
        binding2.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(newLineSegment, BezierSegment.Point2Property, binding2);

        var binding3 = new Binding("Center") { Source = e3 }; // 綁定點 3
        binding3.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(newLineSegment, BezierSegment.Point3Property, binding3);

        vPathFigure.Segments.Add(newLineSegment);
    }

    /// <summary>
    ///     兩次方貝茲曲線
    /// </summary>
    /// <param name="vPathFigure"></param>
    /// <param name="vEllipsePath1"></param>
    /// <param name="vEllipsePath2"></param>
    public void AddQuadraticSegment(PathFigure vPathFigure, Path vEllipsePath1, Path vEllipsePath2)
    {
        var e1 = (EllipseGeometry)vEllipsePath1.Data; // 獲取要綁定的點 1
        var e2 = (EllipseGeometry)vEllipsePath2.Data; // 獲取要綁定的點 2
        var newLineSegment = new QuadraticBezierSegment();

        var binding1 = new Binding("Center") { Source = e1 }; // 綁定點 1
        binding1.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(newLineSegment, QuadraticBezierSegment.Point1Property, binding1);

        var binding2 = new Binding("Center") { Source = e2 }; // 綁定點 2
        binding2.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(newLineSegment, QuadraticBezierSegment.Point2Property, binding2);

        vPathFigure.Segments.Add(newLineSegment);
    }

    /// <summary>
    ///     繪製一個隨鼠標移動的線段
    /// </summary>
    /// <param name="vPathFigure"></param>
    /// <param name="vPoint"></param>
    public void AddHorvorLine(PathFigure vPathFigure, Point vPoint)
    {
        var lastLine = vPathFigure.Segments[vPathFigure.Segments.Count - 1] as LineSegment;
        lastLine.Point = vPoint;
    }

    /// <summary>
    ///     畫一條直線
    /// </summary>
    /// <param name="vGraphAppearance">直線的外觀</param>
    /// <param name="vRootCanvas">直線的容器</param>
    /// <param name="vPath">為了修改終點而是用的變數</param>
    /// <param name="vHitPath">鼠標點擊的圖形</param>
    /// <param name="vIsStartPoint">表示是起點還是終點</param>
    public void AddLine(GraphAppearance vGraphAppearance, Canvas vRootCanvas, ref Path vPath, Path vHitPath,
        ref int vIsStartPoint, ref PathFigure vPathFigure, bool isClose)
    {
        if (vIsStartPoint == 0) // 表示起點
        {
            // 設置直線的外觀
            vPath = new Path
            {
                Stroke = vGraphAppearance.Stroke,
                StrokeThickness = vGraphAppearance.StrokeThickness,
                StrokeDashArray = vGraphAppearance.StrokeDashArray
            };

            if (vGraphAppearance.Fill != Brushes.Transparent)
            {
                vPath.Fill = vGraphAppearance.Fill;
            }

            // 定義直線
            var pathGeometry = new PathGeometry();
            pathGeometry.Figures = new PathFigureCollection();
            vPathFigure = new PathFigure { IsClosed = isClose };
            pathGeometry.Figures.Add(vPathFigure);
            var segmentCollection = new PathSegmentCollection();
            vPathFigure.Segments = segmentCollection;
            var lineSegment = new LineSegment();

            vPathFigure.Segments.Add(lineSegment);
            vPath.Data = pathGeometry;

            // 將直線的起點和某個點綁定在一起
            var e = vHitPath.Data as EllipseGeometry;
            var binding = new Binding("Center") { Source = e };
            binding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(vPathFigure, PathFigure.StartPointProperty, binding);
            lineSegment.Point = e.Center;
            _borderWithAdorner = new BorderWithAdorner(_mainWindowViewModel);
            _borderWithAdorner.EllipseList.Add(vHitPath);
            _borderWithAdorner.Child = vPath;
            vRootCanvas.Children.Add(_borderWithAdorner);
            vIsStartPoint = 1;
        }
        else // 表示終點
        {
            // 將直線的終點和某個點綁定在一起
            var lineSegment = vPathFigure.Segments[vPathFigure.Segments.Count - 1] as LineSegment;
            var e = vHitPath.Data as EllipseGeometry;
            var binding1 = new Binding("Center") { Source = e };
            BindingOperations.SetBinding(lineSegment, LineSegment.PointProperty, binding1);

            var newlineSegment = new LineSegment();
            newlineSegment.Point = new Point { X = e.Center.X, Y = e.Center.Y };
            vPathFigure.Segments.Add(newlineSegment);

            _borderWithAdorner.EllipseList.Add(vHitPath);
        }
    }

    /// <summary>
    ///     在面板上面畫一個圓
    /// </summary>
    /// <param name="vPoint"></param>
    /// <param name="vGraphAppearance"></param>
    /// <param name="vRootCanvas"></param>
    public void AddPoint(Point vPoint, GraphAppearance vGraphAppearance, Canvas vRootCanvas, out Path vPath)
    {
        // 1.設置圓的外觀
        vPath = new Path();
        vPath.Stroke = Brushes.Black;
        vPath.Fill = Brushes.White;
        vPath.StrokeThickness = 0.1;
        var ellipse = new EllipseGeometry();
        ellipse.RadiusX = 0.2;
        ellipse.RadiusY = 0.2;
        ellipse.Center = vPoint;
        vPath.Data = ellipse;

        // 把圓放進 border 裡面，因為 Border 中有裝飾器，同時可以使圓可以被拖動
        _border = new BorderWithDrag();

        _border.Child = vPath;
        vRootCanvas.Children.Add(_border);
    }

    /// <summary>
    ///     在面板上面畫一個圓
    /// </summary>
    /// <param name="vPoint"></param>
    /// <param name="vGraphAppearance"></param>
    /// <param name="vRootCanvas"></param>
    public void AddPointWithNoBorder(Point vPoint, GraphAppearance vGraphAppearance, Canvas vRootCanvas, out Path vPath)
    {
        // 1.設置圓的外觀
        vPath = new Path();
        vPath.Stroke = Brushes.Black;
        vPath.Fill = Brushes.White;
        vPath.StrokeThickness = 0.1;
        var ellipse = new EllipseGeometry();
        ellipse.RadiusX = 0.2;
        ellipse.RadiusY = 0.2;
        ellipse.Center = vPoint;
        vPath.Data = ellipse;
    }

    /// <summary>
    ///     繪製一個圓
    /// </summary>
    /// <param name="vPoint"></param>
    /// <param name="vGraphAppearance"></param>
    /// <param name="vRootCanvas"></param>
    /// <param name="vPath"></param>
    public BorderWithAdorner AddGeometryOfCircle(Point vPoint, GraphAppearance vGraphAppearance, Canvas vRootCanvas,
        out Path vPath)
    {
        // 設置直線的外觀
        vPath = new Path
        {
            Stroke = vGraphAppearance.Stroke,
            StrokeThickness = vGraphAppearance.StrokeThickness,
            StrokeDashArray = vGraphAppearance.StrokeDashArray
        };

        if (vGraphAppearance.Fill != Brushes.Transparent)
        {
            vPath.Fill = vGraphAppearance.Fill;
        }

        // 定義第一個 PathFigure
        var pathGeometry = new PathGeometry();
        vPath.Data = pathGeometry;
        var borderWithAdorner = new BorderWithAdorner(_mainWindowViewModel);
        borderWithAdorner.Child = vPath;
        vRootCanvas.Children.Add(borderWithAdorner);
        pathGeometry.Figures = new PathFigureCollection();
        var pathFigure = new PathFigure();
        pathGeometry.Figures.Add(pathFigure);
        var segmentCollection = new PathSegmentCollection();
        pathFigure.Segments = segmentCollection;

        // 繪製第一個點，並綁定起點
        Path ellipsePath;
        AddPoint(vPoint, vGraphAppearance, vRootCanvas, out ellipsePath);
        borderWithAdorner.EllipseList.Add(ellipsePath);
        var ellipseGeometry = ellipsePath.Data as EllipseGeometry;
        var startGeometry = ellipseGeometry;
        var firstPointBinding = new Binding("Center") { Source = ellipseGeometry };
        firstPointBinding.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(pathFigure, PathFigure.StartPointProperty, firstPointBinding);

        // 繪製第二個和第一條曲線，並綁定終點
        AddPoint(vPoint, vGraphAppearance, vRootCanvas, out ellipsePath); // 增加點
        borderWithAdorner.EllipseList.Add(ellipsePath);
        ellipseGeometry = ellipsePath.Data as EllipseGeometry;
        var firstLineSe = new ArcSegment();
        var secondPointBinding = new Binding("Center") { Source = ellipseGeometry };
        secondPointBinding.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(firstLineSe, ArcSegment.PointProperty, secondPointBinding); // 綁定 Point
        pathFigure.Segments.Add(firstLineSe);

        // 繪製第二條曲線，並綁定終點
        var secondLineSe = new ArcSegment();
        AddPoint(vPoint, vGraphAppearance, vRootCanvas, out ellipsePath); // 增加點
        borderWithAdorner.EllipseList.Add(ellipsePath);
        ellipseGeometry = ellipsePath.Data as EllipseGeometry;
        var forthPointBinding = new Binding("Center") { Source = ellipseGeometry };
        forthPointBinding.Mode = BindingMode.TwoWay;
        BindingOperations.SetBinding(secondLineSe, ArcSegment.PointProperty, forthPointBinding); // 綁定 Point
        pathFigure.Segments.Add(secondLineSe);
        var fiveBinding = new Binding("Center") { Source = startGeometry, Mode = BindingMode.TwoWay };
        BindingOperations.SetBinding(ellipseGeometry, EllipseGeometry.CenterProperty, fiveBinding);

        secondLineSe.Size = new Size { Height = 0.1, Width = 0.1 };
        firstLineSe.Size = new Size { Height = 0.1, Width = 0.1 };
        return borderWithAdorner;
    }
}