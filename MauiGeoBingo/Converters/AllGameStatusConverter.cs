using System.Text.Json;
using System.Text.Json.Serialization;
using MauiGeoBingo.Pagaes;

namespace MauiGeoBingo.Converters;

public class AllGameStatusConverter : JsonConverter<Dictionary<int, List<GameStatus>>>
{
    public override Dictionary<int, List<GameStatus>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = new Dictionary<int, List<GameStatus>>();

        // Eftersom `all_game_status` är en array, måste vi börja med att läsa in arrayen
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            // Nu börjar varje spel-ID objekt (exempelvis "1" eller "2")
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            reader.Read(); // Läser nyckeln (spel-ID)
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            // Hämta spel-ID som är en sträng och konvertera till int
            string keyString = reader.GetString();
            int key = int.Parse(keyString);

            reader.Read(); // Gå vidare till värdet (listan med GameStatus)

            var gameStatusList = JsonSerializer.Deserialize<List<GameStatus>>(ref reader, options);

            dictionary.Add(key, gameStatusList);

            reader.Read(); // Slut på objektet
        }

        return dictionary;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<int, List<GameStatus>> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Writing is not implemented for this converter");
    }
}




