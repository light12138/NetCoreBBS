using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OauthCon
{
    class Program
    {
        static void Main(string[] args)
        {
            GuoDuAsync();
        }

        public static async void GuoDuAsync()
        {
            var disco = DiscoveryClient.GetAsync("http://localhost:5000").Result;
            // await RequestAsync();
            await Request1Async();
            Console.ReadKey();
        }

        public static async Task RequestAsync()
        {
            var disco =  DiscoveryClient.GetAsync("http://localhost:5000").Result;
            var tokenClient = new TokenClient(disco.TokenEndpoint, "client", "secret");
            var tokenResponse =  tokenClient.RequestClientCredentialsAsync("api1").Result;
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("开始访问api");
            Console.ReadKey();
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response =  client.GetAsync("http://localhost:5000/Values/Get").Result;
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content =  response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(JArray.Parse(content));
            }

        }

        public static async  Task Request1Async()
        {
            var disco = DiscoveryClient.GetAsync("http://localhost:5000").Result;
            var tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
            var tokenResponse = tokenClient.RequestResourceOwnerPasswordAsync("alice", "password", "api1").Result;
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");
        }
    }
}