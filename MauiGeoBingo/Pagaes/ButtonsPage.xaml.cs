using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using MauiGeoBingo.Extensions;
using Mopups.PreBaked.PopupPages.Loader;
using Mopups.PreBaked.Services;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiGeoBingo.Pagaes;

public partial class ButtonsPage : ContentPage
{
    private KeyValuePair<string, Button>? ActiveBingoButton { get; set; }

    private Button[,] _bingoButtons;

    private Color _winningColor = Colors.Green;

    private ServerViewModel? Server { get; set; } = null;

    public ButtonsPage()
    {
        InitializeComponent();

        gameGrid.Loaded += GridLoaded;
        _bingoButtons = new Button[4, 4];
    }

    public ButtonsPage(ServerViewModel server)
    {
        InitializeComponent();

        gameGrid.Loaded += GridLoaded;
        _bingoButtons = new Button[4, 4];

        Server = server;
    }

    private async void GridLoaded(object? sender, EventArgs e)
    {
        CreateButtons();

        if (Server != null)
        {
            // TODO: Fixa så att alla startar samtidigt. 
            /*
            Exempel:
            Alla klienter är redo och ägaren har tryckt på ok. (Vem som är ägare finns i variabeln Server.IsMyServer)
            När ägaren tryckt ok börjar en nedräkning på tex 10 sekunder som servern initsierar och skickar antal 
            sekunder kvar till respektive klient. Som sedan börjar nedräkningen genom att sätta tiden att vänta med await Task.Delay();
             */

            // TBD: Tror det kan vara så att jag behöver en websocket här.
            Task action;
            if (Server.IsMyServer)
            {
                action = Task.Run(async () => {
                    await Task.Delay(1000 * 60 * 5);
                });
            } 
            else
            {
                /*IDispatcherTimer timer;
                timer = Dispatcher.CreateTimer();
                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Tick += (s, e) =>
                {
                    // Komma här om ägaren är klar
                };
                timer.Start();*/

                /*while (true)
                {
                    // Gör en check här till ägaren är klar och svaret från api servern har svarat med en tid att vänta
                    if (game.IsReadyBool) { break; }
                }*/

                action = Task.Run(async () => {
                    await Task.Delay(1000 * 60 * 5);
                });
            }

            await PreBakedMopupService.GetInstance().WrapTaskInLoader(action, Colors.Blue, Colors.White, LoadingReasons(), Colors.Black);
        }
    }

    private List<string> LoadingReasons()
    {
        return ["The game will start soon", "It is near now....", "Get ready, soon it starts..."];
    }

    private async void CreateButtons()
    {
        Quiz? quiz = await Helpers.ReadJsonFile<Quiz>(@"quizDB.json");

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
            if (result != null) {
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
            Debug.WriteLine(text);
            questionGrid.Clear();

            if (ActiveBingoButton != null)
            {
                Button activeBtn = ActiveBingoButton.Value.Value;

                if (int.TryParse(activeBtn.Text, out int number))
                {
                    if (ActiveBingoButton.Value.Key == text) number++;
                    else number--;

                    activeBtn.Text = number.ToString();
                    if (number > 0)
                    {
                        activeBtn.BackgroundColor = _winningColor;
                        activeBtn.TextColor = Colors.Black;
                    }
                    else if (number == 0)
                    {
                        activeBtn.BackgroundColor = null;
                    }
                    else
                    {
                        activeBtn.BackgroundColor = Colors.Red;
                        activeBtn.TextColor = Colors.White;
                    }


                    if (CheckIfBingo())
                    {
                        string[] parts = activeBtn.ClassId.Split('-');
                        int row = int.Parse(parts[0]);
                        int col = int.Parse(parts[1]);

                        await SendStatusUpdateAsync(row, col, number, true);

                        await Toast.Make("Grattis du vann!!!", CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
                    }
                    else
                    {
                        string[] parts = activeBtn.ClassId.Split('-');
                        int row = int.Parse(parts[0]);
                        int col = int.Parse(parts[1]);
                        
                        await SendStatusUpdateAsync(row, col, number);
                    }

                }
            }

            ActiveBingoButton = null;
        }
    }

    private async Task SendStatusUpdateAsync(int row, int col, int value, bool winningMove = false)
    {
        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/update/game");
        await rec.PostAsync<ResponseData>(new
        {
            player_id = AppSettings.PlayerId,
            game_id = AppSettings.CurrentGameId,
            grid_row = row,
            grid_col = col,
            num = value,
            winning_move = winningMove,
        });
    }

    public class ResponseData
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
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
}

public class Quiz
{
    [JsonPropertyName("response_code")]
    public int ResponseCode { get; set; }
    public List<Result> Results { get; set; } = new List<Result>();
    public string Token { get; set; }
}

public class Result
{
    public string Category { get; set; }
    public string Type { get; set; }
    public string Difficulty { get; set; }
    public string Question { get; set; }

    [JsonPropertyName("correct_answer")]
    public string CorrectAnswer { get; set; }

    [JsonPropertyName("incorrect_answers")]
    public List<string> IncorrectAnswers { get; set; }
}

public class QuizButton : Button
{
    public Result? QUestionAndAnswer { get; set; } = null;
}




