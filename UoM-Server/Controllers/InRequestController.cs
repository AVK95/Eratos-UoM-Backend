using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UoM_Server.Models;
using System.Collections;

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
            DatabaseController dc = new DatabaseController();
            
            try
            {
                OutRequestController orc = new OutRequestController();
                string userName = orc.GetResource(user.info).name;

                UserTable userTable = Util.MAP_TO_TABLE(user, userName);

                dc.Connect();
                bool response = dc.CreateUser(userTable);
                dc.Disconnect();
                return "{" + $"\"Success\":\"True\",\"UserName\":\"{userName}\"" + "}";
            }
            catch (Exception e)
            {
                dc.Disconnect();
                return "{" + $"\"Success\":\"False\",\"Message\":\"Error: Create user failed. {e.Message}\"" + "}";
            }
            
        }

        public string getUserInfo(string userUri)
        {
            DatabaseController dc = new DatabaseController();
            try
            {
                dc.Connect();
                ArrayList UserInfoList = dc.FindUser("EratosUserID", userUri);
                if (UserInfoList.Count == 0)
                {
                    dc.Disconnect();
                    return "{" + $"\"Success\":\"False\",\"Message\":\"Error: User not found.\"" + "}";
                }
                UserTable userTable = (UserTable)UserInfoList[0];
                string userInfo = Util.WriteObjToJSON(userTable);
                dc.Disconnect();
                return "{" + $"\"Success\":\"True\",\"UserInfo\":\"{userInfo}\"" + "}";
            }
            catch (Exception e)
            {
                dc.Disconnect();
                return "{" + $"\"Success\":\"False\",\"Message\":\"Error: Failed getting user info. {e}\"" + "}";
            }
        }

        public string getAllUserInfo(int start, int end)
        {
            DatabaseController dc = new DatabaseController();
            try
            {
                dc.Connect();
                ArrayList userTableList = dc.FindUser(start, end);
                if (userTableList.Count == 0)
                {
                    dc.Disconnect();
                    return "{" + $"\"Success\":\"False\",\"Message\":\"Error: No users found.\"" + "}";
                }
                List<string> userInfoList = new List<string>();
                foreach (UserTable userTable in userTableList)
                {
                    userInfoList.Add(Util.WriteObjToJSON(userTable));
                }
                dc.Disconnect();
                return "{" + $"\"Success\":\"True\",\"UserInfo\":\"{string.Join("|", userInfoList)}\"" + "}";
            }
            catch (Exception e)
            {
                dc.Disconnect();
                return "{" + $"\"Success\":\"False\",\"Message\":\"Error: Failed getting user info. {e}\"" + "}";
            }
        }

        public string updateUserInfo(UserTable userTable)
        {
            DatabaseController dc = new DatabaseController();
            try
            {
                dc.Connect();
                UserTable oldUserTable = (UserTable)dc.FindUser("UserID", userTable.UserID.ToString())[0];
                if(oldUserTable.Name != userTable.Name)
                {
                    dc.UpdateUser(userTable.UserID, "Name", userTable.Name);
                }
                if (oldUserTable.Email != userTable.Email)
                {
                    dc.UpdateUser(userTable.UserID, "Email", userTable.Email);
                }
                if (oldUserTable.Info != userTable.Info)
                {
                    dc.UpdateUser(userTable.UserID, "Info", userTable.Info);
                }
                if (oldUserTable.isAdmin != userTable.isAdmin)
                {
                    dc.UpdateUser(userTable.UserID, "isAdmin", userTable.isAdmin.ToString());
                }

                dc.Disconnect();
                return "{" + "\"Success\":\"True\",\"Message\":\"The user info is up to date.\"" + "}";
            }
            catch(Exception e)
            {
                dc.Disconnect();
                return "{" + $"\"Success\":\"False\",\"Message\":\"Error: Failed to update user info. {e}\"" + "}";
            }
        }
        #endregion

        #region Module
        public string createModifyModule(string moduleName, string moduleSchema, string isActive)
        {
            DatabaseController dc = new DatabaseController();
            
            try
            {
                dc.Connect();
                ArrayList moduleList = dc.FindModule("moduleschema", moduleSchema);
                if (moduleList.Count == 0)
                {
                    ModuleTable mod = new ModuleTable(0, moduleName, moduleSchema, bool.Parse(isActive));
                    bool resp = dc.CreateModule(mod);
                }
                else
                {
                    ModuleTable mod = (ModuleTable)moduleList[0];
                    if (mod.ModuleName != moduleName) dc.UpdateModule(mod.ModuleID, "modulename", moduleName);
                    if (mod.ModuleSchema != moduleSchema) dc.UpdateModule(mod.ModuleID, "moduleschema", moduleSchema);
                }
            }
            catch (Exception e)
            {
                dc.Disconnect();
                return "{" + $"\"Success\": \"False\",\"Message\":\"Could not create/modify the module. {e.Message}\"" + "}";
            }
            dc.Disconnect();
            return "{" + "\"Success\": \"True\"" + "}";
        }

        public string getActiveModules()
        {
            DatabaseController dc = new DatabaseController();

            ArrayList resp = new ArrayList();
            List<string> moduleList = new List<string>();
            try
            {
                
                dc.Connect();
                resp = dc.FindModule("isactive", "true");
            }
            catch (Exception e)
            {
                dc.Disconnect();
                return "{" + $"\"Success\": \"False\",\"Message\":\"Failed to get active modules. {e.Message}\"" + "}";
            }
            foreach (ModuleTable module in resp)
            {
                moduleList.Add(Util.WriteObjToJSON(module));
            }
            dc.Disconnect();
            return resp.Count == 0 
                ? "{" + "\"Success\": \"False\",\"Message\":\"No active module.\"" + "}"
                : "{" + $"\"Success\": \"True\",\"Modules\":\"{string.Join("|", moduleList)}\"" + "}";
        }

        public string getAllModules(int start, int end)
        {
            DatabaseController dc = new DatabaseController();

            ArrayList resp = new ArrayList();
            List<string> moduleList = new List<string>();
            try
            {
                dc.Connect();
                resp = dc.FindModule(start, end);
            }
            catch (Exception e)
            {
                dc.Disconnect();
                return "{" + $"\"Success\": \"False\",\"Message\":\"Database error. {e.Message}\"" + "}";
            }
            foreach (ModuleTable module in resp)
            {
                moduleList.Add(Util.WriteObjToJSON(module));
            }
            dc.Disconnect();
            return resp.Count == 0 
                ? "{" + "\"Success\": \"False\",\"Message\":\"No module has been created.\"" + "}"
                : "{" + $"\"Success\": \"True\",\"Modules\":\"{string.Join("|", moduleList)}\"" + "}";
        }

        #endregion

        #region Task & Order

        public string createTask(string userUri, string paymentID, string price, string moduleType, string taskType, string name, string geometry, string priority)
        {
            DatabaseController dc = new DatabaseController();
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

                GNTaskResponse taskResponse = orc.CreateNewTask(pri, resource.id, taskType);

                // Update database
                try
                {
                    dc.Connect();

                    resource.geo = geometry;

                    // Get user id
                    UserTable userTable = (UserTable)dc.FindUser("EratosUserID", userUri)[0];
                    int userID = userTable.UserID;
                    // Get Module id
                    ModuleTable moduleTable = (ModuleTable)dc.FindModule("ModuleSchema", resource.type)[0];
                    int moduleID = moduleTable.ModuleID;
                    // Update Resource
                    ResourceTable rscTable = Util.MAP_TO_TABLE(resource);
                    bool rscResponse = dc.CreateResource(rscTable);
                    int rscID = ((ResourceTable)dc.FindResource("EratosResourceID", rscTable.EratosResourceID)[0]).ResourceID;
                    // Update Order
                    OrderTable orderTable = new OrderTable(0, float.Parse(price), "Pending", DateTime.Now.ToString(), userID, paymentID);
                    bool ordResponse = dc.CreateOrder(orderTable);
                    int ordID = ((OrderTable)dc.FindOrder("PaymentID", paymentID)[0]).OrderID;
                    // Update Task
                    TaskTable taskTable = Util.MAP_TO_TABLE(taskResponse, userID, ordID, rscTable.Name);
                    bool taskSucceeded = dc.CreateTask(taskTable);
                    int taskID = ((TaskTable)dc.FindTask("EratosTaskID", taskTable.EratosTaskID)[0]).TaskID;
                    // Update Association
                    bool associationResponse1 = dc.CreateResourceTaskAssociation(rscID, taskID);
                    bool associationResponse2 = dc.CreateResourceModuleAssociation(rscID, moduleID);
                }
                catch(Exception e)
                {
                    dc.Disconnect();
                    return "{" + $"\"Success\":\"False\",\"Message\":\"Database error. {e.Message}\"" + "}";
                }
                dc.Disconnect();
                return "{" +$"\"Success\":\"True\",\"TaskID\":\"{taskResponse.id}\"" + "}";
            }
            catch(Exception e)
            {
                return "{" + $"\"Success\":\"False\",\"Message\":\"Error: Create task failed. {e.Message}\"" + "}";
            }
            
        }

        public string getTasksOrdersOfUser(string userUri)
        {
            DatabaseController dc = new DatabaseController();
            try
            {
                dc.Connect();
                UserTable userTable = (UserTable)dc.FindUser("EratosUserID", userUri)[0];
                ArrayList taskTableList = dc.FindTask("UserID", userTable.UserID.ToString());
                ArrayList orderTableList = dc.FindOrder("UserID", userTable.UserID.ToString());
                
                List<string> taskResultList = new List<string>();
                List<string> orderResultList = new List<string>();
                foreach (TaskTable taskTable in taskTableList)
                {
                    taskResultList.Add(Util.WriteObjToJSON(taskTable));
                }
                foreach (OrderTable orderTable in orderTableList)
                {
                    orderResultList.Add(Util.WriteObjToJSON(orderTable));
                }
                dc.Disconnect();
                return "{" + $"\"Success\":\"True\",\"Tasks\":\"{string.Join("|",taskResultList)}\",\"Orders\":\"{string.Join("|",orderResultList)}\"" + "}";
            }
            catch(Exception e)
            {
                dc.Disconnect();
                return "{" + $"\"Success\":\"False\",\"Message\":\"Error: Get task failed. {e.Message}\"" + "}";
            }
        }

        public string syncTasksAndOrders()
        {
            OutRequestController orc = new OutRequestController();
            bool success = orc.SyncTasksAndOrders();
            return success
                ? "{" + "\"Success\":\"True\",\"Message\":\"Database is up to date.\"" + "}"
                : "{" + "\"Success\":\"False\",\"Message\":\"Sync error.\"" + "}";
        }
        #endregion
    }
}
