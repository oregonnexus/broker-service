using System.Net.Mail;
using Microsoft.Extensions.Logging;
using OregonNexus.Broker.Data;
using OregonNexus.Broker.Domain;
using OregonNexus.Broker.Domain.Specifications;
using OregonNexus.Broker.SharedKernel;
using MimeKit;
using DnsClient;

namespace OregonNexus.Broker.Service.Jobs;

public class SendRequest
{
    private readonly ILogger<SendRequest> _logger;
    private readonly BrokerDbContext _brokerDbContext;
    private readonly IRepository<Request> _requestRepository;
    private readonly IRepository<Message> _messageRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;

    public SendRequest( ILogger<SendRequest> logger, 
                        BrokerDbContext brokerDbContext,
                        IRepository<Request> requestRepository, 
                        IRepository<Message> messageRepository,
                        IRepository<PayloadContent> payloadContentRepository)
    {
        _logger = logger;
        _brokerDbContext = brokerDbContext;
        _requestRepository = requestRepository;
        _messageRepository = messageRepository;
        _payloadContentRepository = payloadContentRepository;
    }
    
    public async Task<string> Process(Request request)
    {
        _logger.LogInformation("Made it here!");

        using var transaction = _brokerDbContext.Database.BeginTransaction();

        // Create Message
        var message = new Message()
        {
            RequestId = request.Id,
            RequestResponse = RequestResponse.Request
        };
        await _messageRepository.AddAsync(message);

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

        // Determine where to send the information
        var to = new MailboxAddress("To", request.RequestManifest?.To?.Email);

        _logger.LogInformation("Domain to find from {0}", to.Domain);

        var lookup = new LookupClient();
        var result = await lookup.QueryAsync("clackesd.k12.or.us", QueryType.TXT);

        if (result.Answers.Count > 0)
        {
            foreach(var answer in result.Answers)
            {
                _logger.LogInformation("DNS TXT entries found: {0}", answer.ToString());
            }
        }
        

        // Call the http endpoint
        

        return "Made it!";
    }
}