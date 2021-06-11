using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using EratosUoMBackend.Controllers;

namespace EratosUoMBackend
{
    class syncTasksAndOrders
    {
        [FunctionName("syncTasksAndOrders")]
        public static async void Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation("Sync tasks and orders.");

                string responseMessage;

                InRequestController irc = new InRequestController();
                responseMessage = await Task.Run(() => irc.syncTasksAndOrders());
                log.LogInformation("Syncing occured successfully at " + System.DateTime.Now);                
            }
            catch
            {
                string responseMessage = "Server error. Please contact the administrator.";
                log.LogWarning(responseMessage);
            }
        }
    }
}
