using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections;
using UoM_Server.Models;
using Newtonsoft.Json;

namespace UoM_Server.Controllers
{
    class InRequestController
    {

        #region Resource
        public string getResourceVersion(string resourceUri)
        {
            OutRequestController orc = new OutRequestController();
            string versions = orc.ShowResourceVersions(resourceUri);
            return versions;
        }

        #endregion

        #region User

        public string createUser(User user)
        {
            OutRequestController orc = new OutRequestController();
            string userName = ((PersonResource)orc.GetResource(user.info)).name;
            UserTable userTable = Util.MAP_TO_TABLE(user, userName);
        }


        #endregion

        #region Order


        #endregion

        #region Module
        public bool createModule(string moduleName, string moduleSchema, string isActive)
        {
            bool resp = false;
                try
                {

               
                DatabaseController dc = new DatabaseController();
                ArrayList moduleList = dc.FindModule("moduleschema", moduleSchema);
                if (moduleList.Count == 0) {
                    ModuleTable mod = new ModuleTable(0, moduleName, moduleSchema, bool.Parse(isActive));
                    resp = dc.CreateModule(mod);
                }

                }
                catch (Exception e)
                {
                // Maybe write to logs
                Console.WriteLine(e);
                return resp;
                }

                return resp;
        }

        public bool modifyModule(string moduleId, string moduleName, string moduleSchema, string isActive)
        {
            bool resp = false;
            try
            {


                DatabaseController dc = new DatabaseController();
                ArrayList moduleList = dc.FindModule("moduleid", moduleId);
                if (moduleList.Count == 0)
                {
                    return resp;
                }
                else
                {
                    ModuleTable mod = (ModuleTable)moduleList[0];
                    if (mod.ModuleName.CompareTo(moduleName) != 0) dc.UpdateModule(mod.ModuleID, "modulename", moduleName);
                    if (mod.ModuleSchema.CompareTo(moduleSchema) != 0) dc.UpdateModule(mod.ModuleID, "moduleschema", moduleSchema);
                    if (!mod.isActive.Equals(bool.Parse(isActive))) dc.UpdateModule(mod.ModuleID, "isactive", isActive);
                    resp = true;
                }
            }
            catch (Exception e)
            {
                // Maybe write to logs
                Console.WriteLine(e);
                return resp;
            }

            return resp;
        }

        public string getActiveModule()
        {
            ArrayList resp = new ArrayList();
            List<string> moduleList = new List<string>();
            try
            {     
               DatabaseController dc = new DatabaseController();
              
               resp = dc.FindModule("isactive", "true");
            }
            catch (Exception e)
            {
                // Maybe write to logs
                Console.WriteLine(e);
                return "failed";
            }
            foreach (ModuleTable module in resp)
            {
                moduleList.Add(Util.WriteObjToJSON(module));
            }

            return resp.Count == 0 ? "failed" : string.Join("|",moduleList);
        }

        public string getAllModule(int start, int end)
        {
            ArrayList resp = new ArrayList();
            List<string> moduleList = new List<string>();
            try
            {
                DatabaseController dc = new DatabaseController();

                resp = dc.FindModule(start,end);
            }
            catch (Exception e)
            {
                // Maybe write to logs
                Console.WriteLine(e);
                return "failed";
            }
            foreach (ModuleTable module in resp)
            {
                moduleList.Add(Util.WriteObjToJSON(module));
            }

            return resp.Count == 0 ? "failed" : string.Join("|", moduleList);
        }

        #endregion

        #region Task

        public string createTask(string userUri, int orderId, string moduleType, string name, string geometry, string priority)
        {
            try
            {
                OutRequestController orc = new OutRequestController();
                
                Resource resource = orc.CreateResource(moduleType, name);
                string geoResponse = orc.UpdateGeometry(resource.id, geometry);

                Priority pri = Priority.Low;
                switch (priority.ToLower())
                {
                    case "low":
                        pri = Priority.Low;
                        break;
                    case "normal":
                        pri = Priority.Normal;
                        break;
                    case "high":
                        pri = Priority.High;
                        break;
                    default:
                        pri = Priority.Low;
                        break;
                }

                GNTaskResponse taskResponse = orc.CreateNewTask(pri, resource.id);

                // Update database
                try
                {
                    resource.geo = geometry;

                    DatabaseController dc = new DatabaseController();
                    // Get user id
                    UserTable userTable = (UserTable)dc.FindUser("EratosUserID", userUri)[0];
                    // Update Resource
                    ResourceTable rscTable = Util.MAP_TO_TABLE(resource);
                    bool rscResponse = dc.CreateResource(rscTable);
                    int rscId = ((ResourceTable)dc.FindResource("EratosResourceID", rscTable.EratosResourceID)[0]).ResourceID;
                    // Update Task
                    TaskTable taskTable = Util.MAP_TO_TABLE(taskResponse, userTable.UserID, orderId);
                    bool taskSucceeded = dc.CreateTask(taskTable);
                    int taskId = ((TaskTable)dc.FindTask("EratosTaskID", taskTable.EratosTaskID)[0]).TaskID;
                    // Update Association
                    bool associationResponse = dc.CreateResourceTaskAssociation(rscId, taskId);

                }
                catch(Exception e)
                {
                    // Maybe write to logs
                }

                return $"Successfully created task {taskResponse.id}.";
            }
            catch(Exception e)
            {
                return "Error: Create task failed";
            }
            
        }

        #endregion
    }
}
