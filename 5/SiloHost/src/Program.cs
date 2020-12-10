using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Grains;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Statistics;

namespace SiloHost
{
    class Program
    {
        public static Task Main()
        {
            var advertisedIp = Environment.GetEnvironmentVariable("ADVERTISEDIP");
            var advertisedIpAddress = advertisedIp == null ? GetLocalIpAddress() : IPAddress.Parse(advertisedIp);
            var gatewayPort = int.Parse(Environment.GetEnvironmentVariable("GATEWAYPORT") ?? throw new Exception("Gateway port missing."));
            var siloPort = int.Parse(Environment.GetEnvironmentVariable("SILOPORT") ?? throw new Exception("Silo port missing."));
            var primarySiloPort = int.Parse(Environment.GetEnvironmentVariable("PRIMARYSILOPORT") ?? throw new Exception("Primary silo port missing."));
            
            var siloEndpointConfiguration = GetSiloEndpointConfiguration(advertisedIpAddress, siloPort, gatewayPort);

            return new HostBuilder()
                .UseOrleans(siloBuilder =>
                {
                    siloBuilder.UseLinuxEnvironmentStatistics();
                    siloBuilder.UseDashboard(dashboardOptions =>
                    {
                        dashboardOptions.Username = "piotr";
                        dashboardOptions.Password = "orleans";
                    });
                    siloBuilder.UseDevelopmentClustering(new IPEndPoint(advertisedIpAddress, primarySiloPort));
                    siloBuilder.Configure<ClusterOptions>(clusterOptions =>
                    {
                        clusterOptions.ClusterId = "this-is-not-relevant-yet";
                        clusterOptions.ServiceId = "this-is-not-relevant-yet";
                    });
                    siloBuilder.Configure<EndpointOptions>(endpointOptions =>
                    {
                        endpointOptions.AdvertisedIPAddress = siloEndpointConfiguration.Ip;
                        endpointOptions.SiloPort = siloEndpointConfiguration.SiloPort;
                        endpointOptions.GatewayPort = siloEndpointConfiguration.GatewayPort;
                        endpointOptions.SiloListeningEndpoint = new IPEndPoint(IPAddress.Any, 2000);
                        endpointOptions.GatewayListeningEndpoint = new IPEndPoint(IPAddress.Any, 3000);
                    });
                    siloBuilder.ConfigureApplicationParts(applicationPartManager =>
                        applicationPartManager.AddApplicationPart(typeof(HelloWorld).Assembly).WithReferences());
                })
                .ConfigureLogging(logging => logging.AddConsole())
                .RunConsoleAsync();
        }

        private static SiloEndpointConfiguration GetSiloEndpointConfiguration(
            IPAddress advertisedAddress,
            int siloPort,
            int gatewayPort)
        {

            return new SiloEndpointConfiguration(
                advertisedAddress,
                siloPort,
                gatewayPort);
        }

        private static IPAddress GetLocalIpAddress()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var network in networkInterfaces)
            {
                if (network.OperationalStatus != OperationalStatus.Up)
                    continue;
            
                var properties = network.GetIPProperties();
                if (properties.GatewayAddresses.Count == 0)
                    continue;
            
                foreach (var address in properties.UnicastAddresses)
                {
                    if (address.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(address.Address))
                    {
                        return address.Address;
                    }
                }
            }
            
            return null;
        }
    }
}