using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiGeoBingo.Classes
{
    internal class AppSettings
    {
        public static double StatLatidude
        {
            get { return Preferences.Get(nameof(StatLatidude), 58.317064); }
            set { Preferences.Set(nameof(StatLatidude), value); }
        }

        public static double StaLongitude
        {
            get { return Preferences.Get(nameof(StaLongitude), 15.102253); }
            set { Preferences.Set(nameof(StaLongitude), value); }
        }

        public static int PlayerId
        {
            get { return Preferences.Get(nameof(PlayerId), 0); }
            set { Preferences.Set(nameof(PlayerId), value); }
        }

        public static string PlayerName
        {
            get { return Preferences.Get(nameof(PlayerName), "player"); }
            set { Preferences.Set(nameof(PlayerName), value); }
        }

        public async Task SaveFile(string fileName, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream(Encoding.Default.GetBytes("Hello from the Community Toolkit!"));
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
    }
}
