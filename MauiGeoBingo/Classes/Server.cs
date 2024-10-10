using MauiGeoBingo.Classes;
using System.Text.Json.Serialization;

namespace MauiGeoBingo.Pagaes;

public class Server
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    public string? created { get; set; }
    public DateTime? Created => StringToDate(created);


    [JsonPropertyName("game_id")]
    public int? GameId { get; set; }

    [JsonPropertyName("game_name")]
    public string? GameName { get; set; }


    public int? game_owner { get; set; }
    [JsonIgnore]
    public bool IsMyServer => game_owner == AppSettings.PlayerId;

    [JsonPropertyName("number_of_players")]
    public int? NumberOfPlayers { get; set; }
    
    [JsonPropertyName("player_ids")]
    public int[]? PlayerIds { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }
    [JsonPropertyName("security_key")]
    public string? SecurityKey { get; set; }

    [JsonPropertyName("is_map")]
    public int IsMap { get; set; }

    public string? last_modified { get; set; }
    public DateTime? LastModified => StringToDate(last_modified);


    public int is_active { get; set; }
    
    public double latitude { get; set; }
    public double longitude { get; set; }

    [JsonPropertyName("servers")]
    public List<Server> Servers { get; set; } = new();

    private static DateTime? StringToDate(string? stringToFormat)
    {
        if (DateTime.TryParse(stringToFormat, out DateTime result))
        {
            return result;
        }

        return null;
    }
}
