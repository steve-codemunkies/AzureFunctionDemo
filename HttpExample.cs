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

namespace My.Functions
{
    public static class HttpExample
    {
        [FunctionName("HttpExample")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
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

            return new OkObjectResult(stringBuilder.ToString());
        }
    }
}
