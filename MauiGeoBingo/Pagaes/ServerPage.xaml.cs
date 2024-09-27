using System.Diagnostics;

namespace MauiGeoBingo.Pagaes;

public partial class ServerPage : ContentPage
{
	public ServerPage()
	{
		InitializeComponent();

        Server b = new();
		b.Servers = [new Server {
			Name = "Game 1",
			Description = "8 spelare",
            IsMyServer = true,
        },
        new Server {
            Name = "Game 2",
            Description = "3 spelare",
            IsMyServer = false,
        },
        new Server {
            Name = "Game 3",
            Description = "11 spelare",
            IsMyServer = true,
        },
        new Server {
            Name = "Det l�ngsta namnet i v�rlden som �r s� h�r l�ngt borde inte f� vara i denna server plats",
            Description = "11 spelare som har f�r l�ng plats h�r",
            IsMyServer = true,
        }

        ];

		BindingContext = b;

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(2000);
        timer.Tick += (s, e) =>
        {

            b = new();
            b.Servers = [new Server {
                Name = "Game 1",
                Description = $"{Random.Shared.Next(10)} spelare",
                IsMyServer = true,
            },
            new Server {
                Name = "Test 2",
                Description = $"{Random.Shared.Next(30)} spelare",
                IsMyServer = false,
            },
            new Server {
                Name = "Game 3",
                Description = $"{Random.Shared.Next(5)} spelare",
                IsMyServer = true,
            },
            new Server {
                Name = "EtttJ�tteL�ngtNamnUtanMellansSlagSomKommerTaS�nderAlltTrorJagP�",
                Description = $"{Random.Shared.Next(20)} spelare som har f�r l�ng plats h�r",
                IsMyServer = false,
            }
        ];

            BindingContext = b;
            OnPropertyChanged(nameof(b));
            Debug.WriteLine("Nu ska den vara uppdaterad");
        };
        timer.Start();
    }
}


public class Server
{
    public string Name { get; set; }
	public string Description { get; set; }
    public bool IsMyServer { get; set; }

    public List<Server> Servers { get; set; }
}