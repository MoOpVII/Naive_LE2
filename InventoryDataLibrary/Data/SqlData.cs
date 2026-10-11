using InventoryDataLibrary.Database;
using InventoryDataLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace InventoryDataLibrary.Data
{
    public class SqlData : ISqlData
    {
        private ISqlDataAccess _db;
        private const string connectionStringName = "SqlDb";

        public SqlData(ISqlDataAccess db)
        {
            _db = db;
        }

        public UserModel Authenticate(string username)
        {
            UserModel result = _db.LoadData<UserModel, dynamic>("dbo.spUsers_Authenticate", new { username }, connectionStringName, true).FirstOrDefault();
            return result;
        }

        public void Register(string username, string firstName, string lastName, string password)
        {
            _db.SaveData<dynamic>("dbo.spUsers_Register", new { username, firstName, lastName, password }, connectionStringName, true);
        }

        public List<ItemModel> ListItems(string? search = null, string? brand = null, bool includeInactive = false, bool lowStockOnly = false)
        {
            return _db.LoadData<ItemModel, dynamic>("dbo.spItems_List", new
            {
                search = string.IsNullOrWhiteSpace(search) ? null : search,
                brand = string.IsNullOrWhiteSpace(brand) ? null : brand,
                includeInactive,
                lowStockOnly
            }, connectionStringName, true).ToList();
        }

        public ItemModel? GetItemById(int id)
        {
            return _db.LoadData<ItemModel, dynamic>("dbo.spItems_GetById", new { id }, connectionStringName, true).FirstOrDefault();
        }

        public ItemModel? GetItemByCode(string code)
        {
            return _db.LoadData<ItemModel, dynamic>("dbo.spItems_GetByCode", new { code }, connectionStringName, true).FirstOrDefault();
        }

        public int AddItem(ItemModel item)
        {
            return _db.LoadData<int, dynamic>("dbo.spItems_Add", new
            {
                item.Name,
                item.Code,
                item.Brand,
                item.UnitPrice,
                item.UnitCost,
                item.ReorderLevel
            }, connectionStringName, true).Single();
        }

        public void UpdateItem(ItemModel item)
        {
            _db.SaveData<dynamic>("dbo.spItems_Update", new
            {
                item.Id,
                item.Name,
                item.Code,
                item.Brand,
                item.UnitPrice,
                item.UnitCost,
                item.ReorderLevel
            }, connectionStringName, true);
        }

        public void RetireItem(int id)
        {
            _db.SaveData<dynamic>("dbo.spItems_Retire", new { id }, connectionStringName, true);
        }

        public void UpdateItemPrice(string code, decimal price)
        {
            _db.SaveData<dynamic>("dbo.spItems_UpdatePrice", new { code, price }, connectionStringName, true);
        }

        public List<string> ListBrands()
        {
            return _db.LoadData<string, dynamic>("dbo.spItems_Brands", new { }, connectionStringName, true).ToList();
        }

        public List<SupplierModel> ListSuppliers(string? search = null, bool includeInactive = false)
        {
            return _db.LoadData<SupplierModel, dynamic>("dbo.spSuppliers_List", new
            {
                search = string.IsNullOrWhiteSpace(search) ? null : search,
                includeInactive
            }, connectionStringName, true).ToList();
        }

        public SupplierModel? GetSupplierById(int id)
        {
            return _db.LoadData<SupplierModel, dynamic>("dbo.spSuppliers_GetById", new { id }, connectionStringName, true).FirstOrDefault();
        }

        public int AddSupplier(SupplierModel supplier)
        {
            return _db.LoadData<int, dynamic>("dbo.spSuppliers_Add", new
            {
                supplier.Name,
                supplier.ContactName,
                supplier.Email,
                supplier.Phone,
                supplier.Address
            }, connectionStringName, true).Single();
        }

        public void UpdateSupplier(SupplierModel supplier)
        {
            _db.SaveData<dynamic>("dbo.spSuppliers_Update", new
            {
                supplier.Id,
                supplier.Name,
                supplier.ContactName,
                supplier.Email,
                supplier.Phone,
                supplier.Address
            }, connectionStringName, true);
        }

        public void RetireSupplier(int id)
        {
            _db.SaveData<dynamic>("dbo.spSuppliers_Retire", new { id }, connectionStringName, true);
        }

        public void ReceiveStock(StockReceiptModel receipt, int createdByUserId)
        {
            _db.SaveData<dynamic>("dbo.spInventory_Receive", new
            {
                receipt.ItemId,
                receipt.SupplierId,
                receipt.Quantity,
                receipt.UnitCost,
                receipt.ExternalReference,
                receipt.Notes,
                createdByUserId
            }, connectionStringName, true);
        }

        public void IssueStock(StockIssueModel issue, int createdByUserId)
        {
            _db.SaveData<dynamic>("dbo.spInventory_Issue", new
            {
                issue.ItemId,
                issue.Quantity,
                issue.ExternalReference,
                issue.Notes,
                createdByUserId
            }, connectionStringName, true);
        }

        public void AdjustStock(StockAdjustmentModel adjustment, int createdByUserId)
        {
            _db.SaveData<dynamic>("dbo.spInventory_Adjust", new
            {
                adjustment.ItemId,
                adjustment.QuantityChange,
                adjustment.ExternalReference,
                adjustment.Notes,
                createdByUserId
            }, connectionStringName, true);
        }

        public List<InventoryMovementModel> ListMovements(InventoryMovementFilterModel filter)
        {
            return _db.LoadData<InventoryMovementModel, dynamic>("dbo.spInventory_ListMovements", new
            {
                filter.ItemId,
                filter.SupplierId,
                filter.MovementType,
                filter.FromUtc,
                filter.ToUtc
            }, connectionStringName, true).ToList();
        }

        public InventorySummaryModel GetInventorySummary()
        {
            return _db.LoadData<InventorySummaryModel, dynamic>("dbo.spInventory_Summary", new { }, connectionStringName, true).Single();
        }
    }
}