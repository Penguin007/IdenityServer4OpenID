using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ClientCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                MainAsync().Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle((x) =>
                {
                    if (x is UnauthorizedAccessException) // This we know how to handle.
                    {
                        Console.WriteLine("You do not have permission to access all folders in this path.");
                        Console.WriteLine("See your network administrator or try another path.");
                        return true;
                    }
                    return false; // Let anything else stop the application.
                });
            }

        }
        static async Task MainAsync()
        {
            try
            {
                // discover endpoints from metadata
                var disco = await DiscoveryClient.GetAsync("http://localhost:5000");

                //request token
                //var tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
                //var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

                var tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
                var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("alice", "password", "api1");

                if (tokenResponse.IsError)
                {
                    Console.WriteLine(tokenResponse.Error);
                    return;
                }

                Console.WriteLine(tokenResponse.Json);
                Console.WriteLine("\n\n");

                //call api
                var client = new HttpClient();
                client.SetBearerToken(tokenResponse.AccessToken);

                var response = await client.GetAsync("http://localhost:5001/identity");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(JArray.Parse(content));
                }
            }
            catch (AggregateException ae)
            {
                ae.Handle((x) =>
                {
                    if (x is UnauthorizedAccessException) // This we know how to handle.
                    {
                        Console.WriteLine("You do not have permission to access all folders in this path.");
                        Console.WriteLine("See your network administrator or try another path.");
                        return true;
                    }
                    return false; // Let anything else stop the application.
                });
            }

        }
    }
}
