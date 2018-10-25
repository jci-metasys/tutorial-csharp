using System;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using JohnsonControls.Net.Http;
using Newtonsoft.Json;

namespace lesson5
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var username = args[0];
            var password = args[1];
            var hostname = args[2];


            FlurlHttp.Configure(settings => settings.HttpClientFactory = new HttpClientRedirectFactory());

            string accessToken;
            using (var client = new FlurlClient($"https://{hostname}/api/v1"))
            {
                var loginObject = new Login
                {
                    Username = username,
                    Password = password
                };
                accessToken = (await client.Request("login").PostJsonAsync(loginObject)
                    .ReceiveJson<LoginResponse>()).AccessToken;
            }

            using (var client = new FlurlClient($"https://{hostname}/api/v1").WithOAuthBearerToken(accessToken))
            {
                var alarms = await client.Request("alarms").GetJsonAsync<AlarmCollection>();

                Console.WriteLine($"Total alarms: {alarms.Total}");
                Console.WriteLine($"First alarm: {JsonConvert.SerializeObject(alarms[5], Formatting.Indented)}");
                Console.WriteLine($"Item reference of 3rd alarm: {alarms.Items[2].ItemReference}");


                var secondPageOfAlarms = await client.Request(alarms.Next).GetJsonAsync<AlarmCollection>();
                Console.WriteLine(secondPageOfAlarms[1].Name);
            }
        }
    }
}
