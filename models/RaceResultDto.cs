using Newtonsoft.Json;

public class RaceResultDto
{
    [JsonProperty("playerId")]
    public string? PlayerId { get; set; }

    [JsonProperty("playerName")]
    public string? PlayerName { get; set; }

    [JsonProperty("sessionToken")]
    public string? SessionToken { get; set; }    // ← new


    [JsonProperty("time")]
    public float Time { get; set; }
}
