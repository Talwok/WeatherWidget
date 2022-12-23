using System.Text.Json.Serialization;

public class Wind
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }
    [JsonPropertyName("deg")]
    public int Degree { get; set; }
    [JsonPropertyName("gust")]
    public double Gust { get; set; }
}