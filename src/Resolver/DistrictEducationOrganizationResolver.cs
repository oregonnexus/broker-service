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
using OregonNexus.Broker.Connector.StudentLookup;
using Ardalis.GuardClauses;

namespace OregonNexus.Broker.Service.Resolvers;

public class DistrictEducationOrganizationResolver
{
    private readonly IRepository<EducationOrganization> _edOrgRepo;
    private readonly IServiceProvider _serviceProvider;
    
    public DistrictEducationOrganizationResolver(IRepository<EducationOrganization> edOrgRepo, IServiceProvider serviceProvider)
    {
        _edOrgRepo = edOrgRepo;
        _serviceProvider = serviceProvider;
    }

    public EducationOrganization Resolve(EducationOrganization educationOrganization)
    {
        if (educationOrganization.EducationOrganizationType == EducationOrganizationType.District)
            return educationOrganization;
        else
        {
            Guard.Against.Null(educationOrganization.ParentOrganization, "Unable to resolve education organization to parent district.");

            return educationOrganization.ParentOrganization;
        }
    }

    public async Task<EducationOrganization> Resolve(string educationOrganizationId)
    {
        Guard.Against.Null(educationOrganizationId, "Missing education organization id.");

        var educationOrganization = await _edOrgRepo.GetByIdAsync(educationOrganizationId);

        Guard.Against.Null(educationOrganization);

        return Resolve(educationOrganization);
    }
}