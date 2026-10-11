using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Contracts
{
    public class ReceiveStockRequest
    {
        [Range(1, int.MaxValue)]
        public int ItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int SupplierId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal UnitCost { get; set; }

        [StringLength(100)]
        public string? ExternalReference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}