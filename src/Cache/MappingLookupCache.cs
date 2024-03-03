using OregonNexus.Broker.Connector;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using OregonNexus.Broker.Service.Resolvers;

namespace OregonNexus.Broker.Service.Cache;

public class MappingLookupCache
{
    private readonly ILogger<MappingLookupCache> _logger;

    private Dictionary<string, List<SelectListItem>> _cachedLookups = new Dictionary<string, List<SelectListItem>>();

    public MappingLookupCache(ILogger<MappingLookupCache> logger)
    {
        _logger = logger;
    }

    public List<SelectListItem>? Get(string cacheKey)
    {
        _logger.LogInformation($"Checking for key in mapping lookup cache: {cacheKey}");
        if (_cachedLookups.ContainsKey(cacheKey))  
        {  
              return _cachedLookups[cacheKey];
        }  
        return null;
    }

    public void Add(string cacheKey, List<SelectListItem> selectList)
    {
        _logger.LogInformation($"Added key in mapping lookup cache: {cacheKey}");
        _cachedLookups.Add(cacheKey, selectList);
    }
}   