using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using MauiGeoBingo.Extensions;
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

public partial class ButtonsPage : ContentPage, IDisposable
{
    private KeyValuePair<string, Button>? ActiveBingoButton { get; set; }

    private Button[,] _bingoButtons;

    private Color _winningColor = Colors.Green;

    private ServerViewModel? Server { get; set; } = null;

    private WebsocketClient _client;

    public ButtonsPage()
    {
        InitializeComponent();

        var url = new Uri(AppSettings.LocalWSBaseEndpoint);
        _client = new WebsocketClient(url);

        _bingoButtons = new Button[4, 4];
        gameGrid.Loaded += GridLoaded;

        Server = null;




        Server = new();
        Server.GameId = 1;
        Server.PlayerIds = new();
        Server.PlayerIds.Add(1);
        Server.PlayerIds.Add(2);
        UpdateAllGameSatus();

        Server = null;
    }

    public ButtonsPage(ServerViewModel server)
    {
        InitializeComponent();

        var url = new Uri(AppSettings.LocalWSBaseEndpoint);
        _client = new WebsocketClient(url);

        _bingoButtons = new Button[4, 4];
        gameGrid.Loaded += GridLoaded;


        Server = server;
    }

    private async void GridLoaded(object? sender, EventArgs e)
    {
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

            _client.MessageReceived.Subscribe(HandleSubscription);

            _client.ReconnectionHappened.Subscribe(async info =>
            {
                Debug.WriteLine($"Reconnection happened, type: {info.Type}");
                await Subscribe();
            });

            await _client.Start();
            await Subscribe();

            UpdateMyGameSatus();
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

    private async void HandleSubscription(ResponseMessage message)
    {
        var recived = JsonSerializer.Deserialize<ResponseData>(message.ToString());
        if (recived != null)
        {
            //Debug.WriteLine($"recived: {recived.IsRunning}");
            if (recived.Type == "sub_auth")
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

                    playerIds.Remove(AppSettings.PlayerId);
                    player1Button.Text = AppSettings.PlayerName;

                    for (int i = 0; i < playerIds.Count; i++)
                    {
                        Button button = buttons[i];
                        if (!button.IsVisible)
                        {
                            button.IsVisible = true;
                            string name = await GetNameAsync(playerIds[i]);
                            //Debug.WriteLine($"Name: {name}, ID: {playerIds[i]}");
                            button.Text = name;
                        }
                    }

                    //Debug.WriteLine($"IsRunning: {recived.IsRunning}");
                    if (recived.IsRunning)
                    {
                        waitingBox.IsVisible = false;
                        for (int row = 0; row < _bingoButtons.GetLength(0); row++)
                        {
                            for (int col = 0; col < _bingoButtons.GetLength(1); col++)
                            {
                                _bingoButtons[row, col].IsEnabled = true;
                            }
                        }
                        Unsubscribe();
                    }
                });
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

    private async void UpdateMyGameSatus()
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
    }

    private async void UpdateAllGameSatus()
    {
        if (Server == null) return;

        string endpoint = AppSettings.LocalBaseEndpoint;
        string url;
        if (Server.PlayerIds != null) url = $"{endpoint}/get/game/status/all/{string.Join(",", Server.PlayerIds)}/{Server.GameId}";
        else return;

        HttpRequest rec = new(url);

        var response = await rec.GetAsync<GameStatusRootobject>();

        if (response != null && response.Success)
        {
            int userId = AppSettings.PlayerId;
            int i = 0;
            int[,] currentUserNums = new int[4, 4];
            bool[,] currentUserHighs = new bool[4, 4];
            for (int row = 0; row < 4/*_bingoButtons.GetLength(0)*/; row++)
            {
                for (int col = 0; col < 4/*_bingoButtons.GetLength(1)*/; col++)
                {
                    int currentPId = Server.PlayerIds[i++];
                    if (i % 4 == Server.PlayerIds.Count) i = 0;

                    int number = response.GetAll(currentPId, row, col);
                    if (userId == currentPId) currentUserNums[row, col] = number;

                    if (number >= currentUserNums[row, col])
                    {
                        currentUserHighs[row, col] = false;
                    }
                    else
                    {
                        currentUserHighs[row, col] = true;
                    }
                    

                    // TODO: Uppdatera respektive fält här och se om det blir rätt

                    //Debug.WriteLine($"{row},{col} = {number}");
                    //_bingoButtons[row, col].Text = number.ToString();

                    //SetButtonColor(_bingoButtons[row, col], number);
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
            quiz = await Helpers.ReadJsonFile<Quiz>(fileName);
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

                        // TODO: Endast för att underlätta vid testning
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
                    SetButtonColor(activeBtn, number);

                    if (CheckIfBingo())
                    {
                        string[] parts = activeBtn.ClassId.Split('-');
                        int row = int.Parse(parts[0]);
                        int col = int.Parse(parts[1]);

                        await SendStatusUpdateAsync(row, col, number, true);

                        if (Server != null)
                        {
                            await SendStatusUpdateAsync(row, col, number, true);
                        }

                        DisableAllButtons();

                        await Toast.Make("Grattis du vann!!!", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
                    }
                    else
                    {
                        string[] parts = activeBtn.ClassId.Split('-');
                        int row = int.Parse(parts[0]);
                        int col = int.Parse(parts[1]);

                        int? winner = null;
                        if (Server != null) winner = await SendStatusUpdateAsync(row, col, number);

                        if (winner != null)
                        {
                            DisableAllButtons();

                            string playerName = await GetNameAsync(winner);
                            await Toast.Make($"'{playerName}' har vunnit!!!", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
                        }
                    }

                }
            }

            ActiveBingoButton = null;
        }
    }

    private void SetButtonColor(Button button, int number)
    {
        button.Text = number.ToString();
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
        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/update/game");
        var result = await rec.PostAsync<ResponseData>(new
        {
            player_id = AppSettings.PlayerId,
            game_id = AppSettings.CurrentGameId,
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

        // Kontrollera diagonalt (neråt höger)
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
        // Kontrollera diagonalt (neråt vänster)
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

    private async void Unsubscribe()
    {
        await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
        {
            action = "unsubscribe",
            topic = "waiting_for_server",
        })));
    }

    public async void Dispose()
    {
        Unsubscribe();

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


public class GameStatusRootobject
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("game_status")]
    public List<GameStatus> GameStatus { get; set; } = new();

    [JsonPropertyName("all_game_status")]
    [JsonConverter(typeof(AllGameStatusConverter))]
    public Dictionary<int, List<GameStatus>> AllGameStatus { get; set; } = new();

    public int Get(int row, int col)
    {
        int result = 0;
        foreach (GameStatus gs in GameStatus)
        {
            if (gs.Col == col && gs.Row == row)
            {
                result = gs.Number;
                break;
            }
        }
        return result;
    }

    public int GetAll(int playerId, int row, int col)
    {
        int result = 0;

        if (AllGameStatus.TryGetValue(playerId, out List<GameStatus>? gsList))
        {
            if (gsList != null)
            foreach (GameStatus gs in gsList)
            {
                if (gs.Col == col && gs.Row == row)
                {
                    result = gs.Number;
                    break;
                }
            }
        }
        return result;
    }

    public void SetHighest()
    {
        var highestScores = new Dictionary<(int Row, int Col), (int Number, GameStatus Status)>();

        foreach (var statusKvp in AllGameStatus)
        {
            List<GameStatus> statuses = statusKvp.Value;
            foreach (var status in statuses)
            {
                var gridPosition = (status.Row, status.Col);

                // Kontrollera om denna ruta redan har en högre poäng
                if (highestScores.ContainsKey(gridPosition))
                {
                    // Jämför den aktuella poängen med den som redan finns i dictionaryn
                    if (status.Number > highestScores[gridPosition].Number)
                    {
                        // Uppdatera högsta poängen och sätt IsHighestScore till false för tidigare vinnare
                        highestScores[gridPosition].Status.IsHighestNumber = false;
                        highestScores[gridPosition] = (status.Number, status);
                        status.IsHighestNumber = true;
                    }
                }
                else
                {
                    // Om det är första gången vi ser denna grid position, lägg till den
                    highestScores[gridPosition] = (status.Number, status);
                    status.IsHighestNumber = true;
                }
            }
        }
    }
}

public class GameStatus
{
    [JsonPropertyName("game_name")]
    public string GameName { get; set; }

    [JsonPropertyName("grid_col")]
    public int Col { get; set; }

    [JsonPropertyName("grid_row")]
    public int Row { get; set; }

    public int is_active { get; set; }
    public bool IsActive { get { return Convert.ToBoolean(is_active); } set { is_active = Convert.ToInt32(value); } }

    public int is_map { get; set; }
    public bool IsMap { get { return Convert.ToBoolean(is_map); } set { is_map = Convert.ToInt32(value); } }

    [JsonPropertyName("num")]
    public int Number { get; set; }

    public int is_highest_number { get; set; }
    public bool IsHighestNumber { get { return Convert.ToBoolean(is_highest_number); } set { is_highest_number = Convert.ToInt32(value); } }

    [JsonPropertyName("player_name")]
    public string PlayerName { get; set; }

    [JsonPropertyName("player_id")]
    public int PlayerId { get; set; }
}



public class AllGameStatusConverter : JsonConverter<Dictionary<int, List<GameStatus>>>
{
    public override Dictionary<int, List<GameStatus>> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = new Dictionary<int, List<GameStatus>>();

        // Eftersom `all_game_status` är en array, måste vi börja med att läsa in arrayen
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                break;

            // Nu börjar varje spel-ID objekt (exempelvis "1" eller "2")
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            reader.Read(); // Läser nyckeln (spel-ID)
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            // Hämta spel-ID som är en sträng och konvertera till int
            string keyString = reader.GetString();
            int key = int.Parse(keyString);

            reader.Read(); // Gå vidare till värdet (listan med GameStatus)

            var gameStatusList = JsonSerializer.Deserialize<List<GameStatus>>(ref reader, options);

            dictionary.Add(key, gameStatusList);

            reader.Read(); // Slut på objektet
        }

        return dictionary;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<int, List<GameStatus>> value, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Writing is not implemented for this converter");
    }
}




