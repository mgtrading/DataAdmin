using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DataAdmin.ServerManager
{
    internal static class SocketServer
    {

        private static Socket _serverSocket;

        private static readonly List<Socket> ClientSockets = new List<Socket>();
        private const int BufferSize = 2048;
        private static readonly byte[] Buffer = new byte[BufferSize];

        public static void SetupServer()
        {
            Console.WriteLine(@"Setting up server...");
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                                                                 ProtocolType.Tcp); 
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            _serverSocket.Listen(7);
            _serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine(@"Server Setup");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients)
        /// </summary>
        public static void CloseAllSockets()
        {
            if (_serverSocket == null) return;
            Console.WriteLine(@"Soping server...");
            foreach (Socket socket in ClientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            Console.WriteLine(@"Server Stoped");
            
                _serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket socket;

            try
            {
                socket = _serverSocket.EndAccept(ar);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            ClientSockets.Add(socket);
            socket.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine(@"Client connected, waiting for request...");
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var current = (Socket) ar.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(ar);
            }
            catch (SocketException)
            {
                Console.WriteLine(@"Client forcefully disconnected");
                current.Close(); // Dont shutdown because the socket may be disposed and its disconnected anyway
                ClientSockets.Remove(current);
                return;
            }

            var recBuf = new byte[received];
            Array.Copy(Buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine(@"Received Text: " + text);

            if (text.ToLower() == "exit") // Client wants to exit gracefully
            {
                // Always Shutdown before closing
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                ClientSockets.Remove(current);
                Console.WriteLine(@"Client disconnected");
                return;
            }
            
            string result = ResponseEvent(text.ToLower());

            byte[] data = Encoding.ASCII.GetBytes(result);
            current.Send(data);

            current.BeginReceive(Buffer, 0, BufferSize, SocketFlags.None, ReceiveCallback, current);
        }

        private static string ResponseEvent(string text)
        {
            switch (text.ToLower())
            {
                case "hello": // Client requested time
                    {
                        Console.WriteLine(@"Text is a Hello request");
                        return @"Hello";
                    }
                case "trylogin":
                    {
                        return @"EnterLogin";
                    }
                default:
                    {
                        Console.WriteLine(@"Text is an invalid request");
                        return @"Invalid request";
                    }
            }
        }
    }

}

