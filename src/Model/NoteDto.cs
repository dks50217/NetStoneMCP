using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public class NoteDto
    {
        public string Id { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Content { get; set; } = "";
    }
}
