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
    namespace UoM_Server.API
    {
        public static class getActiveModules
        {
            [FunctionName("getActiveModules")]
            public static async Task<IActionResult> Run(
              [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
              ILogger log)
            {
                log.LogInformation("Get active modules.");

                InRequestController irc = new InRequestController();
                string resp = await Task.Run(() => irc.getActiveModules());
                string respToFE;
                if (resp.ToLower().CompareTo("false") == 0)
                {
                    respToFE = "{" + "\"Success\": \"False\",\"Modules\":\"\"" + "}";
                }
                else
                {
                    respToFE = "{" + $"\"Success\": \"True\",\"Modules\":\"{resp}\"" + "}";
                }
                return new OkObjectResult(respToFE);
            }
        }
    }
}
