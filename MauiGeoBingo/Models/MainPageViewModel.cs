using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiGeoBingo
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isEnabled = false;
        public bool IsEnabled 
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _warningVisible = false;

        public bool WarningVisible
        {
            get => _warningVisible;
            set
            {
                if (_warningVisible != value)
                {
                    _warningVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainPageViewModel() { }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
