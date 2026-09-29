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

        public List<ItemModel> ListItems()
        {
            return _db.LoadData<ItemModel, dynamic>("dbo.spItems_List", new { }, connectionStringName, true).ToList();
        }

        public ItemModel GetItemByCode(string code)
        {
            return _db.LoadData<ItemModel, dynamic>("dbo.spItems_GetByCode", new { code }, connectionStringName, true).FirstOrDefault();
        }

        public void AddItem(ItemModel item)
        {
            _db.SaveData<dynamic>("dbo.spItems_Add", new { item.Name, item.Code, item.Brand, item.UnitPrice }, connectionStringName, true);
        }

        public void UpdateItemPrice(string code, decimal price)
        {
            _db.SaveData<dynamic>("dbo.spItems_UpdatePrice", new { code, price }, connectionStringName, true);
        }

        public List<string> ListBrands()
        {
            return _db.LoadData<string, dynamic>("dbo.spItems_Brands", new { }, connectionStringName, true).ToList();
        }
    }
}