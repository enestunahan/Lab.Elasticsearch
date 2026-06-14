namespace Lab.Elasticsearch.API.Services;

public interface IElasticsearchSetupService
{
    Task<bool> IndexExistsAsync(CancellationToken ct = default);
    Task CreateIndexAsync(CancellationToken ct = default);
    Task DeleteIndexAsync(CancellationToken ct = default);
    Task SeedSampleDataAsync(CancellationToken ct = default);
}