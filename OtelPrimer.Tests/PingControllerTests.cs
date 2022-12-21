namespace TestProject1;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using OTelPrimer.Configuration;
using OTelPrimer.Controllers;
using OTelPrimer.Model;

public class PingControllerTests
{
    private const int BadRngSeed = 1; // Honestly brute-forced
    
    private static readonly IOptions<PongConfig> DefaultOptions = new OptionsWrapper<PongConfig>(new PongConfig(0));
    
    [Fact]
    public void Can_increment_iteration_for_ping_below_cap()
    {
        // Arrange
        var incomingPing = new Ping(1, 1);
        var sut = new PingController(NullLogger<PingController>.Instance, DefaultOptions);

        // Act
        var resultPing = sut.ProcessPing(incomingPing);

        // Assert
        resultPing.Should().BeEquivalentTo(incomingPing with { PingIteration = 2});
    }
    
    [Fact]
    public void Cannot_increment_iteration_for_ping_above_cap()
    {
        // Arrange
        var incomingPing = new Ping(1, 10);
        var sut = new PingController(NullLogger<PingController>.Instance, DefaultOptions);

        // Act
        var resultPing = sut.ProcessPing(incomingPing);

        // Assert
        resultPing.Should().BeEquivalentTo(incomingPing);
    }

    [Fact]
    public void Can_throw_if_rng_is_blessed()
    {
        // Arrange
        var incomingPing = new Ping(2, 3);
        
        var sut = new PingController(NullLogger<PingController>.Instance, new OptionsWrapper<PongConfig>(new PongConfig(BadRngSeed)));

        // Act
        var action = (() => sut.ProcessPing(incomingPing));

        // Assert
        action.Should().Throw<BadHttpRequestException>();
    }
}