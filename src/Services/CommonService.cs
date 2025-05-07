using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface ICommonService
    {
        Task<IEnumerable<DataCenterDto>?> GetDataCenter();
        Task<IEnumerable<WorldDto>?> GetWorlds();
    }

    public class CommonService : ICommonService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endPoint = "https://paissadb.zhu.codes/worlds";

        public CommonService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<DataCenterDto>?> GetDataCenter()
        {
            try
            {
                var response = await _httpClient.GetAsync(_endPoint);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var worlds = JsonSerializer.Deserialize<List<WorldDto>>(content);

                var dataCenters = worlds?
                    .GroupBy(w => new { w.datacenter_id, w.datacenter_name })
                    .Select(g => new DataCenterDto
                    {
                        DatacenterId = g.Key.datacenter_id,
                        DatacenterName = g.Key.datacenter_name
                    })
                    .ToList();

                return dataCenters;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<WorldDto>?> GetWorlds()
        {
            try
            {
                var response = await _httpClient.GetAsync(_endPoint);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var worlds = JsonSerializer.Deserialize<List<WorldDto>>(content);

                return worlds;
            }
            catch
            {
                return null;
            }
        }
    }
}
