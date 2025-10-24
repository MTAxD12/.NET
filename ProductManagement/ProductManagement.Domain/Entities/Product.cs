using System;
using ProductManagement.Domain.Enums;

namespace ProductManagement.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;

        public ProductCategory Category { get; set; }

        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }

        public string? ImageUrl { get; set; }

        public int StockQuantity { get; set; } = 0;
        public bool IsAvailable => StockQuantity > 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}