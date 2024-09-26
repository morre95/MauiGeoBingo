using System.Text.Json.Serialization;

namespace MauiGeoBingo.Classes
{
    internal class Player
    {
        [JsonPropertyName("player_id")]
        public int? PlayerId { get; set; } = null;

        [JsonPropertyName("player_name")]
        public string? PlayerName { get; set; } = null;

        [JsonPropertyName("last_played")]
        public DateTime? LastPlayed { get; set; } = null;
    }

}
