namespace DarkSunWakeRedux.Resources;

public sealed record GffCharacterAbilityScores(
    byte Strength,
    byte Dexterity,
    byte Constitution,
    byte Intelligence,
    byte Wisdom,
    byte Charisma)
{
    public const int ScoresOffset = 35;
    public const int ScoreCount = 6;
    public const byte MinimumScore = 9;
    public const byte MaximumScore = 24;

    public static GffCharacterAbilityScores Read(ReadOnlyMemory<byte> data, string sourceName)
    {
        GffCharacterRecordEnvelope.Read(data, sourceName);

        var values = data.Span.Slice(ScoresOffset, ScoreCount);
        var result = new GffCharacterAbilityScores(
            values[0], values[1], values[2], values[3], values[4], values[5]);
        foreach (var (name, value) in result.All())
            if (value is < MinimumScore or > MaximumScore)
                throw new InvalidDataException(
                    $"{sourceName}: CHAR {name} score {value} is outside {MinimumScore}..{MaximumScore}.");
        return result;
    }

    public IEnumerable<(string Name, byte Value)> All()
    {
        yield return (nameof(Strength), Strength);
        yield return (nameof(Dexterity), Dexterity);
        yield return (nameof(Constitution), Constitution);
        yield return (nameof(Intelligence), Intelligence);
        yield return (nameof(Wisdom), Wisdom);
        yield return (nameof(Charisma), Charisma);
    }
}
