
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
        playerName.Text = AppSettings.PlayerName;
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