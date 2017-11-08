using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
        private int networkIndex;
        public int NetworkIndex
        {
            get => networkIndex;
            set
            {
                if (value < 0)
                    value = 0;
                networkIndex = value;
            }
        }
        public Network[] NetworkArray { get; set; }
        public string FileName
        {
            get => NetworkArray[NetworkIndex].Name;
            set
            {
                if (!value.Intersect(Path.GetInvalidFileNameChars()).Any())
                    NetworkArray[NetworkIndex].Name = value;
                OnPropertyChanged();
            }
        }
        public string IP
        {
            get => NetworkArray[NetworkIndex].IPaddress;
            set { NetworkArray[NetworkIndex].IPaddress = value; }
        }
        public string Mask
        {
            get => NetworkArray[NetworkIndex].Netmask;
            set { NetworkArray[NetworkIndex].Netmask = value; }
        }
        public string Gateway
        {
            get => NetworkArray[NetworkIndex].Gateway;
            set { NetworkArray[NetworkIndex].Gateway = value; }
        }
        public string DNS
        {
            get => NetworkArray[NetworkIndex].DNSaddresses;
            set { NetworkArray[NetworkIndex].DNSaddresses = value; }
        }

        public ApplicationViewModel()
        {
            try
            {
                Directory.CreateDirectory(ConfigurationsLocation);
                OnChanged(null, null);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

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
            N.Name = FileName;
            N.DNSaddresses = DNS;
            N.Gateway = Gateway;
            N.IPaddress = IP;
            N.Netmask = Mask;
            return N;
        }

        private void Enable()
        {
            //SetGateway(NetworkItem.Gateway);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "wmic";
            startInfo.Arguments = $"nicconfig where (IPEnabled=TRUE) call SetDNSServerSearchOrder ({NetworkItem.DNSaddresses.Replace(" ", ", ")})";
            process.StartInfo = startInfo;
            process.Start();
            MessageBox.Show("Network settings enabled!");
        }

        private void DeleteSetting(object obj)
        {
            try
            {
                File.Delete($"{ConfigurationsLocation}\\{NetworkItem.Name}.ini");
                OnChanged(null, null);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            NetworkIndex = 0;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            var files = Directory.GetFiles(ConfigurationsLocation, "*.ini");
            NetworkArray = new Network[files.Length];
            foreach (var f in files)
            {
                var al = File.ReadAllLines(f);
                NetworkItem = new Network();
                NetworkItem.Name = Path.GetFileNameWithoutExtension(f);
                NetworkItem.IPaddress = al[0];
                NetworkItem.Netmask = al[1];
                NetworkItem.Gateway = al[2];
                NetworkItem.DNSaddresses = al[3];
                NetworkArray[Array.IndexOf(files, f)] = NetworkItem;
            }
        }

        public void SaveSetting(object obj)
        {
            NetworkItem = CreateNetwork();
            try
            {
                File.WriteAllText($"{ConfigurationsLocation}\\{FileName}.ini", $"{NetworkItem.IPaddress}\n{NetworkItem.Netmask}\n{NetworkItem.Gateway}\n{NetworkItem.DNSaddresses}");
                OnChanged(null, null);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            OnChanged(null, null);
        }

        /*public void SetGateway(string gateway)
        {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();

            foreach (ManagementObject objMO in objMOC)
            {
                Debug.WriteLine(objMO["Caption"]);
                {
                    try
                    {
                        ManagementBaseObject setGateway;
                        ManagementBaseObject newGateway = objMO.GetMethodParameters("SetGateways");

                        newGateway["DefaultIPGateway"] = new string[] {gateway
    };
                        newGateway["GatewayCostMetric"] = new int[] { 1 };

                        setGateway = objMO.InvokeMethod("SetGateways", newGateway, null);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }*/

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        public static IPAddress GetDefaultGateway()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .FirstOrDefault(a => a != null);
        }
    }
}