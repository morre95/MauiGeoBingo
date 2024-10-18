using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Pagaes;

namespace MauiGeoBingo.Popups;

public partial class EditServerPopup : Popup
{
    private ServerViewModel Server {  get; set; }

    public EditServerPopup(ServerViewModel server)
	{
		InitializeComponent();

        serverName.Text = server.GameName;

        Server = server;
	}

    private void SaveButtonClicked(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            Server.GameName = serverName.Text;
            await CloseAsync(Server, cts.Token);
        });
    }
    
    private void CancelButtonClicked(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            await CloseAsync(null, cts.Token);
        });
    }
}