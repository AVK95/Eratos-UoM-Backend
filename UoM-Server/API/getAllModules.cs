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
    public static class getAllModules
    {
        [FunctionName("getAllModules")]
        public static async Task<IActionResult> Run(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
          ILogger log)
        {
            try
            {
                log.LogInformation("Get all modules.");

                string start = req.Query["start"];
                string end = req.Query["end"];

                string responseMessage;
                if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                {
                    responseMessage = "{" + "\"Success\": \"False\",\"Message\":\"Missing parameters. Parameters: start, end\"" + "}";
                }
                else
                {
                    InRequestController irc = new InRequestController();
                    responseMessage = await Task.Run(() => irc.getAllModules(int.Parse(start), int.Parse(end)));
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