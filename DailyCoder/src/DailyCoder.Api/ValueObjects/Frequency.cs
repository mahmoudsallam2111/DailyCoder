using static DailyCoder.Api.Enums.Enums;

namespace DailyCoder.Api.ValueObjects;

public sealed class Frequency
{
    public FrequencyType Type { get; set; }
    public int TimesPerPeriod { get; set; }
}
