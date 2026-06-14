using Elastic.Clients.Elasticsearch;
using Lab.Elasticsearch.API.Models;
using Lab.Elasticsearch.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<ElasticsearchOptions>(builder.Configuration.GetSection("Elasticsearch"));

builder.Services.AddSingleton<ElasticsearchClient>(sp =>
{
    var cfg = builder.Configuration.GetSection("Elasticsearch");
    var uri = cfg["Uri"] ?? "http://localhost:9200";
    var defaultIndex = cfg["DefaultIndex"] ?? "products";

    var settings = new ElasticsearchClientSettings(new Uri(uri))
            .DefaultIndex(defaultIndex)
            .PrettyJson(true) // debug için pretty JSON
        // .EnableDebugMode() // tüm istekleri loglar; geliştirirken aç
        ;

    return new ElasticsearchClient(settings);
});

builder.Services.AddScoped<IElasticsearchSetupService, ElasticsearchSetupService>();
builder.Services.AddScoped<IProductService, ProductService>();  


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();