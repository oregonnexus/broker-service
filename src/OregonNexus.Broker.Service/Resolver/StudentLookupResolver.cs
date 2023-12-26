using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Connector.PayloadContentTypes;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Connector.Resolvers;
using OregonNexus.Broker.Connector.StudentLookup;

namespace OregonNexus.Broker.Service.Resolvers;

public class StudentLookupResolver
{
    private readonly IServiceProvider _serviceProvider;
    
    public StudentLookupResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStudentLookupService Resolve(Type TConnector)
    {

        var assembly = TConnector.Assembly.GetTypes();
        // Locate the student lookup service in connector
        var studentLookupServiceType = assembly.Where(x => x.GetInterface(nameof(IStudentLookupService)) is not null && x.IsAbstract == false).FirstOrDefault();
        
        var connectorStudentLookupService = (IStudentLookupService)ActivatorUtilities.CreateInstance(_serviceProvider, studentLookupServiceType);
        
        return connectorStudentLookupService;
    }
}