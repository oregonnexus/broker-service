using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
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