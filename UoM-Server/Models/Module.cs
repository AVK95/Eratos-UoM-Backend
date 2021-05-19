using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UoM_Server.Models
{
    public struct ModuleTable
    {
        public int ModuleID { get; set; }
        public string ModuleName { get; set; }
        public string ModuleSchema { get; set; }
        public bool isActive { get; set; }

        public ModuleTable(int moduleID, string moduleName, string moduleSchema, bool isActive)
        {
            ModuleID = moduleID;
            ModuleName = moduleName;
            ModuleSchema = moduleSchema;
            this.isActive = isActive;
        }
    }
}
