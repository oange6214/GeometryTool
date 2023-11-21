using MainGui.Mvvm.ViewModels;
using MainGui.Services;
using MainGui.Services.Actions;
using MainGui.UI.Adorners;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MainGui.UI.Borders;

/// <summary>
///     主要讓 Border 增加了拖動響應，用於點
/// </summary>
public class BorderWithDrag : Border
{
    MainWindowViewModel _mainWindowViewModel;

    /// <summary>
    ///     用於表示該位置是否含有多個點，來控制 Adorner 的顯示
    /// </summary>
    public static readonly DependencyProperty HasOtherPointProperty =
        DependencyProperty.Register(
            "HasOtherPoint",
            typeof(bool),
            typeof(BorderWithDrag),
            new FrameworkPropertyMetadata(false, null));

    /// <summary>
    ///     表示同一圖層中
    /// </summary>
    public BorderWithDrag BrotherBorder;

    private readonly List<BorderWithDrag> _borderWithDragList = new();

    /// <summary>
    ///     表示是否可以拖動該點
    /// </summary>
    private bool _canLock;

    /// <summary>
    ///     用於表示所在的圖形中所擁有的點集合
    /// </summary>
    private readonly List<Path> _ellipseList;

    /// <summary>
    ///     表示點所在的圖形
    /// </summary>
    private readonly Path _geometryPath;

    /// <summary>
    /// 表示是否可以拖动
    ///     表示是否可以拖動
    /// </summary>
    private bool _isDragDropInEffect;

    /// <summary>
    ///     表示控制項的裝飾器
    /// </summary>
    public LockAdorner LockAdornor { set; get; }

    /// <summary>
    ///     表示這是圖形中第幾個點
    /// </summary>
    private int _number;

    public List<BorderWithDrag> PointList = new();

    /// <summary>
    ///     表示當前點得位置
    /// </summary>
    private Point _currentPoint;

    /// <summary>
    ///     無參數的構造函數，主要是為了給 Border 響應鼠標的事件
    /// </summary>
    public BorderWithDrag()
    {
        MouseLeftButtonDown += Element_MouseLeftButtonDown;
        MouseMove += Element_MouseMove;
        MouseLeftButtonUp += Element_MouseLeftButtonUp;
    }

    /// <summary>
    ///     有參數的構造函數，主要增加了點得刪除功能
    /// </summary>
    /// <param name="path"></param>
    /// <param name="number"></param>
    /// <param name="ellipseList"></param>
    public BorderWithDrag(MainWindowViewModel mainWindowViewModel, Path path, int number, List<Path> ellipseList)
    {
        _mainWindowViewModel = mainWindowViewModel;

        MouseLeftButtonDown += Element_MouseLeftButtonDown;
        MouseMove += Element_MouseMove;
        MouseLeftButtonUp += Element_MouseLeftButtonUp;
        _geometryPath = path;
        _number = number;
        _ellipseList = ellipseList;

        ContextMenu = new ContextMenu();
        var deleteItem = new MenuItem();
        deleteItem.Header = "Delete";
        deleteItem.Click += DeletedItem_Click;
        ContextMenu.Items.Add(deleteItem);
    }

    public bool HasOtherPoint
    {
        get => (bool)GetValue(HasOtherPointProperty);
        set => SetValue(HasOtherPointProperty, value);
    }

    /// <summary>
    ///     拖動鼠標，移動控制項
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void Element_MouseMove(object sender, MouseEventArgs e)
    {
        // 判斷是否在選擇模式下的操作以及是否可以拖動
        if (_isDragDropInEffect && _mainWindowViewModel.ActionMode == "Select")
        {
            // 獲取目前鼠標的相對位置
            var oldPoint = e.GetPosition(_mainWindowViewModel.MyRootCanvas);

            // 計算最近的網格的位置
            var point = new AutoPoints().GetAutoAdsorbPoint(oldPoint);
            _currentPoint = point;
            var currentFrameworkElement = sender as FrameworkElement;
            var borderWithDrag = currentFrameworkElement as BorderWithDrag;
            var path = borderWithDrag.Child as Path;
            var ellipse = path.Data as EllipseGeometry;

            ellipse.Center = new Point { X = point.X, Y = point.Y };
        }
    }

    /// <summary>
    ///     鼠標左擊控制項，使控制項可以被拖動
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 判斷是否在選擇模式下操作
        if (_mainWindowViewModel.ActionMode == "Select")
        {
            var currentFrameworkElement = sender as FrameworkElement;

            // 判斷是否命中的是點
            if (((currentFrameworkElement as BorderWithDrag).Child as Path).Data as EllipseGeometry != null)
            {
                currentFrameworkElement.CaptureMouse();
                currentFrameworkElement.Cursor = Cursors.Hand;

                // 設置可以拖動
                _isDragDropInEffect = true;
                _canLock = true;
            }

            e.Handled = true;
        }
    }

    /// <summary>
    ///     鼠標釋放，使控制項不能被拖動
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // 判斷是否在選擇模式下的操作以及是否可以拖動
        var currentFrameworkElement = sender as FrameworkElement;
        if (_isDragDropInEffect && _mainWindowViewModel.ActionMode == "Select" && _canLock)
        {
            // 獲取當前鼠標的位置
            _currentPoint = e.GetPosition((UIElement)sender); //获取当前鼠标的位置

            // 修正該位置
            _currentPoint = new AutoPoints().GetAutoAdsorbPoint(_currentPoint);

            // 進行點融合的判讀
            var borderLock = new BorderLock(this);
            borderLock.Lock(_currentPoint);
        }

        _isDragDropInEffect = false;
        currentFrameworkElement.ReleaseMouseCapture();
        _canLock = false;
    }

    /// <summary>
    ///     增加 Border 的刪除功能
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void DeletedItem_Click(object sender, RoutedEventArgs e)
    {
        _borderWithDragList.Clear();

        // 進行命中測試
        VisualTreeHelper.HitTest(Parent as Canvas, null,
            MyHitTestResult,
            new PointHitTestParameters(_currentPoint));
        var deleteAll = false;

        foreach (var item in _borderWithDragList)
        {
            if (item.HasOtherPoint)
            {
                deleteAll = true;
                break;
            }
        }

        if (deleteAll)
        {
            foreach (var item in _borderWithDragList)
            {
                if (item._number != 0)
                {
                    DeletePoint(item);
                }
            }
        }
        else
        {
            DeletePoint(this);
        }
    }

    /// <summary>
    ///     命中測試
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public HitTestResultBehavior MyHitTestResult(HitTestResult result)
    {
        if (result.VisualHit is Path path)
        {
            // 如果命中的圖形是 BorderWithDrag，就 Count++
            if (path.Parent is BorderWithDrag border)
            {
                _borderWithDragList.Add(border);
            }
        }

        return HitTestResultBehavior.Continue;
    }

    /// <summary>
    ///     融合點
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void LockItem_Click(object sender, RoutedEventArgs e)
    {
        var borderLock = new BorderLock(this);
        borderLock.Lock(_currentPoint);
        HasOtherPoint = true;
    }

    /// <summary>
    ///     拆分點
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void UnLockItem_Click(object sender, RoutedEventArgs e)
    {
        var borderLock = new BorderLock();
        borderLock.UnLock(this);
        HasOtherPoint = false;
    }

    /// <summary>
    ///     刪除不必要的點
    /// </summary>
    /// <param name="item"></param>
    public void DeletePoint(BorderWithDrag item)
    {
        if (item._number != 0)
        {
            // 獲取該 Border 所在 PathGeometry
            var path = item._geometryPath.Data as PathGeometry;
            var pathFigure = path.Figures[0];


            if (item._number != 1)
            {
                // 如果要刪除的不是起點，直接移除 Segment
                pathFigure.Segments.RemoveAt(item._number - 2);
            }
            else
            {
                // 如果要刪除的是起點
                var pathFigure2 = new PathFigure();
                for (var i = 0; i < pathFigure.Segments.Count; ++i)
                {
                    // 複製出另一個 PathFigure，令其起點原來的 PathFigure 中的 LineSegment 的 Point
                    if (i == 0)
                    {
                        var binding = new Binding("Center")
                        { Source = item._ellipseList[1].Data as EllipseGeometry };
                        BindingOperations.SetBinding(pathFigure2, PathFigure.StartPointProperty, binding);
                    }
                    else
                    {
                        pathFigure2.Segments.Add(pathFigure.Segments[i]);
                    }
                }

                path.Figures.Add(pathFigure2);
                path.Figures.RemoveAt(0);
            }

            // 在視窗上移除該點
            _mainWindowViewModel.MyRootCanvas.Children.Remove(item);

            item._ellipseList.RemoveAt(item._number - 1);

            // 重新定位點集合的位置
            for (var j = 0; j < item._ellipseList.Count; ++j)
            {
                var border = item._ellipseList[j].Parent as BorderWithDrag;
                border._number = j + 1;
            }
        }
    }
}