using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace Carra.Logging;

public class Logging
{
    // create instances for each log options
    public static ILogger  CreateLogFactory(string categoryName, string logOutputFilePath)
    {
        var factory = new LoggerFactory();
        factory.AddProvider(new FileLoggerProvider(logOutputFilePath, false));
        var logger = factory.CreateLogger(categoryName);
        return logger;
    }
}