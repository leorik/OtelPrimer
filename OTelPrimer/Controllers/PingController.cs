using Microsoft.AspNetCore.Mvc;

namespace OTelPrimer.Controllers;

using Microsoft.Extensions.Options;

using OTelPrimer.Configuration;
using OTelPrimer.Model;

[Route("/api/ping")]
public class PingController : ControllerBase
{
    private const int IterationCap = 10;

    private const int FailedRequestCode = 418;
    
    private readonly ILogger<PingController> _logger;

    private readonly Random _rng;
    
    public PingController(ILogger<PingController> logger, IOptions<PongConfig> options)
    {
        _logger = logger;
        _rng = new Random(options.Value.RngSeed);
    }

    [HttpPost]
    public Ping ProcessPing([FromBody] Ping ping)
    {
        _logger.LogInformation("Incoming ping with ID={PingId}, iteration={PingIteration}", ping.PingId, ping.PingIteration);

        var nextIteration = ping.PingIteration + 1;
        _logger.LogDebug("Preparing new iteration of ping with ID={PingId}, new iteration will be {PingIteration}", ping.PingId, nextIteration);

        if (nextIteration > IterationCap)
        {
            _logger.LogInformation("New iteration for ping with ID={PingId} is over iteration cap, no further processing needed", ping.PingId);

            return ping;
        }

        var failureLotteryNumber = _rng.Next(10);
        
        _logger.LogDebug("Failure lottery number selected as {FailureLotteryNumber}", failureLotteryNumber);
        
        var shouldFail = ping.PingId % 2 == 0 && ping.PingIteration > failureLotteryNumber;

        if (shouldFail)
        {
            _logger.LogInformation("Ping with ID={PingId} won failure lottery, returning error", ping);
            
            throw new BadHttpRequestException("Unable to process ping, ", FailedRequestCode);
        }

        _logger.LogInformation("Ping with ID={PingId} passed and assigned new iteration number {PingIteration}", ping.PingId, nextIteration);
        
        return ping with { PingIteration = nextIteration };
    }
}