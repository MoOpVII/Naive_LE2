using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Contracts
{
    public class SupplierRequest
    {
        [Required, StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContactName { get; set; }

        [EmailAddress, StringLength(254)]
        public string? Email { get; set; }

        [Phone, StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }
    }
}