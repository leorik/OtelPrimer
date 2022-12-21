namespace OTelPrimer.Services;

using OTelPrimer.Model;

public class PingSender : IPingSender
{
    private readonly ILogger<PingSender> _logger;

    private readonly IHttpClientFactory _httpClientFactory;

    public PingSender(ILogger<PingSender> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Ping?> SendPing(string target, Ping ping, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Preparing client for target {PingTarget}", target);

        var client = _httpClientFactory.CreateClient(target);

        try
        {
            var requestUri = $"http://{target}/api/ping";
            
            _logger.LogInformation("Sending POST request to URL \"{PingUrl}\" containing ping with ID={PingId}", requestUri, ping.PingId);
            
            var result = await client.PostAsync(requestUri, JsonContent.Create(ping), cancellationToken);
            
            _logger.LogDebug("Received result for POST request to \"{PingUrl}\", deserializing", requestUri);
            
            return await result.Content.ReadFromJsonAsync<Ping>(cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error during sending ping with ID={PingId}", ping.PingId);

            return null;
        }
    }
}