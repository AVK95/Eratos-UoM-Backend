using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace UoMClient
{
    public class Program
    {
        static void Main(String[] args)
        {
            
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Any, 3000));

            byte[] data = new byte[1024];
            int count = clientSocket.Receive(data);
            string msg = Encoding.UTF8.GetString(data, 0, count);

            Console.Write(msg);
            while (true)
            {
                string s = Console.ReadLine();
                clientSocket.Send(Encoding.UTF8.GetBytes(s));
            }


            Console.ReadKey();
            Console.Write("client");
            clientSocket.Close();
            

        }
    }
}