namespace ProductManagement.Domain.Mapping.Resolvers
{
    public static class AvailabilityStatusResolver
    {
        public static string Resolve(bool isAvailable, int stock)
        {
            if (!isAvailable || stock <= 0)
                return "Out of Stock";
            if (stock == 1)
                return "Last Item";
            if (stock <= 10)
                return "Limited Stock";

            return "In Stock";
        }
    }
}