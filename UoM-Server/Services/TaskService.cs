using System;
using System.Collections.Generic;
using System.Linq;
using UoM_Server.Controllers;
using System.Text;
using System.Threading.Tasks;

namespace UoM_Server.Services
{
    public static class TaskService
    {
        static string token = "";
        static TaskService()
        {

        }

        public static void CreateTask(string userID, string orderID,
            string Moduletype, string name, string geometry, string priority)
        {
            
        }

        public static string CheckTaskStatus(string id)
        {
            return OutRequestController.GetTask(token, id).state;
        }
    }
}
