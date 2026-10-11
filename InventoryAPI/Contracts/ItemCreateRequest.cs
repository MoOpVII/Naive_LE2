using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Contracts
{
    public class ItemCreateRequest
    {
        [Required, StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50, MinimumLength = 1)]
        public string Code { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Brand { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal UnitPrice { get; set; }

        [Range(typeof(decimal), "0", "9999999999999999.99")]
        public decimal UnitCost { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderLevel { get; set; }
    }
}