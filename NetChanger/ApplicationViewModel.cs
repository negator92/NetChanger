
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace NetChanger
{
    /// <summary>
    /// Description of ApplicationViewModel.
    /// </summary>
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        public ICommand SaveSettingsCommand { get; set; }

        public ApplicationViewModel()
        {
            SaveSettingsCommand = new RelayCommand(SaveSettings, () => true);
        }

        public void SaveSettings()
        {
            return;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
