using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using System.Text.Json;
using Websocket.Client;
using MauiGeoBingo.Pagaes;
using System.Diagnostics;
using System.Net.WebSockets;

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

        if (server.IsMyServer)
        {
            okButton.IsVisible = true;
        }
    }

    private async void PopupLoaded(object sender, EventArgs e)
    {
        _client.MessageReceived.Subscribe(HandleSubscription);

        _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
        _client.ReconnectionHappened.Subscribe(async info =>
        {
            Debug.WriteLine($"Reconnection to new server scription happened, type: {info.Type}");
            await Subscribe();
        });

        await _client.Start();
        await Subscribe();
    }

    private async void OnOKButtonClicked(object? sender, EventArgs e)
    {
        if (GameStatus != null)
        {
            await Unsubscribe();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await CloseAsync(GameStatus, cts.Token);
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

    private async Task Unsubscribe()
    {
        await Task.Run(async () => {
            _client.Send(JsonSerializer.Serialize(new
            {
                action = "unsubscribe",
                topic = "waiting_for_server",
            }));
            await _client.Stop(WebSocketCloseStatus.NormalClosure, $"Closed in server by the {this.GetType().Name} client");
            //_client.Dispose();
        });
    }

    private GameStatusRootobject? GameStatus { get; set; }

    private async void HandleSubscription(ResponseMessage message)
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
                GameStatus = recived;
                if (recived.IsRunning)
                {
                    await Unsubscribe();
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await CloseAsync(GameStatus, cts.Token);
                    return;
                }

                int numberOfPlayers = recived.PlayerCount;

                List<int> playerIds = recived.PlayerIds.Take(4).ToList();

                Debug.WriteLine($"player_ids: {string.Join(", ", playerIds)} är med här");

                //playerIds.Remove(AppSettings.PlayerId);

                MainThread.BeginInvokeOnMainThread(() => {
                    waitingText.Text = $"Waiting for players to join.\n{numberOfPlayers} joined so far";
                });
            }
        }
    }

    
}