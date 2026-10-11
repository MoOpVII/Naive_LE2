using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Contracts
{
    public class IssueStockRequest
    {
        [Range(1, int.MaxValue)]
        public int ItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [StringLength(100)]
        public string? ExternalReference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}