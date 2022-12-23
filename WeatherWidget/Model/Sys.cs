using System.Text.Json.Serialization;

#nullable disable

public class Sys
{
    [JsonPropertyName("country")]
    public string Country { get; set; }
    [JsonPropertyName("sunrise")]
    public int Sunrise { get; set; }
    [JsonPropertyName("sunset")]
    public int Sunset { get; set; }
}