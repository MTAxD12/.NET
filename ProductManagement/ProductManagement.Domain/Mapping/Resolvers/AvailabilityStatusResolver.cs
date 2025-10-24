namespace ProductManagement.Domain.Mapping.Resolvers
{
    public static class AvailabilityStatusResolver
    {
        public static string Resolve(bool isAvailable, int stock)
        {
            if (!isAvailable)
                return "Out of Stock";
            if (stock <= 0)
                return "Unavailable";
            if (stock == 1)
                return "Last Item";
            if (stock <= 5)
                return "Limited Stock";

            return "In Stock";
        }
    }
}