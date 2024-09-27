using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using MauiGeoBingo.Extensions;
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

    public ButtonsPage()
    {
        InitializeComponent();

        gameGrid.Loaded += GridLoaded;
        _bingoButtons = new Button[4, 4];
    }

    private void GridLoaded(object? sender, EventArgs e)
    {
        CreateButtons();
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

                    btn.Clicked += QuestionBtn_Clicked;

                    Result result = results[Random.Shared.Next(results.Count)];
                    results.Remove(result);
                    ToolTipProperties.SetText(btn, result.Category);

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
                        await Toast.Make("Grattis du vann!!!").Show();
                    }

                }

                await SendStatusUpdateAsync();
            }

            ActiveBingoButton = null;
        }
    }

    private async Task SendStatusUpdateAsync()
    {
        int[,] buttonsNum = new int[4, 4];
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                if (int.TryParse(_bingoButtons[row, col].Text, out int number))
                {
                    buttonsNum[row, col] = number;
                }
            }
        }

        // TODO: 

        HttpRequest rec = new("http://127.0.0.1:5000");

        await rec.PutAsync<ResponseData>(new
        {
            Name = "Fredrik",
            PlayerId = AppSettings.PlayerId,
            IsMap = false
        });
    }

    public class ResponseData
    {

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




