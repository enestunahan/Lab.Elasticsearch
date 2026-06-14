using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Lab.Elasticsearch.API.Models;
using Microsoft.Extensions.Options;

namespace Lab.Elasticsearch.API.Services;

public class ElasticsearchSetupService : IElasticsearchSetupService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;
    private readonly ILogger<ElasticsearchSetupService> _logger;

    public ElasticsearchSetupService(ElasticsearchClient client, IOptions<ElasticsearchOptions> options, ILogger<ElasticsearchSetupService> logger)
    {
        _client = client;
        _indexName = options.Value.DefaultIndex;
        _logger = logger;
    }
    
    public async Task<bool> IndexExistsAsync(CancellationToken ct = default)
    {
        var control = await _client.Indices.ExistsAsync(_indexName, ct);
        return control.Exists;
    }

    public async Task CreateIndexAsync(CancellationToken ct = default)
    {
        if (await IndexExistsAsync(ct))
        {
            _logger.LogInformation("Index {Index} zaten var, atlanıyor.", _indexName);
            return;
        }

        // ===== ADIM 7: Custom Türkçe analyzer =====
        // Filter sırası önemli: lowercase → turkish_stop → turkish_stemmer → asciifolding
        var customAnalyzer = new CustomAnalyzer
        {
            Tokenizer = "standard",
            Filter = new[] { "lowercase", "turkish_stop", "turkish_stemmer", "asciifolding" }
        };

        var analyzers = new Analyzers();
        analyzers.Add("turkish_custom", customAnalyzer);

        // turkish_stop ve turkish_stemmer built-in değil; custom analyzer için kendimiz tanımlıyoruz.
        var tokenFilters = new TokenFilters();
        tokenFilters.Add("turkish_stop", new StopTokenFilter { Stopwords = new[] { "_turkish_" } });
        tokenFilters.Add("turkish_stemmer", new StemmerTokenFilter { Language = "turkish" });

        var indexSettings = new IndexSettings
        {
            NumberOfShards = 1,
            NumberOfReplicas = 0,
            Analysis = new IndexSettingsAnalysis
            {
                Analyzers = analyzers,
                TokenFilters = tokenFilters
            }
        };

        // ===== ADIM 6: Explicit mapping =====
        // ES'in JSON tarafındaki "properties" objesinin birebir karşılığı.
        var properties = new Properties
        {
            // name -> text (arama) + .keyword (sort/agg) multi-field, turkish_custom analyzer
            {
                "name",
                new TextProperty
                {
                    Analyzer = "turkish_custom",
                    Fields = new Properties
                    {
                        { "keyword", new KeywordProperty { IgnoreAbove = 256 } }
                    }
                }
            },
            { "brand",       new KeywordProperty() },
            { "category",    new KeywordProperty() },
            { "subcategory", new KeywordProperty() },

            // para için scaled_float
            { "price",       new ScaledFloatNumberProperty { ScalingFactor = 100 } },
            { "stock",       new IntegerNumberProperty() },
            { "rating",      new HalfFloatNumberProperty() },

            // tags: array da olsa tip yine keyword (ES her alan zaten array olabilir)
            { "tags",        new KeywordProperty() },
            { "isActive",    new BooleanProperty() },
            { "createdAt",   new DateProperty() }
        };

        var createIndexRequest = new CreateIndexRequest(_indexName)
        {
            Settings = indexSettings,
            Mappings = new TypeMapping
            {
                Properties = properties
            }
        };

        var response = await _client.Indices.CreateAsync(createIndexRequest, ct);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Index oluşturulamadı: {Error}", response.DebugInformation);
            throw new InvalidOperationException("Elasticsearch index oluşturma başarısız.");
        }

        _logger.LogInformation("Index {Index} başarıyla oluşturuldu.", _indexName);
    }
    
    
    public async Task DeleteIndexAsync(CancellationToken ct = default)
    {
        if (await IndexExistsAsync(ct))
        {
            await _client.Indices.DeleteAsync(_indexName, ct);
            _logger.LogInformation("Index {Index} silindi.", _indexName);
        }
    }

    public async Task SeedSampleDataAsync(CancellationToken ct = default)
    {
        var products = GetSampleProducts();
        var bulkResponse = await _client.BulkAsync(x => x
            .Index(_indexName)
            .IndexMany(products, (descriptor, product) => descriptor.Id(product.Id)), ct);

        if (bulkResponse.Errors)
        {
            foreach (var error in bulkResponse.ItemsWithErrors)
            {
                _logger.LogError("bulk item hata: id {Id}",error.Id);
            }

            throw new NotImplementedException("bulk seed sırasına hata meydana geldi");
        }
        
        _logger.LogInformation("{Count} ürün başarıyla seed edildi", products.Count());
        await _client.Indices.CreateAsync(_indexName, ct);
    }
    
    private static List<Product> GetSampleProducts() => new()
    {
        new Product { Id = "1", Name = "Sony WH-1000XM5 Kablosuz Kulaklık", Brand = "Sony", Category = "Elektronik", Subcategory = "Kulaklık", Price = 12999.90m, Stock = 25, Rating = 4.7, Tags = new() { "bluetooth", "noise-cancelling", "kablosuz" }, IsActive = true, CreatedAt = new DateTime(2024, 1, 15) },
        new Product { Id = "2", Name = "Logitech MX Master 3S Mouse", Brand = "Logitech", Category = "Elektronik", Subcategory = "Mouse", Price = 2499.00m, Stock = 80, Rating = 4.8, Tags = new() { "kablosuz", "ergonomik", "ofis" }, IsActive = true, CreatedAt = new DateTime(2024, 2, 10) },
        new Product { Id = "3", Name = "Keychron K2 Mekanik Klavye", Brand = "Keychron", Category = "Elektronik", Subcategory = "Klavye", Price = 3299.00m, Stock = 15, Rating = 4.6, Tags = new() { "mekanik", "bluetooth", "rgb" }, IsActive = true, CreatedAt = new DateTime(2024, 3, 5) },
        new Product { Id = "4", Name = "Apple MacBook Air M3", Brand = "Apple", Category = "Elektronik", Subcategory = "Laptop", Price = 49999.00m, Stock = 8, Rating = 4.9, Tags = new() { "laptop", "apple", "m3" }, IsActive = true, CreatedAt = new DateTime(2024, 4, 20) },
        new Product { Id = "5", Name = "Samsung 27 inç 4K Monitor", Brand = "Samsung", Category = "Elektronik", Subcategory = "Monitör", Price = 8999.00m, Stock = 12, Rating = 4.5, Tags = new() { "4k", "monitor", "ips" }, IsActive = true, CreatedAt = new DateTime(2024, 1, 30) },
        new Product { Id = "6", Name = "Nike Air Max 270 Spor Ayakkabı", Brand = "Nike", Category = "Giyim", Subcategory = "Ayakkabı", Price = 4299.00m, Stock = 50, Rating = 4.4, Tags = new() { "spor", "koşu", "siyah" }, IsActive = true, CreatedAt = new DateTime(2024, 2, 15) },
        new Product { Id = "7", Name = "Adidas Originals Stan Smith", Brand = "Adidas", Category = "Giyim", Subcategory = "Ayakkabı", Price = 3799.00m, Stock = 35, Rating = 4.6, Tags = new() { "günlük", "deri", "beyaz" }, IsActive = true, CreatedAt = new DateTime(2024, 3, 12) },
        new Product { Id = "8", Name = "Levi's 501 Original Jean Pantolon", Brand = "Levi's", Category = "Giyim", Subcategory = "Pantolon", Price = 1899.00m, Stock = 100, Rating = 4.5, Tags = new() { "jean", "klasik", "mavi" }, IsActive = true, CreatedAt = new DateTime(2024, 1, 8) },
        new Product { Id = "9", Name = "Mavi Erkek Slim Fit Gömlek", Brand = "Mavi", Category = "Giyim", Subcategory = "Gömlek", Price = 799.00m, Stock = 200, Rating = 4.2, Tags = new() { "slim", "ofis", "beyaz" }, IsActive = true, CreatedAt = new DateTime(2024, 4, 1) },
        new Product { Id = "10", Name = "The North Face Mont", Brand = "The North Face", Category = "Giyim", Subcategory = "Mont", Price = 6499.00m, Stock = 18, Rating = 4.7, Tags = new() { "kış", "outdoor", "su-geçirmez" }, IsActive = true, CreatedAt = new DateTime(2024, 10, 15) },
        new Product { Id = "11", Name = "Suç ve Ceza - Dostoyevski", Brand = "İletişim Yayınları", Category = "Kitap", Subcategory = "Roman", Price = 189.00m, Stock = 300, Rating = 4.9, Tags = new() { "klasik", "rus-edebiyatı", "roman" }, IsActive = true, CreatedAt = new DateTime(2023, 12, 1) },
        new Product { Id = "12", Name = "Sapiens: Hayvanlardan Tanrılara", Brand = "Kolektif Kitap", Category = "Kitap", Subcategory = "Tarih", Price = 245.00m, Stock = 150, Rating = 4.8, Tags = new() { "tarih", "antropoloji", "bestseller" }, IsActive = true, CreatedAt = new DateTime(2024, 2, 20) },
        new Product { Id = "13", Name = "Clean Code - Robert C. Martin", Brand = "Pearson", Category = "Kitap", Subcategory = "Yazılım", Price = 459.00m, Stock = 75, Rating = 4.7, Tags = new() { "yazılım", "kod", "best-practice" }, IsActive = true, CreatedAt = new DateTime(2024, 3, 10) },
        new Product { Id = "14", Name = "Tefal Indüksiyon Tava 28cm", Brand = "Tefal", Category = "Ev", Subcategory = "Mutfak", Price = 1299.00m, Stock = 40, Rating = 4.5, Tags = new() { "mutfak", "yapışmaz", "indüksiyon" }, IsActive = true, CreatedAt = new DateTime(2024, 5, 5) },
        new Product { Id = "15", Name = "Philips Airfryer XXL", Brand = "Philips", Category = "Ev", Subcategory = "Küçük Ev Aleti", Price = 5499.00m, Stock = 22, Rating = 4.6, Tags = new() { "airfryer", "sağlıklı", "mutfak" }, IsActive = true, CreatedAt = new DateTime(2024, 6, 1) },
        new Product { Id = "16", Name = "IKEA MALM Yatak Odası Takımı", Brand = "IKEA", Category = "Ev", Subcategory = "Mobilya", Price = 12999.00m, Stock = 5, Rating = 4.3, Tags = new() { "mobilya", "yatak-odası", "modern" }, IsActive = false, CreatedAt = new DateTime(2024, 7, 20) },
        new Product { Id = "17", Name = "JBL Charge 5 Bluetooth Hoparlör", Brand = "JBL", Category = "Elektronik", Subcategory = "Hoparlör", Price = 4299.00m, Stock = 60, Rating = 4.6, Tags = new() { "bluetooth", "taşınabilir", "su-geçirmez" }, IsActive = true, CreatedAt = new DateTime(2024, 8, 12) },
        new Product { Id = "18", Name = "Xiaomi Mi Band 8 Akıllı Bileklik", Brand = "Xiaomi", Category = "Elektronik", Subcategory = "Akıllı Saat", Price = 1599.00m, Stock = 120, Rating = 4.4, Tags = new() { "akıllı", "fitness", "uygun-fiyat" }, IsActive = true, CreatedAt = new DateTime(2024, 9, 3) },
        new Product { Id = "19", Name = "Puma Eşofman Takımı", Brand = "Puma", Category = "Giyim", Subcategory = "Eşofman", Price = 2199.00m, Stock = 45, Rating = 4.3, Tags = new() { "spor", "rahat", "günlük" }, IsActive = true, CreatedAt = new DateTime(2024, 9, 25) },
        new Product { Id = "20", Name = "Bosch Bulaşık Makinesi", Brand = "Bosch", Category = "Ev", Subcategory = "Beyaz Eşya", Price = 18999.00m, Stock = 7, Rating = 4.7, Tags = new() { "bulaşık-makinesi", "a-enerji", "sessiz" }, IsActive = true, CreatedAt = new DateTime(2024, 10, 30) }
    };
}