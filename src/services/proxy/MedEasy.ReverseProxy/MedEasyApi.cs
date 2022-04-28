namespace MedEasy.ReverseProxy;

/// <summary>
/// Wrapper for describing a MedEasy REST API
/// </summary>
public class MedEasyApi
{
    /// <summary>
    /// Name of the API
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Id of the API
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Binding associated with the API
    /// </summary>
    public string Binding { get; set; }

    public HttpClientConfiguration HttpClient { get; set; }
}

/// <summary>
/// Wraps the configuration for an HttpClient used to forward requests to the MedEasy API
/// </summary>
public record HttpClientConfiguration
{
    /// <summary>
    /// Gets/sets whether if SSL certificates should be thrusted by the reverse proxy when forwarding requests to the MedEasy API
    /// </summary>
    public bool ThrustSslCertificate { get; set; }
}
