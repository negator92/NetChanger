using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace NetChanger
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Deactivated(object sender, EventArgs e)
        {
            MessageBox.Show("Закрыть забыл");
        }
    }
}
