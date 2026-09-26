namespace DarkSunWakeRedux.Core;

/// <summary>
/// Deterministic pseudo-random stream matching the bounded native primitive
/// specified by RULE-RNG-001. Consumers own their seed and call order.
/// </summary>
public sealed class NativeRandom
{
    public const uint Multiplier = 0x015A4E35;
    public const uint Increment = 1;

    /// <summary>
    /// Initializes the stream through the native setter's proven 16-bit seed
    /// boundary; that setter clears the upper state word.
    /// </summary>
    public NativeRandom(ushort seed) => State = seed;

    public uint State { get; private set; }

    /// <summary>Advances the 32-bit stream and returns the native 15-bit result.</summary>
    public ushort Next()
    {
        State = unchecked(State * Multiplier + Increment);
        return (ushort)((State >> 16) & 0x7fff);
    }

    /// <summary>
    /// Matches the native range wrapper: zero returns zero without consuming the
    /// stream; nonzero divisors consume one value and apply modulo reduction.
    /// </summary>
    public ushort NextModulo(ushort divisor) => divisor == 0 ? (ushort)0 :
        (ushort)(Next() % divisor);

    /// <summary>
    /// Matches the observed inclusive-range helper. Equal or reversed bounds
    /// return the lower value without consuming the stream.
    /// </summary>
    public short NextInclusive(short lower, short upper)
    {
        if (lower >= upper)
            return lower;

        var span = upper - lower + 1;
        return checked((short)(lower + (Next() * span) / 0x8000));
    }

    /// <summary>
    /// Matches the observed repeated scaled-roll helper. Non-positive counts
    /// return zero without consuming the stream.
    /// </summary>
    public int NextScaledSum(short count, short scale)
    {
        var total = 0;
        for (var index = 0; index < count; index++)
            total += (Next() * scale) / 0x8000 + 1;
        return total;
    }
}
