using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using ProductManagement.API.Data;
using ProductManagement.API.Features.Products;
using ProductManagement.API.Validators;
using ProductManagement.Domain.DTO;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enums;
using ProductManagement.Domain.Mapping;
using Xunit;

namespace ProductManagement.Tests
{
    public class CreateProductHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly Mock<ILogger<CreateProductHandler>> _handlerLoggerMock;
        private readonly Mock<ILogger<CreateProductProfileValidator>> _validatorLoggerMock;
        private readonly CreateProductHandler _handler;
        private readonly IValidator<CreateProductProfileRequest> _validator;

        public CreateProductHandlerIntegrationTests()
        {
            // Setup in-memory database with unique name
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationContext(options);

            // Setup AutoMapper with product profile
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AdvancedProductMappingProfile>();
            });
            _mapper = config.CreateMapper();

            // Setup memory cache
            _cache = new MemoryCache(new MemoryCacheOptions());

            // Setup mocked loggers
            _handlerLoggerMock = new Mock<ILogger<CreateProductHandler>>();
            _validatorLoggerMock = new Mock<ILogger<CreateProductProfileValidator>>();

            // Setup validator
            _validator = new CreateProductProfileValidator(_context, _validatorLoggerMock.Object);

            // Create handler
            _handler = new CreateProductHandler(_context, _mapper, _cache, _handlerLoggerMock.Object, _validator);
        }

        [Fact]
        public async Task Handle_ValidElectronicsProductRequest_CreatesProductWithCorrectMappings()
        {
            var request = new CreateProductProfileRequest
            {
                Name = "Smart Wireless Headphones",
                Brand = "Tech Audio",
                SKU = "TA-WH-2024",
                Category = ProductCategory.Electronics,
                Price = 149.99m,
                ReleaseDate = new DateTime(2023, 6, 15),
                StockQuantity = 15,
                ImageUrl = "https://example.com/headphones.jpg"
            };

            var result = await _handler.HandleAsync(request);

            Assert.NotNull(result);
            Assert.Equal("Electronics & Technology", result.CategoryDisplayName);
            Assert.Equal("TA", result.BrandInitials); 
            Assert.StartsWith("$", result.FormattedPrice);
            Assert.Contains("year", result.ProductAge.ToLower());
            Assert.Equal("In Stock", result.AvailabilityStatus);
            Assert.Equal(request.ImageUrl, result.ImageUrl); 

            _handlerLoggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.Is<EventId>(e => e.Id == 2001),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DuplicateSKU_ThrowsValidationExceptionWithLogging()
        {
            var existingProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Existing Product",
                Brand = "Existing Brand",
                SKU = "DUPLICATE-SKU",
                Category = ProductCategory.Books,
                Price = 29.99m,
                ReleaseDate = DateTime.UtcNow.AddMonths(-2),
                StockQuantity = 5,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Products.AddAsync(existingProduct);
            await _context.SaveChangesAsync();

            var request = new CreateProductProfileRequest
            {
                Name = "New Product",
                Brand = "New Brand",
                SKU = "DUPLICATE-SKU",
                Category = ProductCategory.Electronics,
                Price = 99.99m,
                ReleaseDate = DateTime.UtcNow.AddMonths(-1),
                StockQuantity = 10
            };

            var exception = await Assert.ThrowsAsync<ValidationException>(
                () => _handler.HandleAsync(request));

            Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);

            _handlerLoggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.Is<EventId>(e => e.Id == 2002),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_HomeProductRequest_AppliesDiscountAndConditionalMapping()
        {
            var request = new CreateProductProfileRequest
            {
                Name = "Decorative Lamp",
                Brand = "Home Decor",
                SKU = "HD-LAMP-001",
                Category = ProductCategory.Home,
                Price = 100.00m,
                ReleaseDate = DateTime.UtcNow.AddYears(-1),
                StockQuantity = 8,
                ImageUrl = "https://example.com/lamp.jpg"
            };

            var result = await _handler.HandleAsync(request);

            Assert.NotNull(result);
            Assert.Equal("Home & Garden", result.CategoryDisplayName);
            Assert.Equal(90.00m, result.Price); 
            Assert.Null(result.ImageUrl);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
