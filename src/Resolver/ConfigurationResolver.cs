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

public class ConfigurationResolver : IConfigurationResolver
{
    private readonly IRepository<EducationOrganizationConnectorSettings> _edOrgConnectorSettings;

    private readonly ISession _session;
    private readonly IServiceProvider _serviceProvider;

    public ConfigurationResolver(IRepository<EducationOrganizationConnectorSettings> edOrgConnectorSettings, IHttpContextAccessor httpContext, IServiceProvider serviceProvider)
    {
        _edOrgConnectorSettings = edOrgConnectorSettings;
        _session = httpContext!.HttpContext.Session;
        _serviceProvider = serviceProvider;
    }
    
    public async Task<T> FetchConnectorSettingsAsync<T>()
    {
        var focusEducationOrganization = Guid.Parse(_session!.GetString("Focus.EducationOrganization.Key"));

        var assembly = typeof(T).GetType().Assembly.FullName;

        var iconfigModel = (T)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(T));
        var objTypeName = iconfigModel.GetType().FullName;
        
        // Get existing object
        if (typeof(T).Assembly.GetName().Name! != null)
        {
            var connectorSpec = new ConnectorByNameAndEdOrgIdSpec(typeof(T).Assembly.GetName().Name!, focusEducationOrganization);
            var repoConnectorSettings = await _edOrgConnectorSettings.FirstOrDefaultAsync(connectorSpec);
            if (repoConnectorSettings is not null)
            {
                var configSettings = Newtonsoft.Json.Linq.JObject.Parse(repoConnectorSettings?.Settings?.RootElement.GetRawText());

                var configSettingsObj = configSettings[objTypeName];

                foreach(var prop in iconfigModel!.GetType().GetProperties())
                {
                    // Check if prop in configSettings
                    var value = configSettingsObj.Value<string>(prop.Name);
                    if (value is not null)
                    {
                        prop.SetValue(iconfigModel, value);
                    }
                }
            }
        }

        return iconfigModel;
    }
}