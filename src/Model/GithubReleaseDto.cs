using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public class GithubReleaseDto
    {
        public string? TagName { get; set; }
        public string? HtmlUrl { get; set; }
        public string? Desc { get; set; }
        public string? Author { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? PublishedAt { get; set; }
    }
}
