using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using Azure.Storage.Blobs;
using System.Security.Claims;

namespace My.Functions
{
    public static class HttpExample
    {
        private static string StorageConnectionStringEnvVar = "AzureWebJobsStorage";
        private static string StorageContainerName = "www";
        private static string AuthorisedUserEnvVar = "SecretUser";
        private static string BlobUrlEnvVar = "BlobUrl";
        private static string ServicePrincipalUserNameHeader = "X-MS-CLIENT-PRINCIPAL-NAME";
        private static string Unauthorised = "/unauthed.html";
        private static string AuthorisedNonSecret = "/authed-nonsecret.html";
        private static string AuthorisedSecret = "/authed-secret.html";

        [FunctionName("HttpExample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var greeting = req.Query["greeting"];
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            var opening = string.IsNullOrEmpty(name) && string.IsNullOrEmpty(greeting)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : string.IsNullOrEmpty(greeting)
                    ? $"Hello, {name}. This HTTP triggered function executed successfully."
                    : string.IsNullOrEmpty(name)
                        ? $"{greeting}, person. This HTTP triggered function executed successfully."
                        : $"{greeting}, {name}. This HTTP triggered function executed successfully.";

            var stringBuilder = new StringBuilder(opening).Append(Environment.NewLine).Append(Environment.NewLine);

            foreach(var kvp in req.Headers)
            {
                stringBuilder = stringBuilder.Append(kvp.Key).Append(": ").Append(kvp.Value).Append(Environment.NewLine);
            }

            stringBuilder = stringBuilder.Append(Environment.NewLine);
            stringBuilder = stringBuilder.Append("Claims principal identity name: ").Append(claimsPrincipal.Identity.Name ?? "Unknown name").Append(Environment.NewLine);
            stringBuilder = stringBuilder.Append("Claims principal identity is authenticated: ").Append(claimsPrincipal.Identity.IsAuthenticated.ToString()).Append(Environment.NewLine);

            foreach(var claim in claimsPrincipal.Claims)
            {
                stringBuilder = stringBuilder.Append("Claim: ").Append(claim.Type).Append("; Value: ").Append(claim.Value).Append(Environment.NewLine);
            }

            return new OkObjectResult(stringBuilder.ToString());
        }

        [FunctionName("FetchHtml")]
        public static async Task<IActionResult> FetchHtml([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest request, ClaimsPrincipal claimsPrincipal, ILogger log)
        {
            var secretUser = GetEnvironmentVariable(AuthorisedUserEnvVar);

            var requestUrl = "";

            if(!claimsPrincipal.Identity.IsAuthenticated)
            {
                requestUrl = $"{GetEnvironmentVariable(BlobUrlEnvVar)}{Unauthorised}";
            }
            else if(string.Compare(secretUser, claimsPrincipal.Identity.Name, StringComparison.OrdinalIgnoreCase) == 0)
            {
                requestUrl = $"{GetEnvironmentVariable(BlobUrlEnvVar)}{AuthorisedSecret}";
            }
            else
            {
                requestUrl = $"{GetEnvironmentVariable(BlobUrlEnvVar)}{AuthorisedNonSecret}";
            }

            log.LogInformation("Requesting blob {requestUrl}", requestUrl);

            var output = "";
            using (var memoryStream = new MemoryStream())
            {
                await new BlobClient(new Uri(requestUrl)).DownloadToAsync(memoryStream);
                output = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            
            return new ContentResult{Content = output, ContentType = "text/html", StatusCode = 200};
        }

        private static string GetEnvironmentVariable(string name) => System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
    }
}
