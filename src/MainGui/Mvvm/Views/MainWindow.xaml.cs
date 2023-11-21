using MainGui.Converters;
using MainGui.Helpers;
using MainGui.Mvvm.Commnads;
using MainGui.Mvvm.Models;
using MainGui.Mvvm.ViewModels;
using MainGui.Services;
using MainGui.Services.Actions;
using MainGui.UI.Borders;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GeometryPath = System.Windows.Shapes;

namespace MainGui;


public partial class MainWindow : Window
{
    private enum MirrorDirection
    {
        Right,
        Left,
        Bottom,
        Top
    }

    private enum FlipDirection
    {
        Vertical,
        Horizontal
    }


    private MainWindowViewModel VM;

    #region Fields

    /// <summary>
    /// 表示圖形的圖片背景
    /// </summary>
    private BitmapImage _backgroundImage;

    /// <summary>
    /// 表示繪制三次方貝塞爾曲線時，曲線所在的 Path
    /// </summary>
    private GeometryPath.Path _bezierPath;

    /// <summary>
    /// 表示圖形是否可以拖動
    /// </summary>
    private bool _canMove;

    /// <summary>
    /// 表示繪制圓的時候，圓所在的 Path
    /// </summary>
    private GeometryPath.Path _circlePath;

    /// <summary>
    /// 表示繪制橢圓的時候，橢圓所在的 Path
    /// </summary>
    private GeometryPath.Path _ellipseGeometryPath;

    private List<GeometryPath.Path> _ellipseList;

    /// <summary>
    /// 表示繪制圖形的時候，點所在 Path
    /// </summary>
    private GeometryPath.Path _ellipsePath;

    /// <summary>
    /// 表示打開的檔案名稱
    /// </summary>
    private string FileName { set; get; }

    /// <summary>
    /// 表示繪制動作的類別
    /// </summary>
    private readonly GraphAdd _graphAdd;

    /// <summary>
    /// 表示圖形的外觀
    /// </summary>
    private readonly GraphAppearance _graphAppearance;

    /// <summary>
    /// 繪制網格時所使用的 Brush
    /// </summary>
    private DrawingBrush _gridBrush;

    /// <summary>
    /// 表示圖形是否是閉合的
    /// </summary>
    private bool _isClose;

    /// <summary>
    /// 表示當前圖形已經保存了
    /// </summary>
    private bool _isSave;

    /// <summary>
    /// 繪制直線的時候，表示是否為第一個點
    /// </summary>
    private int _isStartPoint;

    /// <summary>
    /// 表示繪制直線的時候，直線的 Path
    /// </summary>
    private GeometryPath.Path _linePath;

    /// <summary>
    /// 表示繪制直線的時候，直線所在的 PathFigure
    /// </summary>
    private PathFigure _pathFigure;

    /// <summary>
    /// 表示繪制二次方貝塞爾曲線時，曲線所在的 Path
    /// </summary>
    private GeometryPath.Path _qBezierPath;

    /// <summary>
    /// 表示繪制正方形的時候，正方形所在的 Path
    /// </summary>
    private GeometryPath.Path _rectanglePath;

    /// <summary>
    /// 表示繪制三角形的時候，三角形所在的 Path
    /// </summary>
    private GeometryPath.Path _trianglePath;


    #endregion

    #region Dependency Property

    public bool CanPaste
    {
        get => (bool)GetValue(CanPasteProperty);
        set => SetValue(CanPasteProperty, value);
    }
    /// <summary>
    ///     依賴屬性，用於控制是否啟動貼上功能
    /// </summary>
    public static readonly DependencyProperty CanPasteProperty =
        DependencyProperty.Register(
            "CanPaste",
            typeof(bool),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(false, null));

    #endregion

    public MainWindow()
    {
        InitializeComponent();

        VM = (MainWindowViewModel)DataContext;

        _graphAppearance = new GraphAppearance();
        _graphAdd = new GraphAdd(VM);
        _ellipseList = new List<GeometryPath.Path>();
        _ellipsePath = new GeometryPath.Path();
        _pathFigure = new PathFigure();
        _isStartPoint = 0;
        _linePath = new GeometryPath.Path();
        _graphAppearance.StrokeThickness = 0.1;
        _graphAppearance.Fill = Brushes.Transparent;

        WindowState = WindowState.Maximized; // 設置視窗最大化
        FileName = "";

        DocCanvas_Loaded();
        LineColor.CurrentColor = _graphAppearance.Stroke;
        FillColor.CurrentColor = _graphAppearance.Fill;

        SD_CanvasChange.Value = 20;
        CB_GridSizeCombobox.SelectedIndex = 3;
        CB_RootCanvasBackGround.SelectedIndex = 0;
        PanProperty.DataContext = _graphAppearance;
        MenuOptions.DataContext = this;

        VM.ActionMode = "Select";
        VM.MyRootCanvas = RootCanvas;

        AddCommand();
    }

    private void SD_CanvasChange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        try
        {
            if (CanvasBorder is null)
                return;

            UpdateCanvasSize();
            UpdateCanvasBackground();
        }
        catch (Exception)
        {

            throw;
        }
    }

    private void UpdateCanvasSize()
    {
        VM.Multiple = (int)SD_CanvasChange.Value;

        CanvasBorder.Height = CalculateDimension(VM.GridSize, VM.Multiple, CanvasBorder.ActualHeight, G_Root.ActualHeight - 115);
        CanvasBorder.Width = CalculateDimension(VM.GridSize, VM.Multiple, CanvasBorder.ActualWidth, G_Root.ActualWidth - 215);
    }

    private void UpdateCanvasBackground()
    {
        switch (CB_RootCanvasBackGround.SelectedIndex)
        {
            case 0:
                DocCanvas_Loaded();
                break;
            case 1:
                RootCanvas.Background = _backgroundImage != null ? new ImageBrush(_backgroundImage) : Brushes.White;
                break;
            default:
                RootCanvas.Background = Brushes.White;
                break;
        }
    }

    private double CalculateDimension(int gridSize, int multiple, double actualSize, double maxSize)
    {
        double dimension = gridSize * multiple + 100;
        return dimension >= actualSize ? dimension : maxSize;
    }

    private void DocCanvas_Loaded()
    {
        _gridBrush = new DrawingBrush(
            new GeometryDrawing(
                new SolidColorBrush(Colors.Transparent),
                new Pen(new SolidColorBrush(Colors.LightGray), 0.1),     // 網格粗細為 1
                new RectangleGeometry(new Rect(0, 0, 1, 1))))
        {
            Stretch = Stretch.None,
            TileMode = TileMode.Tile,
            Viewport = new Rect(0.0, 0.0, 1, 1),
            ViewportUnits = BrushMappingMode.Absolute
        }; // 繪製網格的右邊和下邊
        RootCanvas.Background = _gridBrush;
    }

    private void CB_GridSizeCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var comboBoxItem = (ComboBoxItem)(sender as ComboBox).SelectedItem;
            if (comboBoxItem != null)
            {
                RootCanvas.Height = Convert.ToInt32(comboBoxItem.Tag);
                RootCanvas.Width = Convert.ToInt32(comboBoxItem.Tag);
                VM.GridSize = Convert.ToInt32(comboBoxItem.Tag);
                double height = VM.GridSize * (int)SD_CanvasChange.Value + 200; // 計算畫布放大後的座標
                double width = VM.GridSize * (int)SD_CanvasChange.Value + 200;
                CanvasBorder.Height =
                    height >= CanvasBorder.ActualHeight
                        ? height
                        : G_Root.ActualHeight - 115; // 修改 Border 的大小，使得其能顯示放大後的畫布
                CanvasBorder.Width = width >= CanvasBorder.ActualWidth ? width : G_Root.ActualWidth - 215;
                DocCanvas_Loaded();
            }

            _isSave = true;
        }
        catch
        {
            // 忽略
        }
    }


    #region 檔案操作

    /// <summary>
    ///     將圖形保存為 XML 檔
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (TryGetSaveFileName(out string fileName))
        {
            SaveXmlContent(fileName);
        }
    }

    private bool TryGetSaveFileName(out string fileName)
    {
        fileName = FileName;
        if (string.IsNullOrEmpty(fileName))
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "XML Files |*.xml"
            };

            if ((bool)saveDialog.ShowDialog())
            {
                fileName = saveDialog.FileName;
                return true;
            }
            return false;
        }
        return true;
    }

    private void SaveXmlContent(string fileName)
    {
        string xmlContent = GenerateXmlContent();
        using var sw = new StreamWriter(fileName);
        sw.Write(xmlContent);
    }

    private string GenerateXmlContent()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
        sb.AppendLine("<Canvase>");
        sb.AppendLine(" <Geometry>");
        sb.Append("     <Figures>");
        foreach (UIElement item in RootCanvas.Children)
        {
            if (item is BorderWithAdorner borderWithAdorner)
            {
                var xmlGeometryString = new GeometryStringConverter(VM, RootCanvas, _graphAppearance)
                    .StringFromGeometry(borderWithAdorner.Child as GeometryPath.Path);
                sb.Append(xmlGeometryString);
            }
        }
        sb.AppendLine("</Figures>");
        sb.AppendLine(" </Geometry>");
        sb.AppendLine("</Canvase>");
        return sb.ToString();
    }

    /// <summary>
    ///     把圖形儲存為 PNG 檔
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Save2PNG_Click(object sender, RoutedEventArgs e)
    {
        var oldMutiple = Convert.ToInt32(SD_CanvasChange.Value);
        SD_CanvasChange.Value = 1;
        RootCanvas.Background = null;   // 清空背景

        var save = new SaveFileDialog
        {
            Filter = "png Files |*.png"
        };
        if ((bool)save.ShowDialog())    // 選擇要保存的檔案名
        {
            var pngFileName = save.FileName;

            foreach (var item in RootCanvas.Children) // 隱藏點
            {
                if (item is BorderWithDrag borderWithDrag)
                {
                    borderWithDrag.Visibility = Visibility.Hidden;
                }
            }

            var fs = new FileStream(pngFileName, FileMode.Create);

            var bmp = new RenderTargetBitmap((int)RootCanvas.ActualWidth, (int)RootCanvas.ActualHeight, 96, 96,
                PixelFormats.Pbgra32);
            bmp.Render(RootCanvas);
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(fs);
            fs.Close();
            fs.Dispose();

            foreach (var item in RootCanvas.Children)
            {
                if (item is BorderWithDrag borderWithDrag)
                {
                    borderWithDrag.Visibility = Visibility.Visible;
                }
            }

            _isSave = true;
        }

        SD_CanvasChange.Value = oldMutiple; // 顯示點
    }

    /// <summary>
    ///     將圖形儲存為 XML 檔
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SaveXML_Click(object sender, RoutedEventArgs e)
    {
        var save = new SaveFileDialog
        {
            Filter = "XML Files |*.xml"
        };
        if ((bool)save.ShowDialog()) // 選擇要儲存的檔案名
        {
            FileName = save.FileName;

            var sw = new StreamWriter(FileName);
            var xmlGeometryStringConverter = new GeometryStringConverter(VM, RootCanvas, _graphAppearance);
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
            sb.AppendLine("<Canvase>");
            sb.AppendLine(" <Geometry>");
            sb.Append("     <Figures>");

            foreach (UIElement item in RootCanvas.Children)
            {
                if (item is BorderWithAdorner borderWithAdorner) // 點是有 BorderWithDrag 包含著的，圖形是 Path
                {
                    sb.Append(xmlGeometryStringConverter.StringFromGeometry(borderWithAdorner.Child as GeometryPath.Path)); // 生成 Mini-Language
                }
            }

            sb.AppendLine("</Figures>");
            sb.AppendLine(" </Geometry>");
            sb.AppendLine("</Canvase>");
            sw.Write(sb.ToString());
            sw.Close();
            _isSave = true;
        }
    }

    /// <summary>
    ///     使用 PNG 圖片作為背景
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OpenPNG_Click(object sender, RoutedEventArgs e)
    {
        var opf = new OpenFileDialog
        {
            DefaultExt = ".png",
            Filter = "(*.jpg,*.gif,*.bmp;*.png;)|*.jpg;*.gif;*.bmp;*.png" // 只選擇 .xml 檔
        };
        opf.ShowDialog();

        if (!string.IsNullOrEmpty(opf.SafeFileName))
        {
            var uri = new Uri(opf.FileName, UriKind.Absolute);
            _backgroundImage = new BitmapImage(uri);
            if (_backgroundImage == null)
            {
                RootCanvas.Background = Brushes.White;
            }
            else
            {
                RootCanvas.Background = new ImageBrush(_backgroundImage);
            }

            CB_RootCanvasBackGround.SelectedIndex = 1;
            _isSave = false;
        }
    }

    /// <summary>
    ///     把圖形儲存成為 DrawingImage
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SaveDrawingImage_Click(object sender, RoutedEventArgs e)
    {
        var save = new SaveFileDialog
        {
            Filter = "XML Files |*.xml"
        };

        if ((bool)save.ShowDialog()) // 選擇要保存的檔案名
        {
            FileName = save.FileName;

            var streamWriter = new StreamWriter(FileName);
            var geometryStringConverter = new GeometryStringConverter(VM, RootCanvas, _graphAppearance);
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>");
            stringBuilder.AppendLine("<DrawingImage >");
            stringBuilder.AppendLine("  <DrawingImage.Drawing>");
            stringBuilder.AppendLine("      <DrawingGroup>");

            foreach (UIElement item in RootCanvas.Children)
            {
                if (item is BorderWithAdorner borderWithAdorner) // 點是有 BorderWithDrag 包含著的，圖形是 Path
                {
                    stringBuilder.AppendLine(geometryStringConverter.StringFromPathGeometry(borderWithAdorner.Child as GeometryPath.Path)); // 生成 Mini-Language
                }
            }

            stringBuilder.AppendLine("      </DrawingGroup>");
            stringBuilder.AppendLine("  </DrawingImage.Drawing>");
            stringBuilder.AppendLine("</DrawingImage>");
            streamWriter.Write(stringBuilder.ToString());
            streamWriter.Close();
            _isSave = true;
        }
    }

    /// <summary>
    ///     開啟圖形，並繪製成為 Path
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OpenDrawingImage_Click(object sender, RoutedEventArgs e)
    {
        var messageBoxResult = MessageBox.Show(" 是否要保存当前文件", "", MessageBoxButton.YesNoCancel);
        if (messageBoxResult == MessageBoxResult.Yes)
        {
            SaveXML_Click(null, null);
            _isSave = true;
        }
        else if (messageBoxResult == MessageBoxResult.No)
        {
            var openFileDialog = new OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "xml file|*.xml" // 只選擇 .xml 檔
            };
            if (openFileDialog.ShowDialog() == true)  // 打開對話框
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName)) // 如果檔案名稱不為空
                {
                    var geometryStringConverter = new GeometryStringConverter(VM, RootCanvas, _graphAppearance);
                    var streamReader = new StreamReader(openFileDialog.FileName);
                    var xmlString = streamReader.ReadToEnd();
                    var borderWithAdornerList = geometryStringConverter.PathGeometryFromString(xmlString);
                    foreach (var borderWithAdorner in borderWithAdornerList)
                    {
                        AddGeometryIntoCanvas(borderWithAdorner, 0, 0);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     打開 XML 檔，讀取 XML 中的圖形
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Open_Click(object sender, RoutedEventArgs e)
    {
        if (_isSave == false)
        {
            var messageBoxResult = MessageBox.Show(" 是否要儲存當前檔案", "", MessageBoxButton.YesNoCancel);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                SaveXML_Click(null, null);
                RootCanvas.Children.Clear();
                _isSave = true;
            }
            else if (messageBoxResult == MessageBoxResult.No)
            {
                var openFileDlg = new OpenFileDialog
                {
                    DefaultExt = ".xml",
                    Filter = "xml file|*.xml" // 只選擇 .xml 檔
                };
                if (openFileDlg.ShowDialog() == true)  // 打開對話框
                {
                    if (!string.IsNullOrEmpty(openFileDlg.FileName)) // 如果檔案名稱不為空
                    {
                        var xmlHelper = new XMLHelper();
                        var geometryStringConverter = new GeometryStringConverter(VM, RootCanvas, _graphAppearance);
                        var match = xmlHelper.ReadXml(openFileDlg.FileName); // 讀取 XML 檔
                        var matchList = Regex.Matches(match.Groups[0].ToString(), @"M[\.\,\s\+\-\dLACQZ]+");
                        foreach (Match item in matchList.Cast<Match>())
                        {
                            var borderWithAdorner =
                                geometryStringConverter.GeometryFromString(item.Groups[0].ToString()); // 轉化成為圖形
                            RootCanvas.Children.Add(borderWithAdorner); // 把圖形增加到 Canvas 中
                            foreach (var ellipse in borderWithAdorner.EllipseList) // 把點增加到 Canvas 中
                            {
                                var borderWithDrag = ellipse.Parent as BorderWithDrag;
                                RootCanvas.Children.Add(borderWithDrag);
                                var borderLock = new BorderLock(borderWithDrag);
                                borderLock.Lock(((borderWithDrag.Child as GeometryPath.Path).Data as EllipseGeometry)
                                    .Center);
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Canvas 點擊操作

    /// <summary>
    ///     當前鼠標的模式
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Select_Click(object sender, RoutedEventArgs e)
    {
        // 設置選定的模式
        var radioButton = sender as RadioButton;
        if (radioButton != null)
        {
            VM.ActionMode = radioButton.Tag.ToString();
        }

        if (_isStartPoint != 0 && _pathFigure.Segments.Count > 0) // 移除額外的線
        {
            _pathFigure.Segments.RemoveAt(_pathFigure.Segments.Count - 1);
        }

        if (VM.ActionMode != "Point") // 移除畫線功能
        {
            RootCanvas.RemoveHandler(MouseMoveEvent, new MouseEventHandler(DrawLine));
            _ellipseList = new List<GeometryPath.Path>();
            if (_isStartPoint != 0)
            {
                _isStartPoint = 0;
            }
        }

        if (VM.ActionMode != "Select")
        {
            NowSelectedLabel.Content = "Brush Properties";
            PanProperty.DataContext = _graphAppearance;
            SS_SliderStyle.Value = _graphAppearance.StrokeThickness * 10;
            SD_StrokeDash1.Value = _graphAppearance.StrokeDashArray[0];
            SD_StrokeDash2.Value = _graphAppearance.StrokeDashArray[1];
        }

        if (VM.SelectedBorder != null) // 隱藏之前點擊的圖形的選擇框
        {
            VM.SelectedBorder.GeometryAdorner.Visibility = Visibility.Hidden;
        }

        VM.SelectedBorder = null;
        e.Handled = true;
    }

    /// <summary>
    ///     執行鼠標的操作，例如選擇，增加點、連線等
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RootCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        var p = new AutoPoints().GetAutoAdsorbPoint(Mouse.GetPosition(e.Source as FrameworkElement)); // 獲取鼠標當前位置

        if (VM.ActionMode == "Point") // 判斷是不是畫線
        {
            _isClose = false;
            if (_isStartPoint == 0)
            {
                _pathFigure = new PathFigure();
                _graphAdd.AddPointWithNoBorder(p, _graphAppearance, RootCanvas, out _ellipsePath); // 進行畫點
                _graphAdd.AddLine(_graphAppearance, RootCanvas, ref _linePath, _ellipsePath, ref _isStartPoint,
                    ref _pathFigure, _isClose); // 進行畫線
                RootCanvas.AddHandler(MouseMoveEvent, new MouseEventHandler(DrawLine));
            }
            else
            {
                _isStartPoint++;
                _graphAdd.AddPointWithNoBorder(p, _graphAppearance, RootCanvas, out _ellipsePath); // 進行畫點
                _graphAdd.AddLine(_graphAppearance, RootCanvas, ref _linePath, _ellipsePath, ref _isStartPoint,
                    ref _pathFigure, _isClose); // 進行畫線
            }

            _ellipseList.Add(_ellipsePath);
            var border = new BorderWithDrag(VM, _linePath, _isStartPoint, _ellipseList);
            border.Child = _ellipsePath;
            RootCanvas.Children.Add(border);
            _isSave = false;
            e.Handled = true;
        }

        if (VM.SelectedBorder != null) // 設置右側畫板屬性
        {
            PanProperty.DataContext = VM.SelectedBorder.Child;
            NowSelectedLabel.Content = "Graph Properties";
            SS_SliderStyle.Value = (VM.SelectedBorder.Child as GeometryPath.Path).StrokeThickness * 10;
            SD_StrokeDash1.Value = (VM.SelectedBorder.Child as GeometryPath.Path).StrokeDashArray[0];
            SD_StrokeDash2.Value = (VM.SelectedBorder.Child as GeometryPath.Path).StrokeDashArray[1];
            Select.IsChecked = true;
            var borderWithAdorner = VM.SelectedBorder;
            var path = borderWithAdorner.Child as GeometryPath.Path;
            var pg = path.Data as PathGeometry;
            var arcSegment = pg.Figures[0].Segments[0] as ArcSegment;

            if (arcSegment != null && pg.Figures[0].Segments.Count == 1) // 設置曲線屬性
            {
                //var isClockwise = arcSegment.SweepDirection == SweepDirection.Clockwise ? true : false;
                //var isLargeArc = arcSegment.IsLargeArc;
                //var rotationAngle = arcSegment.RotationAngle;
                //var size = arcSegment.Size;
            }
        }
    }

    /// <summary>
    ///     鼠標移動的時候，畫一條線段
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DrawLine(object sender, MouseEventArgs e)
    {
        if (VM.ActionMode == "Point")
        {
            //isStartPoint = 1;
            var p = Mouse.GetPosition(e.Source as FrameworkElement); // 獲取鼠標當前位置
            _graphAdd.AddHorvorLine(_pathFigure, p); // 進行畫線
        }
    }

    /// <summary>
    ///     修改圖形的位置
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RootCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_canMove)
        {
            var p = new AutoPoints().GetAutoAdsorbPoint(e.GetPosition(RootCanvas)); // 獲取當前鼠標的位置
            if (VM.ActionMode == "AddTriangle") // 修改三角形的位置
            {
                if (_trianglePath != null)
                {
                    var triangle = _trianglePath.Data as PathGeometry;
                    var line2 = triangle.Figures[0].Segments[1] as LineSegment;
                    var oldPoint = line2.Point;
                    line2.Point = new Point { X = p.X, Y = p.Y };
                    var line1 = triangle.Figures[0].Segments[0] as LineSegment;
                    oldPoint = triangle.Figures[0].StartPoint;
                    line1.Point = new Point { X = oldPoint.X + (oldPoint.X - p.X), Y = p.Y };
                    e.Handled = true;
                }
            }
            else if (VM.ActionMode == "AddRectangular") // 修改矩形的位置
            {
                var triangle = _rectanglePath.Data as PathGeometry;
                var oldPaint = triangle.Figures[0].StartPoint;

                var line1 = triangle.Figures[0].Segments[0] as LineSegment;
                line1.Point = new Point { X = oldPaint.X, Y = p.Y };
                var line2 = triangle.Figures[0].Segments[1] as LineSegment;
                line2.Point = new Point { X = p.X, Y = p.Y };
                var line3 = triangle.Figures[0].Segments[2] as LineSegment;
                line3.Point = new Point { X = p.X, Y = oldPaint.Y };
                e.Handled = true;
            }
            else if (VM.ActionMode == "AddCircle") // 修改圓的位置
            {
                var circle = _circlePath.Data as PathGeometry;
                var line1 = circle.Figures[0].Segments[1] as ArcSegment;
                line1.Point = new Point { X = p.X, Y = p.Y };
                e.Handled = true;
            }
            else if (VM.ActionMode == "AddEllipse") // 修改橢圓的位置
            {
                var circle = _ellipseGeometryPath.Data as PathGeometry;
                var line1 = circle.Figures[0].Segments[0] as ArcSegment;
                var line2 = circle.Figures[0].Segments[1] as ArcSegment;
                var oldPoint1 = line1.Point;
                var oldPoint2 = line2.Point;
                line1.Point = new Point { X = p.X, Y = oldPoint1.Y };
                if (oldPoint2.X - oldPoint1.X != 0 && p.Y - oldPoint1.Y != 0) // 保證被除數被為 0
                {
                    line1.Size = new Size
                    { Width = Math.Abs(oldPoint2.X - oldPoint1.X) / 2.0, Height = Math.Abs(p.Y - oldPoint1.Y) };
                    line2.Size = new Size
                    { Width = Math.Abs(oldPoint2.X - oldPoint1.X) / 2.0, Height = Math.Abs(p.Y - oldPoint1.Y) };
                }
                else if (oldPoint2.X - oldPoint1.X != 0)
                {
                    line1.Size = new Size { Width = Math.Abs(oldPoint2.X - oldPoint1.X) / 2.0 };
                    line2.Size = new Size { Width = Math.Abs(oldPoint2.X - oldPoint1.X) / 2.0 };
                }
                else if (p.Y - oldPoint1.Y != 0)
                {
                    line1.Size = new Size { Height = Math.Abs(p.Y - oldPoint1.Y) };
                    line2.Size = new Size { Height = Math.Abs(p.Y - oldPoint1.Y) };
                }

                e.Handled = true;
            }
            else if (VM.ActionMode == "QBezier") // 修改二次方貝茲曲線的位置
            {
                var qBezier = _qBezierPath.Data as PathGeometry;
                var qbSegment = qBezier.Figures[0].Segments[0] as QuadraticBezierSegment;
                var oldPoint = qbSegment.Point1;
                var oldPoint2 = qbSegment.Point2;
                qbSegment.Point1 = new Point { X = (p.X + oldPoint.X) / 2.0, Y = p.Y };
                qbSegment.Point2 = new Point { X = p.X, Y = oldPoint2.Y };
            }
            else if (VM.ActionMode == "Bezier") // 修改三次方貝茲曲線的位置
            {
                var bezier = _bezierPath.Data as PathGeometry;
                var bSegment = bezier.Figures[0].Segments[0] as BezierSegment;
                var oldPoint = bSegment.Point1;
                var oldPoint2 = bSegment.Point2;
                var oldPoint3 = bSegment.Point3;
                bSegment.Point1 = new Point { X = oldPoint.X, Y = p.Y };
                bSegment.Point2 = new Point { X = p.X, Y = p.Y };
                bSegment.Point3 = new Point { X = p.X, Y = oldPoint3.Y };
            }
        }
    }

    /// <summary>
    ///     鼠標左擊時，拖動圖形移動
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RootCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var geometryStringConverter = new GeometryStringConverter(VM, RootCanvas, _graphAppearance);

        if (VM.SelectedBorder != null && VM.SelectedBorder.GeometryAdorner != null) // 隱藏之前點擊的圖形的選擇框
        {
            VM.SelectedBorder.GeometryAdorner.Visibility = Visibility.Hidden;
        }

        if (VM.ActionMode == "Select")
        {
            var p = Mouse.GetPosition(e.Source as FrameworkElement);
            VisualTreeHelper.HitTest(RootCanvas, null, // 進行命中測試
                MyHitTestResult,
                new PointHitTestParameters(p));
        }
        else if (VM.ActionMode == "AddTriangle") // 繪製三角形
        {
            var p = new AutoPoints().GetAutoAdsorbPoint(Mouse.GetPosition(e.Source as FrameworkElement));
            var miniLanguage = "M " + p.X + "," + p.Y + " L " + p.X + "," + p.Y + " L " + p.X + "," + p.Y + " Z";

            VM.SelectedBorder = geometryStringConverter.GeometryFromString(miniLanguage);
            _trianglePath = VM.SelectedBorder.Child as GeometryPath.Path;
            AddGeometryIntoCanvas(VM.SelectedBorder, 0, 0);
            PanProperty.DataContext = _trianglePath;
            _canMove = true;
        }
        else if (VM.ActionMode == "AddRectangular") // 繪製矩形
        {
            var p = new AutoPoints().GetAutoAdsorbPoint(Mouse.GetPosition(e.Source as FrameworkElement));
            var miniLanguage = "M " + p.X + "," + p.Y + " L " + p.X + "," + p.Y + " L " + p.X + "," + p.Y + " L " +
                               p.X + "," + p.Y + " Z";

            VM.SelectedBorder = geometryStringConverter.GeometryFromString(miniLanguage);
            _rectanglePath = VM.SelectedBorder.Child as GeometryPath.Path;
            AddGeometryIntoCanvas(VM.SelectedBorder, 0, 0);
            PanProperty.DataContext = _rectanglePath;
            _canMove = true;
        }
        else if (VM.ActionMode == "AddCircle") // 繪製圓形
        {
            var p = new AutoPoints().GetAutoAdsorbPoint(Mouse.GetPosition(e.Source as FrameworkElement));
            VM.SelectedBorder = _graphAdd.AddGeometryOfCircle(p, _graphAppearance, RootCanvas, out _circlePath);
            PanProperty.DataContext = _circlePath;
            _canMove = true;
        }
        else if (VM.ActionMode == "AddEllipse") // 繪製橢圓形
        {
            var p = new AutoPoints().GetAutoAdsorbPoint(Mouse.GetPosition(e.Source as FrameworkElement));
            VM.SelectedBorder = _graphAdd.AddGeometryOfCircle(p, _graphAppearance, RootCanvas, out _ellipseGeometryPath);
            PanProperty.DataContext = _ellipseGeometryPath;
            _canMove = true;
        }
        else if (VM.ActionMode == "QBezier") // 繪製二次方貝茲曲線
        {
            var p = new AutoPoints().GetAutoAdsorbPoint(Mouse.GetPosition(e.Source as FrameworkElement));
            var miniLanguage = "M " + p.X + "," + p.Y + " Q " + p.X + "," + p.Y
                               + " " + p.X + "," + p.Y;
            VM.SelectedBorder = geometryStringConverter.GeometryFromString(miniLanguage);
            _qBezierPath = VM.SelectedBorder.Child as GeometryPath.Path;
            AddGeometryIntoCanvas(VM.SelectedBorder, 0, 0);
            PanProperty.DataContext = _qBezierPath;
            _canMove = true;
        }
        else if (VM.ActionMode == "Bezier") // 繪製三次方貝茲曲線
        {
            var p = new AutoPoints().GetAutoAdsorbPoint(Mouse.GetPosition(e.Source as FrameworkElement));
            var miniLanguage = "M " + p.X + "," + p.Y + " C " + p.X + "," + p.Y
                               + " " + p.X + "," + p.Y + " " + p.X + "," + p.Y;
            VM.SelectedBorder = geometryStringConverter.GeometryFromString(miniLanguage);
            _bezierPath = VM.SelectedBorder.Child as GeometryPath.Path;
            AddGeometryIntoCanvas(VM.SelectedBorder, 0, 0);
            PanProperty.DataContext = _bezierPath;
            _canMove = true;
        }

        _isSave = false;
    }

    /// <summary>
    ///     圖形更改為不能拖動
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RootCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_canMove) // 圖形更改為不能拖動
        {
            BorderWithAdorner borderWithAdorner = null;
            if (VM.ActionMode == "AddCircle") // 繪製圖
            {
                borderWithAdorner = _circlePath.Parent as BorderWithAdorner;
            }
            else if (VM.ActionMode == "AddEllipse") // 後綴橢圓形
            {
                borderWithAdorner = _ellipseGeometryPath.Parent as BorderWithAdorner;
            }

            if (borderWithAdorner != null && borderWithAdorner.EllipseList[0].Parent is BorderWithDrag borderWithDrag)
            {
                var borderLock = new BorderLock(borderWithDrag);
                borderLock.Lock(((borderWithDrag.Child as GeometryPath.Path).Data as EllipseGeometry).Center);
            }

            VM.SelectedBorder.ShowAdorner();
            VM.ActionMode = "Select";
            _canMove = false;
        }

        if (VM.SelectedBorder != null && VM.SelectedBorder.Child != null)
        {
            PanProperty.DataContext = VM.SelectedBorder.Child;
            NowSelectedLabel.Content = "Graph Properties";
            SS_SliderStyle.Value = (VM.SelectedBorder.Child as GeometryPath.Path).StrokeThickness * 10;
            SD_StrokeDash1.Value = (VM.SelectedBorder.Child as GeometryPath.Path).StrokeDashArray[0];
            SD_StrokeDash2.Value = (VM.SelectedBorder.Child as GeometryPath.Path).StrokeDashArray[1];
            Select.IsChecked = true;
        }
        else
        {
            PanProperty.DataContext = _graphAppearance;
            NowSelectedLabel.Content = "Brush Properties";
            SS_SliderStyle.Value = _graphAppearance.StrokeThickness * 10;
            SD_StrokeDash1.Value = _graphAppearance.StrokeDashArray[0];
            SD_StrokeDash2.Value = _graphAppearance.StrokeDashArray[1];
        }
    }

    /// <summary>
    ///     右擊鼠標，變成選擇模式
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RootCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (VM.ActionMode != "Select")
        {
            Select.IsChecked = true; // 更正鼠標模式為選擇模式
            _ellipseList = new List<GeometryPath.Path>();
            VM.ActionMode = "Select";
            if (_isStartPoint != 0 && _pathFigure.Segments.Count > 0) // 移除額外的線
            {
                _pathFigure.Segments.RemoveAt(_pathFigure.Segments.Count - 1);
            }

            if (VM.ActionMode != "Point") // 移除畫線功能
            {
                RootCanvas.RemoveHandler(MouseMoveEvent, new MouseEventHandler(DrawLine));
                if (_isStartPoint != 0)
                {
                    _isStartPoint = 0;
                }
            }

            e.Handled = true;
        }
    }

    #endregion

    #region 右側屬性面板操作

    /// <summary>
    ///     設置 Stroke 的顏色
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StrokeColors_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Button button)
        {
            //StrokeCurrentColor.Background = button.Background;
            LineColor.CurrentColor = button.Background;
        }
    }

    /// <summary>
    ///     設置 Fill 的顏色
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FillColors_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is Button button)
        {
            //FillCurrentColor.Background = button.Background;
            FillColor.CurrentColor = button.Background;
        }
    }

    /// <summary>
    ///     调整画布，进行缩放
    /// </summary>
    private void CanvasChange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        try
        {
            if (CanvasBorder is null) return;

            VM.Multiple = (int)SD_CanvasChange.Value;
            double height = VM.GridSize * (int)SD_CanvasChange.Value + 100; //计算画布放大后的坐标
            double width = VM.GridSize * (int)SD_CanvasChange.Value + 100;
            CanvasBorder.Height = height >= CanvasBorder.ActualHeight ? height : G_Root.ActualHeight - 115; //修改Border的大小，使得其能显示放大后的画布
            CanvasBorder.Width  = width  >= CanvasBorder.ActualWidth  ? width  : G_Root.ActualWidth  - 215;

            if (CB_RootCanvasBackGround.SelectedIndex == 0)
            {
                DocCanvas_Loaded();
            }
            else if (CB_RootCanvasBackGround.SelectedIndex == 1)
            {
                if (_backgroundImage == null)
                {
                    RootCanvas.Background = Brushes.White;
                }
                else
                {
                    RootCanvas.Background = new ImageBrush(_backgroundImage);
                }
            }
            else
            {
                RootCanvas.Background = Brushes.White;
            }
        }
        catch
        {
            // 忽略
        }
    }

    /// <summary>
    ///     拖動 Slider 改變 StrokeThickness
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SliderStyle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        L_LineStyle.StrokeThickness = e.NewValue;
    }

    /// <summary>
    ///     選擇畫布的大小
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            var comboBoxItem = (ComboBoxItem)(sender as ComboBox).SelectedItem;
            if (comboBoxItem != null)
            {
                RootCanvas.Height = Convert.ToInt32(comboBoxItem.Tag);
                RootCanvas.Width = Convert.ToInt32(comboBoxItem.Tag);
                VM.GridSize = Convert.ToInt32(comboBoxItem.Tag);
                double height = VM.GridSize * (int)SD_CanvasChange.Value + 200; //计算画布放大后的坐标
                double width = VM.GridSize * (int)SD_CanvasChange.Value + 200;
                CanvasBorder.Height =
                    height >= CanvasBorder.ActualHeight
                        ? height
                        : G_Root.ActualHeight - 115; //修改Border的大小，使得其能显示放大后的画布
                CanvasBorder.Width = width >= CanvasBorder.ActualWidth ? width : G_Root.ActualWidth - 215;
                DocCanvas_Loaded();
            }

            _isSave = true;
        }
        catch
        {
            // 忽略
        }
    }

    /// <summary>
    ///     改變畫筆的每個實現的長度
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StrokeDash1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var oldStrokeDashArray = new DoubleCollection();
        oldStrokeDashArray.Add(e.NewValue);
        try
        {
            oldStrokeDashArray.Add(L_LineStyle.StrokeDashArray[1]);
            if (VM != null && VM.SelectedBorder != null)
            {
                _isSave = false;
            }
        }
        catch
        {
            oldStrokeDashArray.Add(0);
        }

        L_LineStyle.StrokeDashArray = oldStrokeDashArray;
    }

    /// <summary>
    ///     改變畫筆的每個虛線的長度
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void StrokeDash2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var oldStrokeDashArray = new DoubleCollection();
        try
        {
            oldStrokeDashArray.Add(L_LineStyle.StrokeDashArray[0]);
            if (VM != null && VM.SelectedBorder != null)
            {
                _isSave = false;
            }
        }
        catch
        {
            oldStrokeDashArray.Add(1);
        }

        oldStrokeDashArray.Add(e.NewValue);
        L_LineStyle.StrokeDashArray = oldStrokeDashArray;
    }

    /// <summary>
    ///     實現圖形的複製功能
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PasteItem_Click(object sender, RoutedEventArgs e)
    {
        var borderWithAdorner = new BorderWithAdorner(VM);
        var newBorderWithAdorner = borderWithAdorner.CopyBorder(VM.CopyBorderWithAdorner); //获取要粘贴的图形的副本
        AddGeometryIntoCanvas(newBorderWithAdorner, 2 * (VM.PasteCount + 1), 2 * (VM.PasteCount + 1));
        VM.PasteCount++;
        _isSave = false;
    }

    /// <summary>
    ///     把圖形放進 Root Canvas
    /// </summary>
    /// <param name="vBorderWithAdorner"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void AddGeometryIntoCanvas(BorderWithAdorner vBorderWithAdorner, int x, int y)
    {
        RootCanvas.Children.Add(vBorderWithAdorner);
        foreach (var item in vBorderWithAdorner.EllipseList) //修改图形的点的位置，并把他放进Canvas
        {
            var p = (item.Data as EllipseGeometry).Center;
            (item.Data as EllipseGeometry).Center = new Point { X = p.X + x, Y = p.Y + y };
            RootCanvas.Children.Add(item.Parent as BorderWithDrag);
        }
    }

    /// <summary>
    ///     選擇畫板
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void RootCanvasBackGround_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (CB_RootCanvasBackGround.SelectedIndex)
        {
            case 0:
                DocCanvas_Loaded();
                break;
            case 1:
                RootCanvas.Background = _backgroundImage == null ? Brushes.White : new ImageBrush(_backgroundImage);
                break;
            default:
                RootCanvas.Background = Brushes.White;
                break;
        }
    }

    #endregion

    #region 圖形操作

    /// <summary>
    ///     建立一個新的畫板
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CreateNewCanvas_Click(object sender, RoutedEventArgs e)
    {
        if (_isSave == false)
        {
            var messageBoxResult = MessageBox.Show("Do you want to save the current file?", "", MessageBoxButton.YesNoCancel);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                SaveXML_Click(null, null);
                RootCanvas.Children.Clear();
                _isSave = true;
            }
            else if (messageBoxResult == MessageBoxResult.No)
            {
                RootCanvas.Children.Clear();
                _isSave = true;
            }
        }
    }

    private void VerticalOverturn_Click(object sender, MouseButtonEventArgs e)
    {
        Flip(FlipDirection.Vertical);
        e.Handled = true;
    }

    private void HorizontalOverturn_Click(object sender, MouseButtonEventArgs e)
    {
        Flip(FlipDirection.Horizontal);
        e.Handled = true;
    }

    private void RightMirror_Click(object sender, MouseButtonEventArgs e)
    {
        Mirror(MirrorDirection.Right);
        e.Handled = true;
    }

    private void LeftMirror_Click(object sender, MouseButtonEventArgs e)
    {
        Mirror(MirrorDirection.Left);
        e.Handled = true;
    }

    private void BottomMirror_Click(object sender, MouseButtonEventArgs e)
    {
        Mirror(MirrorDirection.Bottom);
        e.Handled = true;
    }

    private void TopMirror_Click(object sender, MouseButtonEventArgs e)
    {
        Mirror(MirrorDirection.Top);
        e.Handled = true;
    }

    #endregion

    #region 被调用的方法

    private void Mirror(MirrorDirection direction)
    {
        if (VM != null && VM.SelectedBorder != null)
        {
            var borderWithAdorner = VM.SelectedBorder;
            borderWithAdorner.GetFourPoint(borderWithAdorner); // 計算這個圖形四個角落的位置
            double deltaX = 0;
            double deltaY = 0;

            var newBorderWithAdorner = borderWithAdorner.CopyBorder(borderWithAdorner); // 獲取鏡像的圖形的複本
            RootCanvas.Children.Add(newBorderWithAdorner);
            foreach (var item in newBorderWithAdorner.EllipseList) // 修改圖形的點的位置，並把它放進 Canvas
            {
                var p = (item.Data as EllipseGeometry).Center;

                switch (direction)
                {
                    case MirrorDirection.Right:
                        deltaX = -2 * (p.X - borderWithAdorner.MaxX);
                        break;
                    case MirrorDirection.Left:
                        deltaX = -2 * (p.X - borderWithAdorner.MinX);
                        break;
                    case MirrorDirection.Bottom:
                        deltaY = -2 * (p.Y - borderWithAdorner.MaxY);
                        break;
                    case MirrorDirection.Top:
                        deltaY = -2 * (p.Y - borderWithAdorner.MinY);
                        break;
                }

                (item.Data as EllipseGeometry).Center = new Point { X = p.X + deltaX, Y = p.Y + deltaY };
                RootCanvas.Children.Add(item.Parent as BorderWithDrag);
            }

            _isSave = false;
        }
    }

    private void Flip(FlipDirection direction)
    {
        if (VM != null && VM.SelectedBorder != null)
        {
            var borderWithAdorner = VM.SelectedBorder;
            borderWithAdorner.GetFourPoint(borderWithAdorner); // 計算這個圖形四個角落的位置
            double centerX = (borderWithAdorner.MinX + borderWithAdorner.MaxX) / 2.0;
            double centerY = (borderWithAdorner.MinY + borderWithAdorner.MaxY) / 2.0;

            foreach (var item in borderWithAdorner.EllipseList) // 修改圖形的點的位置
            {
                var borderWithDrag = item.Parent as BorderWithDrag;
                if (borderWithDrag.HasOtherPoint)
                {
                    continue;
                }

                var p = (item.Data as EllipseGeometry).Center;
                double newX = p.X;
                double newY = p.Y;

                if (direction == FlipDirection.Vertical)
                {
                    newY = centerY - (p.Y - centerY);
                }
                else if (direction == FlipDirection.Horizontal)
                {
                    newX = centerX - (p.X - centerX);
                }

                (item.Data as EllipseGeometry).Center = new Point { X = newX, Y = newY };
            }

            _isSave = false;
        }
    }

    /// <summary>
    ///     顯示被選中的圖形選擇框
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public HitTestResultBehavior MyHitTestResult(HitTestResult result)
    {
        if (result.VisualHit is GeometryPath.Path path)
        {
            if (path.Parent is BorderWithAdorner borderWithAdorner)
            {
                borderWithAdorner.GeometryAdorner.Visibility = Visibility.Visible;
                VM.SelectedBorder = borderWithAdorner;
                return HitTestResultBehavior.Stop;
            }

            return HitTestResultBehavior.Stop;
        }

        VM.SelectedBorder = null;

        return HitTestResultBehavior.Continue;
    }

    /// <summary>
    ///     綁定路由事件
    /// </summary>
    private void AddCommand()
    {
        CommandBindings.Add(new CommandBinding
        (
            RoutedCommands.PasteCommand,
            PasteItem_Click,
            (sender, e) =>
            {
                if (CanPaste)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }
        ));
        CommandBindings.Add(new CommandBinding
        (
            RoutedCommands.SaveCommand,
            Save_Click,
            (sender, e) => e.CanExecute = true
        ));
        CommandBindings.Add(new CommandBinding
        (
            RoutedCommands.NewCommand,
            CreateNewCanvas_Click,
            (sender, e) => e.CanExecute = true
        ));
        //CommandBindings.Add(new CommandBinding
        //(
        //    RoutedCommands.OpenCommand,
        //    Open_Click,
        //    (sender, e) => e.CanExecute = true
        //));

        CommandBindings.Add(new CommandBinding
        (
            RoutedCommands.CopyCommand,
            (sender, e) =>
            {
                if (VM.SelectedBorder != null && VM.SelectedBorder.GeometryAdorner != null)
                {
                    VM.SelectedBorder.GeometryAdorner.GeometryControl.CopyItem_Click(sender, e);
                }
            },
            (sender, e) =>
            {
                if (VM.SelectedBorder != null && VM.SelectedBorder.GeometryAdorner != null)
                {
                    e.CanExecute = true;
                }
            }
        ));
        CommandBindings.Add(new CommandBinding
        (
            RoutedCommands.CutCommand,
            (sender, e) =>
            {
                if (VM.SelectedBorder != null && VM.SelectedBorder.GeometryAdorner != null)
                {
                    VM.SelectedBorder.GeometryAdorner.GeometryControl.CutItem_Click(sender, e);
                }
            },
            (sender, e) =>
            {
                if (VM.SelectedBorder != null && VM.SelectedBorder.GeometryAdorner != null)
                {
                    e.CanExecute = true;
                }
            }
        ));
        CommandBindings.Add(new CommandBinding
        (
            RoutedCommands.DeleteCommand,
            (sender, e) =>
            {
                if (VM.SelectedBorder != null && VM.SelectedBorder.GeometryAdorner != null)
                {
                    VM.SelectedBorder.GeometryAdorner.GeometryControl.DeleteItem_Click(sender, e);
                }
            },
            (sender, e) =>
            {
                if (VM.SelectedBorder != null && VM.SelectedBorder.GeometryAdorner != null)
                {
                    e.CanExecute = true;
                }
            }
        ));

    }


    #endregion

    private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (_isSave == false)
        {
            var messageBoxResult = MessageBox.Show(" 是否要儲存當前檔案", "", MessageBoxButton.YesNoCancel);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                SaveXML_Click(null, null);
                RootCanvas.Children.Clear();
                _isSave = true;
            }
            else if (messageBoxResult == MessageBoxResult.No)
            {
                var openFileDlg = new OpenFileDialog
                {
                    DefaultExt = ".xml",
                    Filter = "xml file|*.xml" // 只選擇 .xml 檔
                };
                if (openFileDlg.ShowDialog() == true)  // 打開對話框
                {
                    if (!string.IsNullOrEmpty(openFileDlg.FileName)) // 如果檔案名稱不為空
                    {
                        var xmlHelper = new XMLHelper();
                        var geometryStringConverter = new GeometryStringConverter(VM, RootCanvas, _graphAppearance);
                        var match = xmlHelper.ReadXml(openFileDlg.FileName); // 讀取 XML 檔
                        var matchList = Regex.Matches(match.Groups[0].ToString(), @"M[\.\,\s\+\-\dLACQZ]+");
                        foreach (Match item in matchList.Cast<Match>())
                        {
                            var borderWithAdorner =
                                geometryStringConverter.GeometryFromString(item.Groups[0].ToString()); // 轉化成為圖形
                            RootCanvas.Children.Add(borderWithAdorner); // 把圖形增加到 Canvas 中
                            foreach (var ellipse in borderWithAdorner.EllipseList) // 把點增加到 Canvas 中
                            {
                                var borderWithDrag = ellipse.Parent as BorderWithDrag;
                                RootCanvas.Children.Add(borderWithDrag);
                                var borderLock = new BorderLock(borderWithDrag);
                                borderLock.Lock(((borderWithDrag.Child as GeometryPath.Path).Data as EllipseGeometry)
                                    .Center);
                            }
                        }
                    }
                }
            }
        }

    }
}
