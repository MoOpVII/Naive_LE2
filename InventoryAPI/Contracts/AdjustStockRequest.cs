using System.ComponentModel.DataAnnotations;

namespace InventoryAPI.Contracts
{
    public class AdjustStockRequest : IValidatableObject
    {
        [Range(1, int.MaxValue)]
        public int ItemId { get; set; }

        public int QuantityChange { get; set; }

        [StringLength(100)]
        public string? ExternalReference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (QuantityChange == 0 || QuantityChange == int.MinValue)
                yield return new ValidationResult("Adjustment must be a non-zero, representable quantity change.", new[] { nameof(QuantityChange) });
        }
    }
}