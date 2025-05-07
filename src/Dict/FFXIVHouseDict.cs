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
            {339, "海都"},
            {341, "沙都"},
            {340, "森都"},
            {979, "山都"},
            {641, "白銀鄉"}
        };

        public static readonly Dictionary<int, string> SizeDict = new()
        {
            {0, "Small"},
            {1, "Medium"},
            {2, "Large"},
        };
    }
}
