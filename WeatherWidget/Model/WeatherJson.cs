using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

#nullable disable

public class WeatherJson : INotifyPropertyChanged
{
    private Coordinates _coord;
    [JsonPropertyName("coord")]
    public Coordinates Coordinates 
    {
        get => _coord;
        set
        {
            _coord = value;
            OnPropertyChanged("Coordinates");
        }
    }

    private Weather[] _weather;
    [JsonPropertyName("weather")]
    public Weather[] Weather
    {
        get => _weather;
        set
        {
            _weather = value;
            OnPropertyChanged("Weather");
        }
    }

    private string _base;
    [JsonPropertyName("base")]
    public string Base
    {
        get => _base;
        set
        {
            _base = value;
            OnPropertyChanged("Base");
        }
    }

    private Main _main;
    [JsonPropertyName("main")]
    public Main Main
    {
        get => _main;
        set
        {
            _main = value;
            OnPropertyChanged("Main");
        }
    }

    private int _visibility;
    [JsonPropertyName("visibility")]
    public int Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            OnPropertyChanged("Visibility");
        }
    }

    private Wind _wind;
    [JsonPropertyName("wind")]
    public Wind Wind
    {
        get => _wind;
        set
        {
            _wind = value;
            OnPropertyChanged("Wind");
        }
    }

    private Clouds _clouds;
    [JsonPropertyName("clouds")]
    public Clouds Clouds
    {
        get => _clouds;
        set
        {
            _clouds = value;
            OnPropertyChanged("Clouds");
        }
    }

    private Snow _snow;
    [JsonPropertyName("snow")]
    public Snow Snow
    {
        get => _snow;
        set
        {
            _snow = value;
            OnPropertyChanged("Snow");
        }
    }

    private Rain _rain;
    [JsonPropertyName("rain")]
    public Rain Rain
    {
        get => _rain;
        set
        {
            _rain = value;
            OnPropertyChanged("Rain");
        }
    }

    private int _deltaTime;
    [JsonPropertyName("dt")]
    public int DeltaTime
    {
        get => _deltaTime;
        set
        {
            _deltaTime = value;
            OnPropertyChanged("DeltaTime");
        }
    }

    private Sys _sys;
    [JsonPropertyName("sys")]
    public Sys Sys
    {
        get => _sys;
        set
        {
            _sys = value;
            OnPropertyChanged("Sys");
        }
    }

    private int _timezone;
    [JsonPropertyName("timezone")]
    public int Timezone
    {
        get => _timezone;
        set
        {
            _timezone = value;
            OnPropertyChanged("Timezone");
        }
    }

    private int _id;
    [JsonPropertyName("id")]
    public int ID
    {
        get => _id;
        set
        {
            _id = value;
            OnPropertyChanged("ID");
        }
    }

    private string _name;
    [JsonPropertyName("name")]
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged("Name");
        }
    }

    private int _code;
    [JsonPropertyName("cod")]
    public int Code
    {
        get => _code;
        set
        {
            _code = value;
            OnPropertyChanged("Code");
        }
    }

    private string _message;
    [JsonPropertyName("message")]
    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged("Message");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "")
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(prop));
    }
}