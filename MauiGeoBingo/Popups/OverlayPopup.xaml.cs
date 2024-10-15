using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using System.Text.Json;
using Websocket.Client;
using MauiGeoBingo.Pagaes;
using System.Diagnostics;

namespace MauiGeoBingo.Popups;

public partial class OverlayPopup : Popup
{
    private ServerViewModel? Server { get; set; } = null;

    private WebsocketClient _client;

    public OverlayPopup(ServerViewModel server)
	{
		InitializeComponent();

        Server = server;

        var url = new Uri(AppSettings.LocalWSBaseEndpoint);
        _client = new WebsocketClient(url);
    }

    private async void PopupLoaded(object sender, EventArgs e)
    {
        _client.MessageReceived.Subscribe(HandleSubscriptionToServers);

        _client.ReconnectionHappened.Subscribe(async info =>
        {
            Debug.WriteLine($"Reconnection to new server scription happened, type: {info.Type}");
            await SubscribeToServers();
        });

        await _client.Start();
        await SubscribeToServers();
    }

    async void OnOKButtonClicked(object? sender, EventArgs e)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await CloseAsync(true, cts.Token);
    }


    private async Task SubscribeToServers()
    {
        await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
        {
            action = "subscribe",
            topic = "waiting_for_server",
        })));
    }

    private async void HandleSubscriptionToServers(ResponseMessage message)
    {
        var recived = JsonSerializer.Deserialize<GameStatusRootobject>(message.ToString());

        if (recived != null)
        {
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
                int numberOfPlayers = recived.PlayerCount;

                //List<Button> buttons = [player2Button, player3Button, player4Button];

                List<int> playerIds = recived.PlayerIds.Take(4).ToList();

                Debug.WriteLine($"player_ids: {string.Join(", ", playerIds)}");

                playerIds.Remove(AppSettings.PlayerId);
                //player1Button.Text = AppSettings.PlayerName;
            }
        }
    }

    
}