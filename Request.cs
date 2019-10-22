using System;
using System.Collections.Generic;
using System.Text;

namespace Assignment3
{
    public class Request
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Date { get; set; }
        public string Body { get; set; }

        public override string ToString()
        {
            return "Method:" + Method + " Path:" + Path + " Date:" + Date + " Body:" + Body;
        }

    }
}
