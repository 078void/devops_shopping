using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Shopping.API.Data;
using Shopping.API.Models;

namespace Shopping.API.Controllers;

/// <summary>
/// 產品 API 控制器 - 提供產品的 CRUD 操作
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductContext _context;
    private readonly ILogger<ProductController> _logger;

    /// <summary>
    /// 透過建構函式注入相依性
    /// </summary>
    public ProductController(IProductContext context, ILogger<ProductController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 取得所有產品
    /// GET: api/product
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        try
        {
            // 從 MongoDB 取得所有產品
            var products = await _context.Products
                .Find(p => true)  // 不篩選，取得全部
                .ToListAsync();

            _logger.LogInformation($"取得 {products.Count} 筆產品資料");
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError($"取得產品時發生錯誤: {ex.Message}");
            return StatusCode(500, "伺服器內部錯誤");
        }
    }

    /// <summary>
    /// 根據 ID 取得單一產品
    /// GET: api/product/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(string id)
    {
        try
        {
            var product = await _context.Products
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning($"找不到產品 ID: {id}");
                return NotFound($"找不到產品 ID: {id}");
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError($"取得產品 {id} 時發生錯誤: {ex.Message}");
            return StatusCode(500, "伺服器內部錯誤");
        }
    }

    /// <summary>
    /// 新增產品
    /// POST: api/product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        try
        {
            product.Id = null;
            await _context.Products.InsertOneAsync(product);
            _logger.LogInformation($"成功新增產品: {product.Name}");
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError($"新增產品時發生錯誤: {ex.Message}");
            return StatusCode(500, "伺服器內部錯誤");
        }
    }

    /// <summary>
    /// 更新產品
    /// PUT: api/product/{id}
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product product)
    {
        try
        {
            var existingProduct = await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (existingProduct == null)
            {
                return NotFound($"找不到產品 ID: {id}");
            }

            product.Id = id;
            var result = await _context.Products.ReplaceOneAsync(p => p.Id == id, product);

            if (result.ModifiedCount == 0)
            {
                return BadRequest("產品未被修改");
            }

            _logger.LogInformation($"成功更新產品: {product.Name}");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"更新產品時發生錯誤: {ex.Message}");
            return StatusCode(500, "伺服器內部錯誤");
        }
    }

    /// <summary>
    /// 刪除產品
    /// DELETE: api/product/{id}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        try
        {
            var result = await _context.Products.DeleteOneAsync(p => p.Id == id);

            if (result.DeletedCount == 0)
            {
                return NotFound($"找不到產品 ID: {id}");
            }

            _logger.LogInformation($"成功刪除產品 ID: {id}");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError($"刪除產品時發生錯誤: {ex.Message}");
            return StatusCode(500, "伺服器內部錯誤");
        }
    }
}