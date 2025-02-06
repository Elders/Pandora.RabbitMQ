namespace Pandora.RabbitMQ.PandoraConfigurationMessageProcessors;

public interface IConfigurationMessage
{
    public string Tenant { get; }
    public string ServiceKey { get; }
    public Dictionary<string, string> Data { get; }
    /// <summary>
    /// This is optional and can be used by the consuming service to notify the publisher via HTTP that it has finished
    /// </summary>
    public string FinalizeUrl { get; set; }
}

public sealed class ConfigurationMessage : IConfigurationMessage // TODO: we might not need everything here
{
    public ConfigurationMessage(string tenant, string serviceKey, Dictionary<string, string> data)
    {
        Tenant = tenant;
        ServiceKey = serviceKey;
        Data = data ?? new Dictionary<string, string>();
    }

    public string Tenant { get; private set; }

    public string ServiceKey { get; private set; }

    public Dictionary<string, string> Data { get; private set; }

    public string FinalizeUrl { get; set; } 
}
