using System.Windows.Input;

namespace MainGui.Mvvm.Commnads;


/// <summary>
///     定義快捷鍵的路由事件
/// </summary>
public static class RoutedCommands
{
    private static RoutedUICommand CreateRoutedUICommand(string name, string text, Key key, ModifierKeys modifiers)
    {
        return new RoutedUICommand(
            text,
            name,
            typeof(MainWindow),
            new InputGestureCollection { new KeyGesture(key, modifiers) }
        );
    }

    public static RoutedUICommand PasteCommand { get; } = CreateRoutedUICommand("Paste", "Paste", Key.V, ModifierKeys.Control);
    public static RoutedUICommand CopyCommand { get; } = CreateRoutedUICommand("Copy", "Copy", Key.C, ModifierKeys.Control);
    public static RoutedUICommand CutCommand { get; } = CreateRoutedUICommand("Cut", "Cut", Key.X, ModifierKeys.Control);
    public static RoutedUICommand DeleteCommand { get; } = CreateRoutedUICommand("Delete", "Delete", Key.Delete, ModifierKeys.None);
    public static RoutedUICommand SaveCommand { get; } = CreateRoutedUICommand("Save", "Save", Key.S, ModifierKeys.Control);
    public static RoutedUICommand NewCommand { get; } = CreateRoutedUICommand("New", "New", Key.N, ModifierKeys.Control);
    //public static RoutedUICommand OpenCommand { get; } = CreateRoutedUICommand("Open", "Open", Key.O, ModifierKeys.Control);
}