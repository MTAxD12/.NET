namespace ProductManagement.Domain.Mapping.Resolvers
{
    public static class ProductAgeResolver
    {
        public static string Resolve(DateTime releaseDate)
        {
            var ageDays = (DateTime.UtcNow - releaseDate).TotalDays;

            if (ageDays < 30)
                return "New Release";
            if (ageDays < 365)
                return $"{Math.Floor(ageDays / 30)} months old";
            if (ageDays < 1825)
                return $"{Math.Floor(ageDays / 365)} years old";
            if (Math.Abs(ageDays - 1825) < 1)
                return "Classic";

            return "Vintage";
        }
    }
}