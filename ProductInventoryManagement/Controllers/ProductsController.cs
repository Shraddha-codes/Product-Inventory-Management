using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProductInventoryManagement.Database;
using ProductInventoryManagement.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ProductInventoryManagement.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")] 
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILog _log;
        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            ILog _logger1 = LogManager.GetLogger("ProductsController");
            _log = _logger1;
        }
        [Authorize(Roles = "User,Admin")]
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            var categories = await _context.ProductDetails
                .Select(p => p.Categories)
                .ToListAsync();

            var uniqueCategories = categories
                .SelectMany(c => c.Split(','))
                .Distinct()
                .ToList();

            return Ok(uniqueCategories);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpGet("products")]
        public async Task<ActionResult<IEnumerable<ProductDetails>>> GetProduct(string category = null)
        {
            var productsQuery = _context.ProductDetails
               
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Categories.Contains(category));
            }

            var products = await productsQuery.Select(p => new ProductDetailsDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                SKU = p.SKU,
            }).ToListAsync();

            return Ok(products);
        }


        [Authorize (Roles = "User,Admin")]
        [HttpGet("product/{id}")]
        public async Task<ActionResult<ProductDetails>> GetProduct(int id)
        {
            var product = await _context.ProductDetails.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("product/{id}/price")]
        public async Task<IActionResult> AdjustPrice(int id, [FromBody] decimal newPrice)
        {
            _log.Error("AdjustPrice --> Starts");
            if (newPrice < 0)
                return BadRequest("Price cannot be negative.");
            var product = await _context.ProductDetails.FindAsync(id);
            if (product == null)
                return NotFound();
            product.Price = newPrice;
            await _context.SaveChangesAsync();
            _log.Error("AdjustPrice --> Ends");
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("createProduct")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDetails product)
        {
            _log.Error("CreateProduct --> Starts");
            _log.Error($"CreateProduct Request : {JsonConvert.SerializeObject(product)}");

            if (_context.ProductDetails.Any(p => p.SKU == product.SKU))
            {
                return Conflict("SKU must be unique.");
            }

            _context.ProductDetails.Add(product);

            await _context.SaveChangesAsync();

            _log.Error($"CreateProduct Response : {JsonConvert.SerializeObject(CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product))}");
            _log.Error("CreateProduct --> Ends");

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, product);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("updateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDetails updatedProduct)
        {
            _log.Error("UpdateProduct --> Starts");

            if (id != updatedProduct.ProductId)
                return BadRequest("Product ID mismatch.");
            if (_context.ProductDetails.Any(p => p.SKU == updatedProduct.SKU && p.ProductId != id))
            {
                return Conflict("SKU must be unique.");
            }
            _context.Entry(updatedProduct).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            _log.Error("CreateProduct --> Ends");
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("product/{id}/categories")]
        public async Task<IActionResult> AddCategories(int id, [FromBody] List<string> categories)
        {
            var product = await _context.ProductDetails.FindAsync(id);
            if (product == null)
                return NotFound();

            if (product.Categories == null)
            {
                product.Categories = string.Join(",", categories);
            }
            else
            {
                var existingCategories = product.Categories.Split(',').ToList();
                existingCategories.AddRange(categories);
                product.Categories = string.Join(",", existingCategories.Distinct());
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("productsCategory")]
        public async Task<ActionResult<IEnumerable<ProductDetails>>> GetProducts(string category = null)
        {
            var products = _context.ProductDetails.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Categories.Contains(category));
            }

            return await products.ToListAsync();
        }
        [Authorize(Roles = "User,Admin")]
        [HttpGet("product/{id}/inventory")]
        public async Task<ActionResult<InventoryDetails>> GetProductInventory(int id)
        {
            var inventory = await _context.InventoryDetails
                .FirstOrDefaultAsync(i => i.ProductID == id);

            if (inventory == null)
                return NotFound("Inventory not found for this product.");

            return inventory;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("createProductInventory/{id}")]
        public async Task<IActionResult> CreateOrUpdateInventory(int id, [FromBody] InventoryDetails inventoryDetails)
        {
            var product = await _context.ProductDetails.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            var existingInventory = await _context.InventoryDetails.FirstOrDefaultAsync(i => i.ProductID == id);
            if (existingInventory != null)
            {
                existingInventory.Quantity = inventoryDetails.Quantity;
                existingInventory.WarehouseLocation = inventoryDetails.WarehouseLocation;
                _context.Entry(existingInventory).State = EntityState.Modified;
            }
            else
            {
                inventoryDetails.ProductID = id;
                _context.InventoryDetails.Add(inventoryDetails);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("product/{id}/adjustInventory")]
        public async Task<IActionResult> AdjustInventory(int id, [FromBody] InventoryAdjustmentDto adjustmentDto)
        {
            var product = await _context.ProductDetails.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            var transaction = new InventoryTransaction
            {
                ProductID = id,
                QuantityChanged = adjustmentDto.QuantityChanged,
                Timestamp = DateTime.UtcNow,
                Reason = adjustmentDto.Reason,
                Users = adjustmentDto.User 
            };

            var inventory = await _context.InventoryDetails.FirstOrDefaultAsync(i => i.ProductID == id);
            if (inventory != null)
            {
                inventory.Quantity += adjustmentDto.QuantityChanged; 
                _context.Entry(inventory).State = EntityState.Modified;
            }

            _context.InventoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("product/{id}/inventory/audit")]
        public async Task<ActionResult<IEnumerable<InventoryTransaction>>> GetInventoryAudit(int id)
        {
            var transactions = await _context.InventoryTransactions
                .Where(t => t.ProductID == id)
                .ToListAsync();

            if (!transactions.Any())
                return NotFound("No audit records found for this product.");

            return Ok(transactions);
        }

    }
}
