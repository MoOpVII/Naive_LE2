namespace InventoryDataLibrary.Models
{
    public class InventorySummaryModel
    {
        public long ProductCount { get; set; }
        public long TotalUnits { get; set; }
        public long LowStockCount { get; set; }
        public decimal CostValue { get; set; }
        public decimal RetailValue { get; set; }
    }
}