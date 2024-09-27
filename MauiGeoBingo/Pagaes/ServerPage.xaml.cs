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
            Name = "Det längsta namnet i världen som är så här långt borde inte få vara i denna server plats",
            Description = "11 spelare som har fär lång plats här",
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
                Name = "EtttJätteLångtNamnUtanMellansSlagSomKommerTaSönderAlltTrorJagPå",
                Description = $"{Random.Shared.Next(20)} spelare som har fär lång plats här",
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