using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public class RaceInfoDto
    {
        public string ChineseName { get; set; } = string.Empty;
        public Dictionary<string, string> Clans { get; set; } = new();
    }
}
