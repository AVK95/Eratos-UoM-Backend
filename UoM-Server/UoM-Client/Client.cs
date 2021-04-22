using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace UoMClient
{
    public class Program
    {
        static void Main(String[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8888));

            byte[] data = new byte[1024];
            int count = clientSocket.Receive(data);
            string msg = Encoding.UTF8.GetString(data, 0, count);

            Console.Write(msg);

            string s = Console.ReadLine();
            Console.Write(s);

            clientSocket.Send(Encoding.UTF8.GetBytes(s));


            Console.ReadKey();
            clientSocket.Close();
        }
    }
}