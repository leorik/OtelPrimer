namespace OTelPrimer.Services;

using System.Diagnostics;

using OTelPrimer.Model;

public class ActivityEnrichedPingSenderDecorator : IPingSender
{
    private readonly PingSender _decoratee;

    private readonly ActivitySource _activitySource;

    public ActivityEnrichedPingSenderDecorator(PingSender decoratee, ActivitySource activitySource)
    {
        _decoratee = decoratee;
        _activitySource = activitySource;
    }

    public Task<Ping?> SendPing(string target, Ping ping, CancellationToken cancellationToken = default)
    {
        using var sendingActivity = _activitySource.StartActivity(ActivityKind.Client);

        return _decoratee.SendPing(target, ping, cancellationToken);
    }
}