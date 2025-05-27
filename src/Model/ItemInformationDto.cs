using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public class ItemInformationDto
    {
        public int ItemId { get; set; }

        /// <summary>
        /// Language code, e.g., "en", "ja", "de", "fr"
        /// </summary>
        public string Language { get; set; } = "en";

        public string? Name { get; set; }
    }
}
