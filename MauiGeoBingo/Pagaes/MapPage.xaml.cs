using MauiGeoBingo.Classes;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;
using Map = Microsoft.Maui.Controls.Maps.Map;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace MauiGeoBingo.Pagaes;

public partial class MapPage : ContentPage
{
    private Map _map;

    private Dictionary<string, Circle> _circles = new();

    private double _statLatidude = AppSettings.StartLatidude;
    private double _staLongitude = AppSettings.StartLongitude;

    private double _latDiff = AppSettings.LatidudeMarkerDiff;
    private double _lngDiff = AppSettings.LongitudeMarkerDiff;

    private bool _loaded = false;

    public MapPage()
    {
        InitializeComponent();

        Location location = new Location(_statLatidude, _staLongitude);
        MapSpan mapSpan = new MapSpan(location, 0.1, 0.1);

        _map = new Map(mapSpan)
        {
            MapType = MapType.Hybrid,
            IsShowingUser = true,
        };
        Content = _map;

        _map.Loaded += Map_Loaded;
        _map.MapClicked += OnMapClicked;
    }

    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        Debug.WriteLine($"MapClick: {e.Location.Latitude}, {e.Location.Longitude}");
    }

    private void Map_Loaded(object? sender, EventArgs e)
    {
        if (!_loaded) LoadMapContent();

        _loaded = true;
    }

    private void LoadMapContent()
    {
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                Circle circle = new Circle
                {
                    Center = new Location(_statLatidude + (row * _latDiff), _staLongitude + (col * _lngDiff)),
                    Radius = new Distance(250),
                    StrokeColor = Color.FromArgb("#88FF0000"),
                    StrokeWidth = 8,
                    FillColor = Color.FromArgb("#88FFC0CB")
                };

                _circles.Add($"{row}-{col}", circle);
                //map.MapElements.Add(circle);

                Pin boardwalkPin = new Pin
                {
                    Location = new Location(_statLatidude + (row * _latDiff), _staLongitude + (col * _lngDiff)),
                    Label = $"En sport fråga!!! {row}-{col}",
                    Address = $"Your score is: {0}",
                    Type = PinType.Place,
                };


                boardwalkPin.InfoWindowClicked += async (s, args) =>
                {
                    args.HideInfoWindow = true;
                    _map.MapElements.Add(circle);

                    _map.Pins.Remove(boardwalkPin);


                    Debug.WriteLine(_map.MapElements.Count);
                    await CheckWinner();

                };

                _map.Pins.Add(boardwalkPin);
            }
        }
    }

    private async Task CheckWinner()
    {
        Circle c1;
        Circle c2;
        Circle c3;
        Circle c4;
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                // kolla hirisontellt
                if (
                _circles.TryGetValue($"{row}-{col}", out c1) && _map.MapElements.Contains(c1) &&
                _circles.TryGetValue($"{row}-{col + 1}", out c2) && _map.MapElements.Contains(c2) &&
                _circles.TryGetValue($"{row}-{col + 2}", out c3) && _map.MapElements.Contains(c3) &&
                _circles.TryGetValue($"{row}-{col + 3}", out c4) && _map.MapElements.Contains(c4)
                )
                {
                    await DisplayAlert("Du är en vinnare", $"Grattis du har vunnit en dammsugare på avbetalning", "Ok");
                }

                // Kolla vertikalt
                if (
                _circles.TryGetValue($"{row}-{col}", out c1) && _map.MapElements.Contains(c1) &&
                _circles.TryGetValue($"{row + 1}-{col}", out c2) && _map.MapElements.Contains(c2) &&
                _circles.TryGetValue($"{row + 2}-{col}", out c3) && _map.MapElements.Contains(c3) &&
                _circles.TryGetValue($"{row + 3}-{col}", out c4) && _map.MapElements.Contains(c4)
                )
                {
                    await DisplayAlert("Du är en vinnare", $"Grattis du har vunnit en dammsugare på avbetalning", "Ok");
                }
            }
        }
    }
}



