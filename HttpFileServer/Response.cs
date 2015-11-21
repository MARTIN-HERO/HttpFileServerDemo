using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
