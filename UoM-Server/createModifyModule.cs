using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EratosUoMBackend.Controllers;

/* createModifyModule file is Azure Functions HTTP trigger for routing the creating module & modifying module. 
The API receives same query parameter and check whether there’s a schema exist in database and 
create a new one if there’s none; for module modification, it will check each variable in Module object and 
set the object value if there’s a difference */

namespace EratosUoMBackend
{
    public static class createModifyModule
    {
        [FunctionName("createModifyModule")]
        public static async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
           ILogger log)
        {
            try
            {
                log.LogInformation("Create new module.");
                string schema = req.Query["moduleSchema"];
                string moduleName = req.Query["moduleName"];
                string isActive = req.Query["isActive"];
                string description = req.Query["Description"];

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                schema = schema ?? data?.moduleSchema;
                moduleName = moduleName ?? data?.moduleName;
                isActive = isActive ?? data?.isActive;
                description = description ?? data?.Description;

                string responseMessage;
                if (string.IsNullOrEmpty(schema) || string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(isActive) || string.IsNullOrEmpty(description))
                {
                    responseMessage = "{" + "\"Success\": \"False\",\"Message\":\"Missing parameters. Parameters: moduleSchema, moduleName, isActive, Description\"" + "}";
                }
                else
                {
                    // Functions to call eratos server
                    InRequestController irc = new InRequestController();
                    responseMessage = await Task.Run(() => irc.createModifyModule(moduleName, schema, isActive, description));
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
