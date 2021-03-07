using System;
using System.Collections.Generic;
using System.Text;

namespace IOTesting
{
    public class Entry
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Type { get; set; }

        public override string ToString()
        {
            return "{'Name':" + "'" + Name + "'" + ",'Phone':" + "'" + Phone + "'" + ",'Type':" + "'" + Type + "'}";
        }
    }
}
