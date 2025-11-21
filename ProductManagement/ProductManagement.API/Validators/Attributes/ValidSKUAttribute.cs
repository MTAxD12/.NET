using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.RegularExpressions;

namespace ProductManagement.API.Validators.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ValidSKUAttribute : ValidationAttribute, IClientModelValidator
    {
        public ValidSKUAttribute()
        {
            ErrorMessage = "SKU format is invalid. Must be alphanumeric with hyphens, 5-20 characters.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            var sku = value.ToString()!.Replace(" ", "");
            var regex = new Regex(@"^[a-zA-Z0-9\-]{5,20}$");

            if (!regex.IsMatch(sku))
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            context.Attributes["data-val"] = "true";
            context.Attributes["data-val-validsku"] = ErrorMessage ?? "Invalid SKU format";
            context.Attributes["data-val-validsku-pattern"] = @"^[a-zA-Z0-9\-]{5,20}$";
        }
    }
}
