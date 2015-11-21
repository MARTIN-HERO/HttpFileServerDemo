using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpFileServer
{
    public class HttpServer
    {
        public const String MSG_DIR = "/root/msg";
        public const String WEB_DIR = "/root/web";
        public const String VERSION = "HTTP/1.1";
        public const String Name = "Martin Herodt HTTP server v.0.1";

        private bool running = false;

        private TcpListener listener;

        public HttpServer(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
        }
        public void Start()
        {
            Thread serverThread = new Thread(new ThreadStart(Run));
            serverThread.Start();
        }
        public void Run()
        {
            running = true;
            listener.Start();

            while (running)
            {
                Console.WriteLine("Waiting for connection...");
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Client connected...");

                handleClient(client);
                client.Close();
            }
            running = false;
            listener.Stop();
        }

        private void handleClient(TcpClient client)
        {
            StreamReader reader = new StreamReader(client.GetStream());

            String msg = "";
            while (reader.Peek() != -1)
            {
                msg += reader.ReadLine() + "\n";
            }
            Debug.WriteLine("Request: \n" + msg);

            Request req = Request.GetRequest(msg);
            Response resp = Response.From(req);
            resp.Post(client.GetStream());
        }
    }
}
