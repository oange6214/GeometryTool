using MainGui.Services;
using MainGui.UI.Borders;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MainGui.UI.ContentControls;


/// <summary>
///     圖形旋轉的 Thumb
/// </summary>
public class RotateThumb : Thumb
{
    /// <summary>
    /// 圖形的 Border
    /// </summary>
    private BorderWithAdorner _borderWithAdorner;

    /// <summary>
    /// 圖形所在的畫布
    /// </summary>
    private Canvas _canvas;

    /// <summary>
    /// 圖形的重點
    /// </summary>
    private Point _centerPoint;

    /// <summary>
    /// 開始的座標向量
    /// </summary>
    private Vector _startVector;

    public RotateThumb()
    {
        DragDelta += RotateThumb_DragDelta;
        DragStarted += RotateThumb_DragStarted;
        DragCompleted += RotateThumb_DragCompleted;
    }

    /// <summary>
    ///     開始旋轉
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RotateThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        _borderWithAdorner = DataContext as BorderWithAdorner;

        if (_borderWithAdorner != null)
        {
            _canvas = VisualTreeHelper.GetParent(_borderWithAdorner) as Canvas;
            if (_canvas != null)
            {
                _centerPoint = _borderWithAdorner.TranslatePoint(
                    new Point
                    {
                        X = (_borderWithAdorner.MaxX + _borderWithAdorner.MinX) / 2.0,
                        Y = (_borderWithAdorner.MaxY + _borderWithAdorner.MinY) / 2.0
                    },
                    _canvas);

                var startPoint = Mouse.GetPosition(_canvas);
                _startVector = Point.Subtract(startPoint, _centerPoint); // 計算開始的向量
            }
        }
    }

    /// <summary>
    ///     旋轉的過程
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RotateThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_borderWithAdorner != null && _canvas != null)
        {
            var currentPoint = Mouse.GetPosition(_canvas);
            var deltaVector = Point.Subtract(currentPoint, _centerPoint);
            var angle = Vector.AngleBetween(_startVector, deltaVector) / 360 * 2 * Math.PI;
            var centerX = (_borderWithAdorner.MaxX + _borderWithAdorner.MinX) / 2.0;
            var centerY = (_borderWithAdorner.MaxY + _borderWithAdorner.MinY) / 2.0; // 計算旋轉的角度，中心點座標

            foreach (var item in _borderWithAdorner.EllipseList) // 根據公式來計算旋轉後的位置
            {
                if ((item.Parent as BorderWithDrag).HasOtherPoint)
                {
                    continue;
                }

                var ellipse = item.Data as EllipseGeometry;
                var oldPoint = ellipse.Center;
                var newX = (oldPoint.X - centerX) * Math.Cos(angle) - (oldPoint.Y - centerY) * Math.Sin(angle) + centerX;
                var newY = (oldPoint.X - centerX) * Math.Sin(angle) + (oldPoint.Y - centerY) * Math.Cos(angle) + centerY;
                ellipse.Center = new Point { X = newX, Y = newY };
            }

            var startPoint = currentPoint;
            _startVector = Point.Subtract(startPoint, _centerPoint); // 重新賦值開始的向量
        }
    }

    /// <summary>
    ///     結束旋轉，並自動吸附到最近的點
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RotateThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        foreach (var item in _borderWithAdorner.EllipseList)
        {
            var ellipse = item.Data as EllipseGeometry;
            var oldPoint = ellipse.Center;
            var p = new AutoPoints().GetAutoAdsorbPoint(oldPoint);
            ellipse.Center = new Point { X = p.X, Y = p.Y };
        }
    }
}