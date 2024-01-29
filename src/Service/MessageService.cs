using System.Text.Json;
using OregonNexus.Broker.Data;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.Service.Worker;
using OregonNexus.Broker.SharedKernel;

namespace OregonNexus.Broker.Service;

public class MessageService
{
    private readonly IRepository<Message> _messageRepo;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly JobStatusService<MessageService> _jobStatusService;
    private readonly BrokerDbContext _brokerDbContext;

    public MessageService(IRepository<Message> messageRepo,
                        IRepository<PayloadContent> payloadContentRepository,
                        JobStatusService<MessageService> jobStatusService,
                        BrokerDbContext brokerDbContext)
    {
        _messageRepo = messageRepo;
        _payloadContentRepository = payloadContentRepository;
        _jobStatusService = jobStatusService;
        _brokerDbContext = brokerDbContext;
    }

    public async Task<Message> Create(Request request)
    {
        _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Sending, "Create message and move attachments");

        using var transaction = _brokerDbContext.Database.BeginTransaction();

        // Create Message
        var message = new Message()
        {
            RequestId = request.Id,
            RequestResponse = RequestResponse.Request,
            MessageContents = JsonDocument.Parse(JsonSerializer.Serialize(request.RequestManifest))
        };

        await _messageRepo.AddAsync(message);

        // Move any payloadcontents (attachments) to message
        var attachments = await _payloadContentRepository.ListAsync(new PayloadContentsByRequestId(request.Id));
        if (attachments is not null && attachments.Count > 0)
        {
            foreach(var payloadContent in attachments)
            {
                payloadContent.MessageId = message.Id;
                await _payloadContentRepository.UpdateAsync(payloadContent);
            }
        }

        transaction.Commit();

        return message;
    }

    public async Task<Message> MarkSent(Message message)
    {
        // Get message
        var latestMessage = await _messageRepo.GetByIdAsync(message.Id);
        
        latestMessage!.MessageTimestamp = DateTime.UtcNow;
        await _messageRepo.UpdateAsync(latestMessage);

        return latestMessage;
    }
}