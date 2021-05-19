using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoM_Server.Controllers;
using UoM_Server.Models;

namespace UoM_Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new ServerGUI());

            // Code for testing request handler
            //new Test().testFunctions();
            OutRequestController orc = new OutRequestController();
            string response = Util.WriteObjToJSON<Resource>(orc.GetResource("https://staging.e-pn.io/resources/gu4km2nnimvyqepmaw3mfcwg"));
            Console.WriteLine(response);

            ////////Code for testing database handler/////////////////////////////
            DatabaseController dh = new DatabaseController();
            Console.WriteLine(dh.Connect());
            //bool response = dh.CreateUser(new UserTable(0, "demouser3", "demoemail3", "demoname3", "demoauth3", DateTime.Now, null));
            ArrayList entry = dh.FindUser("email", "demoemail3");
            UserTable myUser = (UserTable)entry[0];
            Console.WriteLine(myUser.UserID + " " + myUser.CreatedAt);

            //bool response = dh.CreateOrder(new OrderTable(0, (float)19.65, "Unpaid", DateTime.Now, 3));
            ArrayList orders = dh.FindOrder("userid", "3");
            foreach (OrderTable element in orders)
                Console.WriteLine(element.OrderID + " " + element.Price);

            //bool response = dh.CreateTask(new TaskTable(0, DateTime.Now, default, default, default, "Regular", "Ended", null, null, null, 1, 2));
            ArrayList tasks = dh.FindTask("userid", "1");
            foreach (TaskTable element in tasks)
                Console.WriteLine(element.TaskID + " " + element.CreatedAt + " " + element.OrderID);

            //bool response = dh.CreateModule(new ModuleTable(0, "module2_id", "module2", null, true));
            ArrayList modules = dh.FindModule("isactive", "false");
            foreach (ModuleTable element in modules)
                Console.WriteLine(element.ModuleID + " " + element.ModuleName);

            //bool response = dh.CreateResource(new ResourceTable(0, "resource1_id", default, "policy1", "geo1", null));
            ArrayList ress = dh.FindResource("policy", "policy1");
            foreach (ResourceTable element in ress)
                Console.WriteLine(element.ResourceID + " " + element.EratosResourceID + " " + element.Geo);

            //bool response = dh.CreateResourceTaskAssociation(1, 1);
            //bool response = dh.CreateResourceModuleAssociation(1, 1);
            Console.WriteLine(response);

            Console.ReadLine();
            dh.Disconnect();
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////

            //Console.WriteLine(new OutRequestController().DeleteResource("2tmmtwkry6t7ghtegx6re3vm"));
            //string deleteResponse = new OutRequestController().RemoveTask("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJlcmF0b3MtYXBpIiwiZXhwIjoxNjIxMDA2MjU4LCJpYXQiOjE2MjEwMDU2NTgsImlzcyI6Imh0dHBzOi8vc3RhZ2luZy5lLXBuLmlvIiwibmJmIjoxNjIxMDA1NjU4LCJzdWIiOiJodHRwczovL3N0YWdpbmcuZS1wbi5pby91c2Vycy94c3J1ZGV1bGZ2dHIyZWlhaWtzaWMyamYifQ.AOb9Q56_u2PoZGDNyftDfaO6PpgGtSyuLy5LGGlKdOAnNZHQrYD3eyxYklvYQTlpElBzcw8NhPsplQmDVMDouKjTZ9GGuT07M8aUmrTOtGqJkDDvvu8vK6bXLUst1PdXr6c_Riv9fcEhenpm-_8ASP8lVNsVUcLXmuKenV5ws072CmI1x--JqZwrIZtjRUGU13_CkUJMqO4SJJr2zKpDk1hDjNlDygV9Xo0zKR2lykXsyYWc2lQSq12ioYWYAbO-EuQgGpUHq4_i8CFTlMIzZYbHmdCAN8_BfsNxMi00SPKMskLud5ZNBDPxWohJcvpqp16oU8J-HiM69FgJWctBa5eu9bZm3ihtNcyIkRQEi2h6-umwJ0FtGGZe_KGqDKwbfFtvMs9T3TqUVwU_NvbzEgrsvWcrbBATFAaFjUHN8I7TwjccyvOwdEBuztsYkXwLgtgsw_o-r7s7TwXGQH9z6akPI0esmlq4qtT0JbFCtd9j4rUrFR0EMjXNW8D9Z_UjAdDqEts-lehQjy3-ANR1pjUIIq5U7gS908D3IlPkpq2VpP8T_YhPOQRK0VHwcieuAjs80Wwy8b_bG7GL58gtR4klu7c6S7VHP6j6PBS2QWTQ1l9fw7aG3LiD-3W_OnRZkG1hvG-IV5pmz8aZNyE--SiIdVodO-Nlp3Jd0x56blk", "rrcdjtek6da7e7l67mpmih2q");
            //Console.WriteLine(deleteResponse);
            //Console.WriteLine(orc.FetchGeometry("bqgyzn46aqq37q3jr7pjhofl","wkt","max"));


        }
    }
}
