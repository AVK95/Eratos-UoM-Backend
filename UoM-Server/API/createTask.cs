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
    public static class createTask
    {
        [FunctionName("createTask")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Create a new task.");
                string userUri = req.Query["userUri"];
                string paymentID = req.Query["paymentID"];
                string price = req.Query["price"];
                string moduleType = req.Query["moduleType"];
                string taskType = req.Query["taskType"];
                string name = req.Query["name"];
                string geometry = req.Query["geometry"];
                string priority = req.Query["priority"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                userUri = userUri ?? data?.userUri;
                paymentID = paymentID ?? data?.paymentId;
                price = price ?? data?.price;
                moduleType = moduleType ?? data?.moduleType;
                name = name ?? data?.name;
                geometry = geometry ?? data?.geometry;
                priority = priority ?? data?.priority;
                taskType = taskType ?? data?.taskType;

                InRequestController irc = new InRequestController();

                string errorMessage = "Invalid request. Missing parameters. Parameters: userUri, paymentID, price, moduleType, taskType, name, geometry, priority";
                string responseMessage = (string.IsNullOrEmpty(userUri) || string.IsNullOrEmpty(paymentID) || string.IsNullOrEmpty(moduleType) ||
                    string.IsNullOrEmpty(name) || string.IsNullOrEmpty(geometry) || string.IsNullOrEmpty(priority) || string.IsNullOrEmpty(price))
                    ? "{" + $"\"Success\":\"False\",\"Message\":\"{errorMessage}\"" + "}"
                    : await Task.Run(() => irc.createTask(userUri, paymentID, price, moduleType, taskType, name, geometry, priority)) ;
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