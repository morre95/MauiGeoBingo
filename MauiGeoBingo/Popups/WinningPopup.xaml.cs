using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;

namespace MauiGeoBingo.Popups;

public partial class WinningPopup : Popup
{
	public WinningPopup()
	{
		InitializeComponent();
	}

    public WinningPopup(string winner)
    {
        InitializeComponent();

        winningText.Text = $"{winner} vann f�re dig. Men n�sta g�ng �r det din tur att vinna.";
    }

    private void OnOKButtonClicked(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await CloseAsync(true, cts.Token);
        });
    }
}