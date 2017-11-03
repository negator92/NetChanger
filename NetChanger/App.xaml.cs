using System;
using System.Windows;

namespace NetChanger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ApplicationViewModel ApplicationViewModel { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            ApplicationViewModel = new ApplicationViewModel();
        }
    }
}