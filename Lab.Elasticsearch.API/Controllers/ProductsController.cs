using Lab.Elasticsearch.API.Models;
using Lab.Elasticsearch.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lab.Elasticsearch.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductsController(IProductService productService)
    {
        _productService = productService;   
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id, CancellationToken ct)
    {
        var data = await _productService.GetByIdAsync(id, ct);
        return data is null ? NotFound() : Ok(data);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product, CancellationToken ct)
    {
        var id = await _productService.CreateAsync(product, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }
}