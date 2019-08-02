using System;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using System.Net.Http;

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

                // Get list of AV objects
                var avObjects = await client.Request("objects?type=165").GetJsonAsync();
                Console.WriteLine($"Total AVs: {JsonConvert.SerializeObject(avObjects.total, Formatting.Indented)}");
                Console.WriteLine($"First AV: {JsonConvert.SerializeObject(avObjects.items[0], Formatting.Indented)}");

                // Get the first object by it's id
                var av = await client.Request($"objects/{avObjects.items[0].id}").GetJsonAsync();
                Console.WriteLine($"First AV full: {JsonConvert.SerializeObject(av, Formatting.Indented)}");

                // Get the presentValue of the AV
                var presentValue =  await client.Request($"objects/{av.item.id}/attributes/presentValue").GetJsonAsync();
                Console.WriteLine($"The present value: {JsonConvert.SerializeObject(presentValue, Formatting.Indented)}");

                // Get commands for the AV
                var commands = await client.Request($"objects/{av.item.id}/commands").GetJsonListAsync();
                Console.WriteLine($"Commands: {JsonConvert.SerializeObject(commands, Formatting.Indented)}");

                // Send the disable alarms command
                var response = await client.Request($"objects/{av.item.id}/commands/DisableAlarms").PutJsonAsync(new string[0]);
                Console.WriteLine($"Disable alarms: {response.StatusCode}");

                // Send the enable alarms command
                response = await client.Request($"objects/{av.item.id}/commands/EnableAlarms").PutJsonAsync(new string[0]);
                Console.WriteLine($"Enable alarms: {response.StatusCode}");

                // Update the AV
                var avDescription = av.item.description;
                var avName = av.item.name;
                Console.WriteLine($"AV name: {avName}");
                Console.WriteLine($"AV description: {avDescription}");

                var item = new {description = "Test", name = "AV Test"};
                var update = new {item = item};
                response = await client.Request($"objects/{av.item.id}").PatchJsonAsync(update);
                Console.WriteLine($"Patch AV: {response.StatusCode}");
                
                System.Threading.Thread.Sleep(5000);
                var avUpdated = await client.Request($"objects/{av.item.id}").GetJsonAsync();
                Console.WriteLine($"AV new name: {JsonConvert.SerializeObject(avUpdated.item.name, Formatting.Indented)}");
                Console.WriteLine($"AV new description: {JsonConvert.SerializeObject(avUpdated.item.description, Formatting.Indented)}");

                // Restore AV to previous values
                var item2 = new {description = avDescription, name = avName};
                var restore = new {item = item2};
                response = await client.Request($"objects/{av.item.id}").PatchJsonAsync(restore);
                Console.WriteLine($"Patch AV: {response.StatusCode}");

                System.Threading.Thread.Sleep(5000);
                var avRestored = await client.Request($"objects/{av.item.id}").GetJsonAsync();
                Console.WriteLine($"AV restored name: {JsonConvert.SerializeObject(avRestored.item.name, Formatting.Indented)}");
                Console.WriteLine($"AV restored description: {JsonConvert.SerializeObject(avRestored.item.description, Formatting.Indented)}");
            }
        }
    }
}
