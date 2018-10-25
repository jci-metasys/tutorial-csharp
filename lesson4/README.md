# Lesson 4

In this lesson we'll again login and fetch some alarms. But this time
we'll take advantage of Newtonsoft and Flurl to automatically convert
the string responses into objects we can query. Flurl also makes it easier
to post JSON.

Dependencies:

* .Net Core
* HttpClientRedirectHandler.Flurl

The following dependencies are installed as dependencies of HttpClientRedirectHandler.Flurl

* Netwonsoft.Json
* HttpClientRedirectHandler
* Flurl.Http

The code for this program is in [Program.cs](./Program.cs).

Like all the other lessons you should be able to run the app

```shell
$ dotnet run -c release username password hostname
/alarms?pageSize=100&excludePending=false&excludeAcknowledged=false&excludeDiscarded=false&page=2
thesun:Mars3518/Field Bus MSTP1.VAV-08.DPR-O
/enumSets/507/members/98
System.Dynamic.ExpandoObject
{
  "self": "/alarms/34015016-3432-4521-83bc-61ad10b99545",
  "id": "34015016-3432-4521-83bc-61ad10b99545",
  "itemReference": "thesun:Mars3518/Field Bus MSTP1.VAV-08.DPR-O",
  "name": "DPR-O",
  "message": "",
  "isAckRequired": true,
  "type": "/enumSets/108/members/66",
  "priority": 70,
  "triggerValue": {
    "value": "0.0",
    "units": "/enumSets/507/members/98"
  },
  "creationTime": "2018-10-24T23:00:22Z",
  "isAcknowledged": false,
  "isDiscarded": false,
  "category": "/enumSets/33/members/5",
  "object": "/objects/e6b03623-612d-5034-9c11-7f24005b31ef",
  "annotations": "/alarms/34015016-3432-4521-83bc-61ad10b99545/annotations"
}
thesun:Mars3518/Field Bus MSTP1.VAV-08.DPR-O
```

## Configure Flurl to use our HttpClient Factory

We are introducing a new dependency HttpClientRedirectHandler.Flurl.

This package supplies one new class `HttpClientRedirectFactory` that allows
us to inject our HttpClientRedirectHandler into the construction of Flurl clients.


```csharp
FlurlHttp.Configure(settings => 
  settings.HttpClientFactory = new HttpClientRedirectFactory());
```

## Construct a Flurl Client

Now that Flurl is configured we can create our client, login,
and get our accessToken

```csharp
string accessToken;

using (var client = new FlurlClient($"https://{hostname}/api/v1"))
{

    var loginResult = await client.Request("login")
        .PostJsonAsync(new {username = username, password = password})
        .ReceiveJson();

    // loginResult is a dynamic object. It has dynamic properties based on the JSON received
    accessToken = loginResult.accessToken;
}
```

Some things to note.

* Note we are now creating a FlurlClient rather than an HttpClient. Flurl
provides us with some methods to make it easier to deal with requests and responses.

* Notice that we construct
a C# anonymous object with username and password properties and pass that to 
`PostJsonAsync`. The method takes care to convert it into JSON for us. This is
a little easier than formating JSON in a string literal.

* Also we use the `ReceiveJson` method to have the result parsed and returned
as a dynamic object (actually an [ExpandoObject](https://stackoverflow.com/questions/1653046/what-are-the-true-benefits-of-expandoobject#1663044)).
See the linked article for more details or search MSDN. In a nutshell, we can
query the response object using dynamic properties. That is how we are able
to get the access token using `loginResult.accessToken`.

## Create a new Flurl Client with Authorization Header set

In the preceding section we let our first client be disposed because
we are done with it. We'll now create a new client that will be used
for all the rest of our requests. It will be configured to have our Authorization
header set properly.

```csharp
using (var client = new FlurlClient($"https://{hostname}/api/v1")
  .WithOAuthBearerToken(accessToken))
{
}
```

Now every request we make with this client will have our auth token on it.

## Fetch Alarms and Return Them As Dynamic Objects

Now we'll demonstrate how easy it is to fetch some alarms and access properties
of the result:

```csharp
var alarms = await client.Request("alarms").GetJsonAsync();

// Again alarms is a dynamic object, we can query for any property
// defined in the schema

var nextPageUrl = alarms.next;
Console.WriteLine(nextPageUrl);


var firstAlarmItemReference = alarms.items[0].itemReference;
Console.WriteLine(firstAlarmItemReference);

var triggerValueUnits = alarms.items[0].triggerValue.units;
Console.WriteLine(triggerValueUnits);
```

On the first line we call `GetJsonAsync` on a request to the `alarms` endpoint.

In the next 3 pairs of lines we demonstrate querying different properties like 
`next` which will be a link to the next page of alarms.

The `items` property returns a dynamic object that represents the collection of
alarms. We can even index it like we do in `alarms.items[0].itemReference`.

So this is all very cool. There is one problem however. What if you want
to just print out the whole alarm like this:

```csharp
var firstAlarm = alarms.items[0];
Console.WriteLine(firstAlarm.ToString()); // Outputs "System.Dynamic.ExpandoObject"
```

Unfortunately the dynamic object returned doesn't have a useful `ToString()` method
and we only get the type information.

In the next section we'll explore an alternative approach.

## Fetch Alarms and Return them as Newtonsoft JToken istances

As we saw in a previous lesson, Newtonsoft provides a nice interface for
handling JSON instances. So we'll fetch the alarms again taking advantage of
Newtonsoft. See [Newtonsoft API](https://www.newtonsoft.com/json/help/html/N_Newtonsoft_Json_Linq.htm)
for more information.

```csharp
var alarmsObject = await client.Request("alarms").GetJsonAsync<JToken>();

// Now we can treat alarms as a dictionary
var alarmsCollection = alarmsObject["items"]; // Returns a JArray since items is a collection
var firstAlarmObject = alarmsCollection[0]; // Returns a JObject since each alarm is an object

Console.WriteLine(firstAlarmObject);

var firstAlarmObjectItemReference = alarmsCollection[0]["itemReference"];
Console.WriteLine(firstAlarmObjectItemReference);
```

In the first line of this snippet we tell Flurl to return the JSON as a
`JToken`.  `JToken` is the parent class of several other types including
`JObject`, `JArray` and `JValue` which we use in this example even if
those types aren't explicitly shown.

When we tell Flurl to return a JToken we know it'll return the appropriate subclass
for the data we access. We know it'll be a JObject because we know from the documentation
that a JSON object is returned from alarms.

With a `JObject` instance we can use an indexer to lookup properties on that object.

So we show how to access the `items` collection by doing `alarmsObject["items"]`.
Since items is a collection we know that the result returned will be a `JArray`.
We can use an indexer on `JArray` instances and we do that (`alarmsCollection[0]`)
to get the first alarm.

Now we can print that alarm. Unlike the dyanmic object in the previous section,
JTokens know how to convert themselves to pretty strings:

```json
{
  "self": "/alarms/34015016-3432-4521-83bc-61ad10b99545",
  "id": "34015016-3432-4521-83bc-61ad10b99545",
  "itemReference": "thesun:Mars3518/Field Bus MSTP1.VAV-08.DPR-O",
  "name": "DPR-O",
  "message": "",
  "isAckRequired": true,
  "type": "/enumSets/108/members/66",
  "priority": 70,
  "triggerValue": {
    "value": "0.0",
    "units": "/enumSets/507/members/98"
  },
  "creationTime": "2018-10-24T23:00:22Z",
  "isAcknowledged": false,
  "isDiscarded": false,
  "category": "/enumSets/33/members/5",
  "object": "/objects/e6b03623-612d-5034-9c11-7f24005b31ef",
  "annotations": "/alarms/34015016-3432-4521-83bc-61ad10b99545/annotations"
}
```

Finally we show how to get just the item reference of that alarm:

`var firstAlarmObjectItemReference = alarmsCollection[0]["itemReference"];
`

## Summary

In this lesson we learned how to use Flurl to return either `JToken` instances (provided by Newtonsoft) or 
dynamic objects (provided by .Net core) to make it easier to deal with JSON.

Both of these approaches work pretty well for quick and easy programming tasks.

For more involved applications you may want to create your own model classes. We'll show
you how in Lesson 5.