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
using OregonNexus.Broker.Connector;
using OregonNexus.Broker.Connector.Student;
using Ardalis.GuardClauses;

namespace OregonNexus.Broker.Service.Resolvers;

public class StudentResolver
{
    private readonly IServiceProvider _serviceProvider;
    
    public StudentResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IStudentService Resolve(Type TConnector)
    {
        var assembly = TConnector.Assembly.GetExportedTypes();
        // Locate the student lookup service in connector
        var studentLookupServiceType = assembly
            .Where(x => x.GetInterface(nameof(IStudent)) is not null 
                     && x.IsAbstract == false)
            .FirstOrDefault();

        Guard.Against.Null(studentLookupServiceType, "", "Could not get student type");
        
        var connectorStudentLookupService = (IStudentService)
            ActivatorUtilities.CreateInstance(_serviceProvider, studentLookupServiceType);
        
        return connectorStudentLookupService;
    }
}