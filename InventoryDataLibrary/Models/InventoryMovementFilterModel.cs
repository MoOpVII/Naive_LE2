namespace InventoryDataLibrary.Models
{
    public class InventoryMovementFilterModel
    {
        public int? ItemId { get; set; }
        public int? SupplierId { get; set; }
        public string? MovementType { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
    }
}