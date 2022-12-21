namespace OTelPrimer.Configuration;

public class PingConfig
{
    public TimeSpan PingInterval { get; set; }
    
    public IReadOnlyCollection<string>? PingTargets { get; set; }
}