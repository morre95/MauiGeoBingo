using Android.App;
using Android.Runtime;
using MauiGeoBingo.Classes;
using System.Diagnostics;

namespace MauiGeoBingo
{
    [Application(UsesCleartextTraffic = true)]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();



        public override void OnCreate()
        {
            base.OnCreate();

            var apiKey = AppSettings.GOOGLE_MAPS_ANDROID_API_KEY;

            if (!string.IsNullOrEmpty(apiKey))
            {
                UpdateApiKeyInManifest(apiKey);
            }
        }

        private void UpdateApiKeyInManifest(string apiKey)
        {
            try
            {
                var appInfo = PackageManager.GetApplicationInfo(PackageName, Android.Content.PM.PackageInfoFlags.MetaData);
                var metaData = appInfo.MetaData;
                if (metaData != null)
                {
                    metaData.PutString("com.google.android.geo.API_KEY", apiKey);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to update API key: {ex.Message}");
            }
        }
    }
}
