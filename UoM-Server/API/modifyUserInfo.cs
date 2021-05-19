using System.IO;
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
    class modifyUserInfo
    {

        [FunctionName("modifyUserInfo")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Modify the information of a user.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic user = JsonConvert.DeserializeObject<User>(requestBody);

            InRequestController irc = new InRequestController();
            string response = await Task.Run(() => irc.modifyUser(user));

            string responseMessage = (string.IsNullOrEmpty(user))
                ? "Please pass a valid userInfo JSON."
                : $" {response} ";
            return new OkObjectResult(responseMessage);
        }
    }
}
