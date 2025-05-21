using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public class StoreDto
    {
        public int status { get; set; }
        public StoreMeta? meta { get; set; }
        public List<StoreCategory>? categories { get; set; }
    }

    public class StoreCategory
    {
        public int id { get; set; }
        public string? name { get; set; }
        public List<StoreSubCategory>? subCategories { get; set; }
        public List<StoreFilter>? filters { get; set; }
    }

    public class StoreMeta
    {
        public List<StoreFilter>? filters { get; set; }
    }

    public class StoreFilter
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool isNew { get; set; }
        public bool isSale { get; set; }
        public bool isHot { get; set; }
    }

    public class StoreSubCategory
    {
        public int id { get; set; }
        public string? name { get; set; }
        public List<StoreFilter>? filters { get; set; }
    }
}
