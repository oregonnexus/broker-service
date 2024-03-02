using OregonNexus.Broker.Domain;
using OregonNexus.Broker.SharedKernel;
using System.Text.Json;
using OregonNexus.Broker.Service.Worker;
using OregonNexus.Broker.Service.Resolvers;
using Ardalis.GuardClauses;
using OregonNexus.Broker.Domain.Specifications;
using System.Text.Json.Nodes;
using System.Reflection;
using OregonNexus.Broker.Connector;
using Microsoft.Extensions.DependencyInjection;

namespace OregonNexus.Broker.Service.Jobs;

public class PrepareMapping
{
    private readonly ConnectorLoader _connectorLoader;
    private readonly ConnectorResolver _connectorResolver;
    private readonly PayloadResolver _payloadResolver;
    private readonly JobStatusService<SendRequest> _jobStatusService;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IServiceProvider _serviceProvider;
    private readonly IRepository<Mapping> _mappingRepository;

    public PrepareMapping(
            ConnectorLoader connectorLoader,
            ConnectorResolver connectorResolver,
            PayloadResolver payloadResolver,
            JobStatusService<SendRequest> jobStatusService,
            IRepository<PayloadContent> payloadContentRepository,
            IServiceProvider serviceProvider,
            IRepository<Mapping> mappingRepository)
    {
        _connectorLoader = connectorLoader;
        _connectorResolver = connectorResolver;
        _payloadResolver = payloadResolver;
        _jobStatusService = jobStatusService;
        _payloadContentRepository = payloadContentRepository;
        _serviceProvider = serviceProvider;
        _mappingRepository = mappingRepository;
    }
    
    public async Task Process(Request request)
    {
        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Preparing, "Begin preparing mapping for for: {0}", request.Payload);

        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Preparing, "Begin fetching payload contents for: {0}", request.EducationOrganization?.ParentOrganizationId);

        // Get incoming payload settings
        var payloadSettings = await _payloadResolver.FetchIncomingPayloadSettingsAsync(request.Payload, request.EducationOrganization!.ParentOrganizationId!.Value);

        // Resolve the SIS connector
        Guard.Against.Null(payloadSettings.StudentInformationSystem, null, "No SIS incoming connector set.");
        var sisConnectorType = _connectorResolver.Resolve(payloadSettings.StudentInformationSystem);
        Guard.Against.Null(sisConnectorType, null, "Unable to load connector.");

        // Get file contents
        var payloadContents = request.ResponseManifest?.Contents?.Where(x => x.ContentType == "application/json").ToList();
        if (payloadContents is null || payloadContents.Count == 0)
        {
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Preparing, "Nothing to process.");
            return;
        }

        // For each file run, extract contents and collapse to distinct types
        foreach(var payloadContentDetails in payloadContents)
        {
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Preparing, "Begin processing file {0} with schema {1}.", payloadContentDetails.FileName, payloadContentDetails.ContentType);
            // Retrieve from database
            var payloadContent = await _payloadContentRepository.FirstOrDefaultAsync(new PayloadContentsByRequestIdAndFileName(request.Id, payloadContentDetails.FileName));
        
            Guard.Against.Null(payloadContent, null, "Could not retrieve payload content as specified in manifest.");
            Guard.Against.Null(payloadContent.JsonContent, null, "No Json content in retrieved content payload.");

            // Get Schema
            var payloadContentSchemaJson = payloadContent.JsonContent?.RootElement.GetProperty("Schema");
            Guard.Against.Null(payloadContentSchemaJson, null, "Missing schema element.");
            var payloadContentSchema = JsonSerializer.Deserialize<PayloadContentSchema>(payloadContentSchemaJson.ToString()!);
            Guard.Against.Null(payloadContentSchema?.ObjectType, null, "Schema missing");

            // Deseralize object to the type
            await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Preparing, "Will deseralize object of type: {0}.", payloadContentSchema.ObjectType);
            var payloadContentSchemaType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(s => s.GetExportedTypes())
                        .Where(p => p.FullName == payloadContentSchema.ObjectType).FirstOrDefault();
            Guard.Against.Null(payloadContentSchemaType, null, $"Unable to find concrete type {payloadContentSchema?.ObjectType}");
            
            dynamic payloadContentObject = Convert.ChangeType(JsonSerializer.Deserialize(payloadContent.JsonContent!, payloadContentSchemaType), payloadContentSchemaType)!;

            // Find appropriate transformer
            var transformerType = _connectorLoader.Transformers.Where(x => x.Key == $"{payloadContentSchema?.Schema}::{payloadContentSchema?.SchemaVersion}").FirstOrDefault().Value;
            
            if (transformerType is null) { continue; }

            var methodInfo = transformerType.GetMethod("Map");

            if (methodInfo is null) { continue; }

            var transformerContentType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetExportedTypes())
                .Where(p => p.FullName == payloadContentSchema?.ContentObjectType).FirstOrDefault();

            var records = new List<dynamic>();

            var contentRecords = JsonSerializer.Deserialize<List<dynamic>>(payloadContentObject.Content);

            Type? recordType = null;

            foreach(var record in contentRecords)
            {

                var correctRecordType = Convert.ChangeType(JsonSerializer.Deserialize(record, transformerContentType, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }), transformerContentType);

                // Run through connector's transformer
                dynamic transformer = ActivatorUtilities.CreateInstance(_serviceProvider, transformerType);
                var result = methodInfo!.Invoke(transformer, new object[] { correctRecordType, request.RequestManifest?.Student!, request.EducationOrganization, request.ResponseManifest! });
                
                recordType = result.GetType();
                
                // Save each
                records.Add(result);
            }
            
            var recordsSerialized = JsonSerializer.SerializeToDocument(records);

            await _mappingRepository.AddAsync(new Mapping()
            {
                RequestId = request.Id,
                OriginalSchema = payloadContentSchema,
                MappingType = recordType?.FullName,
                StudentAttributes = null,
                SourceMapping = recordsSerialized,
                DestinationMapping = recordsSerialized
            });

        }

        await _jobStatusService.UpdateRequestJobStatus(request, RequestStatus.Prepared, "Finished preparing mapping.");
    }
}