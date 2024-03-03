using OregonNexus.Broker.Connector;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using OregonNexus.Broker.Service.Resolvers;

namespace OregonNexus.Broker.Service.Lookup;

public class MappingLookupService
{
    private readonly ILogger<MappingLookupService> _logger;
    private readonly MappingLookupResolver _mappingLookupResolver;
    private readonly HttpClient _httpClient;

    private Dictionary<string, List<SelectListItem>> _cachedLookups => new Dictionary<string, List<SelectListItem>>();

    public MappingLookupService(ILogger<MappingLookupService> logger, 
        MappingLookupResolver mappingLookupResolver,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _mappingLookupResolver = mappingLookupResolver;
        _httpClient = httpClientFactory.CreateClient("IgnoreSSL");
    }

    public async Task<List<SelectListItem>> SelectAsync(LookupAttribute lookupAttribute, string value)
    {
        var selectList = new List<SelectListItem>();
        
        // Determine if lookup already called and loaded
        if (!_cachedLookups.TryGetValue(lookupAttribute.LookupType.Name, out selectList))
        {
            // Resolve lookup to call
            var mappingLookupObj = _mappingLookupResolver.Resolve(lookupAttribute.LookupType);
            selectList = await mappingLookupObj.SelectListAsync();

            // Cache the value
            _cachedLookups.Add(lookupAttribute.LookupType.Name, selectList);
        }
        
        // Set the selected value
        var selected = selectList.FindIndex(x => x.Value == value);
        if (selected > -1)
        {
            selectList[selected].Selected = true;
        }
        
        return selectList;
    }

    
}   