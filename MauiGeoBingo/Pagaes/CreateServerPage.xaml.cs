using CommunityToolkit.Maui.Alerts;
using MauiGeoBingo.Classes;
using System.Security.AccessControl;

namespace MauiGeoBingo.Pagaes;

public partial class CreateServerPage : ContentPage
{
    public CreateServerPage()
    {
        InitializeComponent();
    }

    private void CreateServerPageLoaded(object sender, EventArgs e)
    {
        latidude.Text = AppSettings.StartLatitude.ToString();
        longitude.Text = AppSettings.StartLongitude.ToString();

        latidudeDiff.Text = AppSettings.LatitudeMarkerDiff.ToString();
        longitudeDiff.Text = AppSettings.LongitudeMarkerDiff.ToString();

        numberOfPlayers.SelectedIndex = 1;
    }

    private async void NewGameClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(gameName.Text))
        {
            await DisplayAlert("Alert", "You need to put in a name for the game", "OK");
            return;
        }

        if (gameType.SelectedIndex < 0)
        {
            await DisplayAlert("Alert", "Select if it is a map or a button game", "OK");
            return;
        }

        if (sender is Button button)
        {
            string endpoint = AppSettings.LocalBaseEndpoint;
            HttpRequest rec = new($"{endpoint}/new/game");

            Game? game = await rec.PutAsync<Game>(new Game
            {
                GameOwner = AppSettings.PlayerId,
                GameName = gameName.Text,
                Latitude = AppSettings.StartLatitude,
                Longitude = AppSettings.StartLongitude,
                is_map = gameType.SelectedIndex,
            });

            if (game == null)
            {
                await DisplayAlert("Alert", "No server created", "OK");
            }
            else
            {
                await Toast.Make($"You created game: '{gameName.Text}'").Show();
                var page = Navigation.NavigationStack.LastOrDefault();
                await Navigation.PushAsync(new ServerPage());
                Navigation.RemovePage(page);
            }
        }
    }

    private async void OnEntryCompleted(object? sender, EventArgs e)
    {
        if (sender is Entry entry && double.TryParse(entry.Text, out double val))
        {
            if (entry.Placeholder == "Latidude")
            {
                AppSettings.StartLatitude = val;
                await Toast.Make($"You changed Latidude to: '{entry.Text}'").Show();
            }
            else
            {
                AppSettings.StartLongitude = val;
                await Toast.Make($"You changed Longitude to: '{entry.Text}'").Show();
            }
        }
    }


    private async void OnEntryLatDiffCompleted(object? sender, EventArgs e)
    {
        if (sender is Entry entry && double.TryParse(entry.Text, out double val))
        {
            AppSettings.LatitudeMarkerDiff = val;
            await Toast.Make($"You changed Latidude Diff to: '{entry.Text}'").Show();
        }
    }

    private async void OnEntryLonDiffCompleted(object? sender, EventArgs e)
    {
        if (sender is Entry entry && double.TryParse(entry.Text, out double val))
        {
            AppSettings.LongitudeMarkerDiff = val;
            await Toast.Make($"You changed Longitude diff to: '{entry.Text}'").Show();
        }
    }

    private async void SetPositionOnMap_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MapSettingsPage());
    }

    private void GameTypeIndexChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker)
        {
            if (picker.SelectedIndex == 0)
            {
                mapLatLonSettings.IsVisible = false;
                mapMarkerSettings.IsVisible = false;
            }
            else
            {
                mapLatLonSettings.IsVisible = true;
                mapMarkerSettings.IsVisible = true;
            }
        }
    }
}