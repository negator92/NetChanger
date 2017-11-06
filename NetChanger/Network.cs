using System;

namespace NetChanger
{
    /// <summary>
    /// Description of Network.
    /// </summary>
    public class Network
    {
        public Network() { }
        
        public string IPaddress { get; set; }
        public string Netmask { get; set; }
        public string Gateway { get; set; }
        public string DNSaddresses { get; set; }
    }
}
