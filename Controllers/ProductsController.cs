using ClearanceAPI.Data;
using ClearanceAPI.DTOs;
using ClearanceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClearanceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/products?category=Electronics&search=laptop
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductListItemDto>>> GetProducts(
        [FromQuery] string? category, [FromQuery] string? search)
    {
        var query = _db.Products.Include(p => p.Inventory).Include(p => p.Discounts)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        var products = await query.ToListAsync();

        var result = products.Select(p => new ProductListItemDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Category,
            Condition = p.Condition,
            OriginalPrice = p.OriginalPrice,
            CurrentPrice = p.CurrentPrice,
            ImageUrl = p.ImageUrl,
            StockAvailable = p.Inventory?.QuantityAvailable ?? 0,
            HasActiveFlashSale = p.Discounts.Any(d => d.IsFlashSale && d.IsCurrentlyActive)
        });

        return Ok(result);
    }

    // GET /api/products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
    {
        var product = await _db.Products
            .Include(p => p.Inventory)
            .Include(p => p.Discounts)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null) return NotFound();

        var dto = new ProductDetailDto
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Condition = product.Condition,
            OriginalPrice = product.OriginalPrice,
            CurrentPrice = product.CurrentPrice,
            ImageUrl = product.ImageUrl,
            StockAvailable = product.Inventory?.QuantityAvailable ?? 0,
            HasActiveFlashSale = product.Discounts.Any(d => d.IsFlashSale && d.IsCurrentlyActive),
            ActiveDiscounts = product.Discounts
                .Where(d => d.IsCurrentlyActive)
                .Select(d => new DiscountDto
                {
                    Label = d.Label,
                    Type = d.Type.ToString(),
                    Value = d.Value,
                    EndsAt = d.EndsAt,
                    IsFlashSale = d.IsFlashSale
                }).ToList()
        };

        return Ok(dto);
    }

    // POST /api/products  (khusus admin, butuh JWT)
    [Authorize]
    [HttpPost]
    public async Task<ActionResult> CreateProduct(CreateProductDto dto)
    {
        var product = new Product
        {
            Sku = dto.Sku,
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Condition = dto.Condition,
            OriginalPrice = dto.OriginalPrice,
            CurrentPrice = dto.OriginalPrice,
            ImageUrl = dto.ImageUrl,
            Inventory = new Inventory { QuantityAvailable = dto.InitialStock }
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, null);
    }
}
