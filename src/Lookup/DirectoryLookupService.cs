using OregonNexus.Broker.Connector.StudentLookup;
using OregonNexus.Broker.Connector.Resolvers;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Service.Resolvers;
using OregonNexus.Broker.Connector;
using Ardalis.GuardClauses;
using OregonNexus.Broker.Connector.Student;
using DnsClient;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using OregonNexus.Broker.Service.Models;
using System.Web;
using System.Net.Http.Json;

namespace OregonNexus.Broker.Service.Lookup;

public class DirectoryLookupService
{
    private readonly ILogger<DirectoryLookupService> _logger;
    
    private readonly ILookupClient _lookupClient;
    private readonly HttpClient _httpClient;

    public DirectoryLookupService(ILogger<DirectoryLookupService> logger, 
        ILookupClient lookupClient, 
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _lookupClient = lookupClient;
        _httpClient = httpClientFactory.CreateClient("IgnoreSSL");
    }

    public async Task<District> SearchAsync(string searchDomain)
    {
        if (Uri.CheckHostName(searchDomain) == UriHostNameType.Unknown)
        {
            throw new ArgumentException("{0} is not a valid domain", searchDomain);
        }
        
        var txtresult = new BrokerDnsTxtRecord();
        
        var dnsresult = await _lookupClient.QueryAsync(searchDomain, QueryType.TXT);

        var txtRecords = dnsresult.Answers.TxtRecords();
        
        if (txtRecords.Count() > 0)
        {
            var brokerTXTRecord = txtRecords.Where(x => x.Text.First().Contains("v=broker"))?.FirstOrDefault();

            if (brokerTXTRecord is not null)
            {
                txtresult = ParseBrokerTXTRecord(brokerTXTRecord.Text.First());
            }
        }

        // Get directory list
        Guard.Against.Null(txtresult.Host, "host", "Unable to get host from broker TXT record.");
        _httpClient.BaseAddress = new Uri($"https://{txtresult.Host}");
        var path = "/api/v1/directory/search?domain=" + HttpUtility.UrlEncode(searchDomain);
        
        var client = await _httpClient.GetAsync(path);

        var result = await client.Content.ReadFromJsonAsync<District>();
        if (result is not null)
        {
            return result;
        }

        return new District();
    }

    public BrokerDnsTxtRecord ParseBrokerTXTRecord(string txtRecord)
    {
        // v=broker1; a=broker.host.org

        string[] parts = txtRecord.Trim().Split(";");
        var values = new Dictionary<string, string>();

        foreach(var part in parts)
        {
            string[] val = part.Trim().Split("=");
            values.Add(val[0].Trim(), val[1].Trim());
        }

        return new BrokerDnsTxtRecord()
        {
            Version = values.TryGetValue("v", out var v) ? v : null,
            Host = values.TryGetValue("a", out var a) ? a : null,
            KeyAlgorithim = values.TryGetValue("k", out var k) ? k : null,
            PublicKey = values.TryGetValue("p", out var p) ? p : null
        };
    }
}