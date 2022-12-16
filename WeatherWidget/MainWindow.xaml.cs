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
    private Parameters parameters;
    private WeatherJson weather;
    private DispatcherTimer pollTimer;

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    private const string imgUrl = "https://openweathermap.org/img/w/";

    private readonly string language = "ru";
    private readonly string units = "metric";

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
        parameters = ParametersSerializer.Deserialize();
        AutorunItem.IsChecked = parameters.Autorun;
    }

    private void WeatherWidget_Loaded(object sender, RoutedEventArgs e)
    {
        HideFromAltTab(Handle);
        
        Left = parameters.Left ?? SystemParameters.PrimaryScreenWidth - Width;
        Top = parameters.Top ?? 0;

        PopupNotification.HorizontalOffset = SystemParameters.PrimaryScreenWidth;
        PopupNotification.VerticalOffset = SystemParameters.PrimaryScreenHeight;

        if (parameters.Notify)
        {
            NotifyButton.Content = new Image() { Source = new BitmapImage(new Uri("/Images/NotificationBell.png", UriKind.Relative)) };
        }

        CityBox.Text = parameters.LastCity;

        UpdateWeather();

        pollTimer = new DispatcherTimer();
        pollTimer.Interval = new TimeSpan(0, 5, 0);
        pollTimer.Tick += Timer_Tick;
        pollTimer.Start();
        Top = 0;
    }

    private void WeatherWidget_Activated(object sender, EventArgs e)
    {
        ShoveToBackground();
    }
    
    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateWeather();
    }

    public async void UpdateWeather()
    {
        ErrorIcon.Source = new BitmapImage();
        Error.Content = "";
        LoadingImage.Visibility = Visibility.Visible;
        bool IsCity = CityBox.Text != "";
        parameters.LastCity = CityBox.Text;
        bool wrongCity = false;
        while (true)
        {
            try
            {
                await Task.Run(async () =>
                {
                    using (var httpclient = new HttpClient())
                    {
                        if (IsCity)
                        {
                            wrongCity = false;
                            var response = await httpclient.GetAsync(new Uri($"https://api.openweathermap.org/data/2.5/weather?q={parameters.LastCity}&lang={language}&units={units}&appid={parameters.WheatherApiKey}")).ConfigureAwait(true);
                            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                            try
                            {
                                weather = JsonSerializer.Deserialize<WeatherJson>(result) ?? new WeatherJson();
                            }
                            catch (JsonException exception)
                            {
                                wrongCity = true;
                                return;
                            }
                        }
                        else
                        {
                            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
                            GeoCoordinate geolocation = new GeoCoordinate();
                            
                            bool search = true;
                            
                            watcher.PositionChanged += (sender, e) =>
                            {
                                geolocation = e.Position.Location;
                                watcher.Stop();
                                search = false;
                            };
                            watcher.Start();

                            while (search) { }

                            var response = await httpclient.GetAsync(new Uri($"https://api.openweathermap.org/data/2.5/weather?lat={geolocation.Latitude}&lon={geolocation.Longitude}&lang={language}&units={units}&appid={parameters.WheatherApiKey}")).ConfigureAwait(true);
                            string result = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

                            weather = JsonSerializer.Deserialize<WeatherJson>(result) ?? new WeatherJson();
                        }
                    }
                });

                if (wrongCity)
                {
                    Temperature.Content = "";
                    FeelsLike.Content = "";
                    Humidity.Content = "";
                    Description.Content = "";
                    WeatherIcon.Source = new BitmapImage();
                    WindSpeed.Content = "";
                    WindDirection.Content = "";
                    break;
                }

                Temperature.Content = string.Format("Температура: {0}°C", weather?.main.temp);
                FeelsLike.Content = string.Format("Ощущается как: {0}°C", weather?.main.feels_like);
                Humidity.Content = string.Format("Влажность: {0}%", weather?.main.humidity);
                Description.Content = string.Format("Небо: {0}", Regex.Replace(weather?.weather[0].description.ToLower() ?? "", @"\b[a-zа-яё]", m => m.Value.ToUpper()));
                WindSpeed.Content = string.Format("Скорость ветра: {0} м/с", weather?.wind.speed);
                WindDirection.Content = string.Format("Направление: {0}", GetWindDirectionByDegrees(weather?.wind.deg));

                string pathToImage = string.Concat(imgUrl, weather?.weather[0].icon, ".png");

                if ((WeatherIcon.Source?.ToString() ?? "") != pathToImage)
                {
                    WeatherIcon.Source = new BitmapImage(new Uri(pathToImage), new RequestCachePolicy(RequestCacheLevel.Reload));
                }

                if (parameters.Notify)
                {
                    PopupNotification.IsOpen = true;
                }

                ErrorIcon.Source = new BitmapImage(new Uri(@"/Images/Ok.png", UriKind.Relative));
                Error.Content = "";
                break;
            }
            catch (HttpRequestException exc)
            {
                ErrorIcon.Source = new BitmapImage(new Uri(@"/Images/Error.png", UriKind.Relative));
                if (exc.StatusCode.ToString() != "")
                {
                    Error.Content = exc.StatusCode;
                }
                else
                {
                    Error.Content = "No connection";
                }
                continue;
            }
        }
        LoadingImage.Visibility = Visibility.Collapsed;
    }

    public string GetWindDirectionByDegrees(int? degrees)
    {
        if (degrees >= 349 & degrees < 11)
        {
            return "С";
        }
        else if (degrees >= 11 & degrees < 34)
        {
            return "ССВ";
        }
        else if (degrees >= 34 & degrees < 56)
        {
            return "СВ";
        }
        else if (degrees >= 56 & degrees < 79)
        {
            return "ВСВ";
        }
        else if (degrees >= 79 & degrees < 101)
        {
            return "В";
        }
        else if (degrees >= 101 & degrees < 124)
        {
            return "ВЮВ";
        }
        else if (degrees >= 124 & degrees < 146)
        {
            return "ЮВ";
        }
        else if (degrees >= 146 & degrees < 169)
        {
            return "ЮЮВ";
        }
        else if (degrees >= 169 & degrees < 191)
        {
            return "Ю";
        }
        else if (degrees >= 191 & degrees < 214)
        {
            return "ЮЮЗ";
        }
        else if (degrees >= 214 & degrees < 236)
        {
            return "ЮЗ";
        }
        else if (degrees >= 236 & degrees < 259)
        {
            return "ЗЮЗ";
        }
        else if (degrees >= 259 & degrees < 281)
        {
            return "З";
        }
        else if (degrees >= 281 & degrees < 303)
        {
            return "ЗСЗ";
        }
        else if (degrees >= 303 & degrees < 326)
        {
            return "СЗ";
        }
        else if (degrees >= 326 & degrees < 349)
        {
            return "ССЗ";
        }
        return "";
    }

    public static void HideFromAltTab(IntPtr Handle)
    {
        SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);
    }

    private void ShoveToBackground()
    {
        SetWindowPos((int)this.Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
    }

    public bool SetAutorunValue(bool autorun)
    {
        string? name = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        string? exePath = System.Reflection.Assembly.GetEntryAssembly()?.Location.Replace(".dll",".exe");
        RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);
        try
        {
            if (registryKey != null && name != null && exePath != null)
            {
                if (autorun)
                    registryKey.SetValue(name, exePath);
                else
                    registryKey.DeleteValue(name);
                registryKey.Close();
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
        return true;
    }

    private void CollapseButton_Click(object sender, RoutedEventArgs e)
    {
        if (parameters.Collapsed)
        {
            CollapseButton.Content = new Image() { Source = new BitmapImage(new Uri(@"/Images/HideTick.png", UriKind.Relative))};
            CollapseButton.ToolTip = "Свернуть";
            AllDataGrid.Visibility = Visibility.Visible;
            StatusGrid.Visibility = Visibility.Visible;
            parameters.Collapsed = false;
        }
        else
        {
            CollapseButton.Content = new Image() { Source = new BitmapImage(new Uri(@"/Images/ShowTick.png", UriKind.Relative)) };
            CollapseButton.ToolTip = "Развернуть";
            AllDataGrid.Visibility = Visibility.Collapsed;
            StatusGrid.Visibility = Visibility.Collapsed;
            parameters.Collapsed = true;
        }
    }

    private void NotifyButton_Click(object sender, RoutedEventArgs e)
    {
        if (parameters.Notify)
        {
            NotifyButton.Content = new Image() { Source = new BitmapImage(new Uri(@"/Images/NotificationOffBell.png", UriKind.Relative)) };
            NotifyButton.ToolTip = "Включить уведомления";
            parameters.Notify = false;
        }
        else
        {
            NotifyButton.Content = new Image() { Source = new BitmapImage(new Uri(@"/Images/NotificationBell.png", UriKind.Relative)) };
            NotifyButton.ToolTip = "Выключить уведомления";
            parameters.Notify = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        parameters.Autorun = !parameters.Autorun;
        AutorunItem.IsChecked = parameters.Autorun;
        SetAutorunValue(parameters.Autorun);
    }

    private void WeatherWidget_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        parameters.Serialize();
    }

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateWeather();
    }

    private void PopupNotification_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        PopupNotification.IsOpen = false;
    }

    private void CityBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateWeather();
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

        parameters.Left = Left;
        parameters.Top = Top;
    }

    private void CityBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if(e.Key == System.Windows.Input.Key.Enter)
        {
            UpdateWeather();
        }
    }

    private void PopupNotification_Opened(object sender, EventArgs e)
    {
        DispatcherTimer timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(5d);
        timer.Tick += PopupCloseTimerTick;
        timer.Start();
    }

    private void PopupCloseTimerTick(object? sender, EventArgs e)
    {
        DispatcherTimer timer = sender as DispatcherTimer;
        timer.Stop();
        timer.Tick -= PopupCloseTimerTick;
        this.PopupNotification.IsOpen = false;
    }
}