using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;

namespace OregonNexus.Broker.Service.Jobs;

public class SendRequest
{
    private readonly IRepository<Request> _requestRepository;

    public SendRequest(IRepository<Request> requestRepository)
    {
        _requestRepository = requestRepository;
    }
    
    public void Process(Guid requestId)
    {

    }
}