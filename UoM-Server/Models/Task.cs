using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UoM_Server.Models
{
    enum Priority
    {
        Low,
        Normal,
        High
    }


    public class TokenNodeResponse
    {
        public string token { get; set; }
    }
    public class GNTaskMeta
    {
        public string resource { get; set; }
    }
    public class GNTaskRequest
    {
        public string priority { get; set; }
        public string type { get; set; }
        public GNTaskMeta meta { get; set; }

    }
    public class GNTaskResponse
    {

        public string id { get; set; }
        public string createdAt { get; set; }
        public string lastUpdatedAt { get; set; }
        public string startedAt { get; set; }
        public string endedAt { get; set; }
        public string priority { get; set; }
        public string state { get; set; }
        public string type { get; set; }
        public string error { get; set; }
        public GNTaskMeta meta { get; set; }

    }

    public struct TaskTable
    {
        public int TaskID { get; set; }
        public string EratosTaskID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime EndedAt { get; set; }
        public string Priority { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public string Meta { get; set; }
        public string Error { get; set; }
        public int UserID { get; set; }
        public int OrderID { get; set; }

        public TaskTable(int taskID, string eratosTaskID, DateTime createdAt, DateTime lastUpdatedAt, DateTime startedAt, DateTime endedAt, string priority, string state, string type, string meta, string error, int userID, int orderID)
        {
            TaskID = taskID;
            EratosTaskID = eratosTaskID;
            CreatedAt = createdAt;
            LastUpdatedAt = lastUpdatedAt;
            StartedAt = startedAt;
            EndedAt = endedAt;
            Priority = priority;
            State = state;
            Type = type;
            Meta = meta;
            Error = error;
            UserID = userID;
            OrderID = orderID;
        }
    }

}
