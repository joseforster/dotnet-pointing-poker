using System.Text.Json.Serialization;

namespace PointingPoker.Models;

public class AuthModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}