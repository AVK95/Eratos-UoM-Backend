using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UoM_Server.Models;

namespace UoM_Server.Controllers
{
    class InRequestController
    {
        public string getResourceVersion(string resourceUri)
        {
            OutRequestController orc = new OutRequestController();
            string versions = orc.ShowResourceVersions(resourceUri);
            return versions;
        }

        public string createTask(string userUri, string orderId, string moduleType, string name, string geometry, string priority)
        {
            try
            {
                OutRequestController orc = new OutRequestController();
                Resource resource = orc.CreateResource(moduleType, name);
                string geoResponse = orc.UpdateGeometry(resource.id, geometry);
                string token = orc.GetToken();
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

                GNTaskResponse taskResponse = orc.CreateNewTask(token, pri, resource.id);

                DatabaseController dc = new DatabaseController();
                dc.recordTask(taskResponse);
                return $"Successfully created task {taskResponse.id}.";
            }
            catch(Exception e)
            {
                return "Error: Create task failed";
            }
            
        }
    }
}
