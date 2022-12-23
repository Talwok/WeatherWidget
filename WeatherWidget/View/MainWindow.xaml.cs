using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Cache;
using System.Device.Location;

namespace WeatherWidget;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    private const string imgUrl = "https://openweathermap.org/img/w/";

    private IntPtr Handle
    {
        get
        {
            return new WindowInteropHelper(this).Handle;
        }
    }

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr window, int index, int value);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr window, int index);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(int hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    public const int HWND_BOTTOM = 0x1;
    public const uint SWP_NOSIZE = 0x1;
    public const uint SWP_NOMOVE = 0x2;
    public const uint SWP_SHOWWINDOW = 0x40;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void WeatherWidget_Loaded(object sender, RoutedEventArgs e)
    {
        HideFromAltTab(Handle);
        
        PopupNotification.HorizontalOffset = SystemParameters.PrimaryScreenWidth;
        PopupNotification.VerticalOffset = SystemParameters.PrimaryScreenHeight;
    }

    private void WeatherWidget_Activated(object sender, EventArgs e)
    {
        ShoveToBackground();
    }

    public static void HideFromAltTab(IntPtr Handle)
    {
        SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);
    }

    private void ShoveToBackground()
    {
        SetWindowPos((int)this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void PopupNotification_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        PopupNotification.IsOpen = false;
    }

    private void WeatherWidget_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        DragMove();

        if (Top + Height >= SystemParameters.PrimaryScreenHeight)
        {
            Top = SystemParameters.PrimaryScreenHeight - Height;
        }
        if (Top <= 0)
        {
            Top = 0;
        }
        if (Left + Width >= SystemParameters.PrimaryScreenWidth)
        {
            Left = SystemParameters.PrimaryScreenWidth - Width;
        }
        if (Left <= 0)
        {
            Left = 0;
        }
    }
}