using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using OregonNexus.Broker.Connector.Edupoint.Synergy.Authentication;
//using OregonNexus.Broker.Connector.Authentication;
//using OregonNexus.Broker.Connector.Locators;
using OregonNexus.Broker.Connector.Resolvers;
using OregonNexus.Broker.Service.Resolvers;
using OregonNexus.Broker.Service.Lookup;

namespace OregonNexus.Broker.Service;

public static class BrokerServiceCollection //: IConnectorServiceCollection
{
    /*
    public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
    {
        return services;
    }
    */

    public static IServiceCollection AddBrokerServices(this IServiceCollection services)
    {
        services.AddScoped<StudentLookupService>();
        services.AddScoped<StudentLookupResolver>();
        services.AddScoped<PayloadContentTypeService>();
        services.AddScoped<IConfigurationResolver, ConfigurationResolver>();
        services.AddScoped<IPayloadResolver, PayloadResolver>();

        return services;
    }
}