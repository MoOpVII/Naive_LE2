namespace InventoryDataLibrary.Models
{
    public class StockAdjustmentModel
    {
        public int ItemId { get; set; }
        public int QuantityChange { get; set; }
        public string? ExternalReference { get; set; }
        public string? Notes { get; set; }
    }
}