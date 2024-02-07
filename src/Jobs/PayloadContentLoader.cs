using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using System.Text.Json;
using OregonNexus.Broker.Service.Worker;
using OregonNexus.Broker.Service.Resolvers;

namespace OregonNexus.Broker.Service.Jobs;

public class PayloadContentLoader
{
    private readonly PayloadResolver _payloadResolver;
    private readonly PayloadJobResolver _payloadJobResolver;
    private readonly JobStatusService<SendRequest> _jobStatusService;
    private readonly IRepository<PayloadContent> _payloadContentRepository;

    public PayloadContentLoader(
            PayloadResolver payloadResolver,
            PayloadJobResolver payloadJobResolver,
            JobStatusService<SendRequest> jobStatusService,
            IRepository<PayloadContent> payloadContentRepository)
    {
        _payloadResolver = payloadResolver;
        _payloadJobResolver = payloadJobResolver;
        _jobStatusService = jobStatusService;
        _payloadContentRepository = payloadContentRepository;
    }
    
    public async Task Process(Request request)
    {
        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Loading, "Begin outgoing jobs loading for: {0}", request.Payload);

        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Loading, "Begin fetching payload contents for: {0}", request.EducationOrganization?.ParentOrganizationId);

        // Get outgoing payload settings
        var outgoingPayloadSettings = await _payloadResolver.FetchOutgoingPayloadSettingsAsync(request.Payload, request.EducationOrganization!.ParentOrganizationId!.Value);
        var outgoingPayloadContents = outgoingPayloadSettings.PayloadContents;

        if (outgoingPayloadContents is null || outgoingPayloadContents.Count <= 0)
        {
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Loading, "No payload contents");
            return;
        }

        // Determine which jobs to execute based on outgoing payload config
        foreach(var outgoingPayloadContent in outgoingPayloadContents)
        {
            // Resolve job to execute
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Loading, "Resolving job to execute for payload content type: {0}", outgoingPayloadContent.PayloadContentType);
            var jobToExecute = _payloadJobResolver.Resolve(outgoingPayloadContent.PayloadContentType);
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Loading, "Resolved job to execute: {0}", jobToExecute.GetType().FullName);

            // Execute the job
            var result = await jobToExecute.ExecuteAsync(request.Student?.Student?.StudentNumber!, request.EducationOrganization!.ParentOrganizationId!.Value);
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Loading, "Received result: {0}", jobToExecute.GetType().FullName);

            // Save the result
            var payloadContent = new PayloadContent()
            {
                RequestId = request.Id,
                JsonContent = JsonDocument.Parse(result.Content),
                ContentType = result.ContentType,
                FileName =  $"{outgoingPayloadContent.PayloadContentType}.json"
            };
            await _payloadContentRepository.AddAsync(payloadContent);
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Loading, "Saved payload content: {0}", jobToExecute.GetType().FullName);
        }

        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Loaded, "Finished updating request.");
    }
}