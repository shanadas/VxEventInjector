using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace VxEventInjectorSvc
{
    class Utils
    {
        public static IPAddress IPv4
        {
            get
            {
                IPAddress address = Dns.GetHostEntry(Dns.GetHostName())
                     .AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork
                     && !a.Equals(IPAddress.Loopback));
                return address;
            }
        }
    }
}
