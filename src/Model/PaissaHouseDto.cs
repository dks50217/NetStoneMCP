using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public class PaissaHouseDto
    {
        public required string Area { get; set; }
        public required string Type { get; set; }
        public required string Size { get; set; }
        public required string Price { get; set; }
        public required DateTimeOffset LastUpdateTime { get; set; }
    }

    public class PaissaHouseModel
    {
        public int id { get; set; }
        public string? name { get; set; }
        public List<District>? districts { get; set; }
    }

    public class District
    {
        public int id { get; set; }
        public string? name { get; set; }
        public int num_open_plots { get; set; }
        public double oldest_plot_time { get; set; }
        public List<OpenPlot>? open_plots { get; set; }
    }

    public class OpenPlot
    {
        public int world_id { get; set; }
        public int district_id { get; set; }
        public int ward_number { get; set; }
        public int plot_number { get; set; }
        public int size { get; set; }
        public int price { get; set; }
        public double last_updated_time { get; set; }
        public double first_seen_time { get; set; }
        public double est_time_open_min { get; set; }
        public double est_time_open_max { get; set; }
        public int purchase_system { get; set; }
        public int? lotto_entries { get; set; }
        public int? lotto_phase { get; set; }
        public int? lotto_phase_until { get; set; }
    }
}
