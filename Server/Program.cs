using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    /// <summary>
    /// Since the method nems are self-explanatory, 
    /// and I currently have a full time life-consuming work,
    /// skipped the unit tests but I love to work on TDD projects
    /// Credits : MSDN, code Project, stackoverflow...
    /// </summary>
    class Program
    {
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int myBuffer = 2048;
        private const int Port = 100;
        private static readonly byte[] buffer = new byte[myBuffer];
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static ManualResetEvent connectDone =
     new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        private static void SetupServer()
        {
            Console.WriteLine("Starting...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Port));
            serverSocket.Listen(100);
            Console.WriteLine("Server started..");

            while (true)
            {
                allDone.Reset();
                // Start async listening  
                Console.WriteLine("Waiting for a connection...");
                serverSocket.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    serverSocket);

                // Wait till done thread signal
                allDone.WaitOne();
            }

        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                socket = serverSocket.EndAccept(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, myBuffer, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Client connected...");
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket currentSocket = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = currentSocket.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client logged off");
                currentSocket.Close();
                clientSockets.Remove(currentSocket);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine("Received Text: " + text);
            byte[] data = Encoding.ASCII.GetBytes("Echo -" + text + "<EOF>");

            Console.WriteLine("Replied 'echo'");

            foreach (var item in clientSockets)
            {
                Send(item, "Echo<EOF>");


                sendDone.WaitOne();
            }


        }
        private static void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

     
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                client.BeginReceive(buffer, 0, myBuffer, SocketFlags.None, ReceiveCallback, client);
                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        static void Main()
        {
            Console.Title = "Server Console";
            SetupServer();
            Console.ReadLine();//press any ket to close and release sources
            ReleaseSources();
        }

        private static void ReleaseSources()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            serverSocket.Close();
        }
    }
}
