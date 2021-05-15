using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UoM_Server.Models;
using UoM_Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Text.Json;

namespace UoM_Server.Controllers
{
    public class TaskPost
    {
        public string userID { get; set; }
        public string orderID { get; set; }
        public string Moduletype { get; set; }
        public string name { get; set; }
        public string geometry { get; set; }
        public string priority { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    class InRequestController : ControllerBase
    {
        [HttpGet("/active-module")]
        public String GetActiveModule()
        {
            var module = ModuleService.getAllActiveModule();

            return module.ToArray().ToString();
        }
        [HttpGet("/allmodule")]
        public String GetAllModule()
        {
            var module = ModuleService.getAllModule();

            return module.ToArray().ToString();
        }

        [HttpPost("/task")]
        public String NewTask([FromBody] JsonElement json)
        {
            string body = System.Text.Json.JsonSerializer.Serialize(json);
            TaskPost taskcreate = JsonSerializer.Deserialize<TaskPost>(body);
            return "SOMETHING";
        }

        [HttpGet("/task/{id}")]
        public String GetTaskStatus(string id)
        {
            return "SOMETHING";
        }
        [HttpGet("/order/{id}")]
        public String getHistoryOrders(string id)
        {
            return "SOMETHING";
        }
        [HttpGet("/user/{id}")]
        public String GetUserDetail(string id)
        {
            return "SOMETHING";
        }
    }

   
}
