using MainGui.Services;
using MainGui.UI.Borders;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MainGui.UI.ContentControls;


/// <summary>
///     可拖動的選擇框的邊外觀
/// </summary>
public class ResizeThumb : Thumb
{
    private BorderWithAdorner _borderWithAdorner;

    /// <summary>
    ///     構造函數，用於註冊事件
    /// </summary>
    public ResizeThumb()
    {
        DragStarted += ResizeThumb_DragStarted;
        DragDelta += ResizeThumb_DragDelta;
        DragCompleted += ResizeThumb_DragCompleted;
    }

    /// <summary>
    ///     開始縮放
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        _borderWithAdorner = DataContext as BorderWithAdorner;
    }

    /// <summary>
    ///     圖形的縮放
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        // 紀錄圖形的四個角落的值
        double maxX = _borderWithAdorner.MaxX, maxY = _borderWithAdorner.MaxY; 
        double minX = _borderWithAdorner.MinX, minY = _borderWithAdorner.MinY;

        // 紀錄圖形未縮放之前的高度、寬度
        var oldHeight = _borderWithAdorner.ActualHeight;
        var oldWidth = _borderWithAdorner.ActualWidth;

        // 縮放後圖形新的寬度、高度
        double newWidth = 0;
        double newHeight = 0;

        double deltaVertical, deltaHorizontal = 0;
        if (_borderWithAdorner != null)
        {
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Bottom:  // 如果是拖動下面的邊
                    deltaVertical = -e.VerticalChange;
                    newHeight = oldHeight - deltaVertical;
                    break;
                case VerticalAlignment.Top:     // 如果是拖動上面的邊
                    deltaVertical = e.VerticalChange;
                    newHeight = oldHeight - deltaVertical;
                    break;
            }

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:  // 如果是拖動左邊的邊
                    deltaHorizontal = e.HorizontalChange;
                    newWidth = oldWidth - deltaHorizontal;

                    break;
                case HorizontalAlignment.Right: // 如果是拖動右邊的邊
                    deltaHorizontal = -e.HorizontalChange;
                    newWidth = oldWidth - deltaHorizontal;
                    break;
            }
        }

        var multipleX = newWidth / oldWidth;
        var multipleY = newHeight / oldHeight;

        // 改變圖形的高度
        switch (VerticalAlignment)
        {
            case VerticalAlignment.Bottom:
                for (var i = 0; i < _borderWithAdorner.EllipseList.Count; ++i)
                {
                    var item = _borderWithAdorner.EllipseList[i].Data as EllipseGeometry;
                    var borderWithDrag = _borderWithAdorner.EllipseList[i].Parent as BorderWithDrag;
                    if (borderWithDrag.HasOtherPoint)
                    {
                        continue;
                    }

                    var p1 = item.Center;
                    var newY = multipleY * (p1.Y - minY) + minY;
                    item.Center = new Point { X = p1.X, Y = newY };
                }

                break;
            case VerticalAlignment.Top:
                for (var i = 0; i < _borderWithAdorner.EllipseList.Count; ++i)
                {
                    var item = _borderWithAdorner.EllipseList[i].Data as EllipseGeometry;
                    var borderWithDrag = _borderWithAdorner.EllipseList[i].Parent as BorderWithDrag;
                    if (borderWithDrag.HasOtherPoint)
                    {
                        continue;
                    }

                    var p1 = item.Center;
                    var newY = multipleY * (p1.Y - maxY) + maxY;
                    item.Center = new Point { X = p1.X, Y = newY };
                }

                break;
        }

        // 改變圖形的寬度
        switch (HorizontalAlignment)
        {
            case HorizontalAlignment.Left:
                for (var i = 0; i < _borderWithAdorner.EllipseList.Count; ++i)
                {
                    var item = _borderWithAdorner.EllipseList[i].Data as EllipseGeometry;
                    var borderWithDrag = _borderWithAdorner.EllipseList[i].Parent as BorderWithDrag;
                    if (borderWithDrag.HasOtherPoint)
                    {
                        continue;
                    }

                    var p1 = item.Center;
                    var newX = multipleX * (p1.X - maxX) + maxX;
                    item.Center = new Point { X = newX, Y = p1.Y };
                    if (i < 2)
                    {
                        Console.WriteLine("mouse:  " + e.HorizontalChange + "     p" + i + ":  " + newX + "      ");
                    }
                }

                break;
            case HorizontalAlignment.Right:
                for (var i = 0; i < _borderWithAdorner.EllipseList.Count; ++i)
                {
                    var item = _borderWithAdorner.EllipseList[i].Data as EllipseGeometry;
                    var borderWithDrag = _borderWithAdorner.EllipseList[i].Parent as BorderWithDrag;
                    if (borderWithDrag.HasOtherPoint)
                    {
                        continue;
                    }

                    var p1 = item.Center;
                    var newX = multipleX * (p1.X - minX) + minX;

                    item.Center = new Point { X = newX, Y = p1.Y };
                }

                break;
        }

        _borderWithAdorner.MinX = _borderWithAdorner.MaxX;
        _borderWithAdorner.MaxX = 0;
        _borderWithAdorner.MinY = _borderWithAdorner.MaxY;

        _borderWithAdorner.MaxY = 0;

        // 重新計算圖形的四個邊角值
        foreach (var path in _borderWithAdorner.EllipseList)
        {
            var item = path.Data as EllipseGeometry;
            var p = item.Center;
            if (_borderWithAdorner.MaxX < p.X)
            {
                _borderWithAdorner.MaxX = p.X;
            }

            if (_borderWithAdorner.MaxY < p.Y)
            {
                _borderWithAdorner.MaxY = p.Y;
            }

            if (_borderWithAdorner.MinX > p.X)
            {
                _borderWithAdorner.MinX = p.X;
            }

            if (_borderWithAdorner.MinY > p.Y)
            {
                _borderWithAdorner.MinY = p.Y;
            }
        }

        GeometryControl.Arrange(_borderWithAdorner.GeometryAdorner.GeometryControl);
        e.Handled = true;
    }

    /// <summary>
    ///     縮放後自動吸附
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        for (var i = 0; i < _borderWithAdorner.EllipseList.Count; ++i)
        {
            var item = _borderWithAdorner.EllipseList[i].Data as EllipseGeometry;
            var p1 = item.Center;
            item.Center = new AutoPoints().GetAutoAdsorbPoint(p1);
        }

        e.Handled = true;
    }
}