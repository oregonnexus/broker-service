using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using OregonNexus.Broker.Connector.Edupoint.Synergy.Authentication;
//using OregonNexus.Broker.Connector.Authentication;
//using OregonNexus.Broker.Connector.Locators;
using OregonNexus.Broker.Connector.Resolvers;
using OregonNexus.Broker.Service.Resolvers;
using OregonNexus.Broker.Service.Lookup;
using OregonNexus.Broker.Service.Serializers;

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
        // Seralizers
        services.AddScoped<ConfigurationSerializer>();
        services.AddScoped<IncomingPayloadSerializer>();
        services.AddScoped<OutgoingPayloadSerializer>();
        
        // Resolvers
        services.AddScoped<IConfigurationResolver, ConfigurationResolver>();
        services.AddScoped<IPayloadResolver, PayloadResolver>();
        services.AddScoped<FocusEducationOrganizationResolver>();
        services.AddScoped<DistrictEducationOrganizationResolver>();
        services.AddScoped<StudentLookupResolver>();
        services.AddScoped<StudentResolver>();

        // Services
        services.AddScoped<StudentLookupService>();
        services.AddScoped<StudentService>();
        services.AddScoped<PayloadContentTypeService>();

        return services;
    }
}