using OregonNexus.Broker.Connector.StudentLookup;
using OregonNexus.Broker.Connector.Resolvers;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Service.Resolvers;
using OregonNexus.Broker.Connector;
using Ardalis.GuardClauses;

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
        string studentLookupConnector;

        if (payloadDirection == PayloadDirection.Incoming)
        {
            var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync<StudentCumulativeRecord>();

            if (payloadSettings.StudentInformationSystem is null)
            {
                throw new ArgumentNullException("Student Information System missing on incoming payload settings.");
            }

            studentLookupConnector = payloadSettings.StudentInformationSystem;
        }
        else
        {
            throw new ArgumentNullException("Student Information System missing on incoming payload settings.");
        }

        if (payloadDirection == PayloadDirection.Outgoing)
        {
            var payloadSettings = await _payloadResolver.FetchOutgoingPayloadSettingsAsync<StudentCumulativeRecord>();

            if (payloadSettings.PrimaryDataConnector is null)
            {
                throw new ArgumentNullException("Primary data connector missing on outgoing payload settings.");
            }
            studentLookupConnector = payloadSettings.PrimaryDataConnector;
        }
        else
        {
            throw new ArgumentNullException("Primary data connector missing on outgoing payload settings.");
        }

        if (studentLookupConnector is null && studentLookupConnector == "")
        {
            throw new ArgumentNullException("Unable to find connector to use for student lookup.");
        }

        Type typeConnectorToUse = _connectorLoader.GetConnector(studentLookupConnector)!;

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
