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
    public static class createModifyModule
    {
        [FunctionName("createModifyModule")]
        public static async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
           ILogger log)
        {
            try
            {
                log.LogInformation("Create new module.");
                string schema = req.Query["moduleSchema"];
                string moduleName = req.Query["moduleName"];
                string isActive = req.Query["isActive"];

                string responseMessage;
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(isActive))
                {
                    responseMessage = "{" + "\"Success\": \"False\",\"Message\":\"Missing parameters. Parameters: schema, moduleName, isActive\"" + "}";
                }
                else
                {
                    // Functions to call eratos server
                    InRequestController irc = new InRequestController();
                    responseMessage = await Task.Run(() => irc.createModifyModule(moduleName, schema, isActive));
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
