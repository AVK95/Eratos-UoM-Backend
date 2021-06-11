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
    class forceSync
    {
        [FunctionName("forceSync")]
        public static async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
           ILogger log)
        {
            try
            {
                log.LogInformation("Sync tasks and orders.");

                string responseMessage;

                InRequestController irc = new InRequestController();
                responseMessage = await Task.Run(() => irc.syncTasksAndOrders());

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