﻿using System.IO;
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
    class userRegister
    {

        [FunctionName("userRegister")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Register a new user.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            User data = JsonConvert.DeserializeObject<User>(requestBody);

            InRequestController irc = new InRequestController();
            string response = await Task.Run(() => irc.createUser(data));

            string errorMessage = "Invalid request. Missing payload. Format: User JSON";
            string responseMessage = (string.IsNullOrEmpty(requestBody))
                ? "{" + $"\"Success\":\"False\",\"Message\":\"{errorMessage}\"" + "}"
                : $" {response} ";
            return new OkObjectResult(responseMessage);
        }

    }
}
