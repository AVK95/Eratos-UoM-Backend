using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using EratosUoMBackend.Models;
using EratosUoMBackend.Controllers;

/* updateUserInfo is an API file of Azure Functions HTTP trigger for modifying the information for a specific user. 
It requires a JSON that contains the information that needs to be updated with the userURI for the database to query 
and update. It returns a response telling either the operation is successful or failed. */

namespace EratosUoMBackend
{
    class updateUserInfo
    {
        [FunctionName("updateUserInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Modify the information of a user.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                UserTable userTable = JsonConvert.DeserializeObject<UserTable>(requestBody);

                InRequestController irc = new InRequestController();
                string response = await Task.Run(() => irc.updateUserInfo(userTable));

                string responseMessage = (string.IsNullOrEmpty(requestBody))
                    ? "{" + $"\"Success\":\"False\",\"Message\":\"Error: Invalid User Info JSON.\"" + "}"
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
