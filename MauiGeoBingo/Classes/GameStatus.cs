using System.Text.Json.Serialization;

namespace MauiGeoBingo.Pagaes;

public class GameStatus
{
    [JsonPropertyName("game_name")]
    public string GameName { get; set; }

    [JsonPropertyName("grid_col")]
    public int Col { get; set; }

    [JsonPropertyName("grid_row")]
    public int Row { get; set; }

    public int is_active { get; set; }
    public bool IsActive { get { return Convert.ToBoolean(is_active); } set { is_active = Convert.ToInt32(value); } }

    public int is_map { get; set; }
    public bool IsMap { get { return Convert.ToBoolean(is_map); } set { is_map = Convert.ToInt32(value); } }

    [JsonPropertyName("num")]
    public int Number { get; set; }

    public int is_highest_number { get; set; }
    public bool IsHighestNumber { get { return Convert.ToBoolean(is_highest_number); } set { is_highest_number = Convert.ToInt32(value); } }

    [JsonPropertyName("player_name")]
    public string PlayerName { get; set; }

    [JsonPropertyName("player_id")]
    public int PlayerId { get; set; }
}




