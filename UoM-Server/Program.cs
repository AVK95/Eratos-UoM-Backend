using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace UoM_Server
{
    class Program
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
            Console.Write("server");
            startServerAsy();
            Console.ReadKey();
            Console.Write("server");

        }
        static void startServerAsy()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 3000);
            serverSocket.Bind(iPEndPoint);
            serverSocket.Listen(0); // unlimited
            serverSocket.BeginAccept(AcceptCallBack,serverSocket); //receive client connection
                                                                
        }
        static void AcceptCallBack(IAsyncResult asyncResult)
        {
            Socket serverSocket = asyncResult.AsyncState as Socket;
            Socket clientSocket = serverSocket.EndAccept(asyncResult);
            //server send msg to client
            String msg = "hello client! ";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(msg);
            clientSocket.Send(data);

            //receive the msg
            databuffer = new byte[1024];
            clientSocket.BeginReceive(databuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);

            serverSocket.BeginAccept(AcceptCallBack, serverSocket);

        }

        static byte[] databuffer = new byte[1024];

        static void ReceiveCallBack(IAsyncResult asyncResult)
        {
            Socket clientSocket = asyncResult.AsyncState as Socket;
            int count = clientSocket.EndReceive(asyncResult);
            string msg = System.Text.Encoding.UTF8.GetString(databuffer, 0, count);
            Console.WriteLine(msg);
            clientSocket.BeginReceive(databuffer, 0, 1024, SocketFlags.None, ReceiveCallBack, clientSocket);

        }

        void StartServersync() //static?
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddress = IPAddress.Parse("192.168.1.103");
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, 88);
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

            string Receivemsg = System.Text.Encoding.UTF8.GetString(buffer, 0, count);
            Console.WriteLine(Receivemsg);

            Console.ReadKey();

            clientSocket.Close();
            serverSocket.Close();
        }
    }
    }

