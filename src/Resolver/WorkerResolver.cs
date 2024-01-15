using OregonNexus.Broker.Domain;
using Microsoft.Extensions.DependencyInjection;
using OregonNexus.Broker.SharedKernel;
using Ardalis.GuardClauses;

namespace OregonNexus.Broker.Service.Resolvers;

public class WorkerResolver
{
    private readonly IRepository<Request> _requestsRepository;
    private readonly IServiceProvider _serviceProvider;
    
    public WorkerResolver(IRepository<Request> requestsRepository, IServiceProvider serviceProvider)
    {
        _requestsRepository = requestsRepository;
        _serviceProvider = serviceProvider;
    }

    public async Task<Request> ResolveAsync(Guid requestId)
    {
        var request = await _requestsRepository.GetByIdAsync(requestId);
        Guard.Against.Null(request);
        
        // Figure out which job to execute based on the state of the request
        
        
        return request;
    }
}