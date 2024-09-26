using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiGeoBingo.Classes
{
    internal class Helpers
    {
        public static async Task<T?> ReadJsonFile<T>(string filePath)
        {
            using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(filePath);
            return await JsonSerializer.DeserializeAsync<T>(fileStream);
        }
    }
}
