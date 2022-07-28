using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Triplets_Extractor;

namespace Finding_Triplets__Multy_task_
{
    internal static class StartUp
    {
        public static IConfiguration Configuration { get => 
                new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appconfig.json")
                .Build();
        }

        public static IServiceProvider AppServiceProvider { get =>
                new ServiceCollection()
                .AddTransient<ITripletsExtractorBuilder<string>, TripletsExtractorBuilder<string>>()
                .BuildServiceProvider();}
    }
}
