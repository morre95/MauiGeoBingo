using MauiGeoBingo.Classes;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Websocket.Client;

namespace MauiGeoBingo.Pagaes;

public class ServerViewModel : INotifyPropertyChanged, IDisposable, IEquatable<Server>
{
    private string? _gameName;
    private int? _gameId;
    private int? _numberOfPlayers;
    private bool _isMyServer;
    private DateTime? _created;
    private bool _isMeAllowedToPlay;
    private List<int>? _playerIds;

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

    

    public List<int>? PlayerIds
    {
        get => _playerIds;
        set
        {
            if (_playerIds != value)
            {
                _playerIds = value;
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
        _client.ReconnectionHappened.Subscribe(async info =>
        {
            Debug.WriteLine($"Reconnection happened, type: {info.Type}");
            await Subscribe();
        });

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

        if (recived == null) return;

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
            //Debug.WriteLine($"Websocket message({++_msgCount}): {message},\nServer count: {recived.Servers.Count}");
            if (++_msgCount == 1)
            {
                foreach (var server in recived.Servers)
                {
                    AddNewServer(server);
                }
            }
            else if (recived.Servers.Count == Servers.Count)
            {
                foreach (var server in recived.Servers)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Dictionary<int, ServerViewModel> serverToAddDict = new();
                        foreach (var viewServer in Servers)
                        {
                            if (viewServer.GameId.Equals(server.GameId) && !viewServer.Equals(server))
                            {
                                // TBD: Det här fungerar. Men kanske onödigt i det här skedet att readera hela raden och lägga till den igen när det här borde fungera
                                /*GameName = server.GameName;
                                NumberOfPlayers = server.NumberOfPlayers;*/
                                ServerViewModel serverToInsert = new()
                                {
                                    GameName = server.GameName,
                                    Created = server.Created,
                                    NumberOfPlayers = server.NumberOfPlayers,
                                    GameId = server.GameId,
                                    IsMyServer = server.IsMyServer,
                                    _isMap = server.IsMap,
                                    PlayerIds = server.PlayerIds,
                                };
                                int index = Servers.IndexOf(viewServer);
                                serverToAddDict.Add(index, serverToInsert);


                            }
                        }
                        foreach (var kvp in serverToAddDict)
                        {
                            Servers.RemoveAt(kvp.Key);
                            Servers.Insert(kvp.Key, kvp.Value);
                        }
                    });

                }
            }
            else if (recived.Servers.Count > Servers.Count)
            {
                Debug.WriteLine("Japp det ska läggas till här");
                var result = recived.Servers.Where(s => Servers.All(s2 => s2.GameId != s.GameId));
                foreach (var server in result)
                {
                    AddNewServer(server);
                }
            } 
            else if (recived.Servers.Count < Servers.Count)
            {
                var delResult = Servers.Where(s => recived.Servers.All(s2 => s2.GameId != s.GameId)).ToList();
                Debug.WriteLine($"Japp det ska raderas här. Det är {delResult.Count}st som ska bort");
                foreach (var server in delResult)
                {
                    if (server != null)
                    {
                        Debug.WriteLine($"Här ska {server.GameName} raderas");
                        Servers.Remove(server);
                    }
                }


                /*HashSet<ServerViewModel> hashSet = new();
                if (Servers.Any(r => !hashSet.Add(r)))
                {
                    Debug.WriteLine("Japp det finns dubbletter"); 
                }*/



                /*// Kolla om det finns dubbletter
                var query = Servers.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
                Debug.WriteLine($"Det finns {query.Count}st dubletter");
                foreach (var server in query)
                {
                    if (server != null)
                    {
                        Debug.WriteLine($"Här ska {server.GameName} raderas");
                        Servers.Remove(server);
                    }
                }

                var distinct = Servers.Distinct().ToList();
                Debug.WriteLine($"Det finns {distinct.Count}st unika och {Servers.Count}st totalt och {recived.Servers.Count}st motagna");*/

            }
            OnPropertyChanged(nameof(Servers));
        }

    }

    private void AddNewServer(Server server)
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
                PlayerIds = server.PlayerIds,
            });
        });
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void Unsubscribe()
    {
        await Task.Run(async () => 
            _client.Send(JsonSerializer.Serialize(new
            {
                action = "unsubscribe",
                topic = "new_servers",
            }))
        );
    }

    public async void Dispose()
    {
        Unsubscribe();

        if (_client.IsRunning) 
        {
            await Task.Delay(150);
            await _client.Stop(WebSocketCloseStatus.NormalClosure, $"Closed in server by the {this.GetType().Name} client");
            _client.Dispose();
        }
    }

    public bool Equals(Server? other)
    {
        if (other == null)
            return false;

        return GameName.Equals(other.GameName) &&
               Created.Equals(other.Created) &&
               NumberOfPlayers.Equals(other.NumberOfPlayers) &&
               GameId.Equals(other.GameId) &&
               IsMyServer.Equals(other.IsMyServer) &&
               _isMap.Equals(other.IsMap) /*&&
               PlayerIds.Equals(other.PlayerIds)*/;
    }

    internal void Delete(ServerViewModel server)
    {
        Servers.Remove(server);
    }

    /*public bool Equals(ServerViewModel? other)
    {
        if (other == null)
            return false;

        return GameId.Equals(other.GameId);
    }


    public static bool operator ==(ServerViewModel lhs, ServerViewModel rhs) => lhs.Equals(rhs);

    public static bool operator !=(ServerViewModel lhs, ServerViewModel rhs) => !(lhs == rhs);*/
}
