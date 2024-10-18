using MauiGeoBingo.Classes;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Popups;
using CommunityToolkit.Maui.Alerts;
using MauiGeoBingo.Helpers;
using MauiGeoBingo.Models;

namespace MauiGeoBingo.Pagaes;

public partial class ButtonsPage : ContentPage
{

    private ButtonViewModel _buttonViewModel;

    private ServerViewModel? Server { get; set; } = null;

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

        thisPage.Loaded += ContentPageLoaded;

    }

    private async void ContentPageLoaded(object? sender, EventArgs e)
    {
        
        if (Server == null)
        {
            await Navigation.PopAsync();
            return;
        }

        bool playerAdded = await _buttonViewModel.AddPlayerToGame();
        if (!playerAdded)
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

            await _buttonViewModel.StartClient(model.GameId);
        }
        else
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await Toast.Make("Det är här det blir knas!").Show(cts.Token);
            await Navigation.PopAsync();
        }

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
                if (buttonViewModel.Score > 0)
                {
                    buttonViewModel.IsHighest = true;
                }
                // TODO: Bara för testning
                /*if (buttonViewModel.Score > 6)
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
                }*/
            }


            _buttonViewModel.SetButtonColor(buttonViewModel);
            _buttonViewModel.SetNewQiestion(buttonViewModel);

            bool didIWin = isBingo();
            if (Server != null)
            {
                int winner = await _buttonViewModel.SendStatusUpdateAsync(buttonViewModel.Row, buttonViewModel.Col, buttonViewModel.Score, didIWin);
                if (winner > 0) // Någon annan än denna spelare vann
                {
                    var winnerResult = await this.ShowPopupAsync(new WinningPopup(await Helper.GetNameAsync(winner)), CancellationToken.None);
                    if (winnerResult is bool)
                    {
                        await _buttonViewModel.Unsubscribe();
                        await Navigation.PopAsync();
                    }
                }
            }

            if (didIWin) // jag vann
            {
                var winnerResult = await this.ShowPopupAsync(new WinningPopup(), CancellationToken.None);
                if (winnerResult is bool)
                {
                    if (Server != null) await _buttonViewModel.Unsubscribe();
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
                button1.BackgroundColor == ButtonViewModel.ColorWin && button2.BackgroundColor == ButtonViewModel.ColorWin &&
                button3.BackgroundColor == ButtonViewModel.ColorWin && button4.BackgroundColor == ButtonViewModel.ColorWin
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
                button1.BackgroundColor == ButtonViewModel.ColorWin && button2.BackgroundColor == ButtonViewModel.ColorWin &&
                button3.BackgroundColor == ButtonViewModel.ColorWin && button4.BackgroundColor == ButtonViewModel.ColorWin
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
            button1.BackgroundColor == ButtonViewModel.ColorWin && button2.BackgroundColor == ButtonViewModel.ColorWin &&
            button3.BackgroundColor == ButtonViewModel.ColorWin && button4.BackgroundColor == ButtonViewModel.ColorWin
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
            button1.BackgroundColor == ButtonViewModel.ColorWin && button2.BackgroundColor == ButtonViewModel.ColorWin &&
            button3.BackgroundColor == ButtonViewModel.ColorWin && button4.BackgroundColor == ButtonViewModel.ColorWin
            )
        {
            return true;
        }

        return false;
    }

    
}


/*public class StringToColourConverter : IValueConverter, IMarkupExtension
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
}*/

