using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Assignment3
{
    public class Category
    {
        [JsonPropertyName("cid")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }

    }

}
