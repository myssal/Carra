using Microsoft.Extensions.Logging;
using NReco.Logging.File;

namespace Carra.Carra.Logging;

public class Logging
{
    public const string logOutput = @"log.txt";
    public static ILogger  CreateLogFactory(string categoryName)
    {
        var factory = new LoggerFactory();
        factory.AddProvider(new FileLoggerProvider(logOutput, false));
        var logger = factory.CreateLogger(categoryName);
        return logger;
    }
}