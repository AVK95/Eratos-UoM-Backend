using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace UoM_Server
{
    class MultiThreadedServer
    {
        private TcpListener server = null;
        public const int Max_Message_Size = 256;

        //Creates a server at given IP - starts the main thread
        public MultiThreadedServer(string ipAddress, int port)
        {
            IPAddress address = IPAddress.Parse(ipAddress);
            server = new TcpListener(address, port);
            server.Start();
        }

        public void StopServer()
        {
            server.Stop();
        }

        //Starts a client thread
        private void StartClientThread()
        {
            try
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();

                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(client);
                }
            }
            catch (SocketException e)
            {
                server.Stop();
            }
        }

        //Client management code must go here
        private void HandleClient(Object oClient)
        {
            TcpClient client = (TcpClient)oClient;
            NetworkStream cStream = client.GetStream();
            string imei = String.Empty;

            Byte[] cBytes = new byte[Max_Message_Size];
            string data = null;
            int msgLength;

            try
            {
                while((msgLength = cStream.Read(cBytes, 0, cBytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(cBytes);
                    data = Encoding.ASCII.GetString(cBytes, 0, msgLength);

                    string replyMsg = "Hello, you sent: " + data;
                    Byte[] reply = System.Text.Encoding.ASCII.GetBytes(replyMsg);
                    cStream.Write(reply, 0, reply.Length);
                }
            }
            catch (Exception e)
            {
                client.Close();
            }
        }
    }
}
