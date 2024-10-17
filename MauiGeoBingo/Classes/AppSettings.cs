using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;
using MauiGeoBingo.Helpers;

namespace MauiGeoBingo.Classes
{
    internal class AppSettings
    {
        public static double StartLatitude
        {
            get { return Preferences.Get(nameof(StartLatitude), 58.317064); }
            set { Preferences.Set(nameof(StartLatitude), value); }
        }

        public static double StartLongitude
        {
            get { return Preferences.Get(nameof(StartLongitude), 15.102253); }
            set { Preferences.Set(nameof(StartLongitude), value); }
        }

        public static double LatitudeMarkerDiff
        {
            get { return Preferences.Get(nameof(LatitudeMarkerDiff), 0.005); }
            set { Preferences.Set(nameof(LatitudeMarkerDiff), value); }
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

        public static string QuizCategorie
        {
            get { return Preferences.Get(nameof(QuizCategorie), "General Knowledge"); }
            set { Preferences.Set(nameof(QuizCategorie), value); }
        }

        public static string QuizJsonFileName
        {
            get { return Preferences.Get(nameof(QuizJsonFileName), "quizDB.json"); }
            set { Preferences.Set(nameof(QuizJsonFileName), value); }
        }

        /// <summary>
        /// Returns the creation time od the quiz json file. If it not exists it returns DateTime.MinValue
        /// </summary>
        public static string QuizJsonFileCreationTime => File.Exists(QuizJsonFileName) ? File.GetCreationTime(QuizJsonFileName).ToString() : DateTime.MinValue.ToString();

        private static string _baseEndpoint => DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5000" : "http://127.0.0.1:5000";
        public static string LocalBaseEndpoint => _baseEndpoint;

        private static string _baseWSEndpoint => DeviceInfo.Platform == DevicePlatform.Android ? "ws://10.0.2.2:8765" : "ws://127.0.0.1:8765";
        public static string LocalWSBaseEndpoint => _baseWSEndpoint;


        private string _googleMapsApiKey = string.Empty;
        public static string GOOGLE_MAPS_ANDROID_API_KEY => Instance._googleMapsApiKey;
        public static string GOOGLE_MAPS_IOS_API_KEY => Instance._googleMapsApiKey;

        private const string _encryptionPass = "351b3205-7d66-434b-97ee-63dafcf1e48a";

        private static readonly Lazy<AppSettings> lazy = new Lazy<AppSettings>(() => new AppSettings());

        public static AppSettings Instance { get { return lazy.Value; } }

        private AppSettings()
        {

            // TODO: Raderar detta innan det går till publikt
            //_ = EncryptionHelpers.SetAsync("GOOGLE_MAPS_ANDROID_API_KEY", "AIzaSyAdj2XsLd_tRH51mrZatq_cLMsgzk3Qv6Q", _encryptionPass);

            GetVariablesAsync();
        }

        

        private async void GetVariablesAsync()
        {
            string? value = await EncryptionHelpers.GetAsync("GOOGLE_MAPS_ANDROID_API_KEY", _encryptionPass);
            if (value != null) _googleMapsApiKey = value;
        }

        public void ResetToDefalt()
        {

            Preferences.Clear();

            StartLatitude = 58.317064;
            StartLongitude = 15.102253;
            LatitudeMarkerDiff = 0.005;
            LongitudeMarkerDiff = 0.01;
            PlayerId = 0;
            CurrentGameId = 0;
            PlayerName = "player";

            QuizCategorie = "General Knowledge";

            if (QuizJsonFileName != "quizDB.json")
            {
                File.Delete(QuizJsonFileName);
            }

            QuizJsonFileName = "quizDB.json";
        }


        
    }
}
