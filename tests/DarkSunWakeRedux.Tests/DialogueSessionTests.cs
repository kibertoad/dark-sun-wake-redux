using DarkSunWakeRedux.Core;
using Xunit;

namespace DarkSunWakeRedux.Tests;

public sealed class DialogueSessionTests
{
    [Fact]
    public void SelectsPhysicalResponseAndRetainsSourceBranchIdentity()
    {
        var session = Session([Definition(0, 1017), Definition(3, 1183),
            Definition(7, 2905)]);

        var transition = session.Execute(DialogueCommand.Select(1));

        Assert.True(transition.Applied);
        Assert.Equal(DialoguePhase.AwaitingChoice, transition.Before.Phase);
        Assert.Equal(DialoguePhase.BranchSelected, transition.After.Phase);
        Assert.Equal(3, transition.After.SelectedSourceIndex);
        Assert.Equal(1183, transition.After.BranchTargetOffset);
        Assert.False(session.Execute(DialogueCommand.Select(2)).Applied);
        Assert.Equal(3, session.Snapshot().SelectedSourceIndex);
    }

    [Fact]
    public void CompletesTheSelectedBranchAndAppliesLocalFlagsAtomically()
    {
        var session = Session([Definition(7, 2905)],
            new(new Dictionary<ushort, bool> { [0] = true },
                new Dictionary<ushort, int>()));
        session.Execute(DialogueCommand.Select(0));
        var result = Result(7, 2905, DialogueBranchDisposition.Completed,
            new Dictionary<ushort, bool> { [14] = true, [4] = true });

        var transition = session.Execute(DialogueCommand.Apply(result));

        Assert.True(transition.Applied);
        Assert.Equal(DialoguePhase.Completed, transition.After.Phase);
        Assert.True(transition.After.Variables.LocalFlags[0]);
        Assert.True(transition.After.Variables.LocalFlags[14]);
        Assert.True(transition.After.Variables.LocalFlags[4]);
        Assert.False(session.Execute(DialogueCommand.Apply(result)).Applied);
    }

    [Fact]
    public void ReturnedBranchReevaluatesAllDefinitionsInSourceOrder()
    {
        var session = Session(
        [
            Definition(0, 100, DialogueConditionKind.LocalFlag, 0),
            Definition(1, 200, DialogueConditionKind.LocalFlag, 1),
            Definition(7, 700)
        ], new(new Dictionary<ushort, bool> { [0] = true, [1] = true },
            new Dictionary<ushort, int>()));
        session.Execute(DialogueCommand.Select(0));

        var transition = session.Execute(DialogueCommand.Apply(Result(
            0, 100, DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool> { [0] = false })));

        Assert.True(transition.Applied);
        Assert.Equal(DialoguePhase.AwaitingChoice, transition.After.Phase);
        Assert.Null(transition.After.SelectedSourceIndex);
        Assert.Equal([1, 7], transition.After.Choices.Select(choice => choice.SourceIndex));
    }

    [Fact]
    public void RejectsUnknownNumberIncrementWithoutMutation()
    {
        var session = Session([Definition(2, 1148)]);
        session.Execute(DialogueCommand.Select(0));
        var before = session.Snapshot();
        var result = new DialogueBranchResult(2, 1148,
            DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool> { [2] = false },
            new Dictionary<ushort, int> { [0] = 1 });

        Assert.Throws<InvalidOperationException>(() =>
            session.Execute(DialogueCommand.Apply(result)));
        Assert.Same(before, session.Snapshot());
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 2)]
    public void ReturnedBranchesAdvanceAKnownNumberAndRevealItsChoice(
        int firstSource,
        int secondSource)
    {
        var session = Session(
        [
            Definition(2, 200, DialogueConditionKind.LocalFlag, 2),
            Definition(3, 300, DialogueConditionKind.LocalFlag, 3),
            new(4, 400, new(DialogueConditionKind.LocalNumberEquals, 0, 2)),
            Definition(7, 700)
        ], new(new Dictionary<ushort, bool> { [2] = true, [3] = true },
            new Dictionary<ushort, int> { [0] = 0 }));

        ApplyCounterResponse(session, firstSource);
        var transition = ApplyCounterResponse(session, secondSource);

        Assert.Equal(2, transition.After.Variables.LocalNumbers[0]);
        Assert.Equal([4, 7], transition.After.Choices.Select(choice => choice.SourceIndex));
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void AppliesConditionalLocalEffectsFromAKnownGlobalFlag(
        bool initialGlobalValue,
        bool expectsConditionalFlags)
    {
        var variables = new DialogueVariableSnapshot(
            new Dictionary<ushort, bool> { [1] = true },
            new Dictionary<ushort, int>())
        {
            GlobalFlags = new Dictionary<ushort, bool> { [357] = initialGlobalValue }
        };
        var session = Session([Definition(1, 1597)], variables);
        session.Execute(DialogueCommand.Select(0));
        var result = new DialogueBranchResult(1, 1597,
            DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool> { [1] = false }, null,
            new Dictionary<ushort, bool> { [357] = true },
            [
                new(new(357, false), 6, true),
                new(new(357, false), 7, true)
            ]);

        var transition = session.Execute(DialogueCommand.Apply(result));

        Assert.True(transition.After.Variables.GlobalFlags[357]);
        Assert.Equal(expectsConditionalFlags,
            transition.After.Variables.LocalFlags.ContainsKey(6));
        Assert.Equal(expectsConditionalFlags,
            transition.After.Variables.LocalFlags.ContainsKey(7));
    }

    [Fact]
    public void RejectsUnknownConditionalGlobalFlagWithoutMutation()
    {
        var session = Session([Definition(1, 1597)]);
        session.Execute(DialogueCommand.Select(0));
        var before = session.Snapshot();
        var result = new DialogueBranchResult(1, 1597,
            DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool> { [1] = false }, null, null,
            [new(new(357, false), 6, true)]);

        Assert.Throws<InvalidOperationException>(() =>
            session.Execute(DialogueCommand.Apply(result)));
        Assert.Same(before, session.Snapshot());
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void AppliesConditionalLocalEffectsFromAKnownLocalFlag(
        bool initialConditionValue,
        bool expectsAssignment)
    {
        var variables = new DialogueVariableSnapshot(
            new Dictionary<ushort, bool> { [6] = true, [16] = initialConditionValue },
            new Dictionary<ushort, int>());
        var session = Session([Definition(1, 1825)], variables);
        session.Execute(DialogueCommand.Select(0));
        var result = new DialogueBranchResult(1, 1825,
            DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool> { [6] = false },
            conditionalLocalFlagAssignmentsFromLocalFlags:
            [
                new(new(16, false), 10, true)
            ]);

        var transition = session.Execute(DialogueCommand.Apply(result));

        Assert.False(transition.After.Variables.LocalFlags[6]);
        Assert.Equal(expectsAssignment,
            transition.After.Variables.LocalFlags.ContainsKey(10));
    }

    [Fact]
    public void RejectsUnknownConditionalLocalFlagWithoutMutation()
    {
        var session = Session([Definition(1, 1825)]);
        session.Execute(DialogueCommand.Select(0));
        var before = session.Snapshot();
        var result = new DialogueBranchResult(1, 1825,
            DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool> { [6] = false },
            conditionalLocalFlagAssignmentsFromLocalFlags:
            [
                new(new(16, false), 10, true)
            ]);

        Assert.Throws<InvalidOperationException>(() =>
            session.Execute(DialogueCommand.Apply(result)));
        Assert.Same(before, session.Snapshot());
    }

    [Fact]
    public void AppliesBranchOnlyWhenRequiredGlobalNumberMatches()
    {
        var variables = DialogueVariableSnapshot.Empty with
        {
            GlobalNumbers = new Dictionary<ushort, int> { [22] = 1 }
        };
        var session = Session([Definition(3, 1996)], variables);
        session.Execute(DialogueCommand.Select(0));
        var result = new DialogueBranchResult(3, 1996,
            DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool> { [11] = true, [7] = false },
            requiredGlobalNumberConditions: [new(22, 1)]);

        var transition = session.Execute(DialogueCommand.Apply(result));

        Assert.True(transition.After.Variables.LocalFlags[11]);
        Assert.False(transition.After.Variables.LocalFlags[7]);
        Assert.Equal(1, transition.After.Variables.GlobalNumbers[22]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(2)]
    public void RejectsUnknownOrDifferentRequiredGlobalNumberWithoutMutation(
        int? value)
    {
        var variables = DialogueVariableSnapshot.Empty with
        {
            GlobalNumbers = value.HasValue
                ? new Dictionary<ushort, int> { [22] = value.Value }
                : new Dictionary<ushort, int>()
        };
        var session = Session([Definition(3, 1996)], variables);
        session.Execute(DialogueCommand.Select(0));
        var before = session.Snapshot();
        var result = new DialogueBranchResult(3, 1996,
            DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool> { [11] = true },
            requiredGlobalNumberConditions: [new(22, 1)]);

        Assert.Throws<InvalidOperationException>(() =>
            session.Execute(DialogueCommand.Apply(result)));
        Assert.Same(before, session.Snapshot());
    }

    [Fact]
    public void EvaluatesCompositeLocalConditionAfterDirectAssignments()
    {
        var variables = new DialogueVariableSnapshot(
            new Dictionary<ushort, bool> { [1] = false, [8] = true },
            new Dictionary<ushort, int>());
        var session = Session([Definition(6, 3976)], variables);
        session.Execute(DialogueCommand.Select(0));

        var transition = session.Execute(DialogueCommand.Apply(new(6, 3976,
            DialogueBranchDisposition.Completed,
            new Dictionary<ushort, bool> { [8] = false },
            conditionalLocalFlagAssignmentsAfterLocalFlags:
            [
                new([new(1, false), new(8, false)], 14, true)
            ])));

        Assert.False(transition.After.Variables.LocalFlags[8]);
        Assert.True(transition.After.Variables.LocalFlags[14]);
    }

    [Fact]
    public void RejectsUnknownCompositeLocalConditionWithoutMutation()
    {
        var session = Session([Definition(6, 3976)]);
        session.Execute(DialogueCommand.Select(0));
        var before = session.Snapshot();

        Assert.Throws<InvalidOperationException>(() => session.Execute(
            DialogueCommand.Apply(new(6, 3976,
                DialogueBranchDisposition.Completed,
                new Dictionary<ushort, bool> { [8] = false },
                conditionalLocalFlagAssignmentsAfterLocalFlags:
                [
                    new([new(1, false), new(8, false)], 14, true)
                ]))));
        Assert.Same(before, session.Snapshot());
    }

    [Fact]
    public void RejectsInvalidConstructionSelectionAndMismatchedBranch()
    {
        var variables = DialogueVariableSnapshot.Empty;
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DialogueSession(0, [Definition(0, 1)], variables, 5));
        Assert.Throws<ArgumentException>(() => new DialogueSession(1, [], variables, 5));
        Assert.Throws<ArgumentException>(() => new DialogueSession(1,
            [Definition(0, 1), Definition(0, 2)], variables, 5));
        Assert.Throws<ArgumentException>(() => new DialogueSession(1,
            [Definition(-1, 1)], variables, 5));
        Assert.Throws<ArgumentException>(() => new DialogueBranchResult(0, 1,
            DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool>(),
            requiredGlobalNumberConditions: [new(22, 1), new(22, 2)]));
        var session = Session([Definition(7, 2905)]);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            session.Execute(DialogueCommand.Select(1)));
        session.Execute(DialogueCommand.Select(0));
        var other = Result(6, 2905, DialogueBranchDisposition.Completed,
            new Dictionary<ushort, bool> { [4] = true });

        Assert.Throws<InvalidOperationException>(() =>
            session.Execute(DialogueCommand.Apply(other)));
        Assert.Equal(DialoguePhase.BranchSelected, session.Snapshot().Phase);
    }

    [Fact]
    public void CopiesCallerOwnedCollectionsAtDeterministicBoundaries()
    {
        var definitions = new[] { Definition(7, 2905) };
        var flags = new Dictionary<ushort, bool> { [0] = true };
        var globalFlags = new Dictionary<ushort, bool> { [357] = false };
        var globalNumbers = new Dictionary<ushort, int> { [22] = 1 };
        var assignments = new Dictionary<ushort, bool> { [4] = true };
        var session = Session(definitions,
            new(flags, new Dictionary<ushort, int>())
            {
                GlobalFlags = globalFlags,
                GlobalNumbers = globalNumbers
            });
        var result = Result(7, 2905, DialogueBranchDisposition.Completed, assignments);
        definitions[0] = Definition(1, 2);
        flags[0] = false;
        globalFlags[357] = true;
        globalNumbers[22] = 2;
        assignments[4] = false;

        session.Execute(DialogueCommand.Select(0));
        session.Execute(DialogueCommand.Apply(result));

        Assert.Equal(7, session.Snapshot().SelectedSourceIndex);
        Assert.True(session.Snapshot().Variables.LocalFlags[0]);
        Assert.True(session.Snapshot().Variables.LocalFlags[4]);
        Assert.False(session.Snapshot().Variables.GlobalFlags[357]);
        Assert.Equal(1, session.Snapshot().Variables.GlobalNumbers[22]);
    }

    private static DialogueSession Session(
        IReadOnlyList<DialogueChoiceDefinition> definitions,
        DialogueVariableSnapshot? variables = null) =>
        new(135, definitions, variables ?? DialogueVariableSnapshot.Empty, 5);

    private static DialogueChoiceDefinition Definition(
        int index,
        int target,
        DialogueConditionKind kind = DialogueConditionKind.Constant,
        ushort variableId = 0) =>
        new(index, target, new(kind, variableId, 1));

    private static DialogueBranchResult Result(
        int index,
        int target,
        DialogueBranchDisposition disposition,
        IReadOnlyDictionary<ushort, bool> flags) =>
        new(index, target, disposition, flags);

    [Fact]
    public void ReidentifiesAProjectedBranchWithoutChangingItsEffects()
    {
        var result = new DialogueBranchResult(7, 2905,
            DialogueBranchDisposition.Completed,
            new Dictionary<ushort, bool> { [14] = true },
            conditionalLocalFlagAssignmentsFromLocalFlags:
            [
                new(new(16, false), 10, true)
            ],
            requiredGlobalNumberConditions: [new(22, 1)]);

        var reidentified = result.ForSourceIndex(6);

        Assert.Equal(6, reidentified.SourceIndex);
        Assert.Equal(2905, reidentified.BranchTargetOffset);
        Assert.Equal(DialogueBranchDisposition.Completed, reidentified.Disposition);
        Assert.True(reidentified.LocalFlagAssignments[14]);
        Assert.Single(reidentified.ConditionalLocalFlagAssignmentsFromLocalFlags);
        Assert.Equal(new DialogueGlobalNumberCondition(22, 1),
            Assert.Single(reidentified.RequiredGlobalNumberConditions));
    }

    private static DialogueTransition ApplyCounterResponse(
        DialogueSession session,
        int sourceIndex)
    {
        var responseIndex = session.Snapshot().Choices.ToList()
            .FindIndex(choice => choice.SourceIndex == sourceIndex);
        session.Execute(DialogueCommand.Select(responseIndex));
        return session.Execute(DialogueCommand.Apply(new(sourceIndex, sourceIndex * 100,
            DialogueBranchDisposition.ReturnToChoices,
            new Dictionary<ushort, bool> { [checked((ushort)sourceIndex)] = false },
            new Dictionary<ushort, int> { [0] = 1 })));
    }
}
