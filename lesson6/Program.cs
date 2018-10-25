using System;
using System.Threading.Tasks;
using Flurl.Http;
using JohnsonControls.Net.Http;
using Newtonsoft.Json;

namespace lesson6
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var username = args[0];
            var password = args[1];
            var hostname = args[2];

            FlurlHttp.Configure(settings => settings.HttpClientFactory = new HttpClientRedirectFactory());

            using (var client = new FlurlClient($"https://{hostname}/api/v1"))
            {
                var accessToken = (await client.Request("login")
                    .PostJsonAsync(new {username, password})
                    .ReceiveJson()).accessToken;

                client.Headers.Add("Authorization", $"Bearer {accessToken}");

                // Let's exercise the API
                var networkDevices = await client.Request("networkDevices").GetJsonAsync();
                Console.WriteLine($"Total number of network devices: {networkDevices.total}");

                // Get objects from the first network device
                var firstNetworkDevice = networkDevices.items[0];
                var objects = await client.Request($"networkDevices/{firstNetworkDevice.id}/objects").GetJsonAsync();
                Console.Write($"Item Reference of first object on first device: {objects.items[0].itemReference}");


                // Get alarms, but exclude acknowledged and discarded alarms. Also only in the priority
                // range 0-70

                var alarms = await client.Request("alarms")
                    .SetQueryParams(new
                    {
                        excludePending = true,
                        excludeAcknowledged = true,
                        priorityRange = "0,70"
                    })
                    .GetJsonAsync();

                // Do the same thing but manually construct the URL
                alarms = await client.Request("alarms?excludePending=true&excludeAcknowledged=true&priorityRange=0,70")
                    .GetJsonAsync();

                Console.WriteLine(JsonConvert.SerializeObject(alarms.items[0], Formatting.Indented));
            }
        }


    }


}
