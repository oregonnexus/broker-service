using OregonNexus.Broker.Domain;
using Microsoft.Extensions.DependencyInjection;
using OregonNexus.Broker.SharedKernel;
using Ardalis.GuardClauses;
using OregonNexus.Broker.Service.Jobs;
using Microsoft.Extensions.Logging;

namespace OregonNexus.Broker.Service.Resolvers;

public class WorkerResolver
{
    private readonly IRepository<Request> _requestsRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WorkerResolver> _logger;
    
    public WorkerResolver(ILogger<WorkerResolver> logger, IRepository<Request> requestsRepository, IServiceProvider serviceProvider)
    {
        _requestsRepository = requestsRepository;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Request> ProcessAsync(Request request)
    {
        Guard.Against.Null(request);
        
        using (var scoped = _serviceProvider.CreateScope())
        {
            // Figure out which job to execute based on the state of the request
            switch (request.RequestStatus)
            {
                case RequestStatus.WaitingToSend:
                    var sendRequest = (SendRequest)scoped.ServiceProvider.GetService(typeof(SendRequest))!;
                    _logger.LogInformation("{0}: Begin sending request.", request.Id);
                    await sendRequest.Process(request);
                    break;
            }
        }
        
        return request;
    }
}