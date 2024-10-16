using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Classes;
using MauiGeoBingo.Popups;
using System.Diagnostics;

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

                _serverViewModel.Delete(server);
                await Task.Delay(500);

                btn.IsEnabled = true;
                btn.Text = "Delete";

                // TBD: Behövs detta, med att ladda om sidan???
                /*server.Dispose();

                await Task.Delay(500);

                var page = Navigation.NavigationStack.LastOrDefault();
                await Navigation.PushAsync(new ServerPage());
                Navigation.RemovePage(page);*/
            }
        }
    }

    private void GoToServerClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ServerViewModel server)
        {
            Debug.WriteLine($"Gå till server: {server.GameName} med id: {server.GameId}");

            

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                _serverViewModel.Dispose();

                // Ge servern tid att avregistrera
                await Task.Delay(500);

                var page = Navigation.NavigationStack.LastOrDefault();
                //await Navigation.PushAsync(new ButtonsPage(server));
                await Navigation.PushAsync(new ButtonsPage2(server));
                Navigation.RemovePage(page);
            });
        }
    }

    private async void CreateServerClicked(object sender, EventArgs e)
    {
        if (sender is Button)
        {
            //await Navigation.PushAsync(new CreateServerPage());
            CreatingServerPopup popup = new();

            var result = await this.ShowPopupAsync(popup, CancellationToken.None);
            if (result is bool boolResult)
            {
                if (boolResult)
                {
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await Toast.Make("Japp du lyckades precis skapa en ny server").Show(cts.Token);
                }
            }
        }
    }

    private async void ServerPageLoaded(object sender, EventArgs e)
    {
        /*Task task = Task.Run(() =>
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                activityIndicator.IsVisible = true;
                await Task.Delay(2500);
                activityIndicator.IsVisible = false;
            });
        });
        await task;*/

        /*activityIndicator.IsVisible = true;
        await Task.Delay(2500);
        activityIndicator.IsVisible = false;*/
    }

    private void ServerPageUnloaded(object sender, EventArgs e)
    {
        _serverViewModel.Dispose();
    }

    private async void EditServerClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ServerViewModel server)
        {
            EditServerPopup popup = new(server);

            var result = await this.ShowPopupAsync(popup, CancellationToken.None);

            if (result is ServerViewModel resultServer)
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await Toast.Make($"Servern {resultServer.GameName} är sparad").Show(cts.Token);
                string endpoint = AppSettings.LocalBaseEndpoint;
                HttpRequest rec = new($"{endpoint}/edit/game/name");

                Server? response = await rec.PostAsync<Server>(new
                {
                    game_id = resultServer.GameId,
                    game_name = resultServer.GameName,
                });

            }
            else
            {
                //await Toast.Make("Du tryckte på cancel").Show();
            }
        }
    }

    
}
