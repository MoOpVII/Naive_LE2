using InventoryDataLibrary.Models;
using System.Collections.Generic;

namespace InventoryDataLibrary.Data
{
    public interface ISqlData
    {
        UserModel Authenticate(string username);
        void Register(string username, string firstName, string lastName, string password);

        List<ItemModel> ListItems();
        ItemModel GetItemByCode(string code);
        void AddItem(ItemModel item);
        void UpdateItemPrice(string code, decimal price);
        List<string> ListBrands();
    }
}
