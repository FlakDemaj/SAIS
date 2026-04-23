using Presentation.Utils;

namespace Presentation.Server;

public class Program
{

    protected Program()
    {
    }

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Startup.ConfigureServices(builder);

        var app = builder.Build();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.Migrate();
        }

        Startup.ConfigurePipeline(app);

        await app.RunAsync();
    }
}
