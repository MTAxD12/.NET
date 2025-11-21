using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ProductManagement.API.Common.Logging;
using ProductManagement.API.Data;
using ProductManagement.Domain.DTO;
using ProductManagement.Domain.Entities;
using System.Diagnostics;

namespace ProductManagement.API.Features.Products
{
    public class CreateProductHandler
    {
        private readonly ApplicationContext _context;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CreateProductHandler> _logger;
        private readonly IValidator<CreateProductProfileRequest> _validator;

        public CreateProductHandler(
            ApplicationContext context,
            IMapper mapper,
            IMemoryCache cache,
            ILogger<CreateProductHandler> logger,
            IValidator<CreateProductProfileRequest> validator)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
            _validator = validator;
        }

        public async Task<ProductProfileDto> HandleAsync(CreateProductProfileRequest request, CancellationToken cancellationToken = default)
        {
            var operationId = Guid.NewGuid().ToString()[..8];
            var totalStopwatch = Stopwatch.StartNew();
            var validationStopwatch = new Stopwatch();
            var dbStopwatch = new Stopwatch();

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["OperationId"] = operationId,
                ["ProductName"] = request.Name,
                ["SKU"] = request.SKU,
                ["Category"] = request.Category
            }))
            {
                try
                {
                    _logger.LogInformation(
                        LogEvents.ProductCreationStarted,
                        "Starting product creation | OperationId: {OperationId} | Name: {Name} | Brand: {Brand} | SKU: {SKU} | Category: {Category}",
                        operationId, request.Name, request.Brand, request.SKU, request.Category);

=                    validationStopwatch.Start();
                    
                    _logger.LogInformation(LogEvents.SKUValidationPerformed, "Performing SKU validation for: {SKU}", request.SKU);
                    
                    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                    
                    if (!validationResult.IsValid)
                    {
                        validationStopwatch.Stop();
                        
                        var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                        
                        _logger.LogWarning(
                            LogEvents.ProductValidationFailed,
                            "Product validation failed | OperationId: {OperationId} | SKU: {SKU} | Errors: {Errors}",
                            operationId, request.SKU, errors);

                        throw new ValidationException(validationResult.Errors);
                    }

                    _logger.LogInformation(LogEvents.StockValidationPerformed, 
                        "Stock validation completed | StockQuantity: {StockQuantity}", request.StockQuantity);

                    validationStopwatch.Stop();

                    dbStopwatch.Start();

                    _logger.LogInformation(
                        LogEvents.DatabaseOperationStarted,
                        "Starting database operation | OperationId: {OperationId}",
                        operationId);

                    var product = _mapper.Map<Product>(request);

                    await _context.Products.AddAsync(product, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        LogEvents.DatabaseOperationCompleted,
                        "Database operation completed | OperationId: {OperationId} | ProductId: {ProductId}",
                        operationId, product.Id);

                    dbStopwatch.Stop();

                    _cache.Remove("all_products");
                    _logger.LogInformation(
                        LogEvents.CacheOperationPerformed,
                        "Cache invalidated | CacheKey: all_products | OperationId: {OperationId}",
                        operationId);

                    var result = _mapper.Map<ProductProfileDto>(product);

                    totalStopwatch.Stop();

                    var metrics = new ProductCreationMetrics
                    {
                        OperationId = operationId,
                        ProductName = request.Name,
                        SKU = request.SKU,
                        Category = request.Category,
                        ValidationDuration = validationStopwatch.Elapsed,
                        DatabaseSaveDuration = dbStopwatch.Elapsed,
                        TotalDuration = totalStopwatch.Elapsed,
                        Success = true,
                        ErrorReason = null
                    };

                    _logger.LogProductCreationMetrics(metrics);

                    return result;
                }
                catch (Exception ex)
                {
                    totalStopwatch.Stop();

                    var errorMetrics = new ProductCreationMetrics
                    {
                        OperationId = operationId,
                        ProductName = request.Name,
                        SKU = request.SKU,
                        Category = request.Category,
                        ValidationDuration = validationStopwatch.Elapsed,
                        DatabaseSaveDuration = dbStopwatch.Elapsed,
                        TotalDuration = totalStopwatch.Elapsed,
                        Success = false,
                        ErrorReason = ex.Message
                    };

                    _logger.LogProductCreationMetrics(errorMetrics);
                    
                    throw;
                }
            }
        }
    }
}
