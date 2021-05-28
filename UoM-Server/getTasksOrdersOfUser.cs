using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EratosUoMBackend.Controllers;

/*getTasksOrdersOfUsers is an API file for fetching all task and order history of a specific user. The API takes a single parameter userUri. */

namespace EratosUoMBackend
{
    class getTasksOrdersOfUser
    {
        [FunctionName("getTasksOrdersOfUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Get information of a task.");
                string userUri = req.Query["userUri"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                userUri = userUri ?? data?.userUri;

                InRequestController irc = new InRequestController();

                string errorMessage = "Invalid request. Missing parameters. Parameters: userUri";
                string responseMessage = string.IsNullOrEmpty(userUri)
                    ? "{" + $"\"Success\":\"False\",\"Message\":\"{errorMessage}\"" + "}"
                    : await Task.Run(() => irc.getTasksOrdersOfUser(userUri));

                return new OkObjectResult(responseMessage);
            }
            catch
            {
                string responseMessage = "Server error. Please contact the administrator.";
                return new BadRequestObjectResult(responseMessage);
            }
        }
    }
}
