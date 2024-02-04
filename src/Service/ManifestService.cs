using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;

namespace OregonNexus.Broker.Service;

public class ManifestService
{
    private readonly IReadRepository<User> _user;
    private readonly UserManager<IdentityUser<Guid>> _userManager;

    public ManifestService(IReadRepository<User> user, UserManager<IdentityUser<Guid>> userManager)
    {
        _user = user;
        _userManager = userManager;
    }
    
    public async Task<Manifest> AddFrom(Request request, Guid fromUserId)
    {
        Manifest? manifest = null;

        if (request.IncomingOutgoing == IncomingOutgoing.Incoming)
        {
            manifest = request.RequestManifest!;
        } else if (request.IncomingOutgoing == IncomingOutgoing.Outgoing)
        {
            manifest = request.ResponseManifest!;
        }

        Guard.Against.Null(manifest, "Manifest could not be set from request.");

        // Find user
        var user = await _user.GetByIdAsync(fromUserId);
        var userIdentity = await _userManager.FindByIdAsync(fromUserId.ToString());

        Guard.Against.Null(user, "User not found.");

        // Add From
        manifest.From = new RequestAddress()
        {
            District = new District()
            {
                Id = request.EducationOrganization!.ParentOrganizationId!.Value,
                Name = request.EducationOrganization!.ParentOrganization!.Name,
                Number = request.EducationOrganization.ParentOrganization?.Number,
                Domain = request.EducationOrganization.ParentOrganization?.Domain
            },
            School = new School()
            {
                Id = request.EducationOrganizationId,
                Name = request.EducationOrganization.Name,
                Number = request.EducationOrganization.Number
            },
            Sender = new EducationOrganizationContact()
            {
                Name = user.Name,
                Email = userIdentity?.Email?.ToLower()
            }
        };
        
        return manifest;
    }
}