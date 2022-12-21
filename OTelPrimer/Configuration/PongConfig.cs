namespace OTelPrimer.Configuration;

public sealed class PongConfig
{
    public int RngSeed { get; set; }

    public PongConfig()
    {
    }

    public PongConfig(int rngSeed)
    {
        RngSeed = rngSeed;
    }
};