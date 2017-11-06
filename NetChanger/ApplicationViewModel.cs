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

        public ICommand DeleteConfigurationCommand { get; set; }
        public ICommand RunCommand { get; set; }
        public ICommand SaveSettingsCommand { get; set; }
        public string SettingFile { get; set; }
        public string[] SettingFiles { get; set; }
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

            DeleteConfigurationCommand = new RelayCommand(DeleteSetting, () => !string.IsNullOrEmpty(SettingFile));
            RunCommand = new RelayCommand(Enable, () => !string.IsNullOrEmpty(SettingFile));
            SaveSettingsCommand = new RelayCommand(SaveSetting);
        }

        private void Enable(object obj)
        {
        }


        private void DeleteSetting(object obj)
        {
        }


        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            OnChanged(sender, null);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            var files = Directory.GetFiles(ConfigurationsLocation, "*.ini");
            SettingFiles = new string[files.Length];
            foreach (var f in files)
            {
                SettingFiles[Array.IndexOf(files, f)] = Path.GetFileNameWithoutExtension(f);
            }
            settingFilesHashSet = new HashSet<string>(SettingFiles.Select(f => f.ToLower()));
        }

        public void SaveSetting()
        {
            Network n = new Network();
            n.DNSaddresses = DNS;
            n.Gateway = Gateway;
            n.IPaddress = IP;
            n.Netmask = Mask;
            //ArrayNetwork = new Network[];
            
        }
    }
}
