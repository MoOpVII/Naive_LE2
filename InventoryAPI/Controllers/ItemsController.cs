using InventoryDataLibrary.Data;
using InventoryDataLibrary.Models;
using InventoryAPI.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly ISqlData _db;

        public ItemsController(ISqlData db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult<List<ItemModel>> Get(
            [FromQuery] string? search,
            [FromQuery] string? brand,
            [FromQuery] bool includeInactive = false,
            [FromQuery] bool lowStockOnly = false)
        {
            return Ok(_db.ListItems(search, brand, includeInactive, lowStockOnly));
        }

        [HttpGet("brands")]
        public ActionResult<List<string>> Brands()
        {
            var brands = _db.ListBrands();
            return Ok(brands);
        }

        [HttpGet("{id:int}")]
        public ActionResult<ItemModel> GetById(int id)
        {
            var item = _db.GetItemById(id);
            if (item == null || !item.IsActive) return NotFound();
            return Ok(item);
        }

        [HttpGet("by-code/{code}")]
        public ActionResult<ItemModel> GetByCode(string code)
        {
            var item = _db.GetItemByCode(code);
            if (item == null || !item.IsActive) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Add([FromBody] ItemCreateRequest request)
        {
            request.Name = request.Name.Trim();
            request.Code = request.Code.Trim();
            request.Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim();
            if (request.Name.Length == 0 || request.Code.Length == 0)
                return BadRequest("Item name and code are required.");

            try
            {
                var id = _db.AddItem(new ItemModel
                {
                    Name = request.Name,
                    Code = request.Code,
                    Brand = request.Brand,
                    UnitPrice = request.UnitPrice,
                    UnitCost = request.UnitCost,
                    ReorderLevel = request.ReorderLevel
                });
                return CreatedAtAction(nameof(GetById), new { id }, _db.GetItemById(id));
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                return Conflict("Item with this code already exists.");
            }
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] ItemUpdateRequest request)
        {
            var current = _db.GetItemById(id);
            if (current == null || !current.IsActive) return NotFound();

            request.Name = request.Name.Trim();
            request.Code = request.Code.Trim();
            request.Brand = string.IsNullOrWhiteSpace(request.Brand) ? null : request.Brand.Trim();
            if (request.Name.Length == 0 || request.Code.Length == 0)
                return BadRequest("Item name and code are required.");

            try
            {
                _db.UpdateItem(new ItemModel
                {
                    Id = id,
                    Name = request.Name,
                    Code = request.Code,
                    Brand = request.Brand,
                    UnitPrice = request.UnitPrice,
                    UnitCost = current.UnitCost,
                    StockQuantity = current.StockQuantity,
                    ReorderLevel = request.ReorderLevel,
                    IsActive = current.IsActive
                });
                return NoContent();
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                return Conflict("Item with this code already exists.");
            }
        }

        [HttpDelete("{id:int}")]
        public IActionResult Retire(int id)
        {
            try
            {
                _db.RetireItem(id);
                return NoContent();
            }
            catch (SqlException ex) when (ex.Number == 51020)
            {
                return NotFound();
            }
            catch (SqlException ex) when (ex.Number == 51021)
            {
                return Conflict("Items with stock on hand cannot be retired.");
            }
        }

        [HttpPut("{code}/price")]
        public IActionResult UpdatePrice(string code, [FromBody] decimal price)
        {
            var item = _db.GetItemByCode(code);
            if (item == null || !item.IsActive) return NotFound();

            if (price < 0) return BadRequest("Price must be non-negative.");

            _db.UpdateItemPrice(code, price);
            return NoContent();
        }
    }
}
