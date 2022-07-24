using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{
    /// <summary>
    /// Since the method nems are self-explanatory, 
    /// and I currently have a full time life-consuming work,
    /// skipped the unit tests but I love to work on TDD projects
    /// Credits : MSDN, code Project, stackoverflow...
    /// </summary>
    class Program
    {
        private static readonly Socket myClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int Port = 100;

        static void Main()
        {
            Console.Title = "Client";
            ConnectToServer();
            WaitRespone();
            Exit();
        }

        private static void ConnectToServer()
        {
            while (!myClientSocket.Connected)
            {
                try
                {
                    Console.WriteLine("Connecting .. " );
                    myClientSocket.Connect(IPAddress.Parse("127.0.0.1"), Port);
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }
            Console.Clear();
            Console.WriteLine("Connected");
        }

        private static void WaitRespone()
        {
            Console.WriteLine(@"type exit to finalize..");
            while (true)
            {
                SendRequest();
                GetResponse();
            }
        }
 
        private static void Exit()
        {
            Send("exit"); 
            myClientSocket.Shutdown(SocketShutdown.Both);
            myClientSocket.Close();
            Environment.Exit(0);
        }

        private static void SendRequest()
        {
            Console.Write("Send message  :  ");
            string request = Console.ReadLine();
            Send(request);

            if (request.ToLower() == "exit")
            {
                Exit();
            }
        }
        private static void Send(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            myClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void GetResponse()
        {
            var buffer = new byte[2048];
            int received = myClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            Console.WriteLine(text);
        }
    }
}
