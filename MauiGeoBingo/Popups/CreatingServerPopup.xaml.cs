using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using System.Security.AccessControl;

namespace MauiGeoBingo.Popups;

public partial class CreatingServerPopup : Popup
{
	public CreatingServerPopup()
	{
		InitializeComponent();
	}

    private void CreateServerPageLoaded(object sender, EventArgs e)
    {
        latidude.Text = AppSettings.StartLatitude.ToString();
        longitude.Text = AppSettings.StartLongitude.ToString();

        latidudeDiff.Text = AppSettings.LatitudeMarkerDiff.ToString();
        longitudeDiff.Text = AppSettings.LongitudeMarkerDiff.ToString();

        gameType.SelectedIndex = 0;
    }

    private async void NewGameClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(gameName.Text))
        {
            //await DisplayAlert("Alert", "You need to put in a name for the game", "OK");
            await Toast.Make("You need to put in a name for the game").Show();
            return;
        }

        if (gameType.SelectedIndex < 0)
        {
            //await DisplayAlert("Alert", "Select if it is a map or a button game", "OK");
            await Toast.Make("Select if it is a map or a button game").Show();
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

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (game == null)
            {
                //await DisplayAlert("Alert", "No server created", "OK");
                await Toast.Make("No server created").Show();
                await CloseAsync(false, cts.Token);
            }
            else
            {
                await Toast.Make($"You created game: '{gameName.Text}'").Show();
                await CloseAsync(true, cts.Token);
            }
        }
    }

    private async void CancelClicked(object sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(false, cts.Token);
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

    private /*async*/ void SetPositionOnMap_Clicked(object sender, EventArgs e)
    {
        //await Navigation.PushAsync(new MapSettingsPage());
        // TODO: skapa en popup för det här... Eftersom Navigation.PushAsync inte funkar i en popup
    }

    private void GameTypeIndexChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker)
        {
            if (picker.SelectedIndex == 0)
            {
                mapLatLonSettings.IsVisible = false;
                mapMarkerSettings.IsVisible = false;

                Grid.SetRow(buttonsStack, 2);
            }
            else
            {
                mapLatLonSettings.IsVisible = true;
                mapMarkerSettings.IsVisible = true;

                Grid.SetRow(buttonsStack, 3);
            }
        }
    }

    private void GameNameCompleted(object sender, EventArgs e)
    {
        if (sender is Entry entry)
        {
            NewGameClicked(new Button(), e);
        }
    }
}