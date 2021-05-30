using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EratosUoMBackend.Controllers;

/* getAllModules file is Azure Functions HTTP trigger for getting all modules. It can only be accessed by a User with isAdmin value: true. 
It requires 2 integer query parameter that indicates the starting index and ending index of the module being requested. */

namespace EratosUoMBackend
{
    public static class removeTask
    {
        [FunctionName("removeTask")]
        public static async Task<IActionResult> Run(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = null)] HttpRequest req,
          ILogger log)
        {
            try
            {
                log.LogInformation("Delete a task and associations.");

                string taskID = req.Query["taskID"];

                string responseMessage;
                if (string.IsNullOrEmpty(taskID))
                {
                    responseMessage = "{" + "\"Success\": \"False\",\"Message\":\"Missing parameters. Parameters: taskID\"" + "}";
                }
                else
                {
                    InRequestController irc = new InRequestController();
                    responseMessage = await Task.Run(() => irc.removeTask(int.Parse(taskID)));
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
