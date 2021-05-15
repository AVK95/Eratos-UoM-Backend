using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UoM_Server.Models;
using System.Threading.Tasks;

namespace UoM_Server.Services
{

    public static class ModuleService
    {
        private static List<ModuleList> AllModules { get; }
        static ModuleService()
        {
            AllModules = new List<ModuleList>();
        }
        public static List<ModuleList> getAllActiveModule() => AllModules;
    }
}
