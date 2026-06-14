namespace Lab.Elasticsearch.API.Dtos;

public class SearchRequest
{
    public string? Query { get; set; }            // Adım 9: multi_match metni
    public string? Category { get; set; }         // Adım 10: filter
    public string? Brand { get; set; }            // Adım 10: filter
    public decimal? MinPrice { get; set; }        // Adım 8: range
    public decimal? MaxPrice { get; set; }
    public double? MinRating { get; set; }
    public bool? OnlyActive { get; set; } = true; // default aktif olanlar
    public bool? OnlyInStock { get; set; } = true;
    public bool UseFuzziness { get; set; } = true; // Adım 9: typo toleransı
    public bool UsePhraseBoost { get; set; } = true; // Adım 9: phrase match boost
    public string? SortBy { get; set; }           // Adım 12: "price_asc", "price_desc", "rating_desc", "newest"
    public int From { get; set; } = 0;            // Adım 12: pagination
    public int Size { get; set; } = 10;
    public bool Highlight { get; set; } = true;   // Adım 12: highlighting
}