using InventoryDataLibrary.Data;
using InventoryDataLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ISqlData _db;

        public ReportsController(ISqlData db)
        {
            _db = db;
        }

        [HttpGet("inventory-summary")]
        public ActionResult<InventorySummaryModel> InventorySummary()
        {
            return Ok(_db.GetInventorySummary());
        }
    }
}