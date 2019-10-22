using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Assignment3
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread t = new Thread(delegate ()
            {
                Server myServer = new Server("127.0.0.1", 5000);
            });
            t.Start();
            Console.WriteLine("Server started");
            
        }
    }
}
