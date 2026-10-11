namespace InventoryDataLibrary.Models
{
    public class InventoryMovementModel
    {
        public long Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public int QuantityChange { get; set; }
        public decimal UnitCost { get; set; }
        public string? ExternalReference { get; set; }
        public string? Notes { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }
}