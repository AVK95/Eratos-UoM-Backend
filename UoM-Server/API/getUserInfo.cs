using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UoM_Server.Controllers;
using UoM_Server.Models;

namespace UoM_Server.API
{
    class getUserInfo 
    {

        [FunctionName("getUserInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Get the information of a user or all users.");
            string userUri = req.Query["userUri"];

            InRequestController irc = new InRequestController();
            string response = "";
            if (userUri == "ALL")
            {
                response = await Task.Run(() => irc.getAllUserInfo());
            }
            else
            {
                response = await Task.Run(() => irc.getUserInfo(userUri));
            }

            string responseMessage = (string.IsNullOrEmpty(userUri))
                ? "Please pass a valid userUri."
                : $" {response} ";
            return new OkObjectResult(responseMessage);
        }
    }
}
