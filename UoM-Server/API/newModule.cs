﻿using System.IO;
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
    public static class newModule
    {
        [FunctionName("createModule")]
        public static async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
           ILogger log)
        {
            log.LogInformation("Create new module.");
           
            string moduleSchema = req.Query["moduleSchema"];
            string moduleName = req.Query["moduleName"];
            string isActive = req.Query["isActive"];
            // Functions to call eratos server
            InRequestController irc = new InRequestController();
            bool resp = await Task.Run(() => irc.createModule(moduleName,moduleSchema,isActive));

            string responseMessage = (resp)
                ? "{" + "\"Success\": \"True\"}" +"}"
                : "{" + "\"Success\": \"False\"}" + "}";

            return new OkObjectResult(responseMessage);
        }
    }
}
