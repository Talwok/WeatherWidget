using System.Text.Json.Serialization;

public class Rain
{
    [JsonPropertyName("1h")]
    public double OneHour { get; set; }
    [JsonPropertyName("3h")]
    public double ThreeHours { get; set; }
}