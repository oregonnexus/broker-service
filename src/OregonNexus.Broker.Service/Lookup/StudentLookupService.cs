using OregonNexus.Broker.Connector.StudentLookup;
using OregonNexus.Broker.Connector.Resolvers;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Service.Resolvers;
using OregonNexus.Broker.Connector;

namespace OregonNexus.Broker.Service.Lookup;

public class StudentLookupService
{
    private readonly IPayloadResolver _payloadResolver;
    private readonly StudentLookupResolver _studentLookupResolver;
    private readonly ConnectorLoader _connectorLoader;
    
    public StudentLookupService(ConnectorLoader connectorLoader, IPayloadResolver payloadResolver, StudentLookupResolver studentLookupResolver)
    {
        _connectorLoader = connectorLoader;
        _payloadResolver = payloadResolver;
        _studentLookupResolver = studentLookupResolver;
    }

    public async Task<List<StudentLookupResult>> SearchAsync(PayloadDirection payloadDirection, string searchParameter)
    {
        //throw new NotImplementedException();
        // Determine connector to call
        var payloadSettings = await _payloadResolver.FetchPayloadSettingsAsync<StudentCumulativeRecord>(payloadDirection);

        var connectorToUse = payloadSettings.Where(i => i.PayloadContentType == "DataConnector").Select(i => i.Settings).First();

        Type typeConnectorToUse = _connectorLoader.GetConnector(connectorToUse);

        var connectorStudentLookupService = _studentLookupResolver.Resolve(typeConnectorToUse);

        // Prepare parameters
        var searchStudent = new Student()
        {
            StudentNumber = searchParameter,
            FirstName = searchParameter,
            LastName = searchParameter,
            MiddleName = searchParameter
        };

        // Pass search parameters to connector for processing
        var results = await connectorStudentLookupService.SearchAsync(searchStudent);

        return results;
    }
}
