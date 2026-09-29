using InventoryDataLibrary.Data;
using InventoryDataLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

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
        public ActionResult<List<ItemModel>> Get([FromQuery] string brand = null, [FromQuery] string code = null)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                var item = _db.GetItemByCode(code);
                if (item == null) return NotFound();
                return Ok(new List<ItemModel> { item });
            }

            var items = _db.ListItems();
            if (!string.IsNullOrWhiteSpace(brand))
            {
                items = items.FindAll(i => i.Brand != null && i.Brand.ToLower().Contains(brand.ToLower()));
            }

            return Ok(items);
        }

        [HttpGet("brands")]
        public ActionResult<List<string>> Brands()
        {
            var brands = _db.ListBrands();
            return Ok(brands);
        }

        [HttpGet("{code}")]
        public ActionResult<ItemModel> GetByCode(string code)
        {
            var item = _db.GetItemByCode(code);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public IActionResult Add([FromBody] ItemModel item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Code))
                return BadRequest("Item name and code are required.");
            if (item.UnitPrice < 0) return BadRequest("Price must be non-negative.");

            var existing = _db.GetItemByCode(item.Code);
            if (existing != null) return Conflict("Item with this code already exists.");

            try
            {
                _db.AddItem(item);
            }
            catch (SqlException ex) when (ex.Number is 2601 or 2627)
            {
                return Conflict("Item with this code already exists.");
            }

            return CreatedAtAction(nameof(GetByCode), new { code = item.Code }, item);
        }

        [HttpPut("{code}/price")]
        public IActionResult UpdatePrice(string code, [FromBody] decimal price)
        {
            var item = _db.GetItemByCode(code);
            if (item == null) return NotFound();

            if (price < 0) return BadRequest("Price must be non-negative.");

            _db.UpdateItemPrice(code, price);
            return NoContent();
        }
    }
}
