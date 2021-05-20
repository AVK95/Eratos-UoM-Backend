using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UoM_Server.Models;
using UoM_Server.Controllers;

namespace UoM_Server.API
{
    class getUserInfo
    {
        [FunctionName("getUserInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Get the information of a user or all users.");
            string userUri = req.Query["userUri"];
            string start = req.Query["start"];
            string end = req.Query["end"];

            InRequestController irc = new InRequestController();
            string response = "";
            if (userUri.ToLower() == "all")
            {
                response = await Task.Run(() => irc.getAllUserInfo(int.Parse(start), int.Parse(end)));
            }
            else
            {
                response = await Task.Run(() => irc.getUserInfo(userUri));
            }

            string responseMessage = (string.IsNullOrEmpty(userUri))
                ? "{" + $"\"Success\":\"False\",\"Message\":\"Error: Invalid userUri.\"" + "}"
                : $" {response} ";
            return new OkObjectResult(responseMessage);
        }
    }
}
