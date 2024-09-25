using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;

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

    private void CreateButtons()
    {
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                Button btn = new Button
                {
                    Text = "0",
                };

                btn.Clicked += NewQuestion_Clicked;

                gameGrid.Add(btn, col, row);

                _bingoButtons[row, col] = btn;
            }
        }
    }

    private void NewQuestion_Clicked(object? sender, EventArgs e)
    {
        if (ActiveBingoButton != null) return;

        if (sender is Button questionBtn)
        {
            Label label = new()
            {
                Text = "Vad kan frågan vara?"
            };

            questionGrid.Add(label, 0, 0);
            questionGrid.SetColumnSpan(label, 2);

            for (int row = 1; row <= 2; row++)
            {
                for (int col = 0; col < 2; col++)
                {
                    Button btn = new Button
                    {
                        Text = $"Ok {row}-{col}",
                    };
                    btn.Clicked += Answer_ClickedAsync;
                    questionGrid.Add(btn, col, row);
                }
            }
            ActiveBingoButton = new KeyValuePair<string, Button>("Ok 1-1", questionBtn);
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
            }

            ActiveBingoButton = null;
        }
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