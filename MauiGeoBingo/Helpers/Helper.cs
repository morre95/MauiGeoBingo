using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using MauiGeoBingo.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MauiGeoBingo.Helpers
{
    internal class Helper
    {
        public static async Task<T?> ReadJsonFile<T>(string filePath)
        {
            string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, filePath);
            if (!File.Exists(targetFile))
            {
                //Debug.WriteLine($"AppPackageFileExistsAsync({filePath}): {await FileSystem.AppPackageFileExistsAsync(filePath)}");
                //Debug.WriteLine($"FileSystem.AppDataDirectory        : {FileSystem.AppDataDirectory}");
                //Debug.WriteLine($"FileSystem.Current.AppDataDirectory: {FileSystem.Current.AppDataDirectory}");
                using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(filePath);
                return await JsonSerializer.DeserializeAsync<T>(fileStream);
            }
            else
            {

                Debug.WriteLine($"FileSystem.Current.AppDataDirectory: {targetFile}, File.Exists(): {File.Exists(targetFile)}");

                using FileStream fileStream = File.OpenRead(targetFile);

                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                };
                return await JsonSerializer.DeserializeAsync<T>(fileStream, options);
            }
        }

        public static async Task<string> WirteToFile(string fileName, string text)
        {
            string filePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
            using FileStream fileStream = File.OpenWrite(filePath);
            using StreamWriter streamWriter = new(fileStream);
            await streamWriter.WriteAsync(text);

            return filePath;
        }

        public static async Task SaveFile(string fileName, string text, CancellationToken cancellationToken = default)
        {
            using var stream = new MemoryStream(Encoding.Default.GetBytes(text));
            var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream, cancellationToken);
            if (fileSaverResult.IsSuccessful)
            {
                await Toast.Make($"The file was saved successfully to location: {fileSaverResult.FilePath}").Show(cancellationToken);
            }
            else
            {
                await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}").Show(cancellationToken);
            }
        }

        public static async Task<bool> SavePlayerName(string name)
        {
            string endpoint = AppSettings.LocalBaseEndpoint;
            HttpRequest rec = new($"{endpoint}/new/player");

            Player? player = await rec.PutAsync<Player>(new Player
            {
                PlayerId = AppSettings.PlayerId,
                PlayerName = name,
            });

            if (player != null)
            {
                AppSettings.PlayerName = name;
                AppSettings.PlayerId = player.PlayerId ?? 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static async Task<string> GetNameAsync(int playerId)
        {
            string endpoint = AppSettings.LocalBaseEndpoint;
            HttpRequest rec = new($"{endpoint}/player/name?player_id={playerId}");

            try
            {
                var result = await rec.GetAsync<ResponseData>();

                if (result != null && result.PlayerName != null)
                {
                    return result.PlayerName;
                }
            }
            catch (Exception)
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await Toast.Make("Något är fel på servern").Show(cts.Token);
            }

            return string.Empty;
        }

        private class ResponseData
        {
            [JsonPropertyName("player_name")]
            public string? PlayerName { get; set; }
        }

    }
}
