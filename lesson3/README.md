# Lesson 3

In this lesson we do the same thing as Lesson 2 but we don't have
to manually handle redirects. This is accomplished by taking on one
more dependency. We'll add the HttpClientRedirectHandler developed by
Johnson Controls.

Dependencies:

* .Net Core
* Netwonsoft.Json
* [HttpClientRedirectHandler](https://www.nuget.org/packages/HttpClientRedirectHandler/) - source code at <https://github.com/metasys-server/redirect-handler/>

You should be able to run this lesson by supplying a username,
password, and host name. If everything works correctly the application
will print the first alarm from the list it receives from the server.

```shell
$ dotnet run username password hostname
First alarm: {
  "self": "/alarms/7cd18112-f7a6-4b0e-aeb7-2a5cea8b22b8",
  "id": "7cd18112-f7a6-4b0e-aeb7-2a5cea8b22b8",
  "itemReference": "thesun:Granymede421A/Field Bus MSTP1.VAV-08.DPR-O",
  "name": "DPR-O",
  "message": "",
  "isAckRequired": true,
  "type": "/enumSets/108/members/66",
  "priority": 70,
  "triggerValue": {
    "value": "0.0",
    "units": "/enumSets/507/members/98"
  },
  "creationTime": "2018-10-24T21:10:47Z",
  "isAcknowledged": false,
  "isDiscarded": false,
  "category": "/enumSets/33/members/5",
  "object": "/objects/b66917b0-7bfe-5e23-a25e-da35fc61b2b9",
  "annotations": "/alarms/7cd18112-f7a6-4b0e-aeb7-2a5cea8b22b8/annotations"
}
```

The contents of this program are all in [Program.cs](./Program.cs).

We made only slight changes to the Lesson 2 program.

## Use HttpClientRedirectHandler

In lesson 2 we used an HttpClientHandler to disable auto-redirects.
In this lesson we'll replace the HttpClientHandler with an
HttpClientRedirectHandler and then we will no longer need to manually handle each redirect.

Lesson 2 Code:

```csharp
var handler = new HttpClientHandler {AllowAutoRedirect = false};

using (var client = new HttpClient(handler) {BaseAddress = new Uri($"https://{hostname}/api/v1")})
{
  ...
}
```

The modified code

```csharp
var handler = new HttpClientRedirectHandler();

using (var client = new HttpClient(handler) {BaseAddress = new Uri($"https://{hostname}/api/v1")})
{
  ...
}
```

The only difference is the type of the handler we are creating.

The HttpClientRedirectHandler does the following:

1. It turns off auto redirects
2. It handles every redirect. It inspects the redirect URL and as long
   the hostname matches the hostname of the original request it automatically
   follows the redirect.
3. IT retains our authorization header on each request.

## Remove the Manual Handling of Redirects

The other two changes we need to make are to delete the two lines that manually
handled the redirects:

Lesson 2 Code (Each request requires two calls):

```csharp
var loginResponseMessage = await client.PostAsync("login", loginContent);

// Handle the redirect
loginResponseMessage = await client.PostAsync(loginResponseMessage.Headers.Location, loginContent);
```

```csharp
// Let's attempt to retrieve the first page of alarms
var alarmsResponse = await client.GetAsync("alarms");


// Handle the redirect
alarmsResponse = await client.GetAsync(alarmsResponse.Headers.Location);
```

Lesson 3 Code (We can accomplish each request with one call)

```csharp
var loginResponseMessage = await client.PostAsync("login", loginContent);
```

```csharp
var alarmsResponse = await client.GetAsync("alarms");
```

## Summary

After this lesson, we can now quite easily make API calls.

There are still some issues with our current implementation:

1. Manually reading string results after each call
2. Manually parsing results into JSON objects after each call

In Lesson 4 we'll explore two different ways to ease these issues using another
3rd party library named Flurl.Http.
