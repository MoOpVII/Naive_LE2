namespace InventoryDataLibrary.Models
{
    public class StockReceiptModel
    {
        public int ItemId { get; set; }
        public int SupplierId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string? ExternalReference { get; set; }
        public string? Notes { get; set; }
    }
}