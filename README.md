HTTP Server demo

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

-----------------------------------------------------------------------------------------

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

-----------------------------------------------------------------------------------------
namespace HttpFileServer
{
    public class Request
    {
        public String Type { get; set; }
        public String Url { get; set; }
        public String Host { get; set; }
        public String Referer { get; set; }

        private Request(String type , String url, String host, String referer)
        {
            Type = type;
            Url = url;
            Host = host;
            Referer = referer;
        }
        public static Request GetRequest(String request)
        {
            if (String.IsNullOrEmpty(request))
            {
                return null;
            }
            String[] tokens = request.Split(' ','\n');
            String type = tokens[0];
            String url = tokens[1];
            String host = tokens[4];
            String referer = "";
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i]== "Referer:")
                {
                    referer = tokens[i + 1];
                    break;
                }
            }
            Console.WriteLine(String.Format("{0} {1} @ {2} \nReferer: {3}", type, url, host, referer));

            return new Request(type, url, host, referer);
        } 
    }
}

-----------------------------------------------------------------------------------------

namespace HttpFileServer
{
    public class Response
    {
        private Byte[] data = null;
        private String status;
        private String mime;
        private Response(String status, String mime, Byte[] date)
        {
            this.status = status;
            this.mime = mime;
            this.data = date;
        }
        public static Response From(Request request)
        {
            if (request == null)
            {
                return MakeNullRequest();
            }
            if (request.Type == "GET")
            {
                String file = Environment.CurrentDirectory + HttpServer.WEB_DIR + request.Url;
                FileInfo f = new FileInfo(file);
                if (f.Exists && f.Extension.Contains("."))
                {
                    return MakeFromFile(f);
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(f + "/");
                    if (!di.Exists)
                    {
                        return MakePageNotFound();
                    }
                    FileInfo[] files = di.GetFiles();
                    foreach (FileInfo ff in files)
                    {
                        String n = ff.Name;
                        if (n.Contains("default.html") || n.Contains("default.htm") || n.Contains("index.html") || n.Contains("index.htm"))
                        {
                            return MakeFromFile(ff);
                        }
                    }
                }
            }
            else
            {
                MakeMethodNotAllowed();
            }
            return MakePageNotFound();
        }

        private static Response MakeFromFile(FileInfo f)
        {
            FileStream fs = f.OpenRead();
            BinaryReader reader = new BinaryReader(fs);
            Byte[] d = new Byte[fs.Length];
            reader.Read(d, 0, d.Length);
            fs.Close();
            return new Response("200 ok", "html/text", d);
        }

        private static Response MakeNullRequest()
        {
            String file = Environment.CurrentDirectory + HttpServer.MSG_DIR + "/400.html";
            FileInfo fi = new FileInfo(file);
            FileStream fs = fi.OpenRead();
            BinaryReader reader = new BinaryReader(fs);
            Byte[] d = new Byte[fs.Length];
            reader.Read(d, 0,d.Length);
            fs.Close();
            return new Response("400 Bad Request", "html/text", d);
        }
        private static Response MakePageNotFound()
        {
            String file = Environment.CurrentDirectory + HttpServer.MSG_DIR + "/404.html";
            FileInfo fi = new FileInfo(file);
            FileStream fs = fi.OpenRead();
            BinaryReader reader = new BinaryReader(fs);
            Byte[] d = new Byte[fs.Length];
            reader.Read(d, 0, d.Length);
            fs.Close();
            return new Response("404 Page Not Found", "html/text", d);
        }
        private static Response MakeMethodNotAllowed()
        {
            String file = Environment.CurrentDirectory + HttpServer.MSG_DIR + "/405.html";
            FileInfo fi = new FileInfo(file);
            FileStream fs = fi.OpenRead();
            BinaryReader reader = new BinaryReader(fs);
            Byte[] d = new Byte[fs.Length];
            reader.Read(d, 0, d.Length);
            fs.Close();
            return new Response("405 Method Not Allowed", "html/text", d);
        }
        public void Post(NetworkStream stream)
        {
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("{0} {1}\r\nServer: {2}\r\nContent-Type: {3}\r\nAccept-Ranges: bytes\r\nContent-Length: {4}\r\n", 
                HttpServer.VERSION, status, HttpServer.Name, mime, data.Length);
            stream.Write(data, 0, data.Length);
        }
    }
}

-----------------------------------------------------------------------------------------





