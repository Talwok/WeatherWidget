using System.Text.Json.Serialization;

#nullable disable

public class Weather
{
    [JsonPropertyName("id")]
    public int ID { get; set; }
    [JsonPropertyName("main")]
    public string Main { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("icon")]
    public string Icon { get; set; }
}