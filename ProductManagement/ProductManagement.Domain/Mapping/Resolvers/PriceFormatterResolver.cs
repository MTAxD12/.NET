using System.Globalization;

namespace ProductManagement.Domain.Mapping.Resolvers
{
    public static class PriceFormatterResolver
    {
        public static string Format(decimal price) => price.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
    }
}