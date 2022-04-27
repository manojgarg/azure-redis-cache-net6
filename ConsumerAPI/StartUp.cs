using BAL.Interfaces;
using BAL.Services;
using Cache;
using Cache.Implementations;
using Cache.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Reflection;

[assembly: FunctionsStartup(typeof(ConsumerAPI.Startup))]
namespace ConsumerAPI
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(typeof(ICacheProvider), typeof(RedisCacheProvider));
            builder.Services.AddTransient(typeof(IDataService), typeof(DataService));

        }
    }
}