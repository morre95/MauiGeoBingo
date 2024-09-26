using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MauiGeoBingo.Classes
{
    internal class TriviaCategorieList
    {
        [JsonPropertyName("trivia_categories")]
        public List<Categories> TriviaCategories { get; set; }
    }

    internal class Categories
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
