using MauiGeoBingo.Classes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiGeoBingo.Helpers;
using Websocket.Client;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text.Json;
using MauiGeoBingo.Pagaes;


namespace MauiGeoBingo.Models;

public class ButtonViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ButtonViewModel> Buttons { get; set; }

    private WebsocketClient _client;

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

    //private Color _txtColor;
    public Color TxtColor
    {
        get
        {
            if (BackgroundColor == ColorP1) return Colors.Black;
            else if (BackgroundColor == ColorP2) return Colors.White;
            else if (BackgroundColor == ColorP3) return Colors.Black;
            else if (BackgroundColor == ColorP4) return Colors.Black;
            else if (BackgroundColor == Colors.Red) return Colors.Black;
            else if (BackgroundColor == Colors.Green) return Colors.White;
            else if (BackgroundColor == null) return Colors.White;

            return Colors.Black;

        }
    }

    public static Color ColorWin = Colors.Lime;
    public static Color ColorLoose = Colors.Red;

    public Color ColorP1 = Colors.Wheat;
    public Color ColorP2 = Colors.Blue;
    public Color ColorP3 = Colors.DarkSalmon;
    public Color ColorP4 = Colors.Yellow;

    private List<Result> _questions = [];

    public ButtonViewModel()
    {
        Buttons = [];

        BackgroundColor = ColorP1;

        var url = new Uri(AppSettings.LocalWSBaseEndpoint);
        _client = new WebsocketClient(url);
    }

    private int _gameId;
    private List<int> _playerIds;
    /*public ButtonViewModel(int gameId)
    {
        _gameId = gameId;
        Buttons = [];

        BackgroundColor = ColorP1;

        var url = new Uri(AppSettings.LocalWSBaseEndpoint);
        _client = new WebsocketClient(url);
    }*/

    public async Task StartClient(int gameId)
    {
        _gameId = gameId;

        _client.MessageReceived.Subscribe(HandleSubscription);

        _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
        _client.ReconnectionHappened.Subscribe(async info =>
        {
            Debug.WriteLine($"Reconnection to waiting for new server happened, type: {info.Type}");
            await Subscribe();
        });

        await _client.Start();
        await Subscribe();
    }

    private async Task Subscribe()
    {
        await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
        {
            action = "subscribe",
            topic = "stream_game_status",
        })));
    }

    public async Task Unsubscribe()
    {
        Debug.WriteLine("unsubscribe från stream_game_status");
        _client.Send(JsonSerializer.Serialize(new
        {
            action = "unsubscribe",
            topic = "stream_game_status",
        }));
        await _client.Stop(WebSocketCloseStatus.NormalClosure, $"Closed in server by the {this.GetType().Name} client");
    }

    private void HandleSubscription(ResponseMessage message)
    {
        var recived = JsonSerializer.Deserialize<GameStatusRootobject>(message.ToString());
        int msgCount = 0;
        if (recived != null)
        {
            //Debug.WriteLine($"recived.Type: {recived.Type}, recived.Winner: {recived.Winner}");
            if (recived.Type == "sub_auth")
            {
                Debug.WriteLine($"GameId: {_gameId}, recived.SecurityKey: {recived.SecurityKey}");
                _client.Send(JsonSerializer.Serialize(new
                {
                    action = "publish",
                    topic = "stream_game_status",
                    message = "Give me the the game status",
                    security_key = recived.SecurityKey,
                    game_id = _gameId
                }));
            }
            else if (recived.Type == "message")
            {
                int playerId = AppSettings.PlayerId;
                /*if (++msgCount == 1)
                {
                    foreach (var button in Buttons)
                    {
                        (int number, bool isHighest) = recived.GetNumberAndIsHighest(playerId, button.Row, button.Col);

                        button.IsHighest = isHighest;
                        button.Score = number;
                        SetButtonColor(button);
                    }
                }
                else*/
                    // TODO: uppdatera IsHighest för varje knapp som har högre scor än 0

                foreach (var button in Buttons)
                {
                    //button.IsHighest = recived.IsHighest(playerId, button.Row, button.Col);

                    (int number, bool isHighest) = recived.GetNumberAndIsHighest(playerId, button.Row, button.Col);

                    if (number > 0) button.IsHighest = isHighest;
                    //button.Score = number;

                    /*if (number > 0)*/ Debug.WriteLine($"GameId: {_gameId}, playerId: {playerId}, button.Row: {button.Row}, button.Col: {button.Col}, Is highest number: {isHighest}, Number: {number}, count: {recived.GameStatus.Count}");

                    //SetButtonColor(button);
                }
            }
        }
    }

    public void SetButtonColor(ButtonViewModel model)
    {

        if (model.Score > 0 && model.IsHighest)
        {
            model.BackgroundColor = ColorWin;
        }
        else if (model.Score == 0)
        {
            model.BackgroundColor = null;
        }
        else
        {
            model.BackgroundColor = Colors.Red;
        }
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

    public async Task<bool> AddPlayerToGame()
    {
        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/add/player/to/game");
        var response = await rec.PutAsync<GameStatusRootobject>(new
        {
            player_id = AppSettings.PlayerId,
            game_id = _gameId,
        });

        if (response == null)
        {
            //await DisplayAlert("Alert", "Somthing with ther server is wrong", "OK");
            return false;
        }
        return true;
    }

    public async Task<int> SendStatusUpdateAsync(int row, int col, int value, bool winningMove = false)
    {
        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/update/game");
        var result = await rec.PostAsync<GameStatusRootobject>(new
        {
            player_id = AppSettings.PlayerId,
            game_id = _gameId,
            grid_row = row,
            grid_col = col,
            num = value,
            winning_move = winningMove,
        });

        if (result == null) return 0;

        return result.Winner ?? 0;
    }


   public async Task UpdateAllGameSatus(int gameId, List<int> playerIds)
    {
        string endpoint = AppSettings.LocalBaseEndpoint;
        string url = $"{endpoint}/get/game/status/all/{string.Join(",", playerIds)}/{gameId}";

        Debug.WriteLine(url);

        HttpRequest rec = new(url);

        var recived = await rec.GetAsync<GameStatusRootobject>();
        if (recived == null || !recived.Success) return;

        int playerId = AppSettings.PlayerId;

        foreach (var button in Buttons)
        {
            //button.IsHighest = recived.IsHighest(playerId, button.Row, button.Col);

            (int number, bool isHighest) = recived.GetNumberAndIsHighest(playerId, button.Row, button.Col);

            if (number != 0) 
            {
                button.IsHighest = isHighest;
                button.Score = number;
            }

            /*if (number > 0)*/
            Debug.WriteLine($"GameId: {_gameId}, playerId: {playerId}, button.Row: {button.Row}, button.Col: {button.Col}, Is highest number: {isHighest}, Number: {number}, count: {recived.GameStatus.Count}");

            SetButtonColor(button);
        }
    }


    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    
}

