﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UoM_Server.Models;

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


        #endregion

        #region Order


        #endregion

        #region Module


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
