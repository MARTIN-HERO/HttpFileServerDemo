using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpFileServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server on port 8888...");
            HttpServer server = new HttpServer(8888);
            server.Start();
        }
    }
}
