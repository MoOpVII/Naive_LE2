using InventoryDataLibrary.Data;
using InventoryDataLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/inventory/movements")]
    public class InventoryMovementsController : ControllerBase
    {
        private static readonly string[] MovementTypes = { "Receipt", "Issue", "Adjustment" };
        private readonly ISqlData _db;

        public InventoryMovementsController(ISqlData db)
        {
            _db = db;
        }

        [HttpGet]
        public ActionResult<List<InventoryMovementModel>> List([FromQuery] InventoryMovementFilterModel filter)
        {
            if (filter.MovementType != null && !MovementTypes.Contains(filter.MovementType, StringComparer.OrdinalIgnoreCase))
                return BadRequest("Movement type must be Receipt, Issue, or Adjustment.");
            if (filter.FromUtc.HasValue && filter.ToUtc.HasValue && filter.ToUtc <= filter.FromUtc)
                return BadRequest("The end date must be after the start date.");

            if (filter.MovementType != null)
                filter.MovementType = MovementTypes.First(type => string.Equals(type, filter.MovementType, StringComparison.OrdinalIgnoreCase));

            return Ok(_db.ListMovements(filter));
        }
    }
}