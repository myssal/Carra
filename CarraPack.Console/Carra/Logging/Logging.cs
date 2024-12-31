using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace Carra.Logging;

public class Logging
{
    // append log to existing file
    public static ILogger  CreateLogFactory(string categoryName, string logOutputFilePath)
    {
        var factory = new LoggerFactory();
        factory.AddProvider(new FileLoggerProvider(logOutputFilePath, true));
        var logger = factory.CreateLogger(categoryName);
        return logger;
    }
}