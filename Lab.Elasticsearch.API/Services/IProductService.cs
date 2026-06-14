using Lab.Elasticsearch.API.Dtos;
using Lab.Elasticsearch.API.Models;
using SearchRequest = Elastic.Clients.Elasticsearch.SearchRequest;

namespace Lab.Elasticsearch.API.Services;

public interface IProductService
{
    // CRUD (Adım 4)
    Task<Product?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<string> CreateAsync(Product product, CancellationToken ct = default);
    Task UpdatePartialAsync(string id, Dictionary<string, object> fields, CancellationToken ct = default);
    Task UpdateStockAsync(string id, int delta, CancellationToken ct = default); // scripted update
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);

    // Search (Adım 8-12)
    Task<SearchResponse<Product>> SearchAsync(SearchRequest request, CancellationToken ct = default);
    Task<List<Product>> MatchAllAsync(int from, int size, CancellationToken ct = default);
    Task<List<Product>> SearchByTermAsync(string field, string value, CancellationToken ct = default);
    Task<List<Product>> SearchByRangeAsync(decimal? minPrice, decimal? maxPrice, CancellationToken ct = default);

    // Analytics (Adım 13 — sadece metric aggs, basitleştirilmiş)
    Task<PriceStats> GetPriceStatsAsync(string? category, CancellationToken ct = default);
    Task<long> CountDistinctBrandsAsync(CancellationToken ct = default);
}