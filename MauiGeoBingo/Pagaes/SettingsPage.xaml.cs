
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using System.Security.AccessControl;

namespace MauiGeoBingo.Pagaes;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
    }

    private void PageLoaded(object sender, EventArgs e)
    {
        latidude.Text = AppSettings.StartLatitude.ToString();
        longitude.Text = AppSettings.StartLongitude.ToString();

        latidudeDiff.Text = AppSettings.LatitudeMarkerDiff.ToString();
        longitudeDiff.Text = AppSettings.LongitudeMarkerDiff.ToString();

        playerName.Text = AppSettings.PlayerName;

        numberOfPlayers.SelectedIndex = 1;
    }

    private async void CategoriesLoaded(object sender, EventArgs e)
    {
        var cats = await Helpers.ReadJsonFile<TriviaCategorieList>("quizCategories.json");

        if (cats != null)
        {
            cats.TriviaCategories.Add(new Categories { Name = "All" });
            cats.TriviaCategories.Add(new Categories { Name = "Entertainment" });
            cats.TriviaCategories.Add(new Categories { Name = "Science" });
            List<string> catStrings = cats.TriviaCategories.Select(c => c.Name).OrderBy(c => c).ToList();
            categories.ItemsSource = catStrings;
            categories.SelectedIndex = catStrings.FindIndex(c => c == AppSettings.QuizCategorie);
        }
    }

    private void CategoriesSelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker)
        {
            AppSettings.QuizCategorie = picker.SelectedItem.ToString()?? "All";
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
                await DisplayAlert("Alert", "Something whent wrong", "OK");
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
}