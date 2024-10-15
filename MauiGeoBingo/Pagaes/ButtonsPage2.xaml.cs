using MauiGeoBingo.Classes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Popups;
using System.Globalization;
using Websocket.Client;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiGeoBingo.Pagaes;

public partial class ButtonsPage2 : ContentPage
{

    private ButtonViewModel _buttonViewModel;

    private ServerViewModel? Server { get; set; } = null;

    private WebsocketClient _client;

    public ButtonsPage2()
	{
		InitializeComponent();
        _buttonViewModel = new();

        BindingContext = _buttonViewModel;
        _buttonViewModel.CreateButtons();

        //thisPage.Loaded += ContentPage_Loaded;
    }

    public ButtonsPage2(ServerViewModel server)
    {
        InitializeComponent();
        _buttonViewModel = new();
        BindingContext = _buttonViewModel;
        _buttonViewModel.CreateButtons();

        Server = server;

        var url = new Uri(AppSettings.LocalWSBaseEndpoint);
        _client = new WebsocketClient(url);

        thisPage.Loaded += ContentPage_Loaded;

    }

    private async void QuestionButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ButtonViewModel buttonViewModel)
        {
            var question = buttonViewModel.QuestionAndAnswer;
            Debug.WriteLine($"Frågan: {question.Question}, Row: {buttonViewModel.Row}, Kolumn: {buttonViewModel.Col}");
            var popup = new QuestionPopup(question);

            var result = await this.ShowPopupAsync(popup, CancellationToken.None);
            if (result is bool boolResult)
            {
                if (boolResult)
                {
                    buttonViewModel.Score++;
                }
                else
                {
                    buttonViewModel.Score--;
                }
                SetButtonColor(btn, buttonViewModel.Score);
                _buttonViewModel.SetNewQiestion(buttonViewModel);
            }
        }
    }

    private Color _winningColor = Colors.Green;

    private void SetButtonColor(Button button, int score)
    {
        
        if (score > 0)
        {
            button.BackgroundColor = _winningColor;
        } 
        else if (score == 0)
        {
            button.BackgroundColor = null;
        }
        else
        {
            button.BackgroundColor = Colors.Red;
        }
    }

    private async void ContentPage_Loaded(object? sender, EventArgs e)
    {
        if (Server == null) 
        {
            await Navigation.PopAsync();
            return;
        }

        var popup = new OverlayPopup(Server);
        var result = await this.ShowPopupAsync(popup, CancellationToken.None);

        if (result is bool boolResult && boolResult)
        {
            Debug.WriteLine($"boolResult: {boolResult.ToString()}");
        }
        else if (result is GameStatusRootobject model)
        {
            List<Button> buttons = [player2Button, player3Button, player4Button];

            List<int> playerIds = model.PlayerIds.Take(4).ToList();

            Debug.WriteLine($"player_ids: {string.Join(", ", playerIds)}");

            playerIds.Remove(AppSettings.PlayerId);
            player1Button.Text = AppSettings.PlayerName;

            for (int i = 0; i < playerIds.Count; i++)
            {
                Button button = buttons[i];
                if (!button.IsVisible)
                {
                    button.IsVisible = true;
                    string name = await Helpers.GetNameAsync(playerIds[i]);
                    Debug.WriteLine($"Name: {name}, ID: {playerIds[i]}");
                    button.Text = name;
                }
            }
        }
        else
        {
            await Navigation.PopAsync();
        }

    }

    
}

public class ButtonViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ButtonViewModel> Buttons { get; set; }

	private int _score;
	public int Score 
    {
        get => _score;
        set
        {
            if (_score != value)
            {
                _score = value;
                OnPropertyChanged();
            }
        }
    }

    private int _row;
    public int Row
    {
        get => _row;
        set
        {
            if (_row != value)
            {
                _row = value;
                OnPropertyChanged();
            }
        }
    }

    private int _col;
    public int Col
    {
        get => _col;
        set
        {
            if (_col != value)
            {
                _col = value;
                OnPropertyChanged();
            }
        }
    }

    private Result _questionAndAnswer = new();
    public Result QuestionAndAnswer
    {
        get => _questionAndAnswer;
        set
        {
            if (_questionAndAnswer != value)
            {
                _questionAndAnswer = value;
                OnPropertyChanged();
            }
        }
    }

    private string _toolTip = string.Empty;
    public string ToolTip
    {
        get => _toolTip;
        set
        {
            if (_toolTip != value)
            {
                _toolTip = value;
                OnPropertyChanged();
            }
        }
    }

    private Color? _bgColor;
    public Color? BgColor
    {
        get => _bgColor;
        set
        {
            if (_bgColor != value)
            {
                _bgColor = value;
                OnPropertyChanged();
            }
        }
    }

   

    private List<Result> _questions = [];

    private bool _isHighest = true;
    public bool IsHighest => _isHighest;


    public ButtonViewModel()
	{
        Buttons = [];
    }

    public async void CreateButtons()
    {
        Quiz? quiz = null;
        string fileName = AppSettings.QuizJsonFileName;
        if (await FileSystem.Current.AppPackageFileExistsAsync(fileName))
        {
            quiz = await Helpers.ReadJsonFile<Quiz>(fileName);
        }

        if (quiz != null && quiz.Results != null)
        {
            string selectedCat = AppSettings.QuizCategorie;
            if (selectedCat == "All")
            {
                _questions = quiz.Results;
            }
            else
            {
                _questions = quiz.Results.Where(r => r.Category.StartsWith(AppSettings.QuizCategorie)).ToList();
            }

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Result result = _questions[Random.Shared.Next(_questions.Count)];
                    _questions.Remove(result);

                    ButtonViewModel model = new();
                    model.Score = 0;
                    model.Row = row;
                    model.Col = col;
                    model.QuestionAndAnswer = result;
                    model.ToolTip = $"{result.Category} ({result.Difficulty})";
                    Buttons.Add(model);
                }
            }
        }
    }

    public void SetNewQiestion(ButtonViewModel model)
    {
        Result result = _questions[Random.Shared.Next(_questions.Count)];
        _questions.Remove(result);
        model.QuestionAndAnswer = result;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}


public class StringToColourConverter : IValueConverter, IMarkupExtension
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            switch (s)
            {
                case "green":
                    return Colors.Green;
                case "red":
                    return Colors.Red;
            }
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

