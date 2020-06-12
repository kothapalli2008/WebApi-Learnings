using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Linq;

namespace secureclient
{
    class Program
    {
        static void Main(string[] args)
        {  
            Console.WriteLine("Making the call, fingers crossed....");
            RunAsync().GetAwaiter()
                      .GetResult();
        }

        private static async Task RunAsync()
        {
            AuthConfig authConfig = AuthConfig.ReadJsonFromfile("appsettings.json");

            IConfidentialClientApplication application;
            application = ConfidentialClientApplicationBuilder.Create(authConfig.ClientId)
            .WithClientSecret(authConfig.ClientSecret)
            .WithAuthority(new Uri(authConfig.Authority))
            .Build();

            string[] Resourceids = new string[] {authConfig.ResourceId};
            AuthenticationResult result = null;

            try
            {
                result = await  application.AcquireTokenForClient(Resourceids).ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token Acquired \n");
                Console.WriteLine(result.AccessToken);
                Console.ResetColor();
            }
            catch (MsalClientException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            if (!string.IsNullOrEmpty(result.AccessToken))
            {
                var httpclient = new HttpClient();
                var defaultrequestHeaders = httpclient.DefaultRequestHeaders;

                if (defaultrequestHeaders == null || ! defaultrequestHeaders.Accept.Any
                (m => m.MediaType == "application/Json"))
                {
                    httpclient.DefaultRequestHeaders.Accept.Add(new 
                    MediaTypeWithQualityHeaderValue("application/Json"));   
                }

                defaultrequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
                
                HttpResponseMessage responseMessage = await httpclient.GetAsync(authConfig.BaseAddress);

                if (responseMessage.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    string json = await responseMessage.Content.ReadAsStringAsync();
                    Console.WriteLine(json);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"failed to call  API: {responseMessage.StatusCode}");
                    string content = await responseMessage.Content.ReadAsStringAsync();
                    Console.WriteLine($"Content:{content}");
                }
                
                Console.ResetColor();
                
            }           
        }
    }
}
