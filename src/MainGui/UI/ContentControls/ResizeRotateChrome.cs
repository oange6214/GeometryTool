using System.Windows;
using System.Windows.Controls;

namespace MainGui.UI.ContentControls;


/// <summary>
///     可拖動的選擇框的邊外觀
/// </summary>
public class ResizeRotateChrome : Control
{
    static ResizeRotateChrome()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeRotateChrome),
            new FrameworkPropertyMetadata(typeof(ResizeRotateChrome)));
    }
}