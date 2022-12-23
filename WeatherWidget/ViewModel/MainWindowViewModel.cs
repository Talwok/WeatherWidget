using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

#nullable disable

public class MainWindowViewModel : INotifyPropertyChanged
{
    public Parameters parameters;
    public WeatherJson Weather { get; set; }

    public BitmapImage NotifyImage { get; set; }
    public double? Top
    {
        get => parameters.Top;
        set
        {
            parameters.Top = value;
            OnPropertyChanged("Top");
            parameters.Serialize();
        }
    }
    public double? Left
    {
        get => parameters.Left;
        set
        {
            parameters.Left = value;
            OnPropertyChanged("Left");
            parameters.Serialize();
        }
    }
    public bool Notify
    {
        get => parameters.Notify;
        set
        {
            if (value)
            {
                NotifyImage = new BitmapImage(new Uri(@"/Images/NotificationBell.png", UriKind.Relative));
            }
            else
            {
                NotifyImage = new BitmapImage(new Uri(@"/Images/NotificationOffBell.png", UriKind.Relative));
            }
            parameters.Notify = value;
            OnPropertyChanged("Notify");
            OnPropertyChanged("NotifyImage");
            parameters.Serialize();
        }
    }

    public bool Autorun
    {
        get => parameters.Autorun;
        set
        {
            parameters.Autorun = value;
            OnPropertyChanged("Autorun");
            parameters.Serialize();
        }
    }
    private bool _popupOpen;
    public bool PopupOpen
    {
        get => _popupOpen;
        set
        {
            _popupOpen = value;
            OnPropertyChanged("PopupOpen");
        }
    }
    private Visibility _loading = Visibility.Collapsed;
    public Visibility Loading
    {
        get => _loading;
        set
        {
            _loading = value;
            OnPropertyChanged("Loading");
        }
    }
    public string LastCity
    {
        get => parameters.LastCity;
        set
        {
            GetWeather.Execute(value);
            parameters.LastCity = value;
            OnPropertyChanged("LastCity");
        }
    }
    public string Icon
    {
        get => $"https://openweathermap.org/img/wn/{Weather?.Weather.First().Icon}@4x.png";
    }
    public string Temperature 
    { 
        get => $"Òåìïåðàòóðà: {Weather?.Main.Temperature}°C"; 
    }
    public string FeelsLike
    {
        get => $"Îùóùàåòñÿ êàê: {Weather?.Main.FeelsLike}°C";
    }
    public string Humidity
    {
        get => $"Âëàæíîñòü: {Weather?.Main.Humidity}%";
    }
    public string Description
    {
        get => $"Îïèñàíèå: {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Weather?.Weather.First().Description ?? "")}";
    }
    public string Wind
    {
        get => $"Âåòåð: {Weather?.Wind.Speed}ì/ñ";
    }
    public string WindDegree
    {
        get => $"Íàïðàâëåíèå: {GetWindDirectionByDegrees(Weather?.Wind.Degree)}";
    }

    public MainWindowViewModel()
    {
        parameters = ParametersSerializer.Deserialize();
        Notify = Notify;
        GetWeather.Execute(LastCity);

        DispatcherTimer timer = new DispatcherTimer();
        timer.Interval = new TimeSpan(0, 5, 0);
        timer.Tick += WeatherTimer_Tick;
        timer.Start();
    }

    private void WeatherTimer_Tick(object sender, EventArgs e)
    {
        GetWeather.Execute(parameters.LastCity);
    }

    private RelayCommand _getWeather;
    public RelayCommand GetWeather
    {
        get
        {
            return _getWeather ?? (_getWeather = new RelayCommand(async obj =>
            {
                Loading = Visibility.Visible;

                using (var httpclient = new HttpClient())
                {
                    HttpResponseMessage response = null;

                    if(LastCity == "")
                    {
                        GeoCoordinateWatcher watcher = new GeoCoordinateWatcher();
                        GeoCoordinate geolocation = new GeoCoordinate();

                        await Task.Run(new Action(() =>
                        {
                            bool search = true;
                            watcher.PositionChanged += (sender, e) =>
                            {
                                geolocation = e.Position.Location;
                                watcher.Stop();
                                search = false;
                            };
                            watcher.Start();
                            while (search) { };
                        })).ConfigureAwait(true);

                        response = await httpclient.GetAsync(new Uri($"https://api.openweathermap.org/data/2.5/weather?" +
                            $"lat={geolocation.Latitude}&" +
                            $"lon={geolocation.Longitude}&" +
                            $"lang={parameters.Language}&" +
                            $"units={parameters.Units}&" +
                            $"appid={parameters.WheatherApiKey}")).ConfigureAwait(true);
                    }
                    else
                    {
                        while (true)
                        {
                            try
                            {
                                response = await httpclient.GetAsync(new Uri($"https://api.openweathermap.org/data/2.5/weather?" +
                                    $"q={parameters.LastCity}&" +
                                    $"lang={parameters.Language}&" +
                                    $"units={parameters.Units}&" +
                                    $"appid={parameters.WheatherApiKey}")).ConfigureAwait(true);
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            break;
                        }
                        
                    }

                    string result = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    
                    try
                    {
                        Weather = JsonSerializer.Deserialize<WeatherJson>(result) ?? new WeatherJson();
                    }
                    catch (JsonException) { }

                    Loading = Visibility.Collapsed;

                    OnPropertyChanged("Icon");
                    OnPropertyChanged("Temperature");
                    OnPropertyChanged("FeelsLike");
                    OnPropertyChanged("Humidity");
                    OnPropertyChanged("Description");
                    OnPropertyChanged("Wind");
                    OnPropertyChanged("WindDegree");

                    if (parameters.Notify)
                    {
                        PopupOpen = true;
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(5d);
                        timer.Tick += PopupTimer_Tick;
                        timer.Start();
                    }
                    parameters.Serialize();
                }
            }));
        }
    }

    private RelayCommand _setNotification;
    public RelayCommand SetNotification
    {
        get
        {
            return _setNotification ?? (_setNotification = new RelayCommand(obj => 
            {
                Notify = !Notify;
            }));
        }
    }
    private RelayCommand _setAutorun;
    public RelayCommand SetAutorun
    {
        get
        {
            return _setAutorun ?? (_setAutorun = new RelayCommand(obj =>
            {
                parameters.Autorun = !parameters.Autorun;
                SetAutorunValue(parameters.Autorun);
            }));
        }
    }

    public bool SetAutorunValue(bool autorun)
    {
        string? name = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        string? exePath = System.Reflection.Assembly.GetEntryAssembly()?.Location.Replace(".dll", ".exe");
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

    private void PopupTimer_Tick(object sender, EventArgs e)
    {
        DispatcherTimer timer = sender as DispatcherTimer;
        timer.Stop();
        timer.Tick -= PopupTimer_Tick;
        PopupOpen = false;
    }

    public event PropertyChangedEventHandler PropertyChanged; 
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }

    private string GetWindDirectionByDegrees(int? degrees)
    {
        if (degrees >= 349 & degrees < 11)
        {
            return "Ñ";
        }
        else if (degrees >= 11 & degrees < 34)
        {
            return "ÑÑÂ";
        }
        else if (degrees >= 34 & degrees < 56)
        {
            return "ÑÂ";
        }
        else if (degrees >= 56 & degrees < 79)
        {
            return "ÂÑÂ";
        }
        else if (degrees >= 79 & degrees < 101)
        {
            return "Â";
        }
        else if (degrees >= 101 & degrees < 124)
        {
            return "ÂÞÂ";
        }
        else if (degrees >= 124 & degrees < 146)
        {
            return "ÞÂ";
        }
        else if (degrees >= 146 & degrees < 169)
        {
            return "ÞÞÂ";
        }
        else if (degrees >= 169 & degrees < 191)
        {
            return "Þ";
        }
        else if (degrees >= 191 & degrees < 214)
        {
            return "ÞÞÇ";
        }
        else if (degrees >= 214 & degrees < 236)
        {
            return "ÞÇ";
        }
        else if (degrees >= 236 & degrees < 259)
        {
            return "ÇÞÇ";
        }
        else if (degrees >= 259 & degrees < 281)
        {
            return "Ç";
        }
        else if (degrees >= 281 & degrees < 303)
        {
            return "ÇÑÇ";
        }
        else if (degrees >= 303 & degrees < 326)
        {
            return "ÑÇ";
        }
        else if (degrees >= 326 & degrees < 349)
        {
            return "ÑÑÇ";
        }
        return "";
    }
}