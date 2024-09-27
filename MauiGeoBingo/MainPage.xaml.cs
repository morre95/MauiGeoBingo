
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using MauiGeoBingo.Pagaes;
using System.Diagnostics;
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

        private async void Map_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MapPage());
        }

        private async void Buttons_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ButtonsPage());
        }

        private async void Settings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void ScrollView_Loaded(object sender, EventArgs e)
        {
            if (AppSettings.PlayerId == 0)
            {
                string endpoint = AppSettings.LocalBaseEndpoint;
                HttpRequest rec = new($"{endpoint}/new/player");

                Player? player = await rec.PutAsync<Player>(new Player
                {
                    PlayerId = AppSettings.PlayerId,
                    PlayerName = AppSettings.PlayerName,
                });

                if (player != null)
                {
                    Debug.WriteLine("player name: " + player.PlayerName);
                    Debug.WriteLine("player id: " + player.PlayerId);

                    AppSettings.PlayerId = player.PlayerId ?? 0;
                }
                else
                {
                    await DisplayAlert("Alert", "The game server is not working properly", "OK");
                }
            }
        }

        private async void Server_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ServerPage());
        }
    }

}
