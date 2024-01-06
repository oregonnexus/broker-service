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

public class ConfigurationResolver : IConfigurationResolver
{
    private readonly IRepository<EducationOrganizationConnectorSettings> _edOrgConnectorSettings;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly FocusEducationOrganizationResolver _focusEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public ConfigurationResolver(
        IRepository<EducationOrganizationConnectorSettings> edOrgConnectorSettings, 
        DistrictEducationOrganizationResolver districtEdOrg,
        FocusEducationOrganizationResolver focusEdOrg, 
        IServiceProvider serviceProvider
    )
    {
        _edOrgConnectorSettings = edOrgConnectorSettings;
        _districtEdOrg = districtEdOrg;
        _focusEdOrg = focusEdOrg;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<T> FetchConnectorSettingsAsync<T>()
    {
        var iconfigModel = (T)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T));
        var objTypeName = iconfigModel.GetType().FullName;
        
        Guard.Against.Null(typeof(T).Assembly.GetName().Name);

        // Get existing object
        var connectorSpec = new ConnectorByNameAndEdOrgIdSpec(typeof(T).Assembly.GetName().Name!, _districtEdOrg.Resolve(await _focusEdOrg.Resolve()).Id);
        var repoConnectorSettings = await _edOrgConnectorSettings.FirstOrDefaultAsync(connectorSpec);

        Guard.Against.Null(repoConnectorSettings);

        var configSettings = Newtonsoft.Json.Linq.JObject.Parse(repoConnectorSettings?.Settings?.RootElement.GetRawText());

        var configSettingsObj = configSettings[objTypeName];

        foreach(var prop in iconfigModel!.GetType().GetProperties())
        {
            // Check if prop in configSettings
            var value = configSettingsObj.Value<string>(prop.Name);

            Guard.Against.Null(value);
            
            prop.SetValue(iconfigModel, value);
        }

        return iconfigModel;
    }
}