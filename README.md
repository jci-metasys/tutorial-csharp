# tutorial-csharp

A tutorial for consuming Metasys Server API from C#

## Getting Started 

As an introduction, we'll use only what is available in the .Net framework.
We will be using .Net Core.





## Some Things to Remember

The following things are important to successfully consuming the API.

* You need to have a trusted certificate on your server. If your server has a 
self-signed certificate it is likely that your client computer doesn't trust it.
This will cause all web requests to fail. See TODO for information on how to overcome this issue.

* Every request of the API (other than login) requires an Authorization header. This header
needs to contain an `accessToken` that you can obtain by using the `login` endpoint.

* The APIs for Metasys Server 10.0 are all configured to redirect you to a different URL.
While the HttpClient from .Net can be configured to automatically follow redirects,
it strips off authorization header before doing that. You can manually handle this issue
or using a NuGet package we have written for your convenience: 
[HttpClientRedirectHandler](https://www.nuget.org/packages/HttpClientRedirectHandler/).
The examples in this repository all rely on this NuGet package.

* Everything is strings. Request messages, response messages, everything is
just strings. But the strings are JSON and you can use your favorite library
for dealing with JSON.
