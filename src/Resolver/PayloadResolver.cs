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

namespace OregonNexus.Broker.Service.Resolvers;

public class PayloadResolver : IPayloadResolver
{
    private readonly IRepository<EducationOrganizationPayloadSettings> _edOrgPayloadSettings;
    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;

    public PayloadResolver(IRepository<EducationOrganizationPayloadSettings> edOrgPayloadSettings, IHttpContextAccessor httpContext, IServiceProvider serviceProvider)
    {
        _edOrgPayloadSettings = edOrgPayloadSettings;
        _session = httpContext!.HttpContext!.Session;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<IncomingPayloadSettings> FetchIncomingPayloadSettingsAsync<T>()
    {
        var focusEducationOrganization = Guid.Parse(_session!.GetString("Focus.EducationOrganization.Key"));

        var connectorSpec = new PayloadSettingsByNameAndEdOrgIdSpec(typeof(T).FullName, focusEducationOrganization);
        var repoConnectorSettings = await _edOrgPayloadSettings.FirstOrDefaultAsync(connectorSpec);

        return repoConnectorSettings.IncomingPayloadSettings;
    }

    public async Task<OutgoingPayloadSettings> FetchOutgoingPayloadSettingsAsync<T>()
    {
        var focusEducationOrganization = Guid.Parse(_session!.GetString("Focus.EducationOrganization.Key"));

        var connectorSpec = new PayloadSettingsByNameAndEdOrgIdSpec(typeof(T).FullName, focusEducationOrganization);
        var repoConnectorSettings = await _edOrgPayloadSettings.FirstOrDefaultAsync(connectorSpec);

        return repoConnectorSettings.OutgoingPayloadSettings;
    }
}