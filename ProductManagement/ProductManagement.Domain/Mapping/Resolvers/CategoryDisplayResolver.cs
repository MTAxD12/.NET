using ProductManagement.Domain.Enums;

namespace ProductManagement.Domain.Mapping.Resolvers
{
    public static class CategoryDisplayResolver
    {
        public static string GetDisplayName(ProductCategory category) => category switch
        {
            ProductCategory.Electronics => "Electronics & Technology",
            ProductCategory.Clothing => "Clothing & Fashion",
            ProductCategory.Books => "Books & Media",
            ProductCategory.Home => "Home & Garden",
            _ => "Uncategorized"
        };
    }
}