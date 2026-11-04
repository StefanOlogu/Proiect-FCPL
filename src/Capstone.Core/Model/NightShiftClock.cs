namespace Capstone.Core.Model;

/// <summary>The shift runs from 00:00 to 06:00; at 06:00 the bartender goes home.</summary>
public static class NightShiftClock
{
    public const int Hours = 6;

    public static string Format(int hour) => $"{hour:D2}:00";
}
