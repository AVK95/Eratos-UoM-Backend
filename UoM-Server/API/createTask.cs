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
            log.LogInformation("Create a new task.");
            string userUri = req.Query["userUri"];
            string orderId = req.Query["orderId"];
            string moduleType = req.Query["moduleType"];
            string name = req.Query["name"];
            string geometry = req.Query["geometry"];
            string priority = req.Query["priority"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            userUri = userUri ?? data?.userUri;
            orderId = orderId ?? data?.orderId;
            moduleType = moduleType ?? data?.moduleType;
            name = name ?? data?.name;
            geometry = geometry ?? data?.geometry;
            priority = priority ?? data?.priority;

            InRequestController irc = new InRequestController();
            string response = await Task.Run(() => irc.createTask(userUri,int.Parse(orderId),moduleType,name,geometry,priority));

            string responseMessage = (string.IsNullOrEmpty(userUri) || string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(moduleType) || 
                string.IsNullOrEmpty(name) || string.IsNullOrEmpty(geometry) || string.IsNullOrEmpty(priority))
                ? "Invalid request. Missing parameters. Parameters: userUri, orderId, moduleType, name, geometry, priority"
                : $" {response} ";
            return new OkObjectResult(responseMessage);
        }
    }
}