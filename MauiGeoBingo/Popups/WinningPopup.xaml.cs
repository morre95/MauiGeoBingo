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

        winningText.Text = $"{winner} vann före dig. Men nästa gång är det din tur att vinna.";
    }

    private async void OnOKButtonClicked(object? sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(true, cts.Token);
    }
}