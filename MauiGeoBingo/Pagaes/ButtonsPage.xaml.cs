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
using CommunityToolkit.Maui.Alerts;
using System.Net.WebSockets;
using MauiGeoBingo.Helpers;

namespace MauiGeoBingo.Pagaes;

public partial class ButtonsPage : ContentPage
{

    private ButtonViewModel _buttonViewModel;

    private ServerViewModel? Server { get; set; } = null;

    private WebsocketClient _client;

    //IDispatcherTimer timer;

    public ButtonsPage()
	{
		InitializeComponent();
        _buttonViewModel = new();

        BindingContext = _buttonViewModel;
        _ = _buttonViewModel.CreateButtonsAsync();

        //thisPage.Loaded += ContentPageLoaded;

        /*timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(1000);
        timer.Tick += (s, e) =>
        {
            foreach (var button in _buttonViewModel.Buttons)
            {
                if (button.Score > 0)
                {
                    button.IsHighest = Random.Shared.NextDouble() > 0.5;
                }
                SetButtonColor(button);
            }
            
        };
        timer.Start();*/
    }

    public ButtonsPage(ServerViewModel server)
    {
        InitializeComponent();
        _buttonViewModel = new();
        BindingContext = _buttonViewModel;
        _ = _buttonViewModel.CreateButtonsAsync();

        Server = server;

        var url = new Uri(AppSettings.LocalWSBaseEndpoint);
        _client = new WebsocketClient(url);

        thisPage.Loaded += ContentPageLoaded;

    }

    private async void QuestionButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ButtonViewModel buttonViewModel)
        {
            var question = buttonViewModel.QuestionAndAnswer;
            //Debug.WriteLine($"Frågan: {question.Question}, Row: {buttonViewModel.Row}, Kolumn: {buttonViewModel.Col}");
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
               
            }
            else if (result == null)
            {
                buttonViewModel.Score--;
            }


            if (Server == null)
            {
                // TODO: Bara för testning
                if (buttonViewModel.Score > 6)
                {
                    buttonViewModel.IsHighest = true;
                }
                else if (buttonViewModel.Score > 4)
                {
                    buttonViewModel.IsHighest = Random.Shared.NextDouble() < 0.7;
                }
                else if (buttonViewModel.Score > 2)
                {
                    buttonViewModel.IsHighest = Random.Shared.NextDouble() < 0.5;
                }
                else
                {
                    buttonViewModel.IsHighest = Random.Shared.NextDouble() < 0.3;
                }
            }


            SetButtonColor(buttonViewModel);
            _buttonViewModel.SetNewQiestion(buttonViewModel);

            bool didIWin = isBingo();
            if (Server != null)
            {
                int winner = await SendStatusUpdateAsync(buttonViewModel.Row, buttonViewModel.Col, buttonViewModel.Score, didIWin);
                if (winner > 0) // Någon annan än denna spelare vann
                {
                    var winnerResult = await this.ShowPopupAsync(new WinningPopup(await Helper.GetNameAsync(winner)), CancellationToken.None);
                    if (winnerResult is bool)
                    {
                        await Unsubscribe();
                        await Navigation.PopAsync();
                    }
                }
            }

            if (didIWin) // jag vann
            {
                var winnerResult = await this.ShowPopupAsync(new WinningPopup(), CancellationToken.None);
                if (winnerResult is bool)
                {
                    if (Server != null) await Unsubscribe();
                    await Navigation.PopAsync();
                }
            }
        }
    }

    private bool isBingo()
    {
        ButtonViewModel[,] buttons = new ButtonViewModel[4, 4];
        foreach (var button in _buttonViewModel.Buttons)
        {
            buttons[button.Row, button.Col] = button;
        }

        ButtonViewModel button1;
        ButtonViewModel button2;
        ButtonViewModel button3;
        ButtonViewModel button4;
        // Kontrollera horisontellt
        for (int row = 0; row < 4; row++)
        {
            button1 = buttons[row, 0];
            button2 = buttons[row, 1];
            button3 = buttons[row, 2];
            button4 = buttons[row, 3];

            if (
                button1.BackgroundColor == _winningColor && button2.BackgroundColor == _winningColor &&
                button3.BackgroundColor == _winningColor && button4.BackgroundColor == _winningColor
                )
            {
                return true;
            }
        }

        // Kontrollera vertikalt
        for (int col = 0; col < 4; col++)
        {
            button1 = buttons[0, col];
            button2 = buttons[1, col];
            button3 = buttons[2, col];
            button4 = buttons[3, col];

            if (
                button1.BackgroundColor == _winningColor && button2.BackgroundColor == _winningColor &&
                button3.BackgroundColor == _winningColor && button4.BackgroundColor == _winningColor
                )
            {
                return true;
            }
        }

        // Kontrollera diagonalt (neråt höger)
        button1 = buttons[0, 0];
        button2 = buttons[1, 1];
        button3 = buttons[2, 2];
        button4 = buttons[3, 3];

        if (
            button1.BackgroundColor == _winningColor && button2.BackgroundColor == _winningColor &&
            button3.BackgroundColor == _winningColor && button4.BackgroundColor == _winningColor
            )
        {
            return true;
        }
        // Kontrollera diagonalt (neråt vänster)
        button1 = buttons[0, 3];
        button2 = buttons[1, 2];
        button3 = buttons[2, 1];
        button4 = buttons[3, 0];

        if (
            button1.BackgroundColor == _winningColor && button2.BackgroundColor == _winningColor &&
            button3.BackgroundColor == _winningColor && button4.BackgroundColor == _winningColor
            )
        {
            return true;
        }

        return false;
    }

    private Color _winningColor = Colors.Green;

    private void SetButtonColor(ButtonViewModel model)
    {
        
        if (model.Score > 0 && model.IsHighest)
        {
            model.BackgroundColor = _winningColor;
        } 
        else if (model.Score == 0 && model.IsHighest)
        {
            model.BackgroundColor = null;
        }
        else
        {
            model.BackgroundColor = Colors.Red;
        }
    }

    private async Task<bool> AddPlayerToGame()
    {
        if (Server == null) return false;

        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/add/player/to/game");
        var response = await rec.PutAsync<GameStatusRootobject>(new
        {
            player_id = AppSettings.PlayerId,
            game_id = Server.GameId,
        });

        if (response == null)
        {
            await DisplayAlert("Alert", "Somthing with ther server is wrong", "OK");
            return false;
        }
        return true;
    }

    private async void ContentPageLoaded(object? sender, EventArgs e)
    {
        bool playerAdded = await AddPlayerToGame();
        if (Server == null || !playerAdded) 
        {
            await Navigation.PopAsync();
            return;
        }

        var popup = new OverlayPopup(Server);
        popup.CanBeDismissedByTappingOutsideOfPopup = false;
        var result = await this.ShowPopupAsync(popup, CancellationToken.None);

        if (result is bool boolResult && boolResult)
        {
            Debug.WriteLine($"boolResult: {boolResult.ToString()}"); // Den här texten ska alldig skrivas ut
        }
        else if (result is GameStatusRootobject model)
        {
            List<Button> buttons = [player2Button, player3Button, player4Button];

            List<int> playerIds = model.PlayerIds.Take(4).ToList();

            //Debug.WriteLine($"är det detta... player_ids: {string.Join(", ", playerIds)}");

            playerIds.Remove(AppSettings.PlayerId);
            player1Button.Text = AppSettings.PlayerName;

            for (int i = 0; i < playerIds.Count; i++)
            {
                Button button = buttons[i];
                if (!button.IsVisible)
                {
                    button.IsVisible = true;
                    string name = await Helper.GetNameAsync(playerIds[i]);
                    Debug.WriteLine($"Name: {name}, ID: {playerIds[i]}");
                    button.Text = name;
                }
            }

            _client.MessageReceived.Subscribe(HandleSubscription);

            _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
            _client.ReconnectionHappened.Subscribe(async info =>
            {
                Debug.WriteLine($"Reconnection to new server scription happened, type: {info.Type}");
                await Subscribe();
            });

            await _client.Start();
            await Subscribe();

        }
        else
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await Toast.Make("Det är här det blir knas!").Show(cts.Token);
            await Navigation.PopAsync();
        }

    }

    private async Task Subscribe()
    {
        await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
        {
            action = "subscribe",
            topic = "waiting_for_server",
        })));
    }

    private async Task Unsubscribe()
    {
        await Task.Run(async () => {
            _client.Send(JsonSerializer.Serialize(new
            {
                action = "unsubscribe",
                topic = "waiting_for_server",
            }));
            await _client.Stop(WebSocketCloseStatus.NormalClosure, $"Closed in server by the {this.GetType().Name} client");
        });
    }

    private async void HandleSubscription(ResponseMessage message)
    {
        var recived = JsonSerializer.Deserialize<GameStatusRootobject>(message.ToString());

        if (recived != null)
        {
            if (recived.Type == "sub_auth" && Server != null)
            {
                await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
                {
                    action = "publish",
                    topic = "waiting_for_server",
                    message = "Give me the player count",
                    security_key = recived.SecurityKey,
                    game_id = Server.GameId,
                })));
            }
            else if (recived.Type == "message")
            {
                // TODO: uppdatera IsHighest för varje knapp som har högre scor än 0
                int playerId = AppSettings.PlayerId;
                foreach (var button in _buttonViewModel.Buttons)
                {
                    button.IsHighest = recived.IsHighest(playerId, button.Row, button.Col);
                    SetButtonColor(button);
                }
            }
        }
    }

    private async Task<int> SendStatusUpdateAsync(int row, int col, int value, bool winningMove = false)
    {
        if (Server == null) return 0;

        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/update/game");
        var result = await rec.PostAsync<GameStatusRootobject>(new
        {
            player_id = AppSettings.PlayerId,
            game_id = Server.GameId,
            grid_row = row,
            grid_col = col,
            num = value,
            winning_move = winningMove,
        });

        if (result == null) return 0;

        return result.Winner?? 0;
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

    private bool _isHighest = false;
    public bool IsHighest
    {
        get => _isHighest;
        set
        {
            if (_isHighest != value)
            {
                _isHighest = value;
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

    private Color? _backgroundColor;
    public Color? BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor != value)
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }
    }

    private Color _txtColor;
    public Color TxtColor
    {
        get {
            if (BackgroundColor == ColorP1) return Colors.Black;
            if (BackgroundColor == ColorP2) return Colors.White;
            if (BackgroundColor == ColorP3) return Colors.White;
            if (BackgroundColor == ColorP4) return Colors.White;
            if (BackgroundColor == Colors.Red) return Colors.White;
            if (BackgroundColor == Colors.Green) return Colors.White;

            return Colors.Black;
                    
            } 
    }

    public Color ColorWin = Colors.Lime;

    public Color ColorP1 = Colors.Wheat;
    public Color ColorP2 = Colors.Blue;
    public Color ColorP3 = Colors.Red;
    public Color ColorP4 = Colors.Yellow;

    private List<Result> _questions = [];

    public ButtonViewModel()
	{
        Buttons = [];

        BackgroundColor = ColorP1;
    }

    public async Task CreateButtonsAsync()
    {
        Quiz? quiz = null;
        string fileName = AppSettings.QuizJsonFileName;
        if (await FileSystem.Current.AppPackageFileExistsAsync(fileName))
        {
            quiz = await Helper.ReadJsonFile<Quiz>(fileName);
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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

