using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.SharedKernel;
using Ardalis.GuardClauses;
using OregonNexus.Broker.Connector;
using OregonNexus.Broker.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace OregonNexus.Broker.Service.Resolvers;

public class AuthenticationProviderResolver
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly IRepository<EducationOrganizationPayloadSettings> _edOrgPayloadSettings;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public AuthenticationProviderResolver(
        ConnectorLoader connectorLoader,
        IRepository<EducationOrganizationPayloadSettings> edOrgPayloadSettings, 
        DistrictEducationOrganizationResolver districtEdOrg,
        IServiceProvider serviceProvider
    )
    {
        _connectorLoader = connectorLoader;
        _edOrgPayloadSettings = edOrgPayloadSettings;
        _districtEdOrg = districtEdOrg;
        _serviceProvider = serviceProvider;
    }
    
    public IAuthenticationProvider? Resolve(string connectorTypeName)
    {
        Type authProviderType = _connectorLoader.GetAuthenticator(connectorTypeName);       
        return (IAuthenticationProvider)ActivatorUtilities.CreateInstance(_serviceProvider, authProviderType);  
    }
}