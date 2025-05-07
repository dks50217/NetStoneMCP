using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public class WorldDto
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public int datacenter_id { get; set; }
        public string datacenter_name { get; set; } = string.Empty;
    }
}
