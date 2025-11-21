namespace ProductManagement.API.Common.Logging
{
    public static class LoggingExtensions
    {
        public static void LogProductCreationMetrics(this ILogger logger, ProductCreationMetrics metrics)
        {
            logger.LogInformation(
                LogEvents.ProductCreationCompleted,
                "Product Creation Metrics | OperationId: {OperationId} | Name: {ProductName} | SKU: {SKU} | Category: {Category} | " +
                "ValidationDuration: {ValidationDuration}ms | DatabaseSaveDuration: {DatabaseSaveDuration}ms | " +
                "TotalDuration: {TotalDuration}ms | Success: {Success} | ErrorReason: {ErrorReason}",
                metrics.OperationId,
                metrics.ProductName,
                metrics.SKU,
                metrics.Category,
                metrics.ValidationDuration.TotalMilliseconds,
                metrics.DatabaseSaveDuration.TotalMilliseconds,
                metrics.TotalDuration.TotalMilliseconds,
                metrics.Success,
                metrics.ErrorReason ?? "None"
            );
        }
    }
}
