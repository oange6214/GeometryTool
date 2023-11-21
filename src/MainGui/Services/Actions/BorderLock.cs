using MainGui.UI.Adorners;
using MainGui.UI.Borders;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MainGui.Services.Actions;


/// <summary>
///     點的融合和拆分
/// </summary>
public class BorderLock
{
    /// <summary>
    ///     用於紀錄要進行命中測試的圖形
    /// </summary>
    private readonly BorderWithDrag _border;

    /// <summary>
    ///     用於計算命中的 BorderWithDrag 的數量
    /// </summary>
    private int _count;

    public BorderLock(BorderWithDrag border)
    {
        _border = border;
    }

    public BorderLock()
    {
    }

    /// <summary>
    ///     點的合併
    /// </summary>
    /// <param name="point"></param>
    public void Lock(Point point)
    {
        var currentPoint = point;

        _border.PointList.Clear();
        VisualTreeHelper.HitTest(_border.Parent as Canvas, null, // 進行命中測試
            MyHitTestResult,
            new PointHitTestParameters(currentPoint));

        if (_count > 1) // 如果該點有多於兩個 BorderWithDrag，說明有點融合
        {
            if (_border.BrotherBorder == null)
            {
                _border.BrotherBorder = _border.PointList[1];
            }

            if (_border.BrotherBorder.LockAdornor == null) // 只選擇一個圖層來顯示 Adorner
            {
                _border.BrotherBorder.LockAdornor = new LockAdorner(_border.BrotherBorder);
                var layer = AdornerLayer.GetAdornerLayer(_border.BrotherBorder.Parent as Canvas);
                if (layer != null)
                {
                    layer.Add(_border.BrotherBorder.LockAdornor);
                }
            }

            _border.BrotherBorder.HasOtherPoint = true; // 顯示鎖
            _border.BrotherBorder.LockAdornor.LockChrome.Source =
                new BitmapImage(new Uri("Assets/Image/lock.png", UriKind.Relative));

            if (_border != _border.BrotherBorder)
            {
                _border.BrotherBorder.PointList = _border.PointList;
            }

            foreach (var border in _border.BrotherBorder.PointList)
            {
                if (border != _border.BrotherBorder)
                {
                    BindingOperations.ClearBinding((border.Child as Path).Data as EllipseGeometry,
                        EllipseGeometry.CenterProperty);
                    var binding = new Binding("Center")
                    { Source = (_border.BrotherBorder.Child as Path).Data as EllipseGeometry };
                    binding.Mode = BindingMode.TwoWay;
                    BindingOperations.SetBinding((border.Child as Path).Data as EllipseGeometry,
                        EllipseGeometry.CenterProperty, binding);
                }
            }
        }
        else
        {
            _border.HasOtherPoint = false;
        }
    }

    /// <summary>
    ///     命中測試
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public HitTestResultBehavior MyHitTestResult(HitTestResult result)
    {
        var path = result.VisualHit as Path;
        if (path != null)
        {
            var border = path.Parent as BorderWithDrag;
            if (border != null) // 如果命中的圖形是 BorderWithDrag，就 Count++
            {
                _border.PointList.Add(border);
                _count++;
                if (border.LockAdornor != null) // 選擇一個已經帶有 Adorner 的 Border 來顯示鎖
                {
                    _border.BrotherBorder = border;
                }
            }
        }

        return HitTestResultBehavior.Continue;
    }

    /// <summary>
    ///     解開點的融合
    /// </summary>
    /// <param name="border"></param>
    public void UnLock(BorderWithDrag border)
    {
        var brotherBorder = border.BrotherBorder;
        var p = ((border.Child as Path).Data as EllipseGeometry).Center;
        foreach (var item in border.PointList) // 解開所用的 Binding
        {
            BindingOperations.ClearBinding((item.Child as Path).Data as EllipseGeometry,
                EllipseGeometry.CenterProperty);
            item.BrotherBorder = null;
            ((item.Child as Path).Data as EllipseGeometry).Center = p; // 重定位到原來的位置
        }

        border.HasOtherPoint = false;
    }
}
