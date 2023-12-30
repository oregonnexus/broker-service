using OregonNexus.Broker.Connector;

namespace OregonNexus.Broker.Service;

public class PayloadContentTypeDisplay
{
    public string DisplayName { get; set; }
    public string Name { get; set; }
    public string FullName { get; set; }
    public bool AllowMultiple { get; set; }
    public bool AllowConfiguration { get; set; }
}