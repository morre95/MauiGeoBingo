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

        [JsonPropertyName("games")]
        public List<Game>? Games { get; set; } = null;
    }

    public class Game
    {
        [JsonPropertyName("game_id")]
        public int? GameId { get; set; } = null;

        [JsonPropertyName("game_name")]
        public string? GameName { get; set; } = null;

        [JsonPropertyName("game_owner")]
        public int? GameOwner { get; set; } = null;

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; } = null;

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; } = null;


        public int? is_map { get; set; } = null;

        [JsonIgnore]
        public bool IsMap { get { return is_map == 1 ? true : false; } }

    }

}
