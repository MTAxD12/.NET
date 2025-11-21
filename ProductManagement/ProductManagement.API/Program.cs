using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductManagement.API.Common.Middleware;
using ProductManagement.API.Data;
using ProductManagement.API.Features.Products;
using ProductManagement.API.Validators;
using ProductManagement.Domain.DTO;
using ProductManagement.Domain.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database - In-Memory for testing
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseInMemoryDatabase("ProductManagementDb"));

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AdvancedProductMappingProfile>();
});

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IValidator<CreateProductProfileRequest>, CreateProductProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductProfileValidator>();

// Register Handler
builder.Services.AddScoped<CreateProductHandler>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGet("/products", async (
    ApplicationContext context,
    IMapper mapper) =>
{
    var products = await context.Products.ToListAsync();
    var productDtos = mapper.Map<List<ProductProfileDto>>(products);
    return Results.Ok(productDtos);
})
.WithName("GetAllProducts")
.WithOpenApi(operation =>
{
    operation.Summary = "Get all products";
    operation.Description = "Retrieves all products from the database";
    return operation;
});

app.MapPost("/products", async (
    CreateProductProfileRequest request,
    CreateProductHandler handler,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return Results.Created($"/products/{result.Id}", result);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new
        {
            errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "An error occurred while creating the product");
    }
})
.WithName("CreateProduct")
.WithOpenApi(operation =>
{
    operation.Summary = "Create a new product";
    operation.Description = "Creates a new product with advanced validation, logging, and caching";
    return operation;
});

app.Run();
