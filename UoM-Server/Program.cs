using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServerGUI());
            //Console.WriteLine(new RequestHandler().CreateNewResource("https://schemas.eratos.ai/json/utas.climateinfo.csv", "average"));
            //Console.WriteLine(new RequestHandler().ShowResourceVersions(""));
            //Console.WriteLine(new RequestHandler().DeleteResource(""));
            //Console.WriteLine(new RequestHandler().FetchGeometry("uxuq4pgc54ylxrklc4gz25fd", "wkt","max"));

            //Console.WriteLine(RequestHandler.AUTHORISE_USER(Config.Access_Token));
        }
    }
}
