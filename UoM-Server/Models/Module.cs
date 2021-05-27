namespace EratosUoMBackend.Models
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
