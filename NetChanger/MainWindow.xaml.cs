using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
namespace NetChanger
{
    public partial class MainWindow : Window
    {
        
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
        
        public MainWindow()
        {
            InitializeComponent();
            label1.Content = "My IP: " + GetLocalIPAddress();
            label2.Content = "Gateway: " + GetDefaultGateway();
            label3.Content = "Choose your network";
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            setGateway("10.60.70.50");
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "wmic";
            startInfo.Arguments = "nicconfig where (IPEnabled=TRUE) call SetDNSServerSearchOrder (\"10.87.0.50\", \"10.87.0.150\",, \"10.87.24.10\", \"10.87.26.45\", \"10.87.27.30\", \"10.87.27.50\", \"10.87.0.109\", \"10.87.0.110\")";
            process.StartInfo = startInfo;
            process.Start();
            //setDNS("[00000000] WAN Miniport (SSTP)", "10.87.0.50,10.87.0.150,10.87.24.10,10.87.26.45");
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            setGateway("10.60.70.9");
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "wmic";
            startInfo.Arguments = "nicconfig where (IPEnabled=TRUE) call SetDNSServerSearchOrder (\"10.60.70.9\")";
            process.StartInfo = startInfo;
            process.Start();
            //setDNS("[00000000] WAN Miniport (SSTP)", "10.60.70.9");
        }
        
        /// <summary>
        /// Set's a new Gateway address of the local machine
        /// </summary>
        /// <param name="gateway">The Gateway IP Address</param>
        /// <remarks>Requires a reference to the System.Management namespace</remarks>
        public void setGateway(string gateway)
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
                        ManagementBaseObject newGateway =
                            objMO.GetMethodParameters("SetGateways");

                        newGateway["DefaultIPGateway"] = new string[] {gateway};
                        newGateway["GatewayCostMetric"] = new int[] {1};

                        setGateway = objMO.InvokeMethod("SetGateways", newGateway, null);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
    }
}
