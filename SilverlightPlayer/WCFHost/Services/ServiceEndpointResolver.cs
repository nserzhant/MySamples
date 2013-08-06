using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace WCFHost.Services
{
    public interface IServiceEndpointResolver
    {
        string[] GetAvialableEndpoints();
    }

    public class ServiceEndpointResolver : IServiceEndpointResolver
    {
        public string[] GetAvialableEndpoints()
        {
            return Dns.GetHostAddresses(Dns.GetHostName()).Select(addr => addr.ToString()).ToArray();
        }
    }
}
