using System.Text.Json.Serialization;

namespace MauiGeoBingo.Classes;

public class Quiz
{
    [JsonPropertyName("response_code")]
    public int ResponseCode { get; set; }

    [JsonPropertyName("results")]
    public List<Result> Results { get; set; } = new List<Result>();

    [JsonPropertyName("token")]
    public string Token { get; set; }
}


public class Result
{
    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; }

    [JsonPropertyName("question")]
    public string Question { get; set; }

    [JsonPropertyName("correct_answer")]
    public string CorrectAnswer { get; set; }

    [JsonPropertyName("incorrect_answers")]
    public List<string> IncorrectAnswers { get; set; }
}




