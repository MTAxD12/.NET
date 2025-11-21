using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductManagement.API.Common.Logging;
using ProductManagement.API.Data;
using ProductManagement.Domain.DTO;
using ProductManagement.Domain.Enums;
using System.Text.RegularExpressions;

namespace ProductManagement.API.Validators
{
    public class CreateProductProfileValidator : AbstractValidator<CreateProductProfileRequest>
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<CreateProductProfileValidator> _logger;

        private static readonly string[] InappropriateWords = { "badword1", "badword2", "inappropriate" };
        private static readonly string[] HomeRestrictedWords = { "weapon", "violent", "adult" };
        private static readonly string[] TechnologyKeywords = { "smart", "digital", "tech", "electronic", "wireless", "bluetooth", "wifi", "computer", "mobile", "processor" };

        public CreateProductProfileValidator(ApplicationContext context, ILogger<CreateProductProfileValidator> logger)
        {
            _context = context;
            _logger = logger;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MinimumLength(1).WithMessage("Product name must be at least 1 character.")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.")
                .Must(BeValidName).WithMessage("Product name contains inappropriate content.")
                .MustAsync(BeUniqueName).WithMessage("A product with this name already exists for this brand.");

            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Brand name is required.")
                .MinimumLength(2).WithMessage("Brand name must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Brand name cannot exceed 100 characters.")
                .Must(BeValidBrandName).WithMessage("Brand name contains invalid characters.");

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required.")
                .Must(BeValidSKU).WithMessage("SKU format is invalid. Must be alphanumeric with hyphens, 5-20 characters.")
                .MustAsync(BeUniqueSKU).WithMessage("SKU already exists in the system.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Invalid product category.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .LessThan(10000).WithMessage("Price must be less than $10,000.");

            RuleFor(x => x.ReleaseDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Release date cannot be in the future.")
                .GreaterThan(new DateTime(1900, 1, 1)).WithMessage("Release date cannot be before year 1900.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
                .LessThanOrEqualTo(100000).WithMessage("Stock quantity cannot exceed 100,000.");

            RuleFor(x => x.ImageUrl)
                .Must(BeValidImageUrl).When(x => !string.IsNullOrWhiteSpace(x.ImageUrl))
                .WithMessage("Image URL must be a valid HTTP/HTTPS URL ending with .jpg, .jpeg, .png, .gif, or .webp.");

            RuleFor(x => x)
                .MustAsync(PassBusinessRules).WithMessage("Product does not meet business requirements.");

            When(x => x.Category == ProductCategory.Electronics, () =>
            {
                RuleFor(x => x.Price)
                    .GreaterThanOrEqualTo(50).WithMessage("Electronics products must have a minimum price of $50.00.");

                RuleFor(x => x.Name)
                    .Must(ContainTechnologyKeywords).WithMessage("Electronics products must contain technology-related keywords in the name.");

                RuleFor(x => x.ReleaseDate)
                    .GreaterThan(DateTime.UtcNow.AddYears(-5)).WithMessage("Electronics products must be released within the last 5 years.");
            });

            When(x => x.Category == ProductCategory.Home, () =>
            {
                RuleFor(x => x.Price)
                    .LessThanOrEqualTo(200).WithMessage("Home products must have a maximum price of $200.00.");

                RuleFor(x => x.Name)
                    .Must(BeAppropriateForHome).WithMessage("Home product name contains restricted content.");
            });

            When(x => x.Category == ProductCategory.Clothing, () =>
            {
                RuleFor(x => x.Brand)
                    .MinimumLength(3).WithMessage("Clothing brand name must be at least 3 characters.");
            });

            RuleFor(x => x)
                .Must(x => !(x.Price > 100 && x.StockQuantity > 20))
                .WithMessage("Expensive products (>$100) must have limited stock (≤20 units).");
        }

        private bool BeValidName(string name)
        {
            return !InappropriateWords.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> BeUniqueName(CreateProductProfileRequest request, string name, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validating name uniqueness for Name: {Name}, Brand: {Brand}", name, request.Brand);
            
            var exists = await _context.Products
                .AnyAsync(p => p.Name == name && p.Brand == request.Brand, cancellationToken);

            return !exists;
        }

        private bool BeValidBrandName(string brand)
        {
            var regex = new Regex(@"^[a-zA-Z0-9\s\-'\.]+$");
            return regex.IsMatch(brand);
        }

        private bool BeValidSKU(string sku)
        {
            var cleanedSKU = sku.Replace(" ", "");
            var regex = new Regex(@"^[a-zA-Z0-9\-]{5,20}$");
            return regex.IsMatch(cleanedSKU);
        }

        private async Task<bool> BeUniqueSKU(string sku, CancellationToken cancellationToken)
        {
            _logger.LogInformation(LogEvents.SKUValidationPerformed, "Validating SKU uniqueness: {SKU}", sku);
            
            var exists = await _context.Products.AnyAsync(p => p.SKU == sku, cancellationToken);
            
            return !exists;
        }

        private bool BeValidImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return true;

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                return false;

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return false;

            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            return validExtensions.Any(ext => imageUrl.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> PassBusinessRules(CreateProductProfileRequest request, CancellationToken cancellationToken)
        {
            //  addition limit (max 500 per day)
            var today = DateTime.UtcNow.Date;
            var todayCount = await _context.Products
                .CountAsync(p => p.CreatedAt.Date == today, cancellationToken);

            if (todayCount >= 500)
            {
                _logger.LogWarning("Daily product addition limit reached: {Count}/500", todayCount);
                return false;
            }

            if (request.Category == ProductCategory.Electronics && request.Price < 50)
            {
                _logger.LogWarning("Electronics product below minimum price: {Price}", request.Price);
                return false;
            }

            if (request.Category == ProductCategory.Home)
            {
                if (HomeRestrictedWords.Any(word => request.Name.Contains(word, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning("Home product contains restricted word in name: {Name}", request.Name);
                    return false;
                }
            }

            if (request.Price > 500 && request.StockQuantity > 10)
            {
                _logger.LogWarning("High-value product exceeds stock limit: Price={Price}, Stock={Stock}", 
                    request.Price, request.StockQuantity);
                return false;
            }

            return true;
        }

        private bool ContainTechnologyKeywords(string name)
        {
            return TechnologyKeywords.Any(keyword => name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private bool BeAppropriateForHome(string name)
        {
            return !HomeRestrictedWords.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase));
        }
    }
}
