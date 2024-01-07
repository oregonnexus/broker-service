using OregonNexus.Broker.Connector.StudentLookup;
using OregonNexus.Broker.Connector.Resolvers;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Service.Resolvers;
using OregonNexus.Broker.Connector;
using Ardalis.GuardClauses;
using OregonNexus.Broker.Connector.Student;

namespace OregonNexus.Broker.Service.Lookup;

public class StudentService
{
    private readonly IPayloadResolver _payloadResolver;
    private readonly StudentResolver _studentResolver;
    private readonly ConnectorLoader _connectorLoader;
    
    public StudentService(ConnectorLoader connectorLoader, IPayloadResolver payloadResolver, StudentResolver studentResolver)
    {
        _connectorLoader = connectorLoader;
        _payloadResolver = payloadResolver;
        _studentResolver = studentResolver;
    }

    public async Task<IStudent?> FetchAsync(PayloadDirection payloadDirection, Student studentToFetch)
    {
        string studentLookupConnector = default!;

        if (payloadDirection == PayloadDirection.Incoming)
        {
            var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync<StudentCumulativeRecord>();

            if (payloadSettings.StudentInformationSystem is null)
            {
                throw new ArgumentNullException("Student Information System missing on incoming payload settings.");
            }

            studentLookupConnector = payloadSettings.StudentInformationSystem;
        }

        if (payloadDirection == PayloadDirection.Outgoing)
        {
            var payloadSettings = await _payloadResolver.FetchOutgoingPayloadSettingsAsync<StudentCumulativeRecord>();

            if (payloadSettings.StudentLookupConnector is null)
            {
                throw new ArgumentNullException("Student Lookup Connector missing on outgoing payload settings.");
            }
            studentLookupConnector = payloadSettings.StudentLookupConnector;
        }

        if (studentLookupConnector == default)
        {
            throw new ArgumentNullException("Unable to find connector to use for student lookup.");
        }

        Type typeConnectorToUse = _connectorLoader.GetConnector(studentLookupConnector)!;

        var connectorStudentService = _studentResolver.Resolve(typeConnectorToUse);

        return await connectorStudentService.FetchAsync(studentToFetch);
    }
}
