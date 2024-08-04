using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctioSvcHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public AuctioSvcHttpClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<List<Item>> GetItemForSearchDb()
        {
            var lastUpdated = await DB.Find<Item, string>()
                .Sort(u => u.Descending(x => x.UpDatedAt))
                .Project(u => u.UpDatedAt.ToString())
                .ExecuteFirstAsync();

            return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"]
                + "/api/auctions?date=" + lastUpdated);
        }
    }
}
