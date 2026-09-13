namespace DarkSunWakeRedux.Resources;

public sealed record GffCharacterCatalogEntry(
    uint ResourceNumber,
    uint ResourceSize,
    byte TailRecordCount,
    string Name,
    GffCharacterAbilityScores AbilityScores,
    byte RawPsionicMask);

public static class GffCharacterCatalog
{
    public static IReadOnlyList<GffCharacterCatalogEntry> Read(
        GffArchive archive, string sourceName)
    {
        ArgumentNullException.ThrowIfNull(archive);
        var selections = archive.Resources.Where(resource => resource.Tag == "PSIN")
            .ToDictionary(resource => resource.Number, resource =>
                GffPsionicMask.Read(
                    archive.GetResource(resource.Tag, resource.Number),
                    $"{sourceName}:{resource.Tag}#{resource.Number}"));
        var characters = archive.Resources.Where(resource => resource.Tag == "CHAR")
            .OrderBy(resource => resource.Number)
            .Select(resource =>
            {
                var data = archive.GetResource(resource.Tag, resource.Number);
                var envelope = GffCharacterRecordEnvelope.Read(
                    data, $"{sourceName}:{resource.Tag}#{resource.Number}");
                var identity = GffCharacterIdentity.Read(
                    data,
                    $"{sourceName}:{resource.Tag}#{resource.Number}");
                var abilityScores = GffCharacterAbilityScores.Read(
                    data,
                    $"{sourceName}:{resource.Tag}#{resource.Number}");
                if (!selections.Remove(resource.Number, out var selection))
                    throw new InvalidDataException(
                        $"{sourceName}: CHAR #{resource.Number} has no matching PSIN resource.");
                return new GffCharacterCatalogEntry(
                    resource.Number, resource.Size, envelope.TailRecordCount,
                    identity.Name, abilityScores, selection.RawMask);
            }).ToArray();
        if (selections.Count != 0)
            throw new InvalidDataException(
                $"{sourceName}: PSIN resources have no matching CHAR records: " +
                string.Join(", ", selections.Keys.Order()) + ".");
        return characters;
    }
}
