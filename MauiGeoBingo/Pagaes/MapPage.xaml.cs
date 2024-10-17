using MauiGeoBingo.Classes;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;
using Map = Microsoft.Maui.Controls.Maps.Map;
using System.Diagnostics;
using System.Text.Json.Serialization;
using MauiGeoBingo.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Devices.Sensors;
using System.Reflection.Metadata;
using Microsoft.Maui.ApplicationModel;

namespace MauiGeoBingo.Pagaes;

public partial class MapPage : ContentPage
{
    private MapViewModel _viewModel;

    public MapPage()
    {
        InitializeComponent();

        Location location = new Location(AppSettings.StartLatitude, AppSettings.StartLongitude);
        MapSpan mapSpan = new MapSpan(location, 0.1, 0.1);
        map.MoveToRegion(mapSpan);

        _viewModel = new MapViewModel();

        _ = _viewModel.CreatePins();

        BindingContext = _viewModel;
    }

    private async void MarkerClicked(object? sender, PinClickedEventArgs e)
    {
        if (sender is Pin pin && pin.BindingContext is MapViewModel model)
        {
            Location? myLocation = await GetCurrentLocation();

            if (myLocation != null)
            {
                Location pinLocation = pin.Location;

                Distance distance = Distance.BetweenPositions(pinLocation, myLocation);

                Debug.WriteLine($"Avståndet är {distance.Meters} meter");

                // FIXME: Det går inte att sätta address så här
                pin.Address = $"Du är {distance.Meters} meter ifrån";

                model.ToolTip = $"Du är {distance.Meters} meter ifrån";
                model.Score = 4;

                // Det funkar att radera en model dock
                //_viewModel.Pins.Remove(model);
            }
            else
            {
                pin.Address = "Kan inte hitta någon GPS koordinat";
            }
            
        }
    }



    private CancellationTokenSource _cancelTokenSource;
    private bool _isCheckingLocation = false;

    public async Task<Location?> GetCurrentLocation()
    {
        Location? location = default;
        try
        {
            _isCheckingLocation = true;

            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));

            _cancelTokenSource = new CancellationTokenSource();

            location = await Geolocation.Default.GetLocationAsync(request, _cancelTokenSource.Token);

            if (location != null)
            {
                Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
            }
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            Debug.WriteLine("Handle not supported on device exception");
        }
        catch (FeatureNotEnabledException fneEx)
        {
            Debug.WriteLine("Handle not enabled on device exception");
        }
        catch (PermissionException pEx)
        {
            Debug.WriteLine("Handle permission exception");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Unable to get location");
        }
        finally
        {
            _isCheckingLocation = false;
        }

        return location;
    }

    public void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
            _cancelTokenSource.Cancel();
    }
}

public class MapViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<MapViewModel> Pins { get; set; }

    private int _score;
    public int Score
    {
        get => _score;
        set
        {
            if (_score != value)
            {
                _score = value;
                OnPropertyChanged();
            }
        }
    }

    private double _latitude;
    public double Latitude
    {
        get => _latitude;
        set
        {
            if (_latitude != value)
            {
                _latitude = value;
                OnPropertyChanged();
            }
        }
    }

    private double _longitude;
    public double Longitude
    {
        get => _longitude;
        set
        {
            if (_longitude != value)
            {
                _longitude = value;
                OnPropertyChanged();
            }
        }
    }

    public Location Location => new Location(Latitude, Longitude);

    private bool _isHighest = false;
    public bool IsHighest
    {
        get => _isHighest;
        set
        {
            if (_isHighest != value)
            {
                _isHighest = value;
                OnPropertyChanged();
            }
        }
    }

    private int _row;
    public int Row
    {
        get => _row;
        set
        {
            if (_row != value)
            {
                _row = value;
                OnPropertyChanged();
            }
        }
    }

    private int _col;
    public int Col
    {
        get => _col;
        set
        {
            if (_col != value)
            {
                _col = value;
                OnPropertyChanged();
            }
        }
    }

    private Result _questionAndAnswer = new();
    public Result QuestionAndAnswer
    {
        get => _questionAndAnswer;
        set
        {
            if (_questionAndAnswer != value)
            {
                _questionAndAnswer = value;
                OnPropertyChanged();
            }
        }
    }

    private string _toolTip = string.Empty;
    public string ToolTip
    {
        get => _toolTip;
        set
        {
            if (_toolTip != value)
            {
                _toolTip = value;
                OnPropertyChanged();
            }
        }
    }

    private string _text = string.Empty;
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                OnPropertyChanged();
            }
        }
    }

    private List<Result> _questions;

    public MapViewModel()
    {
        Pins = [];
        _questions = [];
    }


    public async Task CreatePins()
    {
        double lat = AppSettings.StartLatitude;
        double lon = AppSettings.StartLongitude;

        double latDiff = AppSettings.LatitudeMarkerDiff;
        double lonDiff = AppSettings.LongitudeMarkerDiff;

        Quiz? quiz = null;
        string fileName = AppSettings.QuizJsonFileName;
        if (await FileSystem.Current.AppPackageFileExistsAsync(fileName))
        {
            quiz = await Helper.ReadJsonFile<Quiz>(fileName);
        }

        if (quiz != null && quiz.Results != null)
        {
            string selectedCat = AppSettings.QuizCategorie;
            if (selectedCat == "All")
            {
                _questions = quiz.Results;
            }
            else
            {
                _questions = quiz.Results.Where(r => r.Category.StartsWith(AppSettings.QuizCategorie)).ToList();
            }

            

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    Result result = _questions[Random.Shared.Next(_questions.Count)];
                    _questions.Remove(result);

                    MapViewModel model = new();
                    model.Score = 0;
                    model.Row = row;
                    model.Col = col;
                    model.Latitude = lat + (row * latDiff);
                    model.Longitude = lon + (col * lonDiff);
                    model.QuestionAndAnswer = result;
                    model.ToolTip = $"{result.Category} ({result.Difficulty})";

                    model.Text = $"Du har {model.Score} poäng och är okänt antal meter bort";
                    Pins.Add(model);
                }
            }
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}



