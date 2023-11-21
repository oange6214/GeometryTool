using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;

namespace MainGui.Mvvm.Models;


/// <summary>
///     圖形的外觀
/// </summary>
public class GraphAppearance : ObservableObject
{
    public GraphAppearance()
    {
        StrokeThickness = 0.1;

        Stroke = Brushes.Black;
        Fill = Brushes.Transparent;
        FillRule = FillRule.EvenOdd;
        StrokeDashArray = new DoubleCollection { 1, 0 };
    }

    /// <summary>
    ///     設置圖形的線條的大小
    /// </summary>
    private double _strokeThickness;
    public double StrokeThickness
    {
        get => _strokeThickness;
        set
        {
            SetProperty(ref _strokeThickness, value);
        }
    }

    /// <summary>
    ///     設置虛實
    /// </summary>
    private DoubleCollection _strokeDashArray;
    public DoubleCollection StrokeDashArray
    {
        get => _strokeDashArray;
        set
        {
            SetProperty(ref _strokeDashArray, value);
        }
    }

    /// <summary>
    ///     設置圖形的顏色
    /// </summary>
    private Brush _stroke;
    public Brush Stroke
    {
        get => _stroke;
        set
        {
            SetProperty(ref _stroke, value);
        }
    }

    /// <summary>
    ///     設置圖形填充的顏色
    /// </summary>
    private Brush _fill;
    public Brush Fill
    {
        get => _fill;
        set
        {
            SetProperty(ref _fill, value);
        }
    }

    /// <summary>
    ///     填充規則
    /// </summary>
    private FillRule _fillRule;
    public FillRule FillRule
    {
        get => _fillRule;
        set
        {
            SetProperty(ref _fillRule, value);
        }
    }
}