# Lesson 2

In this lesson we build upon [Lesson 1](../lesson1/README.md) and successfully
call the alarms API to retrieve one page of alarms.

Dependencies:

* .Net Core
* Netwonsoft.Json

You should be able to run this lesson by supplying a username,
password, and host name. If everything works correctly the application
will print the first alarm from the list it receives from the server.

```shell
$ dotnet run username password hostname
First alarm: {
  "self": "/alarms/e3721d94-a165-46a2-8ec8-73f52752048b",
  "id": "e3721d94-a165-46a2-8ec8-73f52752048b",
  "itemReference": "thesun:Granymede4216/Field Bus MSTP1.VAV-08.DPR-O",
  "name": "DPR-O",
  "message": "",
  "isAckRequired": true,
  "type": "/enumSets/108/members/66",
  "priority": 70,
  "triggerValue": {
    "value": "0.0",
    "units": "/enumSets/507/members/98"
  },
  "creationTime": "2018-10-24T20:28:08Z",
  "isAcknowledged": false,
  "isDiscarded": false,
  "category": "/enumSets/33/members/5",
  "object": "/objects/023fd854-2eb2-54d2-b31c-81f75aef92c9",
  "annotations": "/alarms/e3721d94-a165-46a2-8ec8-73f52752048b/annotations"
}
```

The source code for this application is all in the [Program.cs](./Program.cs)

Much of this program is identical to the one in lesson 1. We'll highlight
the differences.

## Create an HttpClientHandler

Recall from lesson 1 that even though we had an access token we couldn't fetch
alarms. This was because on redirect our authorization header was being stripped.
What this means is we need to stop using auto redirects.

We do this by configuring an HttpClientHandler and passing that to our
HttpClient:

```csharp
var handler = new HttpClientHandler {AllowAutoRedirect = false};

using (var client = new HttpClient(handler) {BaseAddress = new Uri($"https://{hostname}/api/v1")})
{
}
```

So now whenever we make a call and the server returns a redirect instruction, we need
to manually handle this. (This is less than ideal, but we'll make it simpler in a future lesson)

## Handle the redirect on login

So the next change we'll make to Program 1 is to handle the redirect for login:

```csharp
var loginResponseMessage = await client.PostAsync("login", loginContent);

// Handle the redirect
loginResponseMessage = await client.PostAsync(loginResponseMessage.Headers.Location, loginContent);
```

What we are doing is making the call just like we did in Program 1. But we know
that we are going to get a redirect, and we know that the response will have a
Location header of the actual URL we need to use. So the second call uses that URL.

## Handle the redirect on a call to alarms

Finally we can make our call to fetch the first page of alarms. Like our login call,
we are manually handling the redirect.

```csharp
// Let's attempt to retrieve the first page of alarms
var alarmsResponse = await client.GetAsync("alarms");


// Handle the redirect
alarmsResponse = await client.GetAsync(alarmsResponse.Headers.Location);
```

## Parse and Print the Response

Now we actually have a response from our `alarms` endpoint. We can parse it
and access one or more alarms. The response from the alarms endpoint contains
an `items` property that contains the list of alarms.

```csharp
// Parse the response using Newtonsoft and print the first one.
var alarmsObject = JObject.Parse(await alarmsResponse.Content.ReadAsStringAsync());
var alarms = alarmsObject["items"];
Console.WriteLine($"First alarm: {alarms?[0]}");
```

## Next Time

So now we know how to handle redirects and therefore we can make any call of the API.
But our solution is less than ideal. Having to make each call twice is less than
ideal.

In our next solution we'll rely on a NuGet package we provide that automates the
redirect handling for us.