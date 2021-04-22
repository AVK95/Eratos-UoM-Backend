using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace UoM_Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new ServerGUI());
        //}
        static void Main(String[] args)
        {
           
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 8888);
            serverSocket.Bind(iPEndPoint);
            serverSocket.Listen(0); // unlimited
            Socket clientSocket = serverSocket.Accept(); //receive client connection

            //server send msg to client
            String msg = "hello client! ";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);

            //receive the msg
            byte[] buffer = new byte[1024];
            int count = clientSocket.Receive(buffer);

            string Receivemsg = System.Text.Encoding.UTF8.GetString(buffer,0, count);
            Console.WriteLine(Receivemsg);

            Console.ReadKey();

            clientSocket.Close();
            serverSocket.Close();

        }
    }
}
