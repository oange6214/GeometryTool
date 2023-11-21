using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MainGui.UIComponents;

public partial class PaletteGroup : UserControl
{
    public PaletteGroup()
    {
        InitializeComponent();
    }

    public Brush CurrentColor
    {
        get { return (Brush)GetValue(CurrentColorProperty); }
        set { SetValue(CurrentColorProperty, value); }
    }
    public static readonly DependencyProperty CurrentColorProperty =
        DependencyProperty.Register(
            nameof(CurrentColor), 
            typeof(Brush),
            typeof(PaletteGroup),
            new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    private void Palette_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Button button)
        {
            CurrentColor = button.Background;
        }
    }
}
