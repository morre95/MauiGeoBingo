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
        public static double StartLatidude
        {
            get { return Preferences.Get(nameof(StartLatidude), 58.317064); }
            set { Preferences.Set(nameof(StartLatidude), value); }
        }

        public static double StartLongitude
        {
            get { return Preferences.Get(nameof(StartLongitude), 15.102253); }
            set { Preferences.Set(nameof(StartLongitude), value); }
        }

        public static double LatidudeMarkerDiff
        {
            get { return Preferences.Get(nameof(LatidudeMarkerDiff), 0.005); }
            set { Preferences.Set(nameof(LatidudeMarkerDiff), value); }
        }

        public static double LongitudeMarkerDiff
        {
            get { return Preferences.Get(nameof(LongitudeMarkerDiff), 0.01); }
            set { Preferences.Set(nameof(LongitudeMarkerDiff), value); }
        }

        public static int PlayerId
        {
            get { return Preferences.Get(nameof(PlayerId), 0); }
            set { Preferences.Set(nameof(PlayerId), value); }
        }

        public static int CurrentGameId
        {
            get { return Preferences.Get(nameof(CurrentGameId), 0); }
            set { Preferences.Set(nameof(CurrentGameId), value); }
        }

        public static string PlayerName
        {
            get { return Preferences.Get(nameof(PlayerName), "player"); }
            set { Preferences.Set(nameof(PlayerName), value); }
        }

        private static string _baseEndpoint => DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : "http://127.0.0.1:5000";
        public static string LocalBaseEndpoint => _baseEndpoint;

        public void ResetToDefalt()
        {
            StartLatidude = 58.317064;
            StartLongitude = 15.102253;
            LatidudeMarkerDiff = 0.005;
            LongitudeMarkerDiff = 0.01;
            PlayerId = 0;
            CurrentGameId = 0;
            PlayerName = "player";
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
