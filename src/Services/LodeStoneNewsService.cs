using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface ILodeStoneNewsService
    {
        Task<IEnumerable<LodeStoneNewsMaintenance>?> GetCurrentMaintenances();
    }

    public class LodeStoneNewsService : ILodeStoneNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endPoint = "https://lodestonenews.com/news";

        public LodeStoneNewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<LodeStoneNewsMaintenance>?> GetCurrentMaintenances()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_endPoint}/maintenance/current");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var results = JsonSerializer.Deserialize<IEnumerable<LodeStoneNewsMaintenance>>(content);

                return results;
            }
            catch
            {
                return null;
            }
        }
    }
}
