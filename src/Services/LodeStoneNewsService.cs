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
        Task<LodeStoneNewsMaintenance?> GetCurrentMaintenances();
        Task<IEnumerable<LodeStoneNewsItem>?> GetTopics();
        Task<LodeStoneNewsItem?> GetPost(string id);
    }

    public class LodeStoneNewsService : ILodeStoneNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _endPoint = "https://lodestonenews.com/news";

        public LodeStoneNewsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LodeStoneNewsMaintenance?> GetCurrentMaintenances()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_endPoint}/maintenance/current");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var results = JsonSerializer.Deserialize<LodeStoneNewsMaintenance>(content);

                return results;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<LodeStoneNewsItem>?> GetTopics()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_endPoint}/topics?locale=jp");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var results = JsonSerializer.Deserialize<IEnumerable<LodeStoneNewsItem>>(content);

                return results;
            }
            catch
            {
                return null;
            }
        }

        public async Task<LodeStoneNewsItem?> GetPost(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_endPoint}/posts/{id}");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var results = JsonSerializer.Deserialize<LodeStoneNewsItem>(content);

                return results;
            }
            catch
            {
                return null;
            }
        }
    }
}
