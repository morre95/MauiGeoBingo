using MauiGeoBingo.Classes;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;
using Websocket.Client;

namespace MauiGeoBingo.Pagaes;

public partial class ServerPage : ContentPage
{
    private ServerViewModel _serverViewModel;

    public ServerPage()
    {
        InitializeComponent();

        _serverViewModel = new ServerViewModel();
        _serverViewModel.UpdateData();
        BindingContext = _serverViewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        _serverViewModel.Dispose();
        base.OnBackButtonPressed();
        return false;
    }

    private async void DeleteServerClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ServerViewModel server)
        {
            string endpoint = AppSettings.LocalBaseEndpoint;
            HttpRequest rec = new($"{endpoint}/delete/servers/{server.GameId}");

            Server? response = await rec.DeleteAsync<Server>();

            if (response != null && response.Success)
            {
                btn.IsEnabled = false;
                btn.Text = "Deleting...";

                server.Dispose();

                await Task.Delay(500);

                var page = Navigation.NavigationStack.LastOrDefault();
                await Navigation.PushAsync(new ServerPage());
                Navigation.RemovePage(page);
            }
        }
    }

    private async void GoToServerClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ServerViewModel server)
        {
            Debug.WriteLine($"Go to server: {server.GameName} with id: {server.GameId}");

            var page = Navigation.NavigationStack.LastOrDefault();
            await Navigation.PushAsync(new ButtonsPage(server));
            Navigation.RemovePage(page);
            _serverViewModel.Dispose();
        }
    }

    private async void CreateServerClicked(object sender, EventArgs e)
    {
        if (sender is Button)
        {
            await Navigation.PushAsync(new CreateServerPage());
        }
    }

    private void ServerPageUnloaded(object sender, EventArgs e)
    {
        _serverViewModel.Dispose();
    }
}


public class ServerViewModel : INotifyPropertyChanged, IDisposable, IEquatable<Server>
{
    private string? _gameName;
    private int? _gameId;
    private int? _numberOfPlayers;
    private bool _isMyServer;
    private DateTime? _created;
    private bool _isMeAllowedToPlay;

    public string? GameName
    {
        get => _gameName;
        set
        {
            if (_gameName != value)
            {
                _gameName = value;
                OnPropertyChanged();
            }
        }
    }

    public int? GameId
    {
        get => _gameId;
        set
        {
            if (_gameId != value)
            {
                _gameId = value;
                OnPropertyChanged();
            }
        }
    }

    public int? NumberOfPlayers
    {
        get => _numberOfPlayers;
        set
        {
            if (_numberOfPlayers != value)
            {
                _numberOfPlayers = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsMyServer
    {
        get => _isMyServer;
        set
        {
            if (_isMyServer != value)
            {
                _isMyServer = value;
                OnPropertyChanged();
            }
        }
    }

    private int? _isMap { get; set; } = null;

    public string MapOrButton 
    { 
        get => _isMap == 1 ? "Map" : "Button"; 
        set 
        { 
            _isMap = value == "Map" ? 1 : 0; 
            OnPropertyChanged(); 
        } 
    }

    public DateTime? Created
    {
        get => _created;
        set
        {
            if (_created != value)
            {
                _created = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsMeAllowedToPlay
    { 
        get => _isMeAllowedToPlay;
        set
        {
            if (_isMeAllowedToPlay != value)
            {
                _isMeAllowedToPlay = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<ServerViewModel> Servers { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private WebsocketClient _client;

    public ServerViewModel()
    {
        Servers = [];

        string endpoint = AppSettings.LocalWSBaseEndpoint;
        var url = new Uri(endpoint);
        _client = new WebsocketClient(url);

        _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
        _client.ReconnectionHappened.Subscribe(info => Debug.WriteLine($"Reconnection happened, type: {info.Type}"));

        _client.MessageReceived.Subscribe(HandleSubscription);
    }

    public async void UpdateData()
    {
        await _client.Start();

        await Subscribe();

        OnPropertyChanged(nameof(Servers));
    }

    private async Task Subscribe()
    {
        await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
        {
            action = "subscribe",
            topic = "new_servers",
        })));
    }

    int _msgCount = 0;

    private async void HandleSubscription(ResponseMessage message)
    {
        //Debug.WriteLine($"Message received: {message}");
        var recived = JsonSerializer.Deserialize<Server>(message.ToString());

        if (recived != null)
        {
            if (recived.Type == "sub_auth")
            {
                Debug.WriteLine($"sub_auth: {message}");
                await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
                {
                    action = "publish",
                    topic = "new_servers",
                    message = "Latest server update",
                    security_key = recived.SecurityKey,
                })));
            }
            else if (recived.Type == "message")
            {
                Debug.WriteLine($"Websocket message({++_msgCount}): {message},\nServer count: {recived.Servers.Count}");
                foreach (var server in recived.Servers)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Servers.Add(new()
                        {
                            GameName = server.GameName,
                            Created = server.Created,
                            NumberOfPlayers = server.NumberOfPlayers,
                            GameId = server.GameId,
                            IsMyServer = server.IsMyServer,
                            _isMap = server.IsMap, 
                        });
                        //if (server.PlayerIds != null)
                        //Debug.WriteLine($"Number of players PlayerIds.Length: {server.PlayerIds.Length}, server.NumberOfPlayers: {server.NumberOfPlayers}");
                    });
                }

                /*MainThread.BeginInvokeOnMainThread(() =>
                {
                    Servers.Add(new ServerViewModel
                    {
                        GameName = "Test spelet",
                        Created = DateTime.Now,
                        NumberOfPlayers = 1,
                        GameId = 55,
                        IsMyServer = false,
                        _isMap = 0
                    });
                });*/

                OnPropertyChanged(nameof(Servers));
            }
        }
    
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void Unsubscribe()
    {
        await Task.Run(() => _client.Send(JsonSerializer.Serialize(new
        {
            action = "unsubscribe",
            topic = "new_servers",
        })));
    }

    public async void Dispose()
    {
        Unsubscribe();

        // FIXME: Båda dessa rader under här skapar Debuggern att kracha. Den funkar utan debuggern dock
        //MainThread.BeginInvokeOnMainThread(() => _client?.Dispose());
        if (_client.IsRunning) 
        {
            await Task.Delay(50);
            await _client.Stop(WebSocketCloseStatus.NormalClosure, $"Closed in server by the {this.GetType().Name} client");
            _client.Dispose();
        }
    }

    public bool Equals(Server? other)
    {
        if (other == null)
            return false;

        return GameName == other.GameName &&
               Created.Equals(other.Created) &&
               NumberOfPlayers.Equals(other.NumberOfPlayers) &&
               GameId.Equals(other.GameId) &&
               IsMyServer.Equals(other.IsMyServer) &&
               _isMap.Equals(other.IsMap);
    }
}


public class Server
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    public string? created { get; set; }
    public DateTime? Created => StringToDate(created);


    [JsonPropertyName("game_id")]
    public int? GameId { get; set; }

    [JsonPropertyName("game_name")]
    public string? GameName { get; set; }


    public int? game_owner { get; set; }
    [JsonIgnore]
    public bool IsMyServer => game_owner == AppSettings.PlayerId;

    [JsonPropertyName("number_of_players")]
    public int? NumberOfPlayers { get; set; }
    
    [JsonPropertyName("player_ids")]
    public int[]? PlayerIds { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("topic")]
    public string? Topic { get; set; }
    [JsonPropertyName("security_key")]
    public string? SecurityKey { get; set; }

    [JsonPropertyName("is_map")]
    public int IsMap { get; set; }

    public string? last_modified { get; set; }
    public DateTime? LastModified => StringToDate(last_modified);


    public int is_active { get; set; }
    
    public double latitude { get; set; }
    public double longitude { get; set; }

    [JsonPropertyName("servers")]
    public List<Server> Servers { get; set; } = new();

    private static DateTime? StringToDate(string? stringToFormat)
    {
        if (DateTime.TryParse(stringToFormat, out DateTime result))
        {
            return result;
        }

        return null;
    }
}
