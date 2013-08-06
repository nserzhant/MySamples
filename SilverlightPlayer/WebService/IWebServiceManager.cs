using System;
namespace WebService
{
    public interface IWebServiceManager
    {
        void ShootDown();
        bool TryStart(string endpointAddress, out string errorMessage);
    }
}
