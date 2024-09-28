using MauiGeoBingo.Classes;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Text.Json.Serialization;

namespace MauiGeoBingo.Pagaes;

public partial class ServerPage : ContentPage
{
    private IDispatcherTimer _timer;

    public ServerPage()
    {
        InitializeComponent();

        ServerViewModel svm = new ServerViewModel();
        svm.UpdateData();
        BindingContext = svm;

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(5);
        _timer.Tick += (s, e) =>
        {
            svm.UpdateData();
        };
        _timer.Start();
    }

    protected override bool OnBackButtonPressed()
    {
        _timer.Stop();
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
        }
    }
}


public class ServerViewModel : INotifyPropertyChanged
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
                OnPropertyChanged(nameof(GameName));
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
                OnPropertyChanged(nameof(GameId));
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
                OnPropertyChanged(nameof(NumberOfPlayers));
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
                OnPropertyChanged(nameof(IsMyServer));
            }
        }
    }

    public int? is_map { get; set; } = null;

    public string MapOrButton 
    { 
        get => is_map == 1 ? "Map" : "Button"; 
        set 
        { 
            is_map = value == "Map" ? 1 : 0; 
            OnPropertyChanged(nameof(MapOrButton)); 
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
                OnPropertyChanged(nameof(_created));
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
                OnPropertyChanged(nameof(_isMeAllowedToPlay));
            }
        }
    }

    public ObservableCollection<ServerViewModel> Servers { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ServerViewModel()
    {
        Servers = [];
    }

    public async void UpdateData()
    {
        Servers.Clear();

        string endpoint = AppSettings.LocalBaseEndpoint;
        HttpRequest rec = new($"{endpoint}/get/active/servers?player_id={AppSettings.PlayerId}");

        Server? servers = await rec.GetAsync<Server>();

        if (servers != null)
        {
            foreach (var server in servers.Servers)
            {
                if (server.PlayerIds == null) break;
                Servers.Add(new()
                {
                    GameName = server.GameName,
                    Created = server.Created,
                    NumberOfPlayers = server.NumberOfPlayers,
                    GameId = server.GameId,
                    IsMyServer = server.IsMyServer,
                    IsMeAllowedToPlay = server.PlayerIds.Contains(AppSettings.PlayerId) || server.IsMyServer,
                });
            }

        }

        OnPropertyChanged(nameof(Servers));
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    [JsonPropertyName("is_my_server")]
    public bool IsMyServer { get; set; }

    [JsonPropertyName("number_of_players")]
    public int? NumberOfPlayers { get; set; }
    
    [JsonPropertyName("player_ids")]
    public int[]? PlayerIds { get; set; }


    public int is_active { get; set; }
    public int is_map { get; set; }
    public double latitude { get; set; }
    public double longitude { get; set; }

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
