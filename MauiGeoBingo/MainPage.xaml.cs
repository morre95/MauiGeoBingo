
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using MauiGeoBingo.Helpers;
using MauiGeoBingo.Pagaes;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;

namespace MauiGeoBingo
{
    public partial class MainPage : ContentPage
    {

        private MainPageViewModel _mainPageViewModel;

        public MainPage()
        {
            InitializeComponent();

            _mainPageViewModel = new();

            BindingContext = _mainPageViewModel;

            SetName();



            /*if (!AppSettings.TryAddEnv("GOOGLE_MAPS_ANDROID_IOS_API_KEY", "AIzaSyAdj2XsLd_tRH51mrZatq_cLMsgzk3Qv6Q", EnvironmentVariableTarget.User))
            {
                Debug.WriteLine("******************* Nej det gick inte att sätta env varibeln ****************************");
            }
            else
            {
                Debug.WriteLine("******************* Jipppi det gick att sätta env varibeln ****************************");
            }

            Debug.WriteLine("********************* AppSettings.GOOGLE_MAPS_ANDROID_API_KEY: " + AppSettings.GOOGLE_MAPS_ANDROID_API_KEY + " ********************************************");*/

            //_ = SecureStorage.Default.SetAsync("GOOGLE_MAPS_ANDROID_API_KEY", "AIzaSyAdj2XsLd_tRH51mrZatq_cLMsgzk3Qv6Q");
        }

        public static void TryAddEnv(string environment, Func<string?> func, out bool isExist)
        {
            var value = Environment.GetEnvironmentVariable(environment);
            isExist = value == null;
            if (isExist)
            {
                Environment.SetEnvironmentVariable(environment, func.Invoke());
            }
        }

        private async void SetName()
        {
            string name = await Helper.GetNameAsync(AppSettings.PlayerId);
            if (name != string.Empty)
            {
                AppSettings.PlayerName = name;
                _mainPageViewModel.IsEnabled = true;
                _mainPageViewModel.WarningVisible = false;
            }
            else
            {
                _mainPageViewModel.IsEnabled = false;
                _mainPageViewModel.WarningVisible = true;
            }
        }

        private async void MapClicked(object sender, EventArgs e)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await Toast.Make("Ingen kartsida på plats än!!!").Show(cts.Token);
            await Navigation.PushAsync(new MapPage());
        }

        private async void ButtonsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ButtonsPage());
        }
        

        private async void SettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void ScrollViewLoaded(object sender, EventArgs e)
        {
            if (AppSettings.PlayerId == 0 && !await Helper.SavePlayerName(AppSettings.PlayerName))
            {
                await DisplayAlert("Alert", "The game server is not working properly", "OK");
            }
        }

        private async void ServerClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ServerPage());
        }
    }

}
