using MainGui.Converters;
using MainGui.Services.Actions;
using MainGui.UI.Borders;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MainGui.UI.Chromes;

/// <summary>
///     鎖真正的外觀
/// </summary>
public class LockChrome : Image
{
    private readonly bool _isLock;

    public LockChrome()
    {
        // 給鎖增加事件，用於解開融合
        MouseDown += Element_MouseLeftButtonDown;
        Source = new BitmapImage(new Uri("Assets/Image/lock.png", UriKind.Relative));
        _isLock = true;
    }

    /// <summary>
    ///     繪製鎖
    /// </summary>
    /// <param name="arrangeBounds"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        Width = 1.2;
        Height = 1.2;

        // 放在點的右下角
        HorizontalAlignment = HorizontalAlignment.Right;
        VerticalAlignment = VerticalAlignment.Bottom;

        // 做一個旋轉的變換
        var translateTransform = new TranslateTransform
        {
            X = 1.2,
            Y = -0.3
        };
        RenderTransform = translateTransform;

        var binding = new Binding("HasOtherPoint") { Converter = new ImageVisibilityConverter() };

        // 當沒有重合點的時候，隱藏鎖
        SetBinding(VisibilityProperty, binding);

        return base.ArrangeOverride(arrangeBounds);
    }

    /// <summary>
    ///     用於解開融合
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is LockChrome chrome)
        {
            if (_isLock)
            {
                var border = DataContext as BorderWithDrag;
                var borderLock = new BorderLock();
                borderLock.UnLock(border);
            }

            e.Handled = true;
        }
    }
}