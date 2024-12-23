using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using MauiGeoBingo.Converters;
using MauiGeoBingo.Extensions;
using MauiGeoBingo.Helpers;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Websocket.Client;

namespace MauiGeoBingo.Pagaes;

public partial class _ButtonsPage : ContentPage, IDisposable
{
    private KeyValuePair<string, Button>? ActiveBingoButton { get; set; }

    private Button[,] _bingoButtons;

    private Color _winningColor = Colors.Green;

    private ServerViewModel? Server { get; set; } = null;

    private WebsocketClient _client;

    public _ButtonsPage()
    {
        InitializeComponent();

        var url = new Uri(AppSettings.LocalWSBaseEndpoint);
        _client = new WebsocketClient(url);

        _bingoButtons = new Button[4, 4];
        gameGrid.Loaded += GridLoaded;

        Server = null;
    }

    public _ButtonsPage(ServerViewModel server)
    {
        InitializeComponent();

        var url = new Uri(AppSettings.LocalWSBaseEndpoint);
        _client = new WebsocketClient(url);

        _bingoButtons = new Button[4, 4];
        gameGrid.Loaded += GridLoaded;


        Server = server;


        // FIXME: Full�sning f�r att server.PlayerIds ibland �r tom under android
        if (server.PlayerIds == null || server.PlayerIds.Count < 1)
        {
            _ = SetMissingPlayerIds();
        }

    }

    private async Task SetMissingPlayerIds()
    {
        if (Server == null) return;

        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/get/game/players/{Server.GameId}");
        var result = await rec.GetAsync<ResponseData>();
        if (result != null)
        {
            Server.PlayerIds = result.PlayerIds;
        }
    }

    private async void GridLoaded(object? sender, EventArgs e)
    {
        player1Button.Text = AppSettings.PlayerName;

        await Task.Delay(500);

        CreateButtons();

        if (Server != null && await AddPlayerToGame())
        {

            waitingBox.IsVisible = true;

            while (_bingoButtons[3, 3] == null)
            {
                await Task.Delay(10);
            }

            DisableAllButtons();

            if (Server.IsMyServer)
            {
                startGame.IsVisible = true;
            }

            var url = new Uri(AppSettings.LocalWSBaseEndpoint);
            _client = new WebsocketClient(url);

            _client.MessageReceived.Subscribe(HandleSubscriptionToServers);

            _client.ReconnectionHappened.Subscribe(async info =>
            {
                Debug.WriteLine($"Reconnection to new server scription happened, type: {info.Type}");
                await SubscribeToServers();
            });

            await _client.Start();
            await SubscribeToServers();

            //UpdateMyGameSatus();
            await UpdateAllGameSatus();
        }
    }

    private async Task SubscribeToServers()
    {
        await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
        {
            action = "subscribe",
            topic = "waiting_for_server",
        })));
    }

    private async void UnsubscribeToServers()
    {
        await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
        {
            action = "unsubscribe",
            topic = "waiting_for_server",
        })));
    }

    private async void HandleSubscriptionToServers(ResponseMessage message)
    {
        // TODO kolla om det �r m�jligt att skicka med den datan som finns i UpdateAllGameSatus() f�r att minska api calls
        var recived = JsonSerializer.Deserialize<ResponseData>(message.ToString());
        if (recived != null)
        {
            //Debug.WriteLine($"recived: {recived.IsRunning}");
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
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    int playerNum = recived.PlayerCount;
                    waitingText.Text = $"Waiting for players to join.\n{playerNum} joined so far";
                

                    List<Button> buttons = [player2Button, player3Button, player4Button];

                    List<int> playerIds = recived.PlayerIds.Take(4).ToList();

                    Debug.WriteLine($"player_ids: {string.Join(", ", playerIds)}");

                    playerIds.Remove(AppSettings.PlayerId);
                    player1Button.Text = AppSettings.PlayerName;

                    for (int i = 0; i < playerIds.Count; i++)
                    {
                        Button button = buttons[i];
                        if (!button.IsVisible)
                        {
                            button.IsVisible = true;
                            string name = await GetNameAsync(playerIds[i]);
                            Debug.WriteLine($"Name: {name}, ID: {playerIds[i]}");
                            button.Text = name;
                        }
                    }

                    //Debug.WriteLine($"IsRunning: {recived.IsRunning}");
                    if (recived.IsRunning && waitingBox.IsVisible)
                    {
                        waitingBox.IsVisible = false;
                        Debug.WriteLine("####### Avregga ToServers prenumerationen ##########");

                        for (int row = 0; row < _bingoButtons.GetLength(0); row++)
                        {
                            for (int col = 0; col < _bingoButtons.GetLength(1); col++)
                            {
                                _bingoButtons[row, col].IsEnabled = true;
                            }
                        }

                        UnsubscribeToServers();

                        // Get python servern lite tid att svregestrera prenumerationen
                        await Task.Delay(300);

                        // Prenumerera p� game status
                        var url = new Uri(AppSettings.LocalWSBaseEndpoint);
                        _client = new WebsocketClient(url);

                        _client.MessageReceived.Subscribe(HandleSubscriptionToGameStatus);

                        _client.ReconnectionHappened.Subscribe(async info =>
                        {
                            Debug.WriteLine($"Reconnection to GameStatus scription happened, type: {info.Type}");
                            await SubscribeToGameStatus();
                        });

                        await _client.Start();
                        await SubscribeToGameStatus();

                    }
                });

            }

        }
    }

    private async Task SubscribeToGameStatus()
    {
        await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
        {
            action = "subscribe",
            topic = "stream_game_status",
        })));
    }

    private async void UnsubscribeGameStatus()
    {
        await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
        {
            action = "unsubscribe",
            topic = "stream_game_status",
        })));
    }

    private async void HandleSubscriptionToGameStatus(ResponseMessage message)
    {
        var recived = JsonSerializer.Deserialize<GameStatusRootobject>(message.ToString());
        if (Server != null && recived != null && recived.Success)
        {
            if (recived.Type == "sub_auth" && Server != null)
            {
                await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
                {
                    action = "publish",
                    topic = "stream_game_status",
                    message = "Give me the the game status",
                    security_key = recived.SecurityKey,
                    game_id = Server.GameId,
                })));
            }
            else if (recived.Type == "message")
            {
                UpdateGrid(recived);
            }
        }
    }

    private void DisableAllButtons()
    {
        for (int row = 0; row < _bingoButtons.GetLength(0); row++)
        {
            for (int col = 0; col < _bingoButtons.GetLength(1); col++)
            {
                _bingoButtons[row, col].IsEnabled = false;
            }
        }
    }

    /*private async void UpdateMyGameSatus()
    {
        if (Server == null) return;

        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/get/game/status/{AppSettings.PlayerId}/{Server.GameId}");
        //HttpRequest rec = new($"{endpoint}/get/game/status/1/1");

        var response = await rec.GetAsync<GameStatusRootobject>();

        if (response != null && response.Success)
        {
            for (int row = 0; row < _bingoButtons.GetLength(0); row++)
            {
                for (int col = 0; col < _bingoButtons.GetLength(1); col++)
                {
                    int number = response.Get(row, col);
                    _bingoButtons[row, col].Text = number.ToString();

                    SetButtonColor(_bingoButtons[row, col], number);
                }
            }
        }
    }*/

    private async Task UpdateAllGameSatus()
    {
        if (Server == null) return;

        string endpoint = AppSettings.LocalBaseEndpoint;
        string url;
        if (Server.PlayerIds != null) url = $"{endpoint}/get/game/status/all/{string.Join(",", Server.PlayerIds)}/{Server.GameId}";
        else return;

        Debug.WriteLine(url);

        HttpRequest rec = new(url);

        var response = await rec.GetAsync<GameStatusRootobject>();
        if (response == null || !response.Success) return;

        UpdateGrid(response);
    }

    private void UpdateGrid(GameStatusRootobject response)
    {
        int userId = AppSettings.PlayerId;
        //int userId = 1;
        int i = 0;
        for (int row = 0; row < _bingoButtons.GetLength(0); row++)
        {
            for (int col = 0; col < _bingoButtons.GetLength(1); col++)
            {
                (int number, bool isHighest) = response.GetNumberAndIsHighest(userId, row, col);


                // TBD: b�r komma p� n�got b�ttre s�tt att visa om spelaren har h�gst po�ng eller inte f�r knappen
                _bingoButtons[row, col].Text = number.ToString();
                if (isHighest)
                {
                    SetButtonColor(_bingoButtons[row, col], number);
                }
                else
                {
                    SetButtonColor(_bingoButtons[row, col], -1);
                }
            }
        }
    }

    private async Task<bool> AddPlayerToGame()
    {
        if (Server == null) return false;

        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/add/player/to/game");
        var response = await rec.PutAsync<ResponseData>(new
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

    private async void StartGameClicked(object sender, EventArgs e)
    {
        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/set/game/as/running");

        if (Server != null)
        {
            await rec.PostAsync<ResponseData>(new
            {
                game_id = Server.GameId
            });
        }
    }

    private async void CreateButtons()
    {
        Quiz? quiz = null;
        string fileName = AppSettings.QuizJsonFileName;
        if (await FileSystem.Current.AppPackageFileExistsAsync(fileName))
        {
            quiz = await Helper.ReadJsonFile<Quiz>(fileName);
        }

        if (quiz != null && quiz.Results != null)
        {
            List<Result> results;
            string selectedCat = AppSettings.QuizCategorie;
            if (selectedCat == "All")
            {
                results = quiz.Results;
            }
            else
            {
                results = quiz.Results.Where(r => r.Category.StartsWith(AppSettings.QuizCategorie)).ToList();
            }

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    QuizButton btn = new QuizButton
                    {
                        Text = "0",
                    };

                    btn.ClassId = $"{row}-{col}";

                    btn.Clicked += QuestionBtn_Clicked;

                    Result result = results[Random.Shared.Next(results.Count)];
                    results.Remove(result);
                    ToolTipProperties.SetText(btn, $"{result.Category} ({result.Difficulty})");

                    btn.QUestionAndAnswer = result;

                    gameGrid.Add(btn, col, row);

                    _bingoButtons[row, col] = btn;
                }
            }
        }
        else
        {
            await DisplayAlert("Alert", "Could not find any questions for you", "OK");
        }

    }

    private void QuestionBtn_Clicked(object? sender, EventArgs e)
    {
        if (ActiveBingoButton != null) return;


        if (sender is QuizButton questionBtn)
        {
            Result? result = questionBtn.QUestionAndAnswer;
            if (result != null)
            {
                Label label = new()
                {
                    Text = result.Question,
                };

                questionGrid.Add(label, 0, 0);
                questionGrid.SetColumnSpan(label, 2);

                List<string> answers = result.IncorrectAnswers;
                answers.Add(result.CorrectAnswer);
                answers.Shuffle();

                int rows = 2, cols = 2;
                if (answers.Count < 4)
                {
                    rows = 1;
                }

                int index = 0;
                for (int row = 1; row <= rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        Button btn = new Button
                        {
                            Text = answers[index],
                        };

                        // TODO: Endast f�r att underl�tta vid testning
                        if (result.CorrectAnswer == answers[index])
                        {
                            btn.BackgroundColor = Colors.Gold;
                        }

                        btn.Clicked += Answer_ClickedAsync;
                        questionGrid.Add(btn, col, row);

                        index++;
                    }
                }
                ActiveBingoButton = new KeyValuePair<string, Button>(result.CorrectAnswer, questionBtn);
            }
        }
    }

    private async void Answer_ClickedAsync(object? sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            string text = btn.Text;
            //Debug.WriteLine(text);
            questionGrid.Clear();

            if (ActiveBingoButton != null)
            {
                Button activeBtn = ActiveBingoButton.Value.Value;

                if (int.TryParse(activeBtn.Text, out int number))
                {
                    if (ActiveBingoButton.Value.Key == text) number++;
                    else number--;

                    activeBtn.Text = number.ToString();

                    SetButtonColor(activeBtn, number);

                    
                    string[] parts = activeBtn.ClassId.Split('-');
                    int row = int.Parse(parts[0]);
                    int col = int.Parse(parts[1]);
                    if (Server != null) 
                    {
                        int? winner = await SendStatusUpdateAsync(row, col, number);

                        if (winner != null)
                        {
                            DisableAllButtons();

                            string playerName = await GetNameAsync(winner);
                            Debug.WriteLine(playerName + " har vunnit....");
                            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                            await Toast.Make($"'{playerName}' har vunnit!!!", ToastDuration.Long).Show(cts.Token);
                            ActiveBingoButton = null;
                            return;
                        }
                    }

                    if (CheckIfBingo())
                    {
                        Debug.WriteLine("Japp jag vann!!!");

                        DisableAllButtons();

                        if (Server != null) SetWinner(Server.GameId ?? 0, AppSettings.PlayerId);

                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await Toast.Make("Grattis du vann!!!", ToastDuration.Long).Show(cts.Token);
                    }
                }
            }

            ActiveBingoButton = null;
        }
    }

    private async void SetWinner(int gameId, int playerId)
    {
        if (Server != null)
        {
            string endpoint = AppSettings.LocalBaseEndpoint;
            HttpRequest rec = new($"{endpoint}/set/game/winner");
            var response = await rec.PutAsync<ResponseData>(new
            {
                player_id = playerId,
                game_id = gameId,
            });
        } 
    }

    private void SetButtonColor(Button button, int number)
    {
        
        if (number > 0)
        {
            button.BackgroundColor = _winningColor;
            button.TextColor = Colors.Black;
        }
        else if (number == 0)
        {
            button.BackgroundColor = null;
        }
        else
        {
            button.BackgroundColor = Colors.Red;
            button.TextColor = Colors.White;
        }
    }

    private async Task<string> GetNameAsync(int? playerId)
    {
        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/player/name?player_id={playerId}");

        var result = await rec.GetAsync<ResponseData>();

        if (result != null && result.PlayerName != null)
        {
            return result.PlayerName;
        }
        return string.Empty;
    }

    private async Task<int?> SendStatusUpdateAsync(int row, int col, int value, bool winningMove = false)
    {
        if (Server == null) return null;

        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/update/game");
        var result = await rec.PostAsync<ResponseData>(new
        {
            player_id = AppSettings.PlayerId,
            game_id = Server.GameId,
            grid_row = row,
            grid_col = col,
            num = value,
            winning_move = winningMove,
        });

        if (result == null) return null;

        return result.Winner;
    }

    public class ResponseData
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("player_count")]
        public int PlayerCount { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("topic")]
        public string? Topic { get; set; }
        [JsonPropertyName("security_key")]
        public string? SecurityKey { get; set; }

        [JsonPropertyName("player_ids")]
        public List<int> PlayerIds { get; set; } = new();

        [JsonPropertyName("is_running")]
        public bool IsRunning { get; set; }

        [JsonPropertyName("winner")]
        public int? Winner { get; set; }

        [JsonPropertyName("player_name")]
        public string? PlayerName { get; set; }
    }

    private bool CheckIfBingo()
    {
        Button button1;
        Button button2;
        Button button3;
        Button button4;
        // Kontrollera horisontellt
        for (int row = 0; row < 4; row++)
        {
            button1 = _bingoButtons[row, 0];
            button2 = _bingoButtons[row, 1];
            button3 = _bingoButtons[row, 2];
            button4 = _bingoButtons[row, 3];

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
            button1 = _bingoButtons[0, col];
            button2 = _bingoButtons[1, col];
            button3 = _bingoButtons[2, col];
            button4 = _bingoButtons[3, col];

            if (
                button1.BackgroundColor == _winningColor && button2.BackgroundColor == _winningColor &&
                button3.BackgroundColor == _winningColor && button4.BackgroundColor == _winningColor
                )
            {
                return true;
            }
        }

        // Kontrollera diagonalt (ner�t h�ger)
        button1 = _bingoButtons[0, 0];
        button2 = _bingoButtons[1, 1];
        button3 = _bingoButtons[2, 2];
        button4 = _bingoButtons[3, 3];

        if (
            button1.BackgroundColor == _winningColor && button2.BackgroundColor == _winningColor &&
            button3.BackgroundColor == _winningColor && button4.BackgroundColor == _winningColor
            )
        {
            return true;
        }
        // Kontrollera diagonalt (ner�t v�nster)
        button1 = _bingoButtons[0, 3];
        button2 = _bingoButtons[1, 2];
        button3 = _bingoButtons[2, 1];
        button4 = _bingoButtons[3, 0];

        if (
            button1.BackgroundColor == _winningColor && button2.BackgroundColor == _winningColor &&
            button3.BackgroundColor == _winningColor && button4.BackgroundColor == _winningColor
            )
        {
            return true;
        }

        return false;
    }

    protected override bool OnBackButtonPressed()
    {
        Dispose();
        base.OnBackButtonPressed();
        return false;
    }

    public async void Dispose()
    {
        UnsubscribeToServers();

        UnsubscribeGameStatus();

        if (_client.IsRunning)
        {
            await Task.Delay(50);
            await _client.Stop(WebSocketCloseStatus.NormalClosure, $"Closed in server by the {this.GetType().Name} client");
            _client.Dispose();
        }
    }
}


public class QuizButton : Button
{
    public Result? QUestionAndAnswer { get; set; } = null;
}




