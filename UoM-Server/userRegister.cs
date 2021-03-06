using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EratosUoMBackend.Controllers;
using EratosUoMBackend.Models;

/* userRegister is an API file of Azure Functions HTTP trigger for creating a new user with its information. 
It requires a JSON that contains the information of the user with the userURI for the database to 
query and create. It returns a response telling either the operation is successful (with the user name of the newly created user) or failed. */

namespace EratosUoMBackend
{
    class userRegister
    {
        [FunctionName("userRegister")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
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
            catch
            {
                string responseMessage = "Server error. Please contact the administrator.";
                return new BadRequestObjectResult(responseMessage);
            }
        }
    }
}
