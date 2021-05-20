using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UoM_Server.Controllers;

namespace API
{
    public static class getResourceVersion
    {
        [FunctionName("getResourceVersion")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Get versions of a resource.");
            string resourceUri = req.Query["resourceUri"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            resourceUri = resourceUri ?? data?.resourceUri;
            // Functions to call eratos server
            InRequestController irc = new InRequestController();
            string versions = await Task.Run(() => irc.getResourceVersion(resourceUri));

            string errorMessage = "Please pass a valid resourceUri (not a resource id)";
            string responseMessage = (string.IsNullOrEmpty(resourceUri))
                ? "{" + $"\"Success\":\"False\",\"Message\":\"{errorMessage}\"" + "}"
                : $" {versions} ";
            return new OkObjectResult(responseMessage);
        }
    }
}