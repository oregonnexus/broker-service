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
using Ardalis.GuardClauses;

namespace OregonNexus.Broker.Service.Resolvers;

public class PayloadResolver : IPayloadResolver
{
    private readonly IRepository<EducationOrganizationPayloadSettings> _edOrgPayloadSettings;
    private readonly FocusEducationOrganizationResolver _focusEdOrg;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public PayloadResolver(
        IRepository<EducationOrganizationPayloadSettings> edOrgPayloadSettings, 
        FocusEducationOrganizationResolver focusEdOrg, 
        DistrictEducationOrganizationResolver districtEdOrg,
        IServiceProvider serviceProvider
    )
    {
        _edOrgPayloadSettings = edOrgPayloadSettings;
        _focusEdOrg = focusEdOrg;
        _districtEdOrg = districtEdOrg;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<IncomingPayloadSettings> FetchIncomingPayloadSettingsAsync<T>() where T : IPayload
    {
        Guard.Against.Null(typeof(T));
        
        var connectorSpec = new PayloadSettingsByNameAndEdOrgIdSpec(typeof(T).FullName!, _districtEdOrg.Resolve(await _focusEdOrg.Resolve()).Id);
        var repoConnectorSettings = await _edOrgPayloadSettings.FirstOrDefaultAsync(connectorSpec);
        
        Guard.Against.Null(repoConnectorSettings);
        Guard.Against.Null(repoConnectorSettings.IncomingPayloadSettings);

        return repoConnectorSettings!.IncomingPayloadSettings;
    }

    public async Task<OutgoingPayloadSettings> FetchOutgoingPayloadSettingsAsync<T>() where T : IPayload
    {
        Guard.Against.Null(typeof(T));

        var connectorSpec = new PayloadSettingsByNameAndEdOrgIdSpec(typeof(T).FullName!, _districtEdOrg.Resolve(await _focusEdOrg.Resolve()).Id);
        var repoConnectorSettings = await _edOrgPayloadSettings.FirstOrDefaultAsync(connectorSpec);

        Guard.Against.Null(repoConnectorSettings);
        Guard.Against.Null(repoConnectorSettings.OutgoingPayloadSettings);

        return repoConnectorSettings!.OutgoingPayloadSettings;
    }
}