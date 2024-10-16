using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using MauiGeoBingo.Extensions;
using System.Diagnostics;

namespace MauiGeoBingo.Popups;

public partial class QuestionPopup : Popup
{
    private Result _question;

	public QuestionPopup(Result result)
    {
        InitializeComponent();

        question.Text = result.Question;

        _question = result;
        CreateAnswerButtons();

    }

    private void CreateAnswerButtons()
    {
        List<string> answers = _question.IncorrectAnswers;
        answers.Add(_question.CorrectAnswer);
        answers.Shuffle();

        int row = 0, col = 0;
        foreach (string answer in answers)
        {
            Button button = new()
            {
                Text = answer
            };

            button.Clicked += AnswerClicked;

            // TODO: måste tas bort vid skarpt läge
            if (_question.CorrectAnswer == answer) button.BackgroundColor = Colors.Gold;

            questionGrid.Add(button);
            questionGrid.SetRow(button, row);
            questionGrid.SetColumn(button, col);

            col++;
            if (col >= 2)
            {
                col = 0;
                row++;
            }
        }
    }

    private async void AnswerClicked(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            if (button.Text == _question.CorrectAnswer)
            {
                //Debug.WriteLine("Japp det är korrekt");
                await CloseAsync(true, cts.Token);
            }
            else
            {
                //Debug.WriteLine("Nej det är fel");
                await CloseAsync(false, cts.Token);
            }
        }
    }
}