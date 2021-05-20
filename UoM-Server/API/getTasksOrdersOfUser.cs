using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UoM_Server.Controllers;

namespace UoM_Server.API
{
    class getTasksOrdersOfUser
    {
        [FunctionName("getTasksOrdersOfUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Get information of a task.");
            string userUri = req.Query["userUri"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            userUri = userUri ?? data?.userUri;

            InRequestController irc = new InRequestController();
            string response = await Task.Run(() => irc.getTasksOrdersOfUser(userUri));

            string errorMessage = "Invalid request. Missing parameters. Parameters: userUri, orderId, moduleType, name, geometry, priority";
            string responseMessage = string.IsNullOrEmpty(userUri) 
                ? "{" + $"\"Success\":\"False\",\"Message\":\"{errorMessage}\"" + "}"
                : $" {response} ";
            return new OkObjectResult(responseMessage);
        }
    }
}
