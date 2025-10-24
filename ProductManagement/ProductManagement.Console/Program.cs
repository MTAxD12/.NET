using System;
using ProductManagement.Domain.DTO;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enums;

namespace ProductManagement.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var product = new Product
            {
                Name = "test",
                Brand = "Asad DA",
                SKU = "sd-2eds",
                Category = ProductCategory.Electronics,
                Price = 1399.99m,
                ReleaseDate = new DateTime(2024, 9, 1),
                StockQuantity = 10
            };

            Console.WriteLine($"Product: {product.Name} ({product.Category})");
            Console.WriteLine($"Available: {product.IsAvailable}");
            Console.WriteLine($"Created At: {product.CreatedAt}");
            Console.WriteLine($"Brand: {product.Brand}");
        }
    }
}