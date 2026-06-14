using Lab.Elasticsearch.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lab.Elasticsearch.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SetupController : ControllerBase
{
    private readonly IElasticsearchSetupService _elasticsearchSetupService;
    public SetupController(IElasticsearchSetupService  elasticsearchSetupService)
    {
        _elasticsearchSetupService = elasticsearchSetupService;
    }

    [HttpGet("exists")]
    public async Task<IActionResult> Exists(CancellationToken cancellationToken)
    {
        var control = await _elasticsearchSetupService.IndexExistsAsync(cancellationToken);
        return Ok(control);
    }

    [HttpPost("create-index")]
    public async Task<IActionResult> CreateIndex(CancellationToken cancellationToken)
    {
        await _elasticsearchSetupService.CreateIndexAsync(cancellationToken);
        return Ok();
    }
    
    [HttpDelete("index")]
    public async Task<IActionResult> Delete(CancellationToken ct)
    {
        await _elasticsearchSetupService.DeleteIndexAsync(ct);
        return Ok(new { message = "Index silindi." });
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        if (!await _elasticsearchSetupService.IndexExistsAsync(ct))
        {
            await _elasticsearchSetupService.CreateIndexAsync(ct);
        }
        await _elasticsearchSetupService.SeedSampleDataAsync(ct);
        return Ok();
    }
    
    [HttpPost("reset")]
    public async Task<IActionResult> Reset(CancellationToken ct)
    {
        await _elasticsearchSetupService.DeleteIndexAsync(ct);
        await _elasticsearchSetupService.CreateIndexAsync(ct);
        await _elasticsearchSetupService.SeedSampleDataAsync(ct);
        return Ok(new { message = "Index sıfırlandı ve 20 ürün yüklendi." });
    }
    
}