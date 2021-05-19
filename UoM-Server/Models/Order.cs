using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UoM_Server.Models
{
    public struct OrderTable
    {
        public int OrderID { get; set; }
        public float Price { get; set; }
        public string Status { get; set; }
        public DateTime OrderTime { get; set; }
        public int UserID { get; set; }

        public OrderTable(int orderID, float price, string status, DateTime orderTime, int userID)
        {
            OrderID = orderID;
            Price = price;
            Status = status;
            OrderTime = orderTime;
            UserID = userID;
        }
    }
}
