using Microsoft.Extensions.DependencyInjection;
using OregonNexus.Broker.Connector.Resolvers;
using OregonNexus.Broker.Service.Resolvers;
using OregonNexus.Broker.Service.Lookup;
using OregonNexus.Broker.Service.Serializers;
using OregonNexus.Broker.Service.Jobs;
using DnsClient;
using OregonNexus.Broker.Service.Worker;
using OregonNexus.Broker.Service.Cache;

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
        // Caches
        services.AddSingleton<MappingLookupCache>();
        
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
        services.AddScoped<MappingLookupResolver>();

        // Services
        services.AddScoped<StudentLookupService>();
        services.AddScoped<StudentService>();
        services.AddScoped<PayloadContentTypeService>();
        services.AddScoped<DirectoryLookupService>();
        services.AddScoped<ManifestService>();
        services.AddScoped<MappingLookupService>();

        return services;
    }

    public static IServiceCollection AddBrokerServicesForWorker(this IServiceCollection services)
    {
        // Services
        services.AddSingleton<ILookupClient, LookupClient>();
        services.AddScoped<DirectoryLookupService>();
        services.AddScoped<MessageService>();
        
        // Resolvers
        services.AddScoped<IConfigurationResolver, ConfigurationResolver>();
        services.AddSingleton<WorkerResolver>();
        services.AddScoped<IPayloadResolver, PayloadResolver>();
        services.AddScoped<PayloadResolver>();
        services.AddScoped<ConnectorResolver>();
        services.AddScoped<FocusEducationOrganizationResolver>();
        services.AddScoped<DistrictEducationOrganizationResolver>();
        services.AddScoped<PayloadJobResolver>();
        
        // Jobs
        services.AddScoped<SendRequest>();
        services.AddScoped<PayloadContentLoader>();
        services.AddScoped<PrepareMapping>();
        services.AddScoped<ImportMapping>();

        // Worker
        services.AddScoped(typeof(JobStatusService<>));
        
        return services;
    }
}