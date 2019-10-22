using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Assignment3
{
    public class Response
    {
        public string Status { get; set; }
        public string Body { get; set; }

        public override string ToString()
        {
            return "Status:" + Status + " Body:" + Body;
        }


    }
}
