using OregonNexus.Broker.Connector.Payload;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.SharedKernel;
using OregonNexus.Broker.Connector.Resolvers;
using Ardalis.GuardClauses;
using OregonNexus.Broker.Connector.PayloadContents;
using OregonNexus.Broker.Connector.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace OregonNexus.Broker.Service.Resolvers;

public class PayloadJobResolver //: IPayloadResolver
{
    private readonly IRepository<EducationOrganizationPayloadSettings> _edOrgPayloadSettings;
    private readonly FocusEducationOrganizationResolver _focusEdOrg;
    private readonly DistrictEducationOrganizationResolver _districtEdOrg;
    private readonly IServiceProvider _serviceProvider;

    public PayloadJobResolver(
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

    public IPayloadContentJob Resolve(string payloadContentType)
    {
        Guard.Against.Null(payloadContentType);

        var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetExportedTypes())
                .Where(p => p.FullName == payloadContentType);

        // Locate the payload content service in connector
        var resolvedPayloadContentType = types.FirstOrDefault();

        Guard.Against.Null(resolvedPayloadContentType, "", "Could not get payload content type");

        // Locate the attribute to determien the job to run
        var jobPayloadContentType = ((JobAttribute)resolvedPayloadContentType.GetCustomAttributes(false).Where(x => x.GetType() == typeof(JobAttribute)).FirstOrDefault()!).JobType;
  
        var payloadContentJob = ActivatorUtilities.CreateInstance(_serviceProvider, jobPayloadContentType);
        
        return (IPayloadContentJob)payloadContentJob;
    }
}