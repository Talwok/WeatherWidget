public class WeatherJson
{
    public Coordinates coord { get; set; }
    public Weather[] weather { get; set; }
    public string @base { get; set; }
    public Main main { get; set; }
    public int visibility { get; set; }
    public Wind wind { get; set; }
    public Clouds clouds { get; set; }
    public Snow snow { get; set; }
    public Rain rain { get; set; }
    public int dt { get; set; }
    public Sys sys { get; set; }
    public int timezone { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public int cod { get; set; }
    public string message { get; set; }
    public override string ToString()
    {
        return string.Format("Температура: {0}, {1}", main.temp, weather[0].description);
    }
}