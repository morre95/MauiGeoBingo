using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiGeoBingo
{
    internal class AppSettings
    {
        public string FilePath
        {
            get { return Preferences.Get(nameof(FilePath), string.Empty); }
            set { Preferences.Set(nameof(FilePath), value); }
        }

        public string? Token
        {
            get { return Preferences.Get(nameof(Token), null); }
            set { Preferences.Set(nameof(Token), value); }
        }

        public async Task SaveFile(CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream(Encoding.Default.GetBytes("Hello from the Community Toolkit!"));
            var fileSaverResult = await FileSaver.Default.SaveAsync("test.txt", stream, cancellationToken);
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
