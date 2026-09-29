using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryDataLibrary.Models
{
    public class ItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Brand { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
