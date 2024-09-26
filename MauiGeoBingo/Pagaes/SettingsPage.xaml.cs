using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;

namespace MauiGeoBingo.Pagaes;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();

        latidude.Text = AppSettings.StartLatidude.ToString();
        longitude.Text = AppSettings.StartLongitude.ToString();

        latidudeDiff.Text = AppSettings.LatidudeMarkerDiff.ToString();
        longitudeDiff.Text = AppSettings.LongitudeMarkerDiff.ToString();

        playerName.Text = AppSettings.PlayerName;
    }

    private async void OnEntryCompleted(object? sender, EventArgs e)
    {
        if (sender is Entry entry && double.TryParse(entry.Text, out double val))
        {
            if (entry.Placeholder == "Latidude")
            {
                AppSettings.StartLatidude = val;
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
            AppSettings.LatidudeMarkerDiff = val;
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

    private async void PlayerNameCompleted(object sender, EventArgs e)
    {
        if (sender is Entry entry)
        {
            string endpoint = AppSettings.LocalBaseEndpoint;
            HttpRequest rec = new($"{endpoint}/update/player");

            Player? player = await rec.PostAsync<Player>(new Player
            {
                PlayerId = AppSettings.PlayerId,
                PlayerName = entry.Text,
            });

            if (player == null)
            {
                await DisplayAlert("Alert", "Something whent wrong", "OK");
            }
            else
            {
                AppSettings.PlayerName = entry.Text;
                await Toast.Make($"You changed your player name to: '{entry.Text}'").Show();
            }
        }
    }

    private async void ResetClicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            AppSettings app = new();
            app.ResetToDefalt();

            var page = Navigation.NavigationStack.LastOrDefault();

            // Load page
            await Navigation.PushAsync(new SettingsPage());

            // Remove old page
            Navigation.RemovePage(page);
        }
    }
}