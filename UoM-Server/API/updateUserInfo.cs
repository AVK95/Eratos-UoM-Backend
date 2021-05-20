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
    class updateUserInfo
    {
        [FunctionName("updateUserInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
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
    }
}
