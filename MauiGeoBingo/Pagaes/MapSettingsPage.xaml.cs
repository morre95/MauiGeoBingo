using MauiGeoBingo.Classes;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Map = Microsoft.Maui.Controls.Maps.Map;
using System.Diagnostics;

namespace MauiGeoBingo.Pagaes;

public partial class MapSettingsPage : ContentPage
{
	public MapSettingsPage()
	{
		InitializeComponent();
	}

    private void MapGrid_Loaded(object sender, EventArgs e)
    {
        Location location = new Location(AppSettings.StartLatidude, AppSettings.StartLongitude);
        MapSpan mapSpan = new MapSpan(location, 0.1, 0.1);
        Map map = new Map(mapSpan)
        {
            MapType = MapType.Hybrid,
            IsShowingUser = true,
        };

        map.MapClicked += OnMapClicked;

        MapGrid.Add(map);
    }

    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        Debug.WriteLine($"MapClick: lat:{e.Location.Latitude}, lng:{e.Location.Longitude}");
        if (sender is Map map) 
        {
            AppSettings.StartLatidude = e.Location.Latitude;
            AppSettings.StartLongitude = e.Location.Longitude;

            double lat = AppSettings.StartLatidude;
            double lon = AppSettings.StartLongitude;

            double latDiff = AppSettings.LatidudeMarkerDiff;
            double lonDiff = AppSettings.LongitudeMarkerDiff;

            map.Pins.Clear();

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Pin boardwalkPin = new Pin
                    {
                        Location = new Location(lat + (row * latDiff), lon + (col * lonDiff)),
                        Label = "Here is your questions",
                        Address = "Where you placed it",
                        Type = PinType.Generic,
                    };

                    map.Pins.Add(boardwalkPin);
                }
            }
        }
    }
}