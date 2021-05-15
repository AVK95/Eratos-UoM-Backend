using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UoM_Server.Models;
using System.Threading.Tasks;

namespace UoM_Server.Services
{

    public class ModuleService
    {
        private static List<ModuleList> AllModules { get; }
        static ModuleService()
        {
            var AllModules = new List<ModuleList>();
        }
        public static List<ModuleList> getAllActiveModule() => AllModules.FindAll(x => x.isAvailable == "False");
        public static List<ModuleList> getAllModule() => AllModules;
    }
}
