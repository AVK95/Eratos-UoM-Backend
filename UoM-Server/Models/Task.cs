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
}
