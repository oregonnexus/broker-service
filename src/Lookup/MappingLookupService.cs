using OregonNexus.Broker.Connector.StudentLookup;
using OregonNexus.Broker.Connector.Resolvers;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Service.Resolvers;
using OregonNexus.Broker.Connector;
using Ardalis.GuardClauses;
using OregonNexus.Broker.Connector.Student;
using Microsoft.Extensions.Logging;
using OregonNexus.Broker.Service.Models;
using System.Web;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OregonNexus.Broker.Service.Lookup;

public class MappingLookupService
{
    private readonly ILogger<MappingLookupService> _logger;
    
    private readonly HttpClient _httpClient;

    public MappingLookupService(ILogger<MappingLookupService> logger,  
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("IgnoreSSL");
    }

    public List<SelectListItem> Select(LookupAttribute lookupAttribute, string value)
    {
        // Determine if lookup already called and loaded
        
        // Resolve lookup to call

        // Call it to load

        // Cache the value

        
        return new List<SelectListItem>();
    }

    
}   