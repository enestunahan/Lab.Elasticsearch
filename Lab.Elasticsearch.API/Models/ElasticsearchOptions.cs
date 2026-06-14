namespace Lab.Elasticsearch.API.Models;

public class ElasticsearchOptions
{
    public string Uri { get; set; } = "http://localhost:9200";
    public string DefaultIndex { get; set; } = "products";
    public bool AutoSeedOnStartup { get; set; } = true;
}