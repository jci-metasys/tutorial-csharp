# Lesson 4

In this lesson we'll further explore how to use Flurl to model requests. This is the last lesson that focuses on alarms.

Dependencies:

* .Net Core
* Netwonsoft.Json
* Flurl.Http

The code for this program is in [Program.cs](./Program.cs).

Like all the other lessons you should be able to run the app.

Note: This app assumes you have alarms with a priority range from 0 to 70 that are not pending or acknowledged. If you do not the app will crash.

## Network Devices

Queries for network devices is similar to getting alarms. If you look at the [documentation](https://metasys-server.github.io/api-landing/api/v2/#/reference/network-devices/get-network-devices/get-network-devices) for
the network devices API you see more options for forming requests.

```csharp
var networkDevices = await client.Request("networkDevices").GetJsonAsync();
Console.WriteLine($"Total number of network devices: {networkDevices.total}");

// Get alarms on the first network device
var firstNetworkDevice = networkDevices.items[0];
var alarms = await client.Request($"networkDevices/{firstNetworkDevice.id}/alarms").GetJsonAsync();
Console.WriteLine($"Total alarms on the first Network Device: {JsonConvert.SerializeObject(alarms.total, Formatting.Indented)}");
```

This section shows how to get network devices and how to use their respective id's to get objects and alarms. Since a network device is an object it's alarms can also be queried using the objects endpoint.

```csharp
// Get alarms on the first first network device using the objects endpoint
alarms = await client.Request($"objects/{firstNetworkDevice.id}/alarms").GetJsonAsync();
Console.WriteLine($"Total alarms on the Object: {JsonConvert.SerializeObject(alarms.total, Formatting.Indented)}");
```

## More Alarms

There is an alternative format for including parameters you can utilize for requests:

```csharp
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
```

Which is the same as:

```csharp
// Do the same thing but manually construct the URL
alarms = await client.Request("alarms?excludePending=true&excludeAcknowledged=true&priorityRange=0,70")
    .GetJsonAsync();

Console.WriteLine(JsonConvert.SerializeObject(alarms.items[0], Formatting.Indented));
```

## Conclusion

This section concludes examples on how to handle alarms. In the next lesson we will focus on the object API endpoints.
