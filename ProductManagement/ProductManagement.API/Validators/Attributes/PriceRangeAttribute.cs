using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ProductManagement.API.Validators.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class PriceRangeAttribute : ValidationAttribute
    {
        private readonly decimal _minPrice;
        private readonly decimal _maxPrice;

        public PriceRangeAttribute(double minPrice, double maxPrice)
        {
            _minPrice = (decimal)minPrice;
            _maxPrice = (decimal)maxPrice;
            ErrorMessage = $"Price must be between {_minPrice:C} and {_maxPrice:C}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is decimal price)
            {
                if (price >= _minPrice && price <= _maxPrice)
                    return ValidationResult.Success;

                return new ValidationResult(ErrorMessage);
            }

            return new ValidationResult("Invalid price type.");
        }
    }
}
