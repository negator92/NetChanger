using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace NetChanger
{
    /// <summary>
    /// Description of ApplicationViewModel.
    /// </summary>
    public class ApplicationViewModel : PropertyChangedClass, INotifyPropertyChanged
    {
        internal static string AssemblyLocation { get; } = Assembly.GetExecutingAssembly().Location;
        internal static string ConfigurationsLocation { get; } = $"{Path.GetDirectoryName(AssemblyLocation)}\\NetworkSettings";
        private HashSet<string> settingFilesHashSet;
        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set
            {
                if (!value.Intersect(Path.GetInvalidFileNameChars()).Any())
                    fileName = value;
                OnPropertyChanged();
            }
        }

        public ICommand DeleteSettingCommand { get; set; }
        public ICommand LoadSettingCommand { get; set; }
        public ICommand SaveSettingCommand { get; set; }
        private Network networkItem;
        public Network NetworkItem
        {
            get
            {
                if (networkItem == null)
                {
                    IP = Mask = Gateway = DNS = FileName = "";
                }
                return networkItem;
            }
            set { networkItem = value; }
        }
        public Network[] NetworkArray { get; set; }
        public string IP { get; set; }
        public string Mask { get; set; }
        public string Gateway { get; set; }
        public string DNS { get; set; }

        public ApplicationViewModel()
        {
            try
            {
                Directory.CreateDirectory(ConfigurationsLocation);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            var watcher = new FileSystemWatcher(ConfigurationsLocation, "*.ini");
            watcher.NotifyFilter = NotifyFilters.Attributes |
                                   NotifyFilters.CreationTime |
                                   NotifyFilters.DirectoryName |
                                   NotifyFilters.FileName |
                                   NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.Security |
                                   NotifyFilters.Size;
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);
            watcher.EnableRaisingEvents = true;
            OnChanged(null, null);

            DeleteSettingCommand = new RelayCommand(DeleteSetting, () => NetworkItem != null);
            LoadSettingCommand = new RelayCommand(EnableSetting, () => NetworkItem != null);
            SaveSettingCommand = new RelayCommand(SaveSetting, () => !string.IsNullOrEmpty(FileName));
        }

        private void EnableSetting(object obj)
        {
            try
            {
                NetworkItem = CreateNetwork();
                Enable();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }

        private Network CreateNetwork()
        {
            var N = new Network();
            N.DNSaddresses = DNS;
            N.Gateway = Gateway;
            N.IPaddress = IP;
            N.Netmask = Mask;
            return N;
        }

        private void Enable()
        {
            MessageBox.Show("Network settings enabled!");
        }

        private void DeleteSetting(object obj)
        {
            try
            {
                File.Delete($"{ConfigurationsLocation}\\{NetworkItem}.ini");
                OnChanged(null, null);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }


        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            OnChanged(sender, null);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            var files = Directory.GetFiles(ConfigurationsLocation, "*.ini");
            NetworkArray = new Network[files.Length];
            foreach (var f in files)
            {
                var al = File.ReadAllLines(f);
                IP = al[0];
                Mask = al[1];
                Gateway = al[2];
                DNS = al[3];
                NetworkItem = CreateNetwork();
                NetworkArray[Array.IndexOf(files, f)] = NetworkItem;
            }
            settingFilesHashSet = new HashSet<string>(NetworkArray.Select(f => f.ToString()));
        }

        public void SaveSetting(object obj)
        {
            NetworkItem = CreateNetwork();
            try
            {
                File.WriteAllText($"{ConfigurationsLocation}\\{FileName}.ini", $"{NetworkItem.IPaddress}\n{NetworkItem.Netmask}\n{NetworkItem.Gateway}\n{NetworkItem.DNSaddresses}");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }
    }
}
