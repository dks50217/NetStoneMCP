using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Dict
{
    public static class FFXIVHouseDict
    {
        public static readonly Dictionary<int, string> MapDict = new()
        {
            {339, "海霧村 (Mist)"},
            {340, "薰衣草苗圃 (The Lavender Beds)"},
            {341, "高腳孤丘 (The Goblet)"},
            {641, "白銀鄉 (Shirogane)"},
            {979, "穹頂鄉 (Empyreum)"}
        };

        public static readonly Dictionary<int, string> SizeDict = new()
        {
            {0, "Small"},
            {1, "Medium"},
            {2, "Large"},
        };
    }
}
