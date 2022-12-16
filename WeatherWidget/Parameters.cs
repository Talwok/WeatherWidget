public class Parameters
{
    public string WheatherApiKey { get; set; } //openweathermap.org

    public bool Collapsed { get; set; } = false;
    public bool Notify { get; set; } = false;
    public bool Autorun { get; set; } = false;
    public double? Left { get; set; }
    public double? Top { get; set; }
    public string LastCity { get; set; }

    public Parameters()
    {
        WheatherApiKey = "none";
    }

    public Parameters(string wheatherApiKey)
    {
        WheatherApiKey = wheatherApiKey;
    }
}