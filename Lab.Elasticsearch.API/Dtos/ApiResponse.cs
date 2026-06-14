namespace Lab.Elasticsearch.API.Dtos;

public class SearchHit<T>
{
    public T Source { get; set; } = default!;
    public string Id { get; set; } = default!;
    public double? Score { get; set; }
    public Dictionary<string, List<string>>? Highlight { get; set; }
}

public class SearchResponse<T>
{
    public long Total { get; set; }
    public long TookMs { get; set; }
    public int From { get; set; }
    public int Size { get; set; }
    public List<SearchHit<T>> Hits { get; set; } = new();
}

public class PriceStats
{
    public long Count { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public double? Avg { get; set; }
    public double? Sum { get; set; }
}