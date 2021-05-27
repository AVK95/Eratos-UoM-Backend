using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EratosUoMBackend.Controllers;

namespace EratosUoMBackend
{
    class getUserInfo
    {
        [FunctionName("getUserInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Get the information of a user or all users.");
                string userUri = req.Query["userUri"];
                string start = req.Query["start"];
                string end = req.Query["end"];

                string responseMessage;
                if (string.IsNullOrEmpty(userUri))
                {
                    responseMessage = "{" + $"\"Success\":\"False\",\"Message\":\"Missing parameters. Parameters: 1. userUri(can be userUri or all), if userUri=all, then 2. start, 3. end\"" + "}";
                }
                else
                {
                    InRequestController irc = new InRequestController();
                    responseMessage = userUri.ToLower() == "all"
                        ? await Task.Run(() => irc.getAllUserInfo(int.Parse(start), int.Parse(end)))
                        : await Task.Run(() => irc.getUserInfo(userUri));
                }

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
