using MainGui.Services;
using MainGui.UI.Borders;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MainGui.UI.ContentControls;


/// <summary>
///     實現圖形拖動的 Thumb
/// </summary>
public class MoveThumb : Thumb
{
    /// <summary>
    ///     包含該圖形的 BorderWithAdorner
    /// </summary>
    private BorderWithAdorner _borderWithAdorner;

    /// <summary>
    ///     構造函數，註冊拖動事件
    /// </summary>
    public MoveThumb()
    {
        DragStarted += MoveThumb_DragStarted;
        DragDelta += MoveThumb_DragDelta;
        DragCompleted += MoveThumb_DragCompleted;
    }

    /// <summary>
    ///     開始拖動，主要是獲取包含著被拖動的圖形的 BorderWithAdorner
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveThumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        _borderWithAdorner = DataContext as BorderWithAdorner;
    }

    /// <summary>
    ///     圖形被拖動過程
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_borderWithAdorner != null)
        {
            // 獲得移動的距離
            var dragDelta = new Point(e.HorizontalChange, e.VerticalChange); 
            var border = _borderWithAdorner;
            for (var i = 0; i < border.EllipseList.Count; ++i)
            {
                var borderWithDrag = border.EllipseList[i].Parent as BorderWithDrag;

                // 如果該點有鎖，便跳過移動
                if (borderWithDrag.HasOtherPoint) 
                {
                    continue;
                }

                // 更新點的座標
                var item = border.EllipseList[i].Data as EllipseGeometry;
                item.Center = new Point
                { X = item.Center.X + e.HorizontalChange, Y = item.Center.Y + e.VerticalChange };
            }
        }
    }

    /// <summary>
    ///     圖形結束拖動，實現自動吸附
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MoveThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        for (var i = 0; i < _borderWithAdorner.EllipseList.Count; ++i)
        {
            var item = _borderWithAdorner.EllipseList[i].Data as EllipseGeometry;
            var p1 = item.Center;
            item.Center = new AutoPoints().GetAutoAdsorbPoint(p1);
        }
    }
}