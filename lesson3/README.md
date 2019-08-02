# Lesson 3

In this lesson we'll implement some model classes and illustrate how they
can make the API easier to use.

Dependencies:

* .Net Core
* Netwonsoft.Json
* Flurl.Http

The code for this program is in [Program.cs](./Program.cs).

Like all the other lessons you should be able to run the app.

Note: This app assumes you have at least 105 alarms as it attempts to fetch the
second page of alarms. If you don't have that many alarms you can modify the program
to not fetch the second page.

```shell
$ dotnet run  username password hostname
alarms: 6537248
First alarm: {
  "Id": "f7e76e3c-193d-4256-9231-286128080fac",
  "ItemReference": "thesun:Granymede420B",
  "Name": "Granymede420B",
  "Message": "",
  "Object": "/objects/0d37204f-5418-53c5-867f-f2e66a542772",
  "Priority": 200,
  "IsAcknowledged": false,
  "CreationTime": "2018-10-25T00:51:19Z"
}
Item reference of 3rd alarm: thesun:Granymede4219
Name of object in second alarm on second page $DPR-O
```

## Create Model Classes

Model classes are classes that represent our data. For this example we'll create
classes related to login and alarms.

All the model classes can be found in [Alarm.cs](./Alarm.cs).

The first one is Login. It has the two properties we need for logging in. The only
difference is in the casing. We use the standard naming convention for C#. The framework
we use will make sure that the casing is converted to standard JSON casing (eg. `username`, `password`).

```csharp
public class Login
{
    public string Username { get; set; }
    public string Password { get; set; }
}
```

The next class represent the response we get back from the server.

```csharp
public class LoginResponse
{
    public string AccessToken { get; set; }
}
```

Next we define the Alarm class. (**Note:** If you look at the API the server
returns more properties than what we have shown. That's okay. Those will just be ignored.
If you want to include some of those properties you can go ahead and add them.)
The Alarm class represents one alarm in the collection of alarms returned by the server.

```csharp
public class Alarm
{
    public Guid Id { get; set; }
    public string ItemReference { get; set; }
    public string Name { get; set; }
    public string Message { get; set; }
    public string @Object { get; set; }
    public int Priority { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime CreationTime { get; set; }
}
```

Note that Id is returned as a string in the API but we defined it as a GUID here. The framework converts it for us.

And finally we have the AlarmCollection class. This represents the actual payload returned
by the server when you ask for alarms. In addition to a collection of alarms it includes
properties for Total count and the Next, Previous and Self links.

```csharp
public class AlarmCollection
{
    private string _next;
    private string _previous;
    private string _self;

    public int Total { get; set; }

    private static string StripLeadingSlash(string value)
    {
        return string.IsNullOrEmpty(value) || value[0] != '/' ? value : value.Substring(0);
    }

    public string Next
    {
        get => _next;
        set => _next = StripLeadingSlash(value);
    }

    public string Previous
    {  
        get => _previous;
        set => _previous = StripLeadingSlash(value);
    }

    public string Self
    {
        get => _self;
        set => _self = StripLeadingSlash(value);
    }

    public IList<Alarm> Items { get; set; }

    public Alarm this[int index] => Items[index];
}
```

The previous classes were all simple objects. That is they didn't contain any logic at all.
They were simply containers for data. This class demonstrates how you can add a little bit of
logic to make your classes easier to work with.

The first thing to point out is that the `Next`, `Previous`, and `Self` properties contain some validation logic. The links
returned by the server include leading slashes  (eg. "/alarms?page=2"). When we use our FlurlClient
we don't want that leading slash. So our setters check for that slash and remove it.
We'll see later how this makes it very easy to then fetch the next page.

The other bit of logic we added was to add an indexer on AlarmCollection. This allows us to more
easily access an alarm. Assuming `alarmCollection` is an instance of `AlarmCollection` we could
access the alarm at index 5 with `alarmCollection[5]` instead of `alarmCollection.Items[5]`.

These are just two simple ideas of how you can make it easier to work with the API by creating
model classes.

## Use Flurl Generic Methods to Deserialize JSON

Now that we have the model classes we can use them with Flurl.

This program is very similar to the last lesson but we will not have
to work with dynamic objects or JTokens. We'll only be working with our model
classes.

```csharp
string accessToken;
using (var client = new FlurlClient($"https://{hostname}/api/v2"))
{
    var loginObject = new Login
    {
        Username = username,
        Password = password
    };
    accessToken = (await client.Request("login").PostJsonAsync(loginObject)
        .ReceiveJson<LoginResponse>()).AccessToken;
}
```

This section looks very much like Lesson 2. The only difference is
that instead of using an anonymous object we are using the Login class.
Flurl and Newtonsoft take care of serializing that object to JSON.

The response comes back now as a `LoginResponse` rather than as a dynamic object. And
we use the `AccessToken` property we defined to retrieve the access token.

Next we'll request some alarms:

```csharp
using (var client = new FlurlClient($"https://{hostname}/api/v2").WithOAuthBearerToken(accessToken))
{
    var alarms = await client.Request("alarms").GetJsonAsync<AlarmCollection>();

    Console.WriteLine($"Total alarms: {alarms.Total}");
    Console.WriteLine($"First alarm: {JsonConvert.SerializeObject(alarms[5], Formatting.Indented)}");
    Console.WriteLine($"Item reference of 3rd alarm: {alarms.Items[2].ItemReference}");


    var secondPageOfAlarms = await client.Request(alarms.Next).GetJsonAsync<AlarmCollection>();
    Console.WriteLine($"Name of object in second alarm on second page ${secondPageOfAlarms[1].Name}");
}
```

When we call `GetJsonAsync<AlarmCollection>` Flurl will make sure we get an
instance of `AlarmCollection` and then we can access the properties on that
class.

We do this to print the total # of alarms `alarms.Total` or
access the alarm with index 2 `alarms.Items[2]`.

We also show an example of using the `Next` property. As discussed above this property
helps us by stripping the leading `/` returned by the server. That way
we can retrieve the second page of alarms using that property.

Finally we show an example of using the indexer directly on the alarm collection
`secondPageOfAlarms[1].Name` which is just short hand for
`secondPageOfAlarms.Items[1].Name`.
