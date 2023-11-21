using MainGui.Converters;
using MainGui.Mvvm.Models;
using MainGui.Mvvm.ViewModels;
using MainGui.UI.Adorners;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MainGui.UI.Borders;


/// <summary>
///     包含著圖形的 border，主要是顯示選擇框
/// </summary>
public class BorderWithAdorner : Border
{
    MainWindowViewModel _mainWindowViewModel;

    public List<Path> EllipseList { get; }

    /// <summary>
    /// 圖形的選擇框
    /// </summary>
    public GeometryAdorner GeometryAdorner { set; get; }

    /// <summary>
    /// 最大的 X 位置
    /// </summary>
    public double MaxX { set; get; }

    /// <summary>
    /// 最大的 Y 位置
    /// </summary>
    public double MaxY { set; get; }

    /// <summary>
    /// 最小的 X 位置
    /// </summary>
    public double MinX { set; get; }

    /// <summary>
    /// 最小的 Y 位置
    /// </summary>
    public double MinY { set; get; }

    public BorderWithAdorner(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        MouseDown += Element_MouseDown;
        EllipseList = new List<Path>();
    }


    /// <summary>
    ///     當滑鼠點擊圖形的時候，產生邊線
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void Element_MouseDown(object sender, MouseEventArgs e)
    {
        if (_mainWindowViewModel.ActionMode == "Select")
        {
            ShowAdorner();
        }
    }

    /// <summary>
    ///     展示 Adorner
    /// </summary>
    public void ShowAdorner()
    {
        var layer = AdornerLayer.GetAdornerLayer(Parent as Canvas);
        if (layer != null)
        {
            GetFourPoint(this);
            if (GeometryAdorner == null)
            {
                var path = Child as Path;
                GeometryAdorner = new GeometryAdorner(_mainWindowViewModel, this);
                layer.Add(GeometryAdorner);
            }
        }
    }

    /// <summary>
    ///     獲取圖形四個邊角的位置
    /// </summary>
    public void GetFourPoint(BorderWithAdorner borderWithAdorner)
    {
        if (borderWithAdorner.EllipseList.Count > 0)
        {
            borderWithAdorner.MinX = (EllipseList[0].Data as EllipseGeometry).Center.X;
            borderWithAdorner.MinY = (EllipseList[0].Data as EllipseGeometry).Center.Y;
            borderWithAdorner.MaxX = 0;
            borderWithAdorner.MaxY = 0;
        }

        foreach (var path in borderWithAdorner.EllipseList)
        {
            var item = path.Data as EllipseGeometry;
            var point = item.Center;
            if (borderWithAdorner.MaxX < point.X)
            {
                borderWithAdorner.MaxX = point.X;
            }

            if (borderWithAdorner.MaxY < point.Y)
            {
                borderWithAdorner.MaxY = point.Y;
            }

            if (borderWithAdorner.MinX > point.X)
            {
                borderWithAdorner.MinX = point.X;
            }

            if (borderWithAdorner.MinY > point.Y)
            {
                borderWithAdorner.MinY = point.Y;
            }
        }
    }

    /// <summary>
    ///     用於複製 Border，模仿深度複製
    /// </summary>
    /// <param name="borderWithAdorner"></param>
    /// <returns></returns>
    public BorderWithAdorner CopyBorder(BorderWithAdorner borderWithAdorner)
    {
        var path = borderWithAdorner.Child as Path;
        var graphAppearance = new GraphAppearance
        {
            Stroke = path.Stroke,
            StrokeThickness = path.StrokeThickness,
            StrokeDashArray = path.StrokeDashArray,
            Fill = path.Fill
        };
        var geometryStringConverter = new GeometryStringConverter(_mainWindowViewModel, _mainWindowViewModel.MyRootCanvas, graphAppearance);

        // 把該圖形轉化成為 Mini-Language
        var miniLanguage = geometryStringConverter.StringFromGeometry(borderWithAdorner.Child as Path);

        // 把 Mini-Language 轉化成為圖形，實現圖形的深度複製
        var newBorderWithAdorner = geometryStringConverter.GeometryFromString(miniLanguage);
        var newPath = newBorderWithAdorner.Child as Path;
        newPath.Stroke = path.Stroke;
        newPath.StrokeThickness = path.StrokeThickness;
        newPath.StrokeDashArray = path.StrokeDashArray;
        newPath.Fill = path.Fill;
        return newBorderWithAdorner;
    }
}