using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using UoM_Server.Controllers;

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

            new Test().testFunctions();
            //Console.WriteLine(new OutRequestController().DeleteResource("2tmmtwkry6t7ghtegx6re3vm"));
            //string deleteResponse = new OutRequestController().RemoveTask("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJhdWQiOiJlcmF0b3MtYXBpIiwiZXhwIjoxNjIxMDA2MjU4LCJpYXQiOjE2MjEwMDU2NTgsImlzcyI6Imh0dHBzOi8vc3RhZ2luZy5lLXBuLmlvIiwibmJmIjoxNjIxMDA1NjU4LCJzdWIiOiJodHRwczovL3N0YWdpbmcuZS1wbi5pby91c2Vycy94c3J1ZGV1bGZ2dHIyZWlhaWtzaWMyamYifQ.AOb9Q56_u2PoZGDNyftDfaO6PpgGtSyuLy5LGGlKdOAnNZHQrYD3eyxYklvYQTlpElBzcw8NhPsplQmDVMDouKjTZ9GGuT07M8aUmrTOtGqJkDDvvu8vK6bXLUst1PdXr6c_Riv9fcEhenpm-_8ASP8lVNsVUcLXmuKenV5ws072CmI1x--JqZwrIZtjRUGU13_CkUJMqO4SJJr2zKpDk1hDjNlDygV9Xo0zKR2lykXsyYWc2lQSq12ioYWYAbO-EuQgGpUHq4_i8CFTlMIzZYbHmdCAN8_BfsNxMi00SPKMskLud5ZNBDPxWohJcvpqp16oU8J-HiM69FgJWctBa5eu9bZm3ihtNcyIkRQEi2h6-umwJ0FtGGZe_KGqDKwbfFtvMs9T3TqUVwU_NvbzEgrsvWcrbBATFAaFjUHN8I7TwjccyvOwdEBuztsYkXwLgtgsw_o-r7s7TwXGQH9z6akPI0esmlq4qtT0JbFCtd9j4rUrFR0EMjXNW8D9Z_UjAdDqEts-lehQjy3-ANR1pjUIIq5U7gS908D3IlPkpq2VpP8T_YhPOQRK0VHwcieuAjs80Wwy8b_bG7GL58gtR4klu7c6S7VHP6j6PBS2QWTQ1l9fw7aG3LiD-3W_OnRZkG1hvG-IV5pmz8aZNyE--SiIdVodO-Nlp3Jd0x56blk", "rrcdjtek6da7e7l67mpmih2q");
            //Console.WriteLine(deleteResponse);
            //Console.WriteLine(orc.FetchGeometry("bqgyzn46aqq37q3jr7pjhofl","wkt","max"));
        }
    }
}
