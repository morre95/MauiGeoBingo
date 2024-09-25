
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui.Views;
using MauiGeoBingo.Pagaes;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MauiGeoBingo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Map_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MapPage());
        }

        private async void Buttons_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ButtonsPage());
        }
    }

}
