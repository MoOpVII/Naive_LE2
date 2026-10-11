namespace InventoryDataLibrary.Models
{
    public class StockIssueModel
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string? ExternalReference { get; set; }
        public string? Notes { get; set; }
    }
}