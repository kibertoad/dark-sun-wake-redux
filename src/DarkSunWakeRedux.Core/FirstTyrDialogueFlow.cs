using System.Collections.ObjectModel;

namespace DarkSunWakeRedux.Core;

public sealed record FirstTyrDialogueContinuation(
    DialogueVariableSnapshot Variables,
    bool AdvancesToSecondMenu);

public static class FirstTyrDialogueFlow
{
    public static FirstTyrDialogueContinuation ContinueOpeningMenu(
        DialogueVariableSnapshot variables)
    {
        ArgumentNullException.ThrowIfNull(variables);
        var flags = new Dictionary<ushort, bool>(variables.LocalFlags);
        var numbers = new Dictionary<ushort, int>(variables.LocalNumbers);
        var globalFlags = new Dictionary<ushort, bool>(variables.GlobalFlags);
        var firstQuestionRemains = RequiredFlag(flags, 1);
        var secondQuestionRemains = RequiredFlag(flags, 2);
        var thirdQuestionRemains = RequiredFlag(flags, 3);
        var counter = RequiredNumber(numbers, 0);
        var anyQuestionRemains = firstQuestionRemains || secondQuestionRemains ||
            thirdQuestionRemains;
        if (anyQuestionRemains)
        {
            if (counter == 0) flags[4] = true;
        }
        else
            flags[0] = false;
        if (counter == 2) flags[5] = true;
        var advances = RequiredFlag(flags, 4);
        if (advances)
        {
            if (RequiredFlag(globalFlags, 357))
            {
                flags[6] = true;
                flags[7] = true;
            }
            else
                flags[1] = true;
            flags[8] = true;
        }
        return new(new(
            new ReadOnlyDictionary<ushort, bool>(flags),
            new ReadOnlyDictionary<ushort, int>(numbers))
        {
            GlobalFlags = new ReadOnlyDictionary<ushort, bool>(globalFlags)
        }, advances);
    }

    private static bool RequiredFlag(
        IReadOnlyDictionary<ushort, bool> flags,
        ushort id) => flags.TryGetValue(id, out var value)
            ? value
            : throw new InvalidOperationException($"Dialogue flag #{id} is unknown.");

    private static int RequiredNumber(
        IReadOnlyDictionary<ushort, int> numbers,
        ushort id) => numbers.TryGetValue(id, out var value)
            ? value
            : throw new InvalidOperationException($"Dialogue number #{id} is unknown.");
}
