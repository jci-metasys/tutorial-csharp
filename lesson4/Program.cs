using System;
using System.Threading.Tasks;
using Flurl.Http;
using JohnsonControls.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace lesson4
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var username = args[0];
            var password = args[1];
            var hostname = args[2];

            // Configure Flurl to use the HttpClientRedirectFactory
            // This ensures we get the redirect handling discussed in lesson 3
            FlurlHttp.Configure(settings => settings.HttpClientFactory = new HttpClientRedirectFactory());

            string accessToken;

            using (var client = new FlurlClient($"https://{hostname}/api/v1"))
            {

                var loginResult = await client.Request("login")
                    .PostJsonAsync(new {username = username, password = password})
                    .ReceiveJson();

                // loginResult is a dynamic object. It has dynamic properties based on the JSON received
                accessToken = loginResult.accessToken;
            }

            // Create a new client that has the OAuthHeader set on each request.
            using (var client = new FlurlClient($"https://{hostname}/api/v1").WithOAuthBearerToken(accessToken))
            {
                var alarms = await client.Request("alarms").GetJsonAsync();

                // Again alarms is a dynamic object, we can query for any property
                // defined in the schema

                var nextPageUrl = alarms.next;
                Console.WriteLine($"The next page of alarms url: {nextPageUrl}");


                var firstAlarmItemReference = alarms.items[0].itemReference;
                Console.WriteLine($"The item reference of the first alarm: {firstAlarmItemReference}");

                var triggerValueUnits = alarms.items[0].triggerValue.units;
                Console.WriteLine($"The trigger value units of first alarm: {triggerValueUnits}");

                // But the bad thing is if we just want to get an Alarm
                // it doesn't work like we want
                var firstAlarm = alarms.items[0];
                Console.WriteLine($"The first alarm (oops!): {firstAlarm.ToString()}"); // Outputs "System.Dynamic.ExpandoObject"

                Console.WriteLine($"The first alarm (much better): {JsonConvert.SerializeObject(firstAlarm, Formatting.Indented)}");

                // So an alternative is to ues GetJsonAsync<JToken> which returns JTokens as defined
                // by Newtonsoft. To illustrate let's fetch the alarms again

                var alarmsObject = await client.Request("alarms").GetJsonAsync<JToken>();

                // Now we can treat alarms as a dictionary
                var alarmsCollection = alarmsObject["items"]; // Returns a JArray since items is a collection
                var firstAlarmObject = alarmsCollection[0]; // Returns a JObject since each alarm is an object

                Console.WriteLine($"The first alarm (one more time): {firstAlarmObject}");

                var firstAlarmObjectItemReference = alarmsCollection[0]["itemReference"];
                Console.WriteLine($"The item reference of the first alarm: {firstAlarmObjectItemReference}");

            }

        }
    }
}
