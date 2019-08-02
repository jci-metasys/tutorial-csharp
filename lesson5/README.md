# Lesson 5

In this lesson we'll explore how to get objects and send commands.

Dependencies:

* .Net Core
* Netwonsoft.Json
* Flurl.Http

The code for this program is in [Program.cs](./Program.cs).

Like all the other lessons you should be able to run the app.

## Getting an object

Objects can be queried for attributes as seen in the [documentation](https://metasys-server.github.io/api-landing/api/v2/#/reference/objects/get-network-device-children-objects/get-a-single-object-attribute). In this section we will attempt to read an analog value (AV).

First you must discover all AV objects using the get objects command which requires a type. The type on an AV can be several different values depending on your hardware. You can manually search for this value using the `/enumSets/508/members` endpoint. Possible values include but are not limited to:

| Title     | member number |
|-----------|---------------|
| AV        | 2             |
| JCI AV    | 165           |
| BACnet AV | 502           |

```csharp
// Get list of AV objects
var avObjects = await client.Request("objects?type=165").GetJsonAsync();
Console.WriteLine($"Total AVs: {JsonConvert.SerializeObject(avObjects.total, Formatting.Indented)}");
Console.WriteLine($"First AV: {JsonConvert.SerializeObject(avObjects.items[0], Formatting.Indented)}");

// Get the first object by it's id
var av = await client.Request($"objects/{avObjects.items[0].id}").GetJsonAsync();
Console.WriteLine($"First AV full: {JsonConvert.SerializeObject(av, Formatting.Indented)}");
```

The get objects command does not return a list of all the attributes available. Use the [get a single object](https://metasys-server.github.io/api-landing/api/v2/#/reference/objects/get-a-single-object/get-a-single-object) endpoint to see all attributes on an object. You should see a response similar to the following:

```shell
Total AVs: 8
First AV: {
  "id": "66e12484-38ce-5b4b-bced-66995c002743",
  "itemReference": "t3502:Manchester0563/Master Objects.Equipment Master Objects.CLGOATLOCKOUT-SP Mstr",
  "name": "CLGOATLOCKOUT-SP Mstr",
  "typeUrl": "https://t3502/api/v2/enumSets/508/members/165",
  "self": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743",
  "parentUrl": "https://t3502/api/v2/objects/6295ea8a-7196-5f6e-ac52-8b118349ecd7",
  "objectsUrl": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743/objects",
  "networkDeviceUrl": "https://t3502/api/v2/networkDevices/20a610af-8810-5501-9a7f-f2c94fbf63f7",
  "pointsUrl": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743/points",
  "trendedAttributesUrl": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743/trendedAttributes",
  "alarmsUrl": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743/alarms",
  "auditsUrl": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743/audits"
}
First AV full: {
  "item": {
    "id": "66e12484-38ce-5b4b-bced-66995c002743",
    "name": "CLGOATLOCKOUT-SP Mstr",
    "description": "Cooling Lockout Setpoint Master",
    "bacnetObjectType": "objectTypeEnumSet.bacAvClass",
    "objectCategory": "objectCategoryEnumSet.generalCategory",
    "defaultAttribute": "attributeEnumSet.presentValue",
    "presentValue": {
      "value": 60.0,
      "reliability": "reliabilityEnumSet.reliable",
      "priority": "writePriorityEnumSet.priorityNone"
    },
    "reliability": "reliabilityEnumSet.reliable",
    "alarmState": "objectStatusEnumSet.osNormal",
    "itemReference": "t3502:Manchester0563/Master Objects.Equipment Master Objects.CLGOATLOCKOUT-SP Mstr",
    "minPresValue": -20.0,
    "maxPresValue": 120.0,
    "units": "unitEnumSet.degF",
    "displayPrecision": "displayPrecisionEnumSet.displayPrecisionPt1",
    "covIncrement": 0.01
  },
  "typeUrl": "https://t3502/api/v2/enumSets/508/members/165",
  "self": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743",
  "parentUrl": "https://t3502/api/v2/objects/20a610af-8810-5501-9a7f-f2c94fbf63f7",
  "objectsUrl": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743/objects",
  "networkDeviceUrl": "https://t3502/api/v2/networkDevices/6295ea8a-7196-5f6e-ac52-8b118349ecd7",
  "pointsUrl": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743/points",
  "trendedAttributesUrl": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743/trendedAttributes",
  "alarmsUrl": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743/alarms",
  "auditsUrl": "https://t3502/api/v2/objects/66e12484-38ce-5b4b-bced-66995c002743/audits"
}
```

There should be an attribute called `presentValue` on your AV. We will attempt to get this value using the [get a single object attribute](https://metasys-server.github.io/api-landing/api/v2/#/reference/objects/get-a-single-object-attribute/get-a-single-object-attribute) endpoint.

```csharp
// Get the presentValue of the AV
var presentValue =  await client.Request($"objects/{avObjects.items[0].id}/attributes/presentValue").GetJsonAsync();
Console.WriteLine($"The present value {JsonConvert.SerializeObject(presentValue, Formatting.Indented)}");
```

```shell
The present value: {
  "item": {
    "presentValue": {
      "value": 60.0,
      "reliability": "reliabilityEnumSet.reliable",
      "priority": "writePriorityEnumSet.priorityNone"
    }
  }
}
```

## Commanding an Object

Now that you can get an AV, we will get the possible commands using the [get commands for an object](https://metasys-server.github.io/api-landing/api/v2/#/reference/objects/get-commands-for-an-object/get-commands-for-an-object) endpoint and send one.

```csharp
// Get commands for the AV
var commands = await client.Request($"objects/{av.item.id}/commands").GetJsonListAsync();
Console.WriteLine($"Commands: {JsonConvert.SerializeObject(commands, Formatting.Indented)}");
```

You should get a response similar to the following:

```shell
Commands: [
  ...
  {
    "$schema": "http://json-schema.org/schema#",
    "commandId": "EnableAlarms",
    "title": "Enable Alarms",
    "type": "array",
    "items": [],
    "minItems": 0,
    "maxItems": 0
  },
  {
    "$schema": "http://json-schema.org/schema#",
    "commandId": "DisableAlarms",
    "title": "Disable Alarms",
    "type": "array",
    "items": [],
    "minItems": 0,
    "maxItems": 0
  }
]
```

Lets attempt to send the DisableAlarms command, then send the EnableAlarms command to undo the action. If your AV has these commands you should get an OK response. Take note that `PutJsonAsync` requires a parameter and since these commands have no body we will just send an empty array. See the [send command to an object](https://metasys-server.github.io/api-landing/api/v2/#/reference/objects/send-a-command-to-an-object/send-a-command-to-an-object) endpoint.

```csharp
// Send the disable alarms command
var response = await client.Request($"objects/{av.item.id}/commands/DisableAlarms").PutJsonAsync(new string[0]);
Console.WriteLine($"Disable alarms: {response.StatusCode}");

// Send the enable alarms command
response = await client.Request($"objects/{av.item.id}/commands/EnableAlarms").PutJsonAsync(new string[0]);
Console.WriteLine($"Enable alarms: {response.StatusCode}");
```

```shell
Disable alarms: OK
Enable alarms: OK
```

## Patch an object

A value on an object can be modified with the [update an object](https://metasys-server.github.io/api-landing/api/v2/#/reference/objects/update-an-object/update-an-object) command. We will change the name and description of our AV object and then convert it back to it's original value.

```csharp
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
```

In order to allow time for the patch command to take effect the thread waits 5 seconds with the line `System.Threading.Thread.Sleep(5000);` before reading the AV values. Without this code the values may not have enough time to update before they are fetched again. You should get an output similar to the following:

```shell
AV name: CLGOATLOCKOUT-SP Mstr
AV description: Cooling Lockout Setpoint Master
Patch AV: Accepted
AV new name: "AV Test"
AV new description: "Test"
Patch AV: Accepted
AV restored name: "CLGOATLOCKOUT-SP Mstr"
AV restored description: "Cooling Lockout Setpoint Master"
```

## Conclusion

This concludes the tutorials. Consult the [Metasys API](https://metasys-server.github.io/api-landing/api/v2/) to explore more endpoints and try them on your own.
