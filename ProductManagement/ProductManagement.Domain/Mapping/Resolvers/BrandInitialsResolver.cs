namespace ProductManagement.Domain.Mapping.Resolvers
{
    public static class BrandInitialsResolver
    {
        public static string GetInitials(string? brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
                return "?";

            var parts = brand.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return parts[0][0].ToString().ToUpper();

            return $"{parts.First()[0]}{parts.Last()[0]}".ToUpper();
        }
    }
}