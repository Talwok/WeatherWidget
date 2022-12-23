using System.Text.Json.Serialization;

public class Main
{
    [JsonPropertyName("temp")]
    public double Temperature { get; set; }
    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }
    [JsonPropertyName("temp_min")]
    public double MinTemperature { get; set; }
    [JsonPropertyName("temp_max")]
    public double MaxTemperature { get; set; }
    [JsonPropertyName("pressure")]
    public int Pressure { get; set; }
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
    [JsonPropertyName("sea_level")]
    public int SeaLevel { get; set; }
    [JsonPropertyName("grnd_level")]
    public int GroundLevel { get; set; }
}