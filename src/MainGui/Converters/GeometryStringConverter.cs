using MainGui.Mvvm.Models;
using MainGui.Mvvm.ViewModels;
using MainGui.Services.Actions;
using MainGui.UI.Borders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MainGui.Converters;


/// <summary>
///     字串轉圖形，圖形轉字串
/// </summary>
public class GeometryStringConverter
{
    MainWindowViewModel _mainWindowViewModel;

    private readonly GraphAppearance _graphAppearance;
    private readonly Canvas _rootCanvas;

    public GeometryStringConverter(MainWindowViewModel mainWindowViewModel, Canvas rootCanvas, GraphAppearance graphAppearance)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _rootCanvas = rootCanvas;
        _graphAppearance = graphAppearance;
    }

    /// <summary>
    ///     從字串中讀取圖形，並繪製出來
    /// </summary>
    /// <param name="vMiniLanguage"></param>
    public BorderWithAdorner GeometryFromString(string vMiniLanguage)
    {
        var graphAdd = new GraphAdd(_mainWindowViewModel);
        var path = new Path
        {
            Stroke = _graphAppearance.Stroke,
            StrokeThickness = _graphAppearance.StrokeThickness,
            StrokeDashArray = _graphAppearance.StrokeDashArray
        };

        if (_graphAppearance.Fill != null)
        {
            path.Fill = _graphAppearance.Fill;
        }

        var regex = new Regex(@"([a-zA-Z])\s*([^a-zA-Z]*)");
        var matchList = regex.Matches(vMiniLanguage);

        // 定義直線
        var pathGeometry = new PathGeometry();
        var borderWithAdorner = new BorderWithAdorner(_mainWindowViewModel);
        borderWithAdorner.Child = path;
        path.Data = pathGeometry;
        pathGeometry.Figures = new PathFigureCollection();
        var mPathFigure = new PathFigure();
        var ellipsePoint = new Point();
        var ellipseStartPath = new Path();
        var ellipseList = new List<Path>();
        var number = 0;
        BorderWithDrag border;

        for (var i = 0; i < matchList.Count; ++i)
        {
            // 產生點
            number++;
            var match = matchList[i];

            // 處理起點的問題
            if (i == 0) 
            {

                // 起點不包含 M
                if (match.Groups[1].ToString() != "M") 
                {
                    mPathFigure.StartPoint = new Point { X = 0, Y = 0 };
                    graphAdd.AddPointWithNoBorder(ellipsePoint, _graphAppearance, _rootCanvas, out ellipseStartPath);
                    borderWithAdorner.EllipseList.Add(ellipseStartPath);
                    pathGeometry.Figures.Add(mPathFigure);
                }
                else
                {
                    // 建立 PathFigure，並綁定 StartPoint
                    GetEllipsePointWithNoBorder(match, out ellipseStartPath, out ellipsePoint, @"([\+\-\d\.]*),([\+\-\d\.]*)");
                    borderWithAdorner.EllipseList.Add(ellipseStartPath);
                    if (Regex.IsMatch(vMiniLanguage, "[HLV]"))
                    {
                        ellipseList.Add(ellipseStartPath);
                        border = new BorderWithDrag(_mainWindowViewModel, path, 1, ellipseList)
                        {
                            Child = ellipseStartPath
                        };
                    }
                    else
                    {
                        border = new BorderWithDrag
                        {
                            Child = ellipseStartPath
                        };
                    }
                }

                var startEllipseGeometry = ellipseStartPath.Data as EllipseGeometry;
                pathGeometry.Figures.Clear();
                mPathFigure = new PathFigure();
                if (Regex.IsMatch(vMiniLanguage, @"\sZ"))
                {
                    mPathFigure.IsClosed = true;
                }

                // 綁定起點
                var binding = new Binding("Center")
                {
                    Source = startEllipseGeometry,
                    Mode = BindingMode.TwoWay
                }; 
                BindingOperations.SetBinding(mPathFigure, PathFigure.StartPointProperty, binding);
                pathGeometry.Figures.Add(mPathFigure);
                var segmentCollection = new PathSegmentCollection();
                mPathFigure.Segments = segmentCollection;
            }

            Path vPath;

            switch (match.Groups[1].ToString())
            {
                case "L":
                    {
                        // 建立 PathFigure，並綁定 StartPoint
                        GetEllipsePointWithNoBorder(match, out vPath, out ellipsePoint, @"([\+\-\d\.]*),([\+\-\d\.]*)");
                        borderWithAdorner.EllipseList.Add(vPath);
                        // 建立 LineSegment，並綁定 Point
                        graphAdd.AddLineSegment(mPathFigure, vPath);
                        ellipseList.Add(vPath);
                        border = new BorderWithDrag(_mainWindowViewModel, path, number, ellipseList);
                        border.Child = vPath;

                        break;
                    }

                case "H":
                    {
                        ellipsePoint.X = Convert.ToDouble(match.Groups[2].ToString());

                        graphAdd.AddPointWithNoBorder(ellipsePoint, _graphAppearance, _rootCanvas, out vPath);
                        borderWithAdorner.EllipseList.Add(vPath);
                        graphAdd.AddLineSegment(mPathFigure, vPath);
                        ellipseList.Add(vPath);
                        border = new BorderWithDrag(_mainWindowViewModel, path, number, ellipseList);
                        border.Child = vPath;

                        break;
                    }

                case "V":
                    {
                        ellipsePoint.Y = Convert.ToDouble(match.Groups[2].ToString());

                        graphAdd.AddPointWithNoBorder(ellipsePoint, _graphAppearance, _rootCanvas, out vPath);
                        borderWithAdorner.EllipseList.Add(vPath);
                        graphAdd.AddLineSegment(mPathFigure, vPath);
                        ellipseList.Add(vPath);
                        border = new BorderWithDrag(_mainWindowViewModel, path, number, ellipseList);
                        border.Child = vPath;

                        break;
                    }

                case "A":
                    {
                        // 建立 LineSegment，並綁定 Point
                        var size = new Size
                        {
                            Width = Convert.ToDouble(Regex
                                .Match(match.Groups[0].ToString(), @"A ([\+\-\d\.]*),([\+\-\d\.]*)").Groups[1].ToString()),
                            Height = Convert.ToDouble(Regex
                                .Match(match.Groups[0].ToString(), @"A ([\+\-\d\.]*),([\+\-\d\.]*)").Groups[2].ToString())
                        };

                        SweepDirection vSweepDirection;
                        var isLargeArc = false;
                        var matches = Regex.Matches(match.Groups[0].ToString(),
                            @"A\s*[\+\-\d\.]+,[\+\-\d\.]+\s*([\+\-\.\d]+)\s*([\+\-\.\d]+)\s*([\+\-\.\d]+)");
                        if (Convert.ToInt32(matches[0].Groups[2].ToString()) == 1) //设置SweepDirection
                        {
                            vSweepDirection = SweepDirection.Clockwise;
                        }
                        else
                        {
                            vSweepDirection = SweepDirection.Counterclockwise;
                        }

                        if (Convert.ToInt32(matches[0].Groups[3].ToString()) == 1) //设置IsLargeArc
                        {
                            isLargeArc = true;
                        }

                        // 建立 PathFigure，並綁定 StartPoint
                        GetEllipsePointWithNoBorder(match, out vPath, out ellipsePoint,
                            @"\d ([\+\-\d\.]*),([\+\-\d\.]*)"); //构造EllipsePoint
                        graphAdd.AddArcSegment(mPathFigure, vPath, size,
                            Convert.ToDouble(matches[0].Groups[1].ToString()), vSweepDirection, isLargeArc);
                        borderWithAdorner.EllipseList.Add(vPath);
                        border = new BorderWithDrag();
                        border.Child = vPath;
                        break;
                    }

                case "C":
                    {
                        // 建立 PathFigure，並綁定 StartPoint
                        Path ellipsePath1;
                        GetEllipsePointWithNoBorder(match, out ellipsePath1, out ellipsePoint, 
                            @"([\+\-\d\.]*),([\+\-\d\.]*)"); 
                        borderWithAdorner.EllipseList.Add(ellipsePath1);
                        border = new BorderWithDrag
                        {
                            Child = ellipsePath1
                        };

                        Path ellipsePath2;
                        GetEllipsePointWithNoBorder(match, out ellipsePath2, out ellipsePoint, 
                            @"C\s+[\+\-\d\.]+,[\+\-\d\.]+\s*([\+\-\d\.]+),([\+\-\d\.]+)");
                        borderWithAdorner.EllipseList.Add(ellipsePath2);
                        border = new BorderWithDrag
                        {
                            Child = ellipsePath2
                        };

                        GetEllipsePointWithNoBorder(match, out vPath, out ellipsePoint,
                            @"([\+\-\d\.]+),([\+\-\d\.]+)\s*$");
                        borderWithAdorner.EllipseList.Add(vPath);
                        graphAdd.AddBezierSegment(mPathFigure, ellipsePath1, ellipsePath2, vPath);
                        border = new BorderWithDrag();
                        border.Child = vPath;

                        break;
                    }

                case "Q":
                    {
                        //创建PathFigure，并绑定StartPoint
                        Path ellipsePath1;
                        GetEllipsePointWithNoBorder(match, out ellipsePath1, out ellipsePoint,
                            @"([\+\-\d\.]*),([\+\-\d\.]*)");
                        borderWithAdorner.EllipseList.Add(ellipsePath1);
                        border = new BorderWithDrag();
                        border.Child = ellipsePath1;

                        GetEllipsePointWithNoBorder(match, out vPath, out ellipsePoint,
                            @"\d\s+([\+\-\d\.]*),([\+\-\d\.]*)");
                        borderWithAdorner.EllipseList.Add(vPath);
                        graphAdd.AddQuadraticSegment(mPathFigure, ellipsePath1, vPath);
                        border = new BorderWithDrag();
                        border.Child = vPath;

                        break;
                    }
            }
        }

        return borderWithAdorner;
    }

    /// <summary>
    ///     把圖形轉換成為字串
    /// </summary>
    /// <param name="vPath"></param>
    /// <returns></returns>
    public string StringFromGeometry(Path vPath)
    {
        var miniLanguage = new StringBuilder();
        var pathGeometry = vPath.Data as PathGeometry;
        var pathFigure = pathGeometry.Figures[0];
        var segmentCol = pathFigure.Segments;
        miniLanguage.Append("M " + pathFigure.StartPoint.X + "," + pathFigure.StartPoint.Y + " ");

        foreach (var item in segmentCol)
        {
            if (item.GetType() == typeof(LineSegment))
            {
                miniLanguage.Append("L " + (item as LineSegment).Point.X + "," + (item as LineSegment).Point.Y + " ");
            }
            else if (item.GetType() == typeof(ArcSegment))
            {
                var arcSegment = item as ArcSegment;
                miniLanguage.Append("A " + arcSegment.Size.Width + "," + arcSegment.Size.Height + " " +
                                    arcSegment.RotationAngle + " ");
                miniLanguage.Append((arcSegment.IsLargeArc ? 1 : 0) + " " +
                                    (arcSegment.SweepDirection == SweepDirection.Clockwise ? 1 : 0) + " ");
                miniLanguage.Append(arcSegment.Point.X + "," + arcSegment.Point.Y + " ");
            }
            else if (item.GetType() == typeof(BezierSegment))
            {
                var bezierSegment = item as BezierSegment;
                miniLanguage.Append("C " + bezierSegment.Point1.X + "," + bezierSegment.Point1.Y + " ");
                miniLanguage.Append(bezierSegment.Point2.X + "," + bezierSegment.Point2.Y + " ");
                miniLanguage.Append(bezierSegment.Point3.X + "," + bezierSegment.Point3.Y + " ");
            }
            else if (item.GetType() == typeof(QuadraticBezierSegment))
            {
                var QBezierSegment = item as QuadraticBezierSegment;
                miniLanguage.Append("Q " + QBezierSegment.Point1.X + "," + QBezierSegment.Point1.Y + " ");
                miniLanguage.Append(QBezierSegment.Point2.X + "," + QBezierSegment.Point2.Y + " ");
            }
        }

        if (pathFigure.IsClosed)
        {
            miniLanguage.Append("Z ");
        }

        return miniLanguage.ToString();
    }

    /// <summary>
    ///     產生一個 EllipsePoint
    /// </summary>
    private void GetEllipsePoint(Match match, out Path ellipsePath, out Point ellipsePoint, string vPattern)
    {
        var graphAdd = new GraphAdd(_mainWindowViewModel);
        ellipsePoint = new Point();
        ellipsePoint.X = Convert.ToDouble(Regex.Match(match.Groups[0].ToString(), vPattern).Groups[1].ToString());
        ellipsePoint.Y = Convert.ToDouble(Regex.Match(match.Groups[0].ToString(), vPattern).Groups[2].ToString());
        ellipsePath = new Path();
        graphAdd.AddPoint(ellipsePoint, _graphAppearance, _rootCanvas, out ellipsePath);
    }

    /// <summary>
    ///     產生一個帶 BorderWithAdorner 的 Ellipse
    /// </summary>
    /// <param name="match"></param>
    /// <param name="ellipsePath"></param>
    /// <param name="ellipsePoint"></param>
    /// <param name="vPattern"></param>
    private void GetEllipsePointWithNoBorder(Match match, out Path ellipsePath, out Point ellipsePoint, string vPattern)
    {
        var graphAdd = new GraphAdd(_mainWindowViewModel);
        ellipsePoint = new Point();
        ellipsePoint.X = Convert.ToDouble(Regex.Match(match.Groups[0].ToString(), vPattern).Groups[1].ToString());
        ellipsePoint.Y = Convert.ToDouble(Regex.Match(match.Groups[0].ToString(), vPattern).Groups[2].ToString());
        ellipsePath = new Path();
        graphAdd.AddPointWithNoBorder(ellipsePoint, _graphAppearance, _rootCanvas, out ellipsePath);
    }

    /// <summary>
    ///     建構 PathGeometry
    /// </summary>
    /// <param name="vPath"></param>
    /// <returns></returns>
    public string StringFromPathGeometry(Path vPath)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("          <GeometryDrawing Brush=\"" + vPath.Fill + "\">");
        stringBuilder.AppendLine("              <GeometryDrawing.Geometry>");
        stringBuilder.AppendLine("                  <PathGeometry Figures=\"" + vPath.Data +
                                 (Regex.IsMatch(vPath.Data.ToString(), "[Zz]") ? " Z" : "") + "\"  />");
        stringBuilder.AppendLine("              </GeometryDrawing.Geometry>");
        stringBuilder.AppendLine("              <GeometryDrawing.Pen>");
        stringBuilder.AppendLine("                  <Pen Thickness=\"" + vPath.StrokeThickness + "\" Brush=\"" +
                                 vPath.Stroke + "\" />");
        stringBuilder.AppendLine("              </GeometryDrawing.Pen>");
        stringBuilder.Append("          </GeometryDrawing>");
        return stringBuilder.ToString();
    }

    /// <summary>
    ///     從 XML 中讀取圖形
    /// </summary>
    /// <param name="vXMLString"></param>
    /// <returns></returns>
    public List<BorderWithAdorner> PathGeometryFromString(string vXMLString)
    {
        var pattern =
            @"<GeometryDrawing Brush=""([#\dA-Fa-f]*)"">\s*<GeometryDrawing.Geometry>\s*<PathGeometry Figures=""([\s\,\.\+\-\dA-Za-z]*)""  />\s*</GeometryDrawing.Geometry>\s*<GeometryDrawing.Pen>\s*<Pen Thickness=""([\d\.\-\+]*)"" Brush=""([#\dA-Fa-f]*)"" />";
        var matchList = Regex.Matches(vXMLString, pattern);
        var borderWithAdornerList = new List<BorderWithAdorner>();

        foreach (Match item in matchList)
        {
            var backgroundColor = item.Groups[1].ToString();
            var strokeColor = item.Groups[4].ToString();
            var miniLanguage = item.Groups[2].ToString();
            var strokeThickness = item.Groups[3].ToString();
            var graphAppearance = new GraphAppearance
            {
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(backgroundColor)),
                Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(strokeColor)),
                StrokeThickness = Convert.ToDouble(strokeThickness)
            };
            var geometryStringConverter = new GeometryStringConverter(_mainWindowViewModel, _mainWindowViewModel.MyRootCanvas, graphAppearance);

            // 把 Mini-Language 轉化成圖形，實現圖形的深度複製
            var newBorderWithAdorner = geometryStringConverter.GeometryFromString(miniLanguage);
            var newPath = newBorderWithAdorner.Child as Path;
            newPath.Stroke = graphAppearance.Stroke;
            newPath.StrokeThickness = graphAppearance.StrokeThickness;
            newPath.Fill = graphAppearance.Fill;
            borderWithAdornerList.Add(newBorderWithAdorner);
        }

        return borderWithAdornerList;
    }
}