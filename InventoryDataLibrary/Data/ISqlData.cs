using InventoryDataLibrary.Models;
using System.Collections.Generic;

namespace InventoryDataLibrary.Data
{
    public interface ISqlData
    {
        UserModel Authenticate(string username);
        void Register(string username, string firstName, string lastName, string password);

        List<ItemModel> ListItems(string? search = null, string? brand = null, bool includeInactive = false, bool lowStockOnly = false);
        ItemModel? GetItemById(int id);
        ItemModel? GetItemByCode(string code);
        int AddItem(ItemModel item);
        void UpdateItem(ItemModel item);
        void RetireItem(int id);
        void UpdateItemPrice(string code, decimal price);
        List<string> ListBrands();

        List<SupplierModel> ListSuppliers(string? search = null, bool includeInactive = false);
        SupplierModel? GetSupplierById(int id);
        int AddSupplier(SupplierModel supplier);
        void UpdateSupplier(SupplierModel supplier);
        void RetireSupplier(int id);

        void ReceiveStock(StockReceiptModel receipt, int createdByUserId);
        void IssueStock(StockIssueModel issue, int createdByUserId);
        void AdjustStock(StockAdjustmentModel adjustment, int createdByUserId);
        List<InventoryMovementModel> ListMovements(InventoryMovementFilterModel filter);
        InventorySummaryModel GetInventorySummary();
    }
}
