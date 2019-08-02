using System;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;

namespace lesson4
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var username = args[0];
            var password = args[1];
            var hostname = args[2];

            using (var client = new FlurlClient($"https://{hostname}/api/v2"))
            {
                var accessToken = (await client.Request("login")
                    .PostJsonAsync(new {username, password})
                    .ReceiveJson()).accessToken;

                client.Headers.Add("Authorization", $"Bearer {accessToken}");

                // Let's exercise the API
                var networkDevices = await client.Request("networkDevices").GetJsonAsync();
                Console.WriteLine($"Total number of network devices: {networkDevices.total}");

                // Get alarms on the first network device
                var firstNetworkDevice = networkDevices.items[0];
                var alarms = await client.Request($"networkDevices/{firstNetworkDevice.id}/alarms").GetJsonAsync();
                Console.WriteLine($"Total alarms on the first Network Device: {JsonConvert.SerializeObject(alarms.total, Formatting.Indented)}");
                
                // Get alarms on the first network device using the objects endpoint
                alarms = await client.Request($"objects/{firstNetworkDevice.id}/alarms").GetJsonAsync();
                Console.WriteLine($"Total alarms on the Object: {JsonConvert.SerializeObject(alarms.total, Formatting.Indented)}");

                // Get alarms, but exclude acknowledged and discarded alarms. Also only in the priority
                // range 0-70
                alarms = await client.Request("alarms")
                    .SetQueryParams(new
                    {
                        excludePending = true,
                        excludeAcknowledged = true,
                        priorityRange = "0,70"
                    })
                    .GetJsonAsync();

                Console.WriteLine(JsonConvert.SerializeObject(alarms.items[0], Formatting.Indented));

                // Do the same thing but manually construct the URL
                alarms = await client.Request("alarms?excludePending=true&excludeAcknowledged=true&priorityRange=0,70")
                    .GetJsonAsync();

                Console.WriteLine(JsonConvert.SerializeObject(alarms.items[0], Formatting.Indented));
            }
        }
    }
}
