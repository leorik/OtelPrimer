namespace OTelPrimer.Services;

using OTelPrimer.Model;

public interface IPingSender
{
    Task<Ping?> SendPing(string target, Ping ping, CancellationToken cancellationToken = default);
}