using InventoryAPI.Contracts;
using InventoryDataLibrary.Data;
using InventoryDataLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ISqlData _db;

        public SuppliersController(ISqlData db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult<List<SupplierModel>> List([FromQuery] string? search, [FromQuery] bool includeInactive = false)
        {
            return Ok(_db.ListSuppliers(search, includeInactive));
        }

        [HttpGet("{id:int}")]
        public ActionResult<SupplierModel> Get(int id)
        {
            var supplier = _db.GetSupplierById(id);
            if (supplier == null || !supplier.IsActive) return NotFound();
            return Ok(supplier);
        }

        [HttpPost]
        public IActionResult Add([FromBody] SupplierRequest request)
        {
            request.Name = request.Name.Trim();
            if (request.Name.Length == 0) return BadRequest("Supplier name is required.");

            var id = _db.AddSupplier(ToModel(request));
            return CreatedAtAction(nameof(Get), new { id }, _db.GetSupplierById(id));
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] SupplierRequest request)
        {
            var current = _db.GetSupplierById(id);
            if (current == null || !current.IsActive) return NotFound();

            request.Name = request.Name.Trim();
            if (request.Name.Length == 0) return BadRequest("Supplier name is required.");
            var supplier = ToModel(request);
            supplier.Id = id;
            supplier.IsActive = current.IsActive;
            _db.UpdateSupplier(supplier);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Retire(int id)
        {
            try
            {
                _db.RetireSupplier(id);
                return NoContent();
            }
            catch (SqlException ex) when (ex.Number == 51030)
            {
                return NotFound();
            }
        }

        private static SupplierModel ToModel(SupplierRequest request)
        {
            return new SupplierModel
            {
                Name = request.Name,
                ContactName = string.IsNullOrWhiteSpace(request.ContactName) ? null : request.ContactName.Trim(),
                Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim()
            };
        }
    }
}