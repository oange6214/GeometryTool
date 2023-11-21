using CommunityToolkit.Mvvm.ComponentModel;
using MainGui.UI.Borders;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace MainGui.Mvvm.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public ObservableCollection<UIElement> CanvasChildren { get; private set; }


    [ObservableProperty]
    private Canvas _myRootCanvas;

    /// <summary>
    /// 表示裝著當前圖形的 Canvas
    /// </summary>
    //public Canvas MyRootCanvas { set; get; }

    /// <summary>
    /// 表示當前和之前選擇中的圖形
    /// </summary>
    public BorderWithAdorner SelectedBorder { set; get; }

    /// <summary>
    /// 用於儲存剪切或者複製的圖形
    /// </summary>
    public BorderWithAdorner CopyBorderWithAdorner { set; get; }

    /// <summary>
    /// 表示當前圖形被複製了多少遍
    /// </summary>
    public int PasteCount { set; get; }

    /// <summary>
    /// 表示當前鼠標的模式
    /// </summary>
    public string ActionMode { set; get; } = "";

    /// <summary>
    /// 表示畫板大小
    /// </summary>
    public int GridSize { set; get; }

    /// <summary>
    /// 表示最底層大小
    /// </summary>
    public int LowestLevel { set; get; } = -1;

    /// <summary>
    /// 表示最頂層大小
    /// </summary>
    public int HightestLevel { set; get; } = 1;

    /// <summary>
    /// 表示面板放大的倍數
    /// </summary>
    public int Multiple { set; get; } = 1;


    public MainWindowViewModel()
    {
        CanvasChildren = new ObservableCollection<UIElement>();
        ActionMode = "Select";
    }
}
