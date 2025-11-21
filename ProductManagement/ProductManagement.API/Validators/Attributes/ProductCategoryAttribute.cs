using System.ComponentModel.DataAnnotations;
using ProductManagement.Domain.Enums;

namespace ProductManagement.API.Validators.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ProductCategoryAttribute : ValidationAttribute
    {
        private readonly ProductCategory[] _allowedCategories;

        public ProductCategoryAttribute(params ProductCategory[] allowedCategories)
        {
            _allowedCategories = allowedCategories;
            ErrorMessage = $"Product category must be one of: {string.Join(", ", allowedCategories.Select(c => c.ToString()))}";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            if (value is ProductCategory category)
            {
                if (_allowedCategories.Contains(category))
                    return ValidationResult.Success;

                return new ValidationResult(ErrorMessage);
            }

            return new ValidationResult("Invalid category type.");
        }
    }
}
