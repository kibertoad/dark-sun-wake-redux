using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Game;
using DarkSunWakeRedux.Resources;

try
{
    if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase)) return 0;
    var platformSmoke = args.Contains("--platform-smoke-test", StringComparer.OrdinalIgnoreCase);
    var contentSmoke = args.Contains("--content-smoke-test", StringComparer.OrdinalIgnoreCase);
    string? assetPack = null;
    if (!platformSmoke)
    {
        assetPack = Option(args, "--asset-pack") ?? OriginalContent.DefaultAssetPackPath();
        var diagnostics = await OriginalContent.VerifyInstalledAsync(assetPack);
        if (diagnostics.Count != 0)
        {
            var details = string.Join(Environment.NewLine,
                diagnostics.Select(diagnostic => $"[{diagnostic.Code}] {diagnostic.Message}"));
            throw new InvalidDataException(
                $"A verified local asset pack is required at '{assetPack}'." + Environment.NewLine +
                "Run DarkSunWakeRedux.Extractor against your legally owned GOG installation." +
                Environment.NewLine + details);
        }
        if (contentSmoke)
        {
            var titlePath = Path.Combine(assetPack,
                OriginalContent.TitleImageAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var titleStream = File.OpenRead(titlePath);
            var title = PackedIndexedImage.Read(titleStream, OriginalContent.TitleImageAssetPath);
            var titleFrame = title.Frames.Single();
            if (titleFrame.Width != 320 || titleFrame.Height != 200)
                throw new InvalidDataException("The installed title image is not the required 320x200 frame.");
            foreach (var layer in OriginalContent.StartMenuLayers.Concat(OriginalContent.PartyOverviewLayers))
            {
                var image = ReadImage(assetPack, layer.Path);
                var frame = AssertSingleFrame(image.Frames, layer.Name);
                if (frame.Width != layer.FrameWidth || frame.Height != layer.FrameHeight)
                    throw new InvalidDataException(
                        $"The installed {layer.Name} image has unexpected geometry.");
            }
            foreach (var asset in OriginalContent.AddExistingCharacterAssets)
            {
                var image = ReadImage(assetPack, asset.Path);
                if (!asset.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException(
                        $"The installed add-existing {asset.Name} image has unexpected geometry.");
            }
            foreach (var button in OriginalContent.StartMenuButtons)
            {
                var buttonPath = Path.Combine(assetPack,
                    button.Path.Replace('/', Path.DirectorySeparatorChar));
                using var buttonStream = File.OpenRead(buttonPath);
                var image = PackedIndexedImage.Read(buttonStream, button.Path);
                if (!button.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException($"The installed {button.Name} image has unexpected geometry.");
            }
            foreach (var button in OriginalContent.CharacterGenerationButtons)
            {
                var buttonPath = Path.Combine(assetPack,
                    button.Path.Replace('/', Path.DirectorySeparatorChar));
                using var buttonStream = File.OpenRead(buttonPath);
                var image = PackedIndexedImage.Read(buttonStream, button.Path);
                if (!button.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException(
                        $"The installed character-generation {button.Name} image has unexpected geometry.");
            }
            foreach (var button in OriginalContent.CharacterGenerationModalButtons)
            {
                var buttonPath = Path.Combine(assetPack,
                    button.Path.Replace('/', Path.DirectorySeparatorChar));
                using var buttonStream = File.OpenRead(buttonPath);
                var image = PackedIndexedImage.Read(buttonStream, button.Path);
                if (!button.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException(
                        $"The installed character-generation modal {button.Name} image has unexpected geometry.");
            }
            var gameMenuLayer = ReadImage(assetPack, OriginalContent.GameMenuLayer.Path);
            var gameMenuFrame = AssertSingleFrame(gameMenuLayer.Frames, "game-menu base");
            if (gameMenuFrame.Width != OriginalContent.GameMenuLayer.FrameWidth ||
                gameMenuFrame.Height != OriginalContent.GameMenuLayer.FrameHeight)
                throw new InvalidDataException("The installed game-menu base has unexpected geometry.");
            foreach (var button in OriginalContent.GameMenuButtons
                         .Concat(OriginalContent.PreferencesButtons)
                         .DistinctBy(asset => asset.Path))
            {
                var image = ReadImage(assetPack, button.Path);
                if (!button.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException(
                        $"The installed game-menu {button.Name} image has unexpected geometry.");
            }
            var inventoryLayer = ReadImage(assetPack, OriginalContent.InventoryLayer.Path);
            var inventoryFrame = AssertSingleFrame(inventoryLayer.Frames, "inventory base");
            if (inventoryFrame.Width != OriginalContent.InventoryLayer.FrameWidth ||
                inventoryFrame.Height != OriginalContent.InventoryLayer.FrameHeight)
                throw new InvalidDataException(
                    "The installed inventory base has unexpected geometry.");
            foreach (var layer in OriginalContent.ExplorationDestinationTitleLayers)
            {
                var image = ReadImage(assetPack, layer.Path);
                var frame = AssertSingleFrame(image.Frames, layer.Name);
                if (frame.Width != layer.FrameWidth || frame.Height != layer.FrameHeight)
                    throw new InvalidDataException(
                        $"The installed {layer.Name} image has unexpected geometry.");
            }
            foreach (var cursor in OriginalContent.ExplorationCursorAssets)
            {
                var image = ReadImage(assetPack, cursor.Path);
                if (!cursor.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException(
                        $"The installed exploration cursor {cursor.Name} has unexpected geometry.");
            }
            foreach (var interaction in OriginalContent.InteractionButtonAssets)
            {
                var image = ReadImage(assetPack, interaction.Path);
                if (!interaction.HasExpectedFrames(image.Frames))
                    throw new InvalidDataException(
                        $"The installed interaction image {interaction.Name} has unexpected geometry.");
            }
            var dialoguePortrait = ReadImage(assetPack,
                OriginalContent.FirstTyrDialoguePortraitAssetPath);
            if (dialoguePortrait.Frames.Count != 1 ||
                dialoguePortrait.Frames[0].Width != 72 ||
                dialoguePortrait.Frames[0].Height != 72)
                throw new InvalidDataException(
                    "The installed first Tyr dialogue portrait has unexpected geometry.");
            FirstTyrDialogueProjection dialogueProjection;
            FirstTyrDialogueCompletionProjection dialogueCompletion;
            FirstTyrDialogueResponseProjection firstDialogueResponse;
            FirstTyrDialogueResponseProjection secondDialogueResponse;
            FirstTyrDialogueResponseProjection thirdDialogueResponse;
            FirstTyrDialogueResponseProjection fourthDialogueResponse;
            FirstTyrDialogueResponseProjection fifthDialogueResponse;
            FirstTyrDialogueResponseProjection troubleDialogueResponse;
            FirstTyrDialogueResponseProjection kingDialogueResponse;
            FirstTyrDialogueResponseProjection caravanDialogueResponse;
            FirstTyrDialogueResponseProjection acarDialogueResponse;
            FirstTyrDialogueResponseProjection cityDialogueResponse;
            FirstTyrDialogueResponseProjection thirdMenuFirstDialogueResponse;
            FirstTyrDialogueResponseProjection thirdMenuSecondDialogueResponse;
            FirstTyrDialogueResponseProjection thirdMenuThirdDialogueResponse;
            FirstTyrDialogueResponseProjection thirdMenuFifthDialogueResponse;
            FirstTyrDialogueResponseProjection thirdMenuSixthDialogueResponse;
            FirstTyrDialogueThirdCompletionProjection thirdMenuCompletion;
            using (var scriptStream = File.OpenRead(Path.Combine(assetPack,
                       OriginalContent.FirstTyrDialogueScriptAssetPath.Replace(
                           '/', Path.DirectorySeparatorChar))))
            {
                var script = PackedGplScript.Read(scriptStream,
                    OriginalContent.FirstTyrDialogueScriptAssetPath);
                if (script.ResourceNumber != OriginalContent.FirstTyrDialogueScriptResourceNumber)
                    throw new InvalidDataException(
                        "The installed first Tyr dialogue script has unexpected identity.");
                dialogueProjection = FirstTyrDialogueProjectionReader.Read(script);
                dialogueCompletion = FirstTyrDialogueCompletionProjectionReader.Read(script);
                firstDialogueResponse = FirstTyrDialogueResponseProjectionReader.ReadFirst(script);
                secondDialogueResponse = FirstTyrDialogueResponseProjectionReader.ReadSecond(script);
                thirdDialogueResponse = FirstTyrDialogueResponseProjectionReader.ReadThird(script);
                fourthDialogueResponse = FirstTyrDialogueResponseProjectionReader.ReadFourth(script);
                fifthDialogueResponse = FirstTyrDialogueResponseProjectionReader.ReadFifth(script);
                troubleDialogueResponse =
                    FirstTyrDialogueResponseProjectionReader.ReadTrouble(script);
                kingDialogueResponse =
                    FirstTyrDialogueResponseProjectionReader.ReadKing(script);
                caravanDialogueResponse =
                    FirstTyrDialogueResponseProjectionReader.ReadOpeningCaravan(script);
                acarDialogueResponse =
                    FirstTyrDialogueResponseProjectionReader.ReadAcar(script);
                cityDialogueResponse =
                    FirstTyrDialogueResponseProjectionReader.ReadCityIntroduction(script);
                thirdMenuFirstDialogueResponse =
                    FirstTyrDialogueResponseProjectionReader.ReadThirdMenuFirst(script);
                thirdMenuSecondDialogueResponse =
                    FirstTyrDialogueResponseProjectionReader.ReadThirdMenuSecond(script);
                thirdMenuThirdDialogueResponse =
                    FirstTyrDialogueResponseProjectionReader.ReadThirdMenuThird(script);
                thirdMenuFifthDialogueResponse =
                    FirstTyrDialogueResponseProjectionReader.ReadThirdMenuFifth(script);
                thirdMenuSixthDialogueResponse =
                    FirstTyrDialogueResponseProjectionReader.ReadThirdMenuSixth(script);
                thirdMenuCompletion =
                    FirstTyrDialogueResponseProjectionReader.ReadThirdMenuExit(script);
                if (dialogueProjection.PortraitResourceNumber !=
                        OriginalContent.FirstTyrDialoguePortraitResourceNumber ||
                    dialogueProjection.SpeechVariants.Count != 2 ||
                    dialogueProjection.InitialChoices.Count != 8 ||
                    dialogueProjection.SecondChoices.Count != 7 ||
                    dialogueProjection.ThirdChoices.Count != 7 ||
                    !dialogueProjection.ThirdChoices.Select(choice =>
                            (choice.TargetOffset, choice.Condition.Kind,
                                choice.Condition.VariableId, choice.Condition.Value))
                        .SequenceEqual(new (int, GplDialogueConditionKind, ushort, int)[]
                        {
                            (2921, GplDialogueConditionKind.LocalFlag, 12, 1),
                            (3089, GplDialogueConditionKind.LocalFlag, 13, 1),
                            (3257, GplDialogueConditionKind.LocalFlag, 15, 1),
                            (3479, GplDialogueConditionKind.LocalFlag, 10, 1),
                            (3686, GplDialogueConditionKind.LocalFlag, 17, 1),
                            (3786, GplDialogueConditionKind.LocalFlag, 18, 1),
                            (3976, GplDialogueConditionKind.Constant, 0, 1)
                        }) ||
                    !dialogueProjection.InitialChoices.Select(choice =>
                            (choice.Condition.Kind, choice.Condition.VariableId,
                                choice.Condition.Value))
                        .SequenceEqual(new (GplDialogueConditionKind, ushort, int)[]
                        {
                            (GplDialogueConditionKind.LocalFlag, 0, 1),
                            (GplDialogueConditionKind.LocalFlag, 1, 1),
                            (GplDialogueConditionKind.LocalFlag, 2, 1),
                            (GplDialogueConditionKind.LocalFlag, 3, 1),
                            (GplDialogueConditionKind.LocalNumberEquals, 0, 2),
                            (GplDialogueConditionKind.LocalFlag, 9, 1),
                            (GplDialogueConditionKind.LocalFlag, 5, 1),
                            (GplDialogueConditionKind.Constant, 0, 1)
                        }))
                    throw new InvalidDataException(
                        "The installed first Tyr dialogue projection has drifted.");
            }
            GplGlobalStringProjection globalString;
            GplGlobalStringProjection thirdMenuExit;
            using (var globalsStream = File.OpenRead(Path.Combine(assetPack,
                       OriginalContent.DialogueGlobalStringsScriptAssetPath.Replace(
                           '/', Path.DirectorySeparatorChar))))
            {
                var globals = PackedGplScript.Read(globalsStream,
                    OriginalContent.DialogueGlobalStringsScriptAssetPath);
                globalString = GplGlobalStringProjectionReader.Read(globals);
                thirdMenuExit = GplGlobalStringProjectionReader.ReadThirdMenuExit(globals);
                if (globalString.Destination != new GplDialogueVariable(6, 5) ||
                    string.IsNullOrWhiteSpace(globalString.Text) ||
                    thirdMenuExit.Destination != new GplDialogueVariable(6, 6) ||
                    string.IsNullOrWhiteSpace(thirdMenuExit.Text))
                    throw new InvalidDataException(
                        "The installed dialogue global-string projection has drifted.");
            }
            var openingLeader = ReadImage(assetPack, OriginalContent.OpeningLeaderImageAssetPath);
            var openingLeaderFrame = openingLeader.Frames[0];
            if (!openingLeader.Frames.Select(frame => (frame.Width, frame.Height))
                    .SequenceEqual(OpeningTyrScene.LeaderFrameGeometry) ||
                openingLeaderFrame.Width != OpeningTyrScene.LeaderWidth ||
                openingLeaderFrame.Height != OpeningTyrScene.LeaderHeight ||
                openingLeaderFrame.Alpha.Count(value => value != 0) != 367)
                throw new InvalidDataException(
                    "The installed opening-leader image has unexpected geometry or alpha coverage.");
            var windowImagePath = Path.Combine(assetPack,
                OriginalContent.PartyWindowImageAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using (var windowImageStream = File.OpenRead(windowImagePath))
            {
                var windowImage = PackedIndexedImage.Read(windowImageStream,
                    OriginalContent.PartyWindowImageAssetPath);
                var frame = AssertSingleFrame(windowImage.Frames, "party-window image");
                if (frame.Width != 96 || frame.Height != 9)
                    throw new InvalidDataException("The installed party-window image is not 96x9.");
            }
            var fontPath = Path.Combine(assetPack,
                OriginalContent.InterfaceFontAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var fontStream = File.OpenRead(fontPath);
            var font = PackedIndexedBitmapFont.Read(fontStream, OriginalContent.InterfaceFontAssetPath);
            if (font.Glyphs.Count != IndexedBitmapFont.CharacterCount)
                throw new InvalidDataException("The installed interface font has an unexpected glyph count.");
            var visibleDialogueChoices = DialogueConditionAdapter.SelectVisibleChoices(
                dialogueProjection.InitialChoices,
                FirstTyrDialogueObservedState.Create(),
                DialoguePreviewText.MaximumResponses);
            var dialogueSession = DialogueSessionAdapter.Create(
                dialogueProjection, FirstTyrDialogueObservedState.Create(),
                DialoguePreviewText.MaximumResponses);
            var dialogueVariables = new Dictionary<GplDialogueVariable, string>
            {
                [globalString.Destination] = globalString.Text,
                [thirdMenuExit.Destination] = thirdMenuExit.Text
            };
            var dialogueText = DialoguePreviewText.Create(
                font, dialogueProjection, dialogueSession.Snapshot(),
                dialogueVariables);
            if (dialogueText.Speech.Width is <= 0 or > DialoguePreviewText.SpeechWidth ||
                !visibleDialogueChoices.SequenceEqual([0, 1, 2, 3, 7]) ||
                dialogueText.Responses.Count != 5 ||
                dialogueText.Responses.Any(response =>
                    response.Text.Width is <= 0 or > DialoguePreviewText.ResponseWidth))
                throw new InvalidDataException(
                    "The installed first Tyr dialogue text does not fit its preview layout.");
            var selectedDialogue = dialogueSession.Execute(DialogueCommand.Select(4));
            if (!selectedDialogue.Applied ||
                selectedDialogue.After.SelectedSourceIndex != 7 ||
                selectedDialogue.After.BranchTargetOffset !=
                    dialogueProjection.InitialChoices[7].TargetOffset)
                throw new InvalidDataException(
                    "The installed first Tyr dialogue branch identity was not retained.");
            var completedDialogue = dialogueSession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(dialogueCompletion)));
            if (!completedDialogue.Applied ||
                completedDialogue.After.Phase != DialoguePhase.Completed ||
                !completedDialogue.After.Variables.LocalFlags.TryGetValue(14, out var flag14) ||
                !flag14 ||
                !completedDialogue.After.Variables.LocalFlags.TryGetValue(4, out var flag4) ||
                !flag4)
                throw new InvalidDataException(
                    "The installed first Tyr dialogue completion did not apply its local flags.");
            var returningSession = DialogueSessionAdapter.Create(
                dialogueProjection, FirstTyrDialogueObservedState.Create(),
                DialoguePreviewText.MaximumResponses);
            returningSession.Execute(DialogueCommand.Select(0));
            var returnedDialogue = returningSession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(firstDialogueResponse)));
            if (!returnedDialogue.Applied ||
                returnedDialogue.After.Phase != DialoguePhase.AwaitingChoice ||
                returnedDialogue.After.Variables.LocalFlags[0] ||
                !returnedDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([0, 5, 6]))
                throw new InvalidDataException(
                    "The installed first Tyr response did not advance to the second menu.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                returnedDialogue.After, dialogueVariables, firstDialogueResponse.Output);
            returningSession.Execute(DialogueCommand.Select(0));
            var repeatedIdentity = returningSession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(secondDialogueResponse).ForSourceIndex(0)));
            if (!repeatedIdentity.Applied ||
                !repeatedIdentity.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([1, 3, 5, 6]))
                throw new InvalidDataException(
                    "The installed shared identity branch did not stay on the second menu.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                repeatedIdentity.After, dialogueVariables, secondDialogueResponse.Output);
            returningSession.Execute(DialogueCommand.Select(3));
            var secondMenuCompletion = returningSession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(dialogueCompletion).ForSourceIndex(6)));
            if (!secondMenuCompletion.Applied ||
                secondMenuCompletion.After.Phase != DialoguePhase.Completed ||
                !secondMenuCompletion.After.Variables.LocalFlags[14])
                throw new InvalidDataException(
                    "The installed first Tyr second-menu exit did not complete the dialogue.");
            var counterSession = DialogueSessionAdapter.Create(
                dialogueProjection, FirstTyrDialogueObservedState.Create(),
                DialoguePreviewText.MaximumResponses);
            counterSession.Execute(DialogueCommand.Select(2));
            var firstCounterDialogue = counterSession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(thirdDialogueResponse)));
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                firstCounterDialogue.After, dialogueVariables, thirdDialogueResponse.Output);
            counterSession.Execute(DialogueCommand.Select(2));
            var progressedDialogue = counterSession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(fourthDialogueResponse)));
            if (!progressedDialogue.Applied ||
                progressedDialogue.After.Variables.LocalNumbers[0] != 2 ||
                progressedDialogue.After.Variables.LocalFlags[2] ||
                progressedDialogue.After.Variables.LocalFlags[3] ||
                !progressedDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([0, 1, 4, 6, 7]))
                throw new InvalidDataException(
                    "The installed first Tyr counter responses did not reveal the next choice.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                progressedDialogue.After, dialogueVariables, fourthDialogueResponse.Output);
            counterSession.Execute(DialogueCommand.Select(2));
            var secondMenuDialogue = counterSession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(fifthDialogueResponse)));
            if (!secondMenuDialogue.Applied ||
                secondMenuDialogue.After.Variables.LocalNumbers[0] != 0 ||
                !secondMenuDialogue.After.Variables.LocalFlags[9] ||
                !secondMenuDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([0, 5, 6]))
                throw new InvalidDataException(
                    "The installed first Tyr follow-up did not enter the second menu: " +
                    $"number={secondMenuDialogue.After.Variables.LocalNumbers[0]}, " +
                    $"flag9={secondMenuDialogue.After.Variables.LocalFlags[9]}, " +
                    $"choices={string.Join(',', secondMenuDialogue.After.Choices.Select(
                        choice => choice.SourceIndex))}.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                secondMenuDialogue.After, dialogueVariables, fifthDialogueResponse.Output);
            var identitySession = DialogueSessionAdapter.Create(
                dialogueProjection, FirstTyrDialogueObservedState.Create(),
                DialoguePreviewText.MaximumResponses);
            identitySession.Execute(DialogueCommand.Select(1));
            var identifiedDialogue = identitySession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(secondDialogueResponse)));
            if (!identifiedDialogue.Applied ||
                identifiedDialogue.After.Variables.LocalFlags[1] ||
                !identifiedDialogue.After.Variables.LocalFlags[6] ||
                !identifiedDialogue.After.Variables.LocalFlags[7] ||
                !identifiedDialogue.After.Variables.GlobalFlags[357] ||
                !identifiedDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([1, 3, 5, 6]))
                throw new InvalidDataException(
                    "The installed first Tyr identity response did not apply its effects.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                identifiedDialogue.After, dialogueVariables, secondDialogueResponse.Output);
            identitySession.Execute(DialogueCommand.Select(0));
            var troubleDialogue = identitySession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(troubleDialogueResponse)));
            if (!troubleDialogue.Applied ||
                troubleDialogue.After.Variables.LocalFlags[6] ||
                !troubleDialogue.After.Variables.LocalFlags[10] ||
                !troubleDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([2, 3, 5, 6]))
                throw new InvalidDataException(
                    "The installed first Tyr trouble response did not apply its effects.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                troubleDialogue.After, dialogueVariables, troubleDialogueResponse.Output);
            identitySession.Execute(DialogueCommand.Select(0));
            var kingDialogue = identitySession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(kingDialogueResponse)));
            if (!kingDialogue.Applied ||
                !kingDialogue.After.Variables.LocalFlags[16] ||
                kingDialogue.After.Variables.LocalFlags[10] ||
                !kingDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([3, 5, 6]))
                throw new InvalidDataException(
                    "The installed first Tyr king response did not apply its effects.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                kingDialogue.After, dialogueVariables, kingDialogueResponse.Output);
            identitySession.Execute(DialogueCommand.Select(0));
            var caravanDialogue = identitySession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(caravanDialogueResponse)));
            if (!caravanDialogue.Applied ||
                !caravanDialogue.After.Variables.LocalFlags[11] ||
                caravanDialogue.After.Variables.LocalFlags[7] ||
                !caravanDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([4, 5, 6]))
                throw new InvalidDataException(
                    "The installed first Tyr caravan response did not apply its effects.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                caravanDialogue.After, dialogueVariables, caravanDialogueResponse.Output);
            identitySession.Execute(DialogueCommand.Select(0));
            var acarDialogue = identitySession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(acarDialogueResponse)));
            if (!acarDialogue.Applied ||
                acarDialogue.After.Variables.LocalFlags[11] ||
                !acarDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([5, 6]))
                throw new InvalidDataException(
                    "The installed first Tyr Acar response did not apply its effects.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                acarDialogue.After, dialogueVariables, acarDialogueResponse.Output);
            identitySession.Execute(DialogueCommand.Select(0));
            var cityDialogue = identitySession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(cityDialogueResponse)));
            if (!cityDialogue.Applied ||
                !cityDialogue.After.Variables.LocalFlags[12] ||
                !cityDialogue.After.Variables.LocalFlags[13] ||
                cityDialogue.After.Variables.LocalFlags[10] ||
                !cityDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([0, 1, 6]))
                throw new InvalidDataException(
                    "The installed first Tyr city response did not enter the third menu.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                cityDialogue.After, dialogueVariables, cityDialogueResponse.Output);
            identitySession.Execute(DialogueCommand.Select(0));
            var thirdMenuFirstDialogue = identitySession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(thirdMenuFirstDialogueResponse)));
            if (!thirdMenuFirstDialogue.Applied ||
                thirdMenuFirstDialogue.After.Variables.LocalFlags[12] ||
                !thirdMenuFirstDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([1, 6]))
                throw new InvalidDataException(
                    "The installed first Tyr third-menu response did not apply its effects.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                thirdMenuFirstDialogue.After, dialogueVariables,
                thirdMenuFirstDialogueResponse.Output);
            identitySession.Execute(DialogueCommand.Select(0));
            var thirdMenuSecondDialogue = identitySession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(thirdMenuSecondDialogueResponse)));
            if (!thirdMenuSecondDialogue.Applied ||
                !thirdMenuSecondDialogue.After.Variables.LocalFlags[15] ||
                thirdMenuSecondDialogue.After.Variables.LocalFlags[13] ||
                !thirdMenuSecondDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([2, 6]))
                throw new InvalidDataException(
                    "The installed first Tyr third-menu follow-up did not apply its effects.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                thirdMenuSecondDialogue.After, dialogueVariables,
                thirdMenuSecondDialogueResponse.Output);
            identitySession.Execute(DialogueCommand.Select(0));
            var thirdMenuThirdDialogue = identitySession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(thirdMenuThirdDialogueResponse)));
            if (!thirdMenuThirdDialogue.Applied ||
                thirdMenuThirdDialogue.After.Variables.LocalFlags[15] ||
                !thirdMenuThirdDialogue.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([6]))
                throw new InvalidDataException(
                    "The installed first Tyr third-menu final history response " +
                    "did not apply its effects.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                thirdMenuThirdDialogue.After, dialogueVariables,
                thirdMenuThirdDialogueResponse.Output);
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                thirdMenuThirdDialogue.After, dialogueVariables,
                thirdMenuFifthDialogueResponse.Output);
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                thirdMenuThirdDialogue.After, dialogueVariables,
                thirdMenuSixthDialogueResponse.Output);
            identitySession.Execute(DialogueCommand.Select(0));
            var thirdMenuCompleted = identitySession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(thirdMenuCompletion)));
            if (!thirdMenuCompleted.Applied ||
                thirdMenuCompleted.After.Phase != DialoguePhase.Completed ||
                thirdMenuCompleted.After.Variables.LocalFlags[8] ||
                !thirdMenuCompleted.After.Variables.LocalFlags[14])
                throw new InvalidDataException(
                    "The installed first Tyr third-menu exit did not complete.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                thirdMenuCompleted.After, dialogueVariables,
                thirdMenuCompletion.CompletedOutput);
            var alternateVariables = FirstTyrDialogueObservedState.Create() with
            {
                GlobalNumbers = new Dictionary<ushort, int> { [22] = 2, [84] = 2 }
            };
            var alternateCaravanSession = DialogueSessionAdapter.Create(
                dialogueProjection, alternateVariables,
                DialoguePreviewText.MaximumResponses);
            alternateCaravanSession.Execute(DialogueCommand.Select(1));
            alternateCaravanSession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(secondDialogueResponse)));
            alternateCaravanSession.Execute(DialogueCommand.Select(1));
            var alternateOutput = DialogueSessionAdapter.ResolveOutput(
                caravanDialogueResponse,
                alternateCaravanSession.Snapshot().Variables);
            var alternateCaravan = alternateCaravanSession.Execute(DialogueCommand.Apply(
                DialogueSessionAdapter.ToCore(caravanDialogueResponse)));
            if (!alternateCaravan.Applied ||
                alternateCaravan.After.Variables.LocalFlags[7] ||
                alternateCaravan.After.Variables.LocalFlags.ContainsKey(11) ||
                alternateCaravan.After.Variables.GlobalNumbers[84] != 3 ||
                !alternateCaravan.After.Choices.Select(choice => choice.SourceIndex)
                    .SequenceEqual([1, 5, 6]))
                throw new InvalidDataException(
                    "The installed alternate caravan response did not apply its effects.");
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                alternateCaravan.After, dialogueVariables, alternateOutput);
            var bitClearOutput = DialogueSessionAdapter.ResolveOutput(
                caravanDialogueResponse, alternateVariables with
                {
                    GlobalNumbers = new Dictionary<ushort, int> { [22] = 2, [84] = 0 }
                });
            _ = DialoguePreviewText.Create(font, dialogueProjection,
                alternateCaravan.After, dialogueVariables, bitClearOutput);
            var textPath = Path.Combine(assetPack,
                OriginalContent.TextCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var textStream = File.OpenRead(textPath);
            if (PackedTextCatalog.Read(textStream, OriginalContent.TextCatalogAssetPath).Resources.Count == 0)
                throw new InvalidDataException("The installed text catalog is empty.");
            var preferencesTextPath = Path.Combine(assetPack,
                OriginalContent.PreferencesTextCatalogAssetPath.Replace(
                    '/', Path.DirectorySeparatorChar));
            using var preferencesTextStream = File.OpenRead(preferencesTextPath);
            var preferencesText = PackedTextCatalog.Read(
                preferencesTextStream, OriginalContent.PreferencesTextCatalogAssetPath);
            if (preferencesText.Resources[
                    OriginalContent.PreferencesDifficultyTextResourceNumber].Count !=
                    ExecutablePreferencesTextReader.DifficultyLabelCount ||
                preferencesText.Resources[
                    OriginalContent.PreferencesAboutTextResourceNumber].Count !=
                    ExecutablePreferencesTextReader.AboutLineCount ||
                preferencesText.Resources[
                    OriginalContent.PreferencesDescriptionTextResourceNumber].Count !=
                    ExecutablePreferencesTextReader.DescriptionCount)
                throw new InvalidDataException(
                    "The installed Preferences text catalog is incomplete.");
            var characterPath = Path.Combine(assetPack,
                OriginalContent.CharacterCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var characterStream = File.OpenRead(characterPath);
            if (PackedCharacterCatalog.Read(
                    characterStream, OriginalContent.CharacterCatalogAssetPath).Characters.Count == 0)
                throw new InvalidDataException("The installed character catalog is empty.");
            var regionPath = Path.Combine(assetPack,
                OriginalContent.TyrRegionAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var regionStream = File.OpenRead(regionPath);
            var region = PackedRegion.Read(regionStream, OriginalContent.TyrRegionAssetPath);
            if (region.ResourceNumber != 50 || region.Name != "Tyr" ||
                region.Tiles.Count != 94 || region.Entities.Count != 867)
                throw new InvalidDataException("The installed Tyr region catalog is incomplete.");
            var geometryCounts = region.GeometryMap.GroupBy(value => value)
                .ToDictionary(group => group.Key, group => group.Count());
            var terrain = new RegionTerrainGrid(region);
            if (geometryCounts.Count != 4 ||
                geometryCounts.GetValueOrDefault((byte)0x00) != 8_131 ||
                geometryCounts.GetValueOrDefault((byte)0x40) != 2_044 ||
                geometryCounts.GetValueOrDefault((byte)0x80) != 38 ||
                geometryCounts.GetValueOrDefault((byte)0xc0) != 2_331 ||
                terrain.OpenCellCount != 8_169 ||
                terrain.IsTerrainOpen(60, 84) || !terrain.IsTerrainOpen(74, 94))
                throw new InvalidDataException(
                    "The installed Tyr geometry plane does not match the verified navigation contract.");
            var occupancy = new ExplorationOccupancySession(
                terrain.Width, terrain.Height,
                point => terrain.IsTerrainOpen(point.X, point.Y));
            var actorPlacement = occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
                1, new(OpeningTyrScene.LeaderAnchorCellX,
                    OpeningTyrScene.LeaderAnchorCellY), GridFootprint.SingleCell));
            if (!actorPlacement.Applied)
                throw new InvalidDataException(
                    "The opening leader cannot occupy its evidenced Tyr anchor.");
            var actor = new ExplorationActorController(terrain, occupancy, 1);
            var exploration = new ExplorationSnapshot(
                OpeningTyrScene.OriginX, OpeningTyrScene.OriginY,
                ExplorationCursorMode.Walk, PartyDisplayMode.LeaderOnly,
                ExplorationView.World);
            var targetCellX = OpeningTyrScene.LeaderAnchorCellX + 1;
            var targetLogicalX = targetCellX * GffRegion.TilePixelSize +
                GffRegion.TilePixelSize / 2 - OpeningTyrScene.OriginX;
            var targetLogicalY = OpeningTyrScene.LeaderAnchorCellY *
                GffRegion.TilePixelSize + GffRegion.TilePixelSize / 2 -
                OpeningTyrScene.OriginY;
            var plannedMovement = actor.PlanAt(
                exploration, targetLogicalX, targetLogicalY);
            var advancedMovement = actor.Advance(
                ExplorationActorController.DefaultStepInterval);
            if (plannedMovement is null || !plannedMovement.Applied ||
                advancedMovement.Count != 1 ||
                actor.Snapshot().Position != new GridPoint(
                    targetCellX, OpeningTyrScene.LeaderAnchorCellY))
                throw new InvalidDataException(
                    "The installed Tyr content does not support the opening actor movement contract.");
            var objectPath = Path.Combine(assetPack,
                OriginalContent.TyrObjectCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var objectStream = File.OpenRead(objectPath);
            var objects = PackedObjectFrameCatalog.Read(
                objectStream, OriginalContent.TyrObjectCatalogAssetPath);
            if (objects.Definitions.Count != 287 || objects.Images.Count != 246 ||
                objects.Images.Sum(image => image.Frames.Count) != 477)
                throw new InvalidDataException("The installed Tyr object-frame catalog is incomplete.");
            var observedMeleeTarget = objects.Definitions.SingleOrDefault(definition =>
                definition.ResourceNumber ==
                    OpeningTyrScene.ObservedMeleeTargetObjectResourceNumber);
            if (observedMeleeTarget?.ImageResourceNumber !=
                OpeningTyrScene.ObservedMeleeTargetImageResourceNumber)
                throw new InvalidDataException(
                    "The installed Tyr catalog lacks the observed opening melee target.");
            _ = new ExplorationEntityHitTester(region, objects);
            var objectNumbers = objects.Definitions.Select(item => item.ResourceNumber).ToHashSet();
            if (region.Entities.Any(entity => !objectNumbers.Contains(entity.ObjectResourceNumber)))
                throw new InvalidDataException("The Tyr region references a missing object definition.");
            var topLeft = RegionSceneRasterizer.Rasterize(region, objects, 0, 0,
                LogicalCanvasTransform.LogicalWidth, LogicalCanvasTransform.LogicalHeight);
            var bottomRight = RegionSceneRasterizer.Rasterize(region, objects,
                RegionSceneRasterizer.WorldWidth - LogicalCanvasTransform.LogicalWidth,
                RegionSceneRasterizer.WorldHeight - LogicalCanvasTransform.LogicalHeight,
                LogicalCanvasTransform.LogicalWidth, LogicalCanvasTransform.LogicalHeight);
            var opening = OpeningTyrScene.Rasterize(region, objects);
            if (topLeft.Pixels.Length != 64_000 || bottomRight.Pixels.Length != 64_000 ||
                opening.Pixels.Length != 64_000)
                throw new InvalidDataException("The Tyr scene compositor returned incomplete viewports.");
            var uiPath = Path.Combine(assetPack,
                OriginalContent.StartFlowUiCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var uiStream = File.OpenRead(uiPath);
            var ui = PackedUiCatalog.Read(uiStream, OriginalContent.StartFlowUiCatalogAssetPath);
            if (!ui.Windows.Select(window => window.ResourceNumber)
                .SequenceEqual(OriginalContent.StartFlowWindowResourceNumbers))
                throw new InvalidDataException("The installed start-flow UI catalog has unexpected windows.");
            var resolvedControlCount = OriginalContent.StartFlowWindowResourceNumbers.Sum(number =>
                UiWindowGraphResolver.Resolve(ui, number).Controls.Count);
            if (resolvedControlCount != 56)
                throw new InvalidDataException("The installed start-flow UI graph does not resolve all 56 controls.");
            if (StartMenuInput.Resolve(ui).Count != OriginalContent.StartMenuButtons.Count)
                throw new InvalidDataException("The installed start-window UI graph is incomplete.");
            if (AddExistingCharacterInput.Resolve(ui).Count != 17)
                throw new InvalidDataException("The installed ADD-list UI graph is incomplete.");
            _ = PartyOverviewInput.Resolve(ui);
            var gameMenuUiPath = Path.Combine(assetPack,
                OriginalContent.GameMenuUiCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var gameMenuUiStream = File.OpenRead(gameMenuUiPath);
            var gameMenuUi = PackedUiCatalog.Read(
                gameMenuUiStream, OriginalContent.GameMenuUiCatalogAssetPath);
            if (!gameMenuUi.Windows.Select(window => window.ResourceNumber)
                    .SequenceEqual(OriginalContent.GameMenuWindowResourceNumbers) ||
                GameMenuInput.Resolve(gameMenuUi).Count != OriginalContent.GameMenuButtons.Count ||
                PreferencesInput.Resolve(gameMenuUi).Count !=
                    OriginalContent.PreferencesButtons.Count)
                throw new InvalidDataException("The installed game-menu UI graph is incomplete.");
            var destinationUiPath = Path.Combine(assetPack,
                OriginalContent.ExplorationDestinationUiCatalogAssetPath.Replace(
                    '/', Path.DirectorySeparatorChar));
            using var destinationUiStream = File.OpenRead(destinationUiPath);
            var destinationUi = PackedUiCatalog.Read(destinationUiStream,
                OriginalContent.ExplorationDestinationUiCatalogAssetPath);
            if (!destinationUi.Windows.Select(window => window.ResourceNumber)
                    .SequenceEqual(OriginalContent.ExplorationDestinationWindowResourceNumbers) ||
                UiWindowGraphResolver.Resolve(destinationUi, 11500).Controls.Count != 86 ||
                UiWindowGraphResolver.Resolve(destinationUi, 13500).Controls.Count != 89 ||
                ExplorationDestinationInput.Resolve(
                    destinationUi, ExplorationView.ViewCharacter).Count != 5 ||
                ExplorationDestinationInput.Resolve(
                    destinationUi, ExplorationView.ViewInventory).Count != 5 ||
                ExplorationDestinationInput.Resolve(destinationUi,
                    ExplorationView.CastSpellsOrUsePsionics).Count != 5 ||
                ExplorationDestinationInput.Resolve(destinationUi,
                    ExplorationView.CurrentSpellEffects).Count != 5)
                throw new InvalidDataException(
                    "The installed exploration-destination UI graphs are incomplete.");
            var interactionUiPath = Path.Combine(assetPack,
                OriginalContent.InteractionUiCatalogAssetPath.Replace(
                    '/', Path.DirectorySeparatorChar));
            using var interactionUiStream = File.OpenRead(interactionUiPath);
            var interactionUi = PackedUiCatalog.Read(interactionUiStream,
                OriginalContent.InteractionUiCatalogAssetPath);
            if (!interactionUi.Windows.Select(window => window.ResourceNumber)
                    .SequenceEqual(OriginalContent.InteractionWindowResourceNumbers) ||
                InteractionOptionsInput.Resolve(interactionUi).Count != 4 ||
                DialogueInput.ResolveOverlay(interactionUi).Images.Count !=
                OriginalContent.DialogueLayers.Count + 4 +
                DialogueInput.ResolveResponses(interactionUi).Count)
                throw new InvalidDataException(
                    "The installed interaction UI graph is incomplete.");
            Console.WriteLine($"Verified runtime startup assets at {assetPack}.");
            return 0;
        }
    }
    using var game = new DarkSunWakeReduxGame(assetPack, platformSmoke);
    game.Run();
    return 0;
}
catch (Exception exception)
{
    StartupFailureReporter.Report(exception);
    return 1;
}

static string? Option(string[] values, string name)
{
    var index = Array.FindIndex(values, value => value.Equals(name, StringComparison.OrdinalIgnoreCase));
    return index >= 0 && index + 1 < values.Length ? values[index + 1] : null;
}

static IndexedImageFrame AssertSingleFrame(IReadOnlyList<IndexedImageFrame> frames, string name) =>
    frames.Count == 1 ? frames[0] : throw new InvalidDataException($"The installed {name} must have one frame.");

static PackedIndexedImage ReadImage(string assetPack, string assetPath)
{
    var path = Path.Combine(assetPack, assetPath.Replace('/', Path.DirectorySeparatorChar));
    using var stream = File.OpenRead(path);
    return PackedIndexedImage.Read(stream, assetPath);
}
