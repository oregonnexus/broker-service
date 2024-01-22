using Microsoft.Extensions.DependencyInjection;
using OregonNexus.Broker.Connector.Resolvers;
using OregonNexus.Broker.Service.Resolvers;
using OregonNexus.Broker.Service.Lookup;
using OregonNexus.Broker.Service.Serializers;
using OregonNexus.Broker.Service.Jobs;
using DnsClient;

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
        // Other Services
        services.AddSingleton<ILookupClient, LookupClient>();

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
        services.AddScoped<DirectoryLookupService>();

        return services;
    }

    public static IServiceCollection AddBrokerServicesForWorker(this IServiceCollection services)
    {
        // Services
        services.AddSingleton<ILookupClient, LookupClient>();
        
        // Resolvers
        services.AddSingleton<WorkerResolver>();
        
        // Jobs
        services.AddScoped<SendRequest>();
        
        return services;
    }
}