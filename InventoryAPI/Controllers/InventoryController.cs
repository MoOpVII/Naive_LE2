using InventoryAPI.Contracts;
using InventoryDataLibrary.Data;
using InventoryDataLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly ISqlData _db;

        public InventoryController(ISqlData db)
        {
            _db = db;
        }

        [HttpPost("receipts")]
        public IActionResult Receive([FromBody] ReceiveStockRequest request)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            try
            {
                _db.ReceiveStock(new StockReceiptModel
                {
                    ItemId = request.ItemId,
                    SupplierId = request.SupplierId,
                    Quantity = request.Quantity,
                    UnitCost = request.UnitCost,
                    ExternalReference = Clean(request.ExternalReference),
                    Notes = Clean(request.Notes)
                }, userId);
                return Ok();
            }
            catch (SqlException ex) when (ex.Number == 51002)
            {
                return NotFound("Active supplier not found.");
            }
            catch (SqlException ex) when (ex.Number == 51003)
            {
                return NotFound("Active item not found.");
            }
        }

        [HttpPost("issues")]
        public IActionResult Issue([FromBody] IssueStockRequest request)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            try
            {
                _db.IssueStock(new StockIssueModel
                {
                    ItemId = request.ItemId,
                    Quantity = request.Quantity,
                    ExternalReference = Clean(request.ExternalReference),
                    Notes = Clean(request.Notes)
                }, userId);
                return Ok();
            }
            catch (SqlException ex) when (ex.Number == 51011)
            {
                return Conflict("Item is unavailable or does not have enough stock.");
            }
        }

        [HttpPost("adjustments")]
        public IActionResult Adjust([FromBody] AdjustStockRequest request)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();
            try
            {
                _db.AdjustStock(new StockAdjustmentModel
                {
                    ItemId = request.ItemId,
                    QuantityChange = request.QuantityChange,
                    ExternalReference = Clean(request.ExternalReference),
                    Notes = Clean(request.Notes)
                }, userId);
                return Ok();
            }
            catch (SqlException ex) when (ex.Number == 51013)
            {
                return Conflict("Item is unavailable or the adjustment would make stock negative.");
            }
        }

        private bool TryGetUserId(out int userId)
        {
            return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}