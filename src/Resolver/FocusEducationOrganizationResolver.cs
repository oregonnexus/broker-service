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

public class FocusEducationOrganizationResolver
{
    private readonly ISession? _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<EducationOrganization> _edOrgRepo;
    
    public FocusEducationOrganizationResolver(IHttpContextAccessor httpContext, IRepository<EducationOrganization> edOrgRepo,  IServiceProvider serviceProvider)
    {
        _session = httpContext!.HttpContext!.Session;
        _edOrgRepo = edOrgRepo;
        _serviceProvider = serviceProvider;
    }

    public FocusEducationOrganizationResolver(IRepository<EducationOrganization> edOrgRepo, IServiceProvider serviceProvider)
    {
        _edOrgRepo = edOrgRepo;
        _serviceProvider = serviceProvider;
    }

    public async Task<EducationOrganization> Resolve(string? educationOrganizationId = null)
    {
        if (educationOrganizationId is null && _session is not null)
        {
            educationOrganizationId = _session.GetString("Focus.EducationOrganization.Key")!;    
        }
        
        Guard.Against.Null(educationOrganizationId, "Missing Education Organization Id");

        var educationOrganization = await _edOrgRepo.FirstOrDefaultAsync(new OrganizationByIdWithParentSpec(Guid.Parse(educationOrganizationId)));

        Guard.Against.Null(educationOrganization, "Unable to find Education Organization Id");

        return educationOrganization;
    }
}