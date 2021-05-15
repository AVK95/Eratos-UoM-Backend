using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UoM_Server.Models;
using UoM_Server.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace UoM_Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    class InRequestController : ControllerBase
    {
        [HttpGet("/module")]
        public String Get()
        {
            var module = ModuleService.getAllActiveModule();

            return module.ToArray().ToString();
        }
    }

   
}
