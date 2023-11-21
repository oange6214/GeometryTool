using MainGui.Mvvm.ViewModels;
using MainGui.UI.Borders;
using MainGui.UI.ContentControls;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MainGui.UI.Adorners;

/// <summary>
/// Adorner for select box
/// </summary>
public class GeometryAdorner : Adorner
{
    MainWindowViewModel _mainWindowViewModel;
    private readonly VisualCollection _visuals;

    /// <summary>
    /// 選擇框的真正樣式
    /// </summary>
    public GeometryControl GeometryControl { set; get; }

    /// <summary>
    ///     構造函數，主要是使 Chrome 是 DataContext 為 BorderWithAdorner
    /// </summary>
    /// <param name="borderWithAdorner"></param>
    public GeometryAdorner(MainWindowViewModel mainWindowViewModel, BorderWithAdorner borderWithAdorner)
        : base(borderWithAdorner)
    {
        _mainWindowViewModel = mainWindowViewModel;
        SnapsToDevicePixels = true;

        GeometryControl = new GeometryControl(_mainWindowViewModel)
        {
            DataContext = borderWithAdorner
        };

        _visuals = new VisualCollection(this)
        {
            GeometryControl
        };
    }

    /// <summary>
    ///     重寫的 VisualChildrenCount 函數
    /// </summary>
    protected override int VisualChildrenCount => _visuals.Count;

    /// <summary>
    ///     重寫的 ArrangeOverride
    /// </summary>
    /// <param name="arrangeBounds"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        arrangeBounds.Height += 0.5;
        arrangeBounds.Width += 0.5;
        GeometryControl.Arrange(new Rect(arrangeBounds));
        return arrangeBounds;
    }

    /// <summary>
    ///     重寫 GetVisualChild 函數
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected override Visual GetVisualChild(int index)
    {
        return _visuals[index];
    }
}