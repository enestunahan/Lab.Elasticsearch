using Elastic.Clients.Elasticsearch;
using Lab.Elasticsearch.API.Dtos;
using Lab.Elasticsearch.API.Models;
using Microsoft.Extensions.Options;
using SearchRequest = Elastic.Clients.Elasticsearch.SearchRequest;

namespace Lab.Elasticsearch.API.Services;

public class ProductService : IProductService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;
    private readonly ILogger<ProductService> _logger;
    
    public ProductService(ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        ILogger<ProductService> logger)
    {
        _client = client;
        _indexName = options.Value.DefaultIndex;  
        _logger = logger;
    }
    
    
    public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var resp = await _client.GetAsync<Product>(id,g=>g.Index(_indexName),ct);
        return resp.Found ? resp.Source : null;
    }

    public Task<string> CreateAsync(Product product, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePartialAsync(string id, Dictionary<string, object> fields, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateStockAsync(string id, int delta, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Dtos.SearchResponse<Product>> SearchAsync(SearchRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Product>> MatchAllAsync(int from, int size, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Product>> SearchByTermAsync(string field, string value, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Product>> SearchByRangeAsync(decimal? minPrice, decimal? maxPrice, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<PriceStats> GetPriceStatsAsync(string? category, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<long> CountDistinctBrandsAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}