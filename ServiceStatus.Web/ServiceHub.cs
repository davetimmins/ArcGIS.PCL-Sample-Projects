using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using ArcGIS.ServiceModel;
using System.Threading.Tasks;

namespace ServiceStatus.Web
{
    public class ServiceHub : Hub
    {
        public async Task<String> checkStatuses(String serverUrl, String username, String password)
        {
            if (String.IsNullOrWhiteSpace(serverUrl) || String.IsNullOrWhiteSpace(username) || String.IsNullOrWhiteSpace(password)) return null;

            var gateway = new SecureArcGISServerGateway(serverUrl, username, password);

            var response = await gateway.DescribeSite();
            
            foreach (var resource in response.Services)
            {
                var status = await gateway.ServiceStatus(resource);
                Clients.Caller.addNewServiceToPage(String.Format("{0} ({1})", resource.Name, resource.Type), status.Actual);
            }

            return String.Empty;
        }
    }
}