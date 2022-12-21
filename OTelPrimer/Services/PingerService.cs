namespace OTelPrimer.Services;

using System.Diagnostics;

using Microsoft.Extensions.Options;

using OTelPrimer.Configuration;
using OTelPrimer.Model;

public class PingerService : BackgroundService
{
    private readonly ILogger<PingerService> _logger;

    private readonly IPingSender _pingSender;

    private readonly ActivitySource _activitySource;

    private readonly TimeSpan _pingInterval;

    private IReadOnlyCollection<string> _pingTargets;

    private int _lastPingId;

    public PingerService(ILogger<PingerService> logger, IPingSender pingSender, IOptions<PingConfig> config, ActivitySource activitySource)
    {
        _logger = logger;
        _pingSender = pingSender;
        _activitySource = activitySource;

        _pingInterval = config.Value.PingInterval;
        _pingTargets = config.Value.PingTargets ?? ArraySegment<string>.Empty;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _pingTargets.Any())
        {
            await Task.Delay(_pingInterval, stoppingToken);

            using var activity = SpawnNewRootActivity();
            activity?.Start();
            
            _logger.LogDebug("It's pinging time!");

            var nextPingId = Interlocked.Increment(ref _lastPingId);
            
            _logger.LogInformation("Preparing new ping with ID={PingId}", nextPingId);

            var ping = new Ping(nextPingId, 0);

            var pingTasks = _pingTargets.Select(target => IssuePing(target, ping, stoppingToken)).ToArray();

            await Task.WhenAll(pingTasks);
        }
    }

    private async Task IssuePing(string target, Ping ping, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Issuing ping with ID={PingId} to target {PingTarget}", ping.PingId, target);

        var killSwitch = 100;

        while (killSwitch > 0)
        {
            killSwitch--;

            var pong= await _pingSender.SendPing(target, ping, cancellationToken);

            if (pong is null)
            {
                _logger.LogWarning("Ping target haven't returned anything for ping with ID={PingId} and iteration={PingIteration}, terminating", ping.PingId, ping.PingIteration);
                break;
            }

            if (pong.Equals(ping))
            {
                _logger.LogInformation("Ping-pong complete for ping with ID={PingId}", pong.PingId);
                break;
            }

            ping = pong with { PingIteration = pong.PingIteration + 1 };
            
            _logger.LogDebug("Received pong for ping with ID={PingId}, incremented iteration to {PingIteration}", ping.PingId, ping.PingIteration);
        }

        if (killSwitch == 0)
        {
            _logger.LogError("Kill switch exhausted processing ping with ID={PingId}", ping.PingId);
        }
    }

    private Activity? SpawnNewRootActivity()
    {
        var previousCurrentActivity = Activity.Current;
        Activity.Current = null;
        var rootActivity = _activitySource.CreateActivity("PingPong", ActivityKind.Internal);
        Activity.Current = previousCurrentActivity;

        return rootActivity;
    }
}