using System;

namespace ProductManagement.Domain.DTO
{
    public class ProductProfileDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;

        public string CategoryDisplayName { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public string FormattedPrice => $"{Price:C}";

        public DateTime ReleaseDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; }
        public int StockQuantity { get; set; }

        public string ProductAge => $"{(DateTime.Now - ReleaseDate).Days / 365} years old";

        public string BrandInitials =>
            string.Join("", Brand.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(b => b[0]))
                .ToUpper();

        public string AvailabilityStatus => IsAvailable ? "In Stock" : "Out of Stock";
    }
}