using MauiGeoBingo.Converters;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MauiGeoBingo.Pagaes;

public class GameStatusRootobject
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("game_status")]
    public List<GameStatus> GameStatus { get; set; } = new();

    [JsonPropertyName("winner")]
    public int? Winner { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }
    [JsonPropertyName("security_key")]
    public string? SecurityKey { get; set; }

    [JsonPropertyName("player_count")]
    public int PlayerCount { get; set; }

    [JsonPropertyName("player_ids")]
    public List<int> PlayerIds { get; set; } = new();

    [JsonPropertyName("is_running")]
    public bool IsRunning { get; set; }

    [JsonPropertyName("player_name")]
    public string? PlayerName { get; set; }



    [JsonPropertyName("all_game_status")]
    [JsonConverter(typeof(AllGameStatusConverter))]
    public Dictionary<int, List<GameStatus>> AllGameStatus { get; set; } = new();

    public int Get(int row, int col)
    {
        int result = 0;
        foreach (GameStatus gs in GameStatus)
        {
            if (gs.Col == col && gs.Row == row)
            {
                result = gs.Number;
                break;
            }
        }
        return result;
    }

    public int GetFromAll(int playerId, int row, int col)
    {
        int result = 0;
        //Debug.WriteLine($"id:{playerId}, Är denna sann: " + AllGameStatus.TryGetValue(playerId, out List<GameStatus>? test));
        if (AllGameStatus.TryGetValue(playerId, out List<GameStatus>? gsList))
        {
            
            if (gsList != null)
            {
                //Debug.WriteLine($"Jo det finns något här för id: {playerId}");
                foreach (GameStatus gs in gsList)
                {
                    if (gs.Col == col && gs.Row == row)
                    {
                        result = gs.Number;
                        break;
                    }
                }
            }
            else
            {
                Debug.WriteLine($"player id: {playerId} hittas alltså inte");
            }
            
        }
        return result;
    }

    public bool IsHighest(int playerId, int row, int col)
    {
        bool result = false;

        if (AllGameStatus.TryGetValue(playerId, out List<GameStatus>? gsList))
        {
            if (gsList != null)
            {
                foreach (GameStatus gs in gsList)
                {
                    if (gs.Col == col && gs.Row == row)
                    {
                        result = gs.IsHighestNumber;
                        break;
                    }
                }
            }
        }
        return result;
    }

    public (int number, bool isHighest) GetNumberAndIsHighest(int playerId, int row, int col)
    {
        int number = 0;
        bool isHighest = false;
        if (AllGameStatus.TryGetValue(playerId, out List<GameStatus>? gsList))
        {
            if (gsList != null)
            {
                foreach (GameStatus gs in gsList)
                {
                    if (gs.Col == col && gs.Row == row)
                    {
                        number = gs.Number;
                        isHighest = gs.IsHighestNumber;
                        break;
                    }
                }
            }
            else
            {
                Debug.WriteLine($"player id: {playerId} hittas alltså inte");
            }

        }
        return (number, isHighest);
    }
}




