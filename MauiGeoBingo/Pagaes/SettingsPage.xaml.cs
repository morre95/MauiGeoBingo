
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using System.Diagnostics;
using System.Text;
using System.Text.Json;


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
            if (!await Helpers.SavePlayerName(entry.Text))
            {
                await DisplayAlert("Alert", "Something whent wrong", "OK");
            }
            else 
            {
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

    private async void DownloadNewDBClicked(object sender, EventArgs e)
    {
        updateDbButton.IsEnabled = false;
        updateingDbStatus.IsVisible = true;

        string result = await DownloadFromQuizeDb(updateQuestionDbProg);
        string? filePath = await SaveFile($"quizDb_{DateOnly.FromDateTime(DateTime.Now)}.json", result);

        if (filePath != null)
        {
            AppSettings.QuizJsonFileName = filePath;
            await Toast.Make("The file was saved successfully").Show();
            JsonFileLastUpdateTextLoaded(jsonFileLastUpdateText, e);
        }
        else
        {
            await Toast.Make("The file was NOT saved successfully for some reason").Show();
        }

        updateingDbStatus.IsVisible = false;
        updateDbButton.IsEnabled = true;
    }

    private async Task<string> DownloadFromQuizeDb(ProgressBar progressBar)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        return JsonSerializer.Serialize(await QuizHelpers.GetNewDB(progressBar), options);
    }

    private async Task<string?> SaveFile(string fileName, string text, CancellationToken cancellationToken = default)
    {
        return await Helpers.WirteToFile(fileName, text);
    }

    private void JsonFileLastUpdateTextLoaded(object sender, EventArgs e)
    {
        if (sender is Label label && AppSettings.QuizJsonFileCreationTime != DateTime.MinValue.ToString())
        {
            label.Text = $"Last updated: {AppSettings.QuizJsonFileCreationTime}";
            label.IsVisible = true;
        }
    }
}