
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using MauiGeoBingo.Pagaes;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;

namespace MauiGeoBingo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void MapClicked(object sender, EventArgs e)
        {
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
            if (AppSettings.PlayerId == 0 && !await Helpers.SavePlayerName(AppSettings.PlayerName))
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
