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
    public static class getAllModule
    {
        [FunctionName("getAllModules")]
        public static async Task<IActionResult> Run(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
          ILogger log)
        {
            log.LogInformation("Get all module.");
            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            string start = req.Query["start"];
            string end = req.Query["end"];
            InRequestController irc = new InRequestController();
            string resp = await Task.Run(() => irc.getAllModule(int.Parse(start),int.Parse(end)));
            string respToFE;
            if (resp.ToLower().CompareTo("false") == 0 )
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
