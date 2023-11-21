using MainGui.Mvvm.Commnads;
using MainGui.Mvvm.ViewModels;
using MainGui.UI.Borders;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MainGui.UI.ContentControls;

/// <summary>
///     圖形的外層拖動的 Adorner
/// </summary>
public class GeometryControl : ContentControl
{
    MainWindowViewModel _mainWindowViewModel;

    public GeometryControl(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        InitializeContextMenu();

        //ContextMenu = new ContextMenu();
        //var copyItem = new MenuItem
        //{
        //    Header = "Copy",
        //    Command = RoutedCommands.CopyCommand
        //};
        //ContextMenu.Items.Add(copyItem);

        //var cutItem = new MenuItem
        //{
        //    Header = "Cut",
        //    Command = RoutedCommands.CutCommand
        //};
        //ContextMenu.Items.Add(cutItem);

        //var deleteItem = new MenuItem
        //{
        //    Header = "Delete",
        //    Command = RoutedCommands.DeleteCommand
        //};
        //ContextMenu.Items.Add(deleteItem);

        //var topItem = new MenuItem
        //{
        //    Header = "Move to the top"
        //};
        //topItem.Click += TopItem_Click;
        //ContextMenu.Items.Add(topItem);
        //var bottomItem = new MenuItem
        //{
        //    Header = "Move to the bottom"
        //};
        //bottomItem.Click += BottomItem_Click;
        //ContextMenu.Items.Add(bottomItem);
    }

    private void InitializeContextMenu()
    {
        ContextMenu = new ContextMenu();

        AddContextMenuItem("Copy", RoutedCommands.CopyCommand, CopyItem_Click);
        AddContextMenuItem("Cut", RoutedCommands.CutCommand, CutItem_Click);
        AddContextMenuItem("Delete", RoutedCommands.DeleteCommand, DeleteItem_Click);
        AddContextMenuItem("Move to the top", null, TopItem_Click);
        AddContextMenuItem("Move to the bottom", null, BottomItem_Click);
    }

    public static void Arrange(GeometryControl geometryChrome)
    {
        var border = geometryChrome.DataContext as BorderWithAdorner;
        geometryChrome.ArrangeOverride(new Size { Width = border.MaxX, Height = border.MaxY });
    }

    /// <summary>
    ///     位置這個鎖
    /// </summary>
    /// <param name="arrangeBounds"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var border = DataContext as BorderWithAdorner;
        border.MaxX = 0;
        border.MinX = (border.EllipseList[0].Data as EllipseGeometry).Center.X;
        border.MaxY = 0;
        border.MinY = (border.EllipseList[0].Data as EllipseGeometry).Center.Y;

        foreach (var path in border.EllipseList)
        {
            var item = path.Data as EllipseGeometry;
            var p = item.Center;
            if (border.MaxX < p.X)
            {
                border.MaxX = p.X;
            }

            if (border.MaxY < p.Y)
            {
                border.MaxY = p.Y;
            }

            if (border.MinX > p.X)
            {
                border.MinX = p.X;
            }

            if (border.MinY > p.Y)
            {
                border.MinY = p.Y;
            }
        }

        var pathGeometry = (border.Child as Path).Data as PathGeometry;
        if (pathGeometry.Figures[0].Segments.Count > 0)
        {
            var geometry = pathGeometry.GetFlattenedPathGeometry();
            var bound = geometry.GetRenderBounds(new Pen(null, (border.Child as Path).StrokeThickness));
            Width = bound.Width + 1;
            Height = bound.Height + 1;
            Margin = new Thickness(border.ActualWidth - bound.Width - 0.65, border.ActualHeight - bound.Height - 0.65,
                0, 0);
        }
        else
        {
            Width = border.MaxX - border.MinX + 1;
            Height = border.MaxY - border.MinY + 1;

            Margin = new Thickness(border.MinX - 0.5, border.MinY - 0.5, 0, 0);
        }

        return base.ArrangeOverride(arrangeBounds);
    }

    /// <summary>
    ///     複製圖形
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void CopyItem_Click(object sender, RoutedEventArgs e)
    {
        CopyOrCutItem(false); // Copy
    }

    /// <summary>
    ///     剪切圖形
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void CutItem_Click(object sender, RoutedEventArgs e)
    {
        CopyOrCutItem(true); // Cut
    }

    /// <summary>
    ///     刪除圖形
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void DeleteItem_Click(object sender, RoutedEventArgs e)
    {
        var borderWithAdorner = DataContext as BorderWithAdorner;
        var rootCanvas = _mainWindowViewModel.MyRootCanvas;
        rootCanvas.Children.Remove(borderWithAdorner);      // 移除圖形
        foreach (var item in borderWithAdorner.EllipseList) // 移除圖形上面的點
        {
            var borderWithDrag = item.Parent as BorderWithDrag;
            rootCanvas.Children.Remove(borderWithDrag);
        }
    }

    /// <summary>
    ///     把圖形至於頂層
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void TopItem_Click(object sender, RoutedEventArgs e)
    {
        var borderWithAdorner = DataContext as BorderWithAdorner;
        Panel.SetZIndex(borderWithAdorner, _mainWindowViewModel.HightestLevel++);
        for (var i = 0; i < borderWithAdorner.EllipseList.Count; ++i)
        {
            Panel.SetZIndex(borderWithAdorner.EllipseList[i].Parent as BorderWithDrag, _mainWindowViewModel.HightestLevel);
        }
    }

    /// <summary>
    ///     把圖形至於底層
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void BottomItem_Click(object sender, RoutedEventArgs e)
    {
        var borderWithAdorner = DataContext as BorderWithAdorner;
        Panel.SetZIndex(borderWithAdorner, _mainWindowViewModel.LowestLevel--);
        for (var i = 0; i < borderWithAdorner.EllipseList.Count; ++i)
        {
            Panel.SetZIndex(borderWithAdorner.EllipseList[i].Parent as BorderWithDrag, _mainWindowViewModel.LowestLevel);
        }
    }

    #region Private Methods

    private void AddContextMenuItem(string header, ICommand command, RoutedEventHandler clickHandler)
    {
        var menuItem = new MenuItem
        {
            Header = header,
            Command = command
        };
        menuItem.Click += clickHandler;
        ContextMenu.Items.Add(menuItem);
    }

    private void CopyOrCutItem(bool isCut)
    {
        var borderWithAdorner = DataContext as BorderWithAdorner;

        // 紀錄要粘貼的圖形
        _mainWindowViewModel.CopyBorderWithAdorner = borderWithAdorner;

        // 把粘貼次數重製為 0
        _mainWindowViewModel.PasteCount = 0;

        var rootCanvas = _mainWindowViewModel.MyRootCanvas;
        var rootCanvasParent = rootCanvas.Parent as Canvas;
        var canvasBorder = rootCanvasParent.Parent as Border;
        var scrollViewer = canvasBorder.Parent as ScrollViewer;
        var rootGrid = scrollViewer.Parent as Grid;

        // 獲取 MainWindow 實例，為了修改 CaPaste 屬性
        var mainWindow = rootGrid.Parent as MainWindow;
        mainWindow.CanPaste = true;

        if (isCut)
        {
            rootCanvas.Children.Remove(borderWithAdorner);
            foreach (var item in borderWithAdorner.EllipseList)
            {
                var borderWithDrag = item.Parent as BorderWithDrag;
                rootCanvas.Children.Remove(borderWithDrag);
            }
        }
    }

    #endregion
}