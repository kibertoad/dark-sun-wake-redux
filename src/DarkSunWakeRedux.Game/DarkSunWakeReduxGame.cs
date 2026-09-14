using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DarkSunWakeRedux.Core;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public sealed class DarkSunWakeReduxGame : Microsoft.Xna.Framework.Game
{
    private readonly string? _assetPack;
    private readonly bool _platformSmoke;
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private readonly List<(UiLayerAsset Layer, Texture2D Texture)> _startMenuLayers = [];
    private readonly List<(UiLayerAsset Layer, Texture2D Texture)> _partyOverviewLayers = [];
    private readonly List<(UiImagePlacement Placement, Texture2D Texture)> _addExistingImages = [];
    private readonly List<(StartMenuControl Control, Texture2D Texture)> _startMenuControls = [];
    private IReadOnlyList<AddExistingControl> _addExistingControls = [];
    private Texture2D? _gameMenuBase;
    private readonly List<(GameMenuControl Control, Texture2D Texture)> _gameMenuControls = [];
    private readonly List<(PreferencesControl Control, Texture2D Texture)> _preferencesControls = [];
    private Texture2D? _inventoryBase;
    private readonly Dictionary<ExplorationView, (UiLayerAsset Layer, Texture2D Texture)>
        _explorationDestinationTitles = [];
    private readonly Dictionary<ExplorationView,
        List<(ExplorationDestinationControl Control, Texture2D Texture)>>
        _explorationDestinationControls = [];
    private Texture2D? _explorationSceneTexture;
    private ExplorationViewportLayout? _renderedExplorationLayout;
    private Texture2D? _openingLeaderTexture;
    private IndexedImageFrame? _openingLeaderFrame;
    private readonly Dictionary<ExplorationCursorVisual, Texture2D> _cursorTextures = [];
    private DialogueOverlayLayout? _dialogueOverlay;
    private Texture2D? _dialoguePortrait;
    private Texture2D? _dialogueSpeechText;
    private Texture2D? _solidPixel;
    private readonly List<(DialogueOverlayImage Placement, Texture2D Texture)>
        _dialogueOverlayImages = [];
    private readonly List<(int X, int Y, Texture2D Texture)> _dialogueResponseText = [];
    private IReadOnlyList<DialogueResponseControl> _dialogueResponseControls = [];
    private DialogueSession? _dialogueSession;
    private readonly Dictionary<int, DialogueBranchPresentation> _dialogueBranchesByTarget = [];
    private PackedIndexedBitmapFont? _dialogueFont;
    private FirstTyrDialogueProjection? _dialogueProjection;
    private IReadOnlyList<Rgb24> _dialogueTextPalette = [];
    private IReadOnlyDictionary<GplDialogueVariable, string> _dialogueVariableText =
        new Dictionary<GplDialogueVariable, string>();
    private IReadOnlyList<GplDialogueOutput>? _dialogueSpeechOutput;
    private bool _dialoguePreviewVisible;
    private PackedRegion? _tyrRegion;
    private PackedObjectFrameCatalog? _tyrObjects;
    private ExplorationEntityHitTester? _tyrEntityHitTester;
    private RegionTerrainGrid? _tyrTerrain;
    private ExplorationActorController? _leaderController;
    private (GridPoint Start, GridPoint Destination)? _walkCursorQuery;
    private bool _walkCursorReachable;
    private readonly ExplorationSession _exploration = new(
        RegionSceneRasterizer.WorldWidth,
        RegionSceneRasterizer.WorldHeight,
        OpeningTyrScene.Width,
        OpeningTyrScene.Height,
        OpeningTyrScene.OriginX,
        OpeningTyrScene.OriginY);
    private readonly ExplorationActorPresentation _leaderPresentation = new(
        OpeningTyrScene.LeaderWidth,
        OpeningTyrScene.LeaderHeight,
        OpeningTyrScene.LeaderWorldX -
            OpeningTyrScene.LeaderAnchorCellX * GffRegion.TilePixelSize,
        OpeningTyrScene.LeaderWorldY -
            OpeningTyrScene.LeaderAnchorCellY * GffRegion.TilePixelSize);
    private readonly StartFlowSession _startFlow = new(0);
    private readonly ExplorationRightMouseInput _rightMouseInput = new();
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;

    public DarkSunWakeReduxGame(string? assetPack = null, bool platformSmoke = false)
    {
        _assetPack = assetPack;
        _platformSmoke = platformSmoke;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 960,
            PreferredBackBufferHeight = 600
        };
        IsMouseVisible = false;
        Window.Title = "Dark Sun: Wake of the Ravager Redux";
    }

    protected override void Initialize()
    {
        base.Initialize();
        if (_platformSmoke) Exit();
    }

    protected override void LoadContent()
    {
        if (_platformSmoke) return;
        if (string.IsNullOrWhiteSpace(_assetPack))
            throw new InvalidOperationException("The verified asset-pack path was not supplied.");
        foreach (var layer in OriginalContent.StartMenuLayers)
            _startMenuLayers.Add((layer, LoadTexture(layer)));
        foreach (var layer in OriginalContent.PartyOverviewLayers)
            _partyOverviewLayers.Add((layer, LoadTexture(layer)));

        Texture2D LoadTexture(UiLayerAsset layer)
        {
            var layerPath = Path.Combine(_assetPack,
                layer.Path.Replace('/', Path.DirectorySeparatorChar));
            using var layerStream = File.OpenRead(layerPath);
            var image = PackedIndexedImage.Read(layerStream, layer.Path);
            return CreateTexture(image, image.Frames.Single());
        }
        var addExistingTextures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
        foreach (var placement in OriginalContent.AddExistingCharacterPlacements)
        {
            if (!addExistingTextures.TryGetValue(placement.AssetPath, out var texture))
            {
                var path = Path.Combine(_assetPack,
                    placement.AssetPath.Replace('/', Path.DirectorySeparatorChar));
                using var stream = File.OpenRead(path);
                var image = PackedIndexedImage.Read(stream, placement.AssetPath);
                texture = CreateTexture(image, image.Frames[0]);
                addExistingTextures.Add(placement.AssetPath, texture);
            }
            _addExistingImages.Add((placement, texture));
        }
        var uiPath = Path.Combine(_assetPack,
            OriginalContent.StartFlowUiCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
        using var uiStream = File.OpenRead(uiPath);
        var uiCatalog = PackedUiCatalog.Read(uiStream, OriginalContent.StartFlowUiCatalogAssetPath);
        var controls = StartMenuInput.Resolve(uiCatalog);
        _addExistingControls = AddExistingCharacterInput.Resolve(uiCatalog);
        foreach (var control in controls)
        {
            var buttonPath = Path.Combine(_assetPack,
                control.AssetPath.Replace('/', Path.DirectorySeparatorChar));
            using var buttonStream = File.OpenRead(buttonPath);
            var image = PackedIndexedImage.Read(buttonStream, control.AssetPath);
            _startMenuControls.Add((control, CreateTexture(image, image.Frames[0])));
        }
        _gameMenuBase = LoadImageTexture(OriginalContent.GameMenuLayer.Path);
        var gameMenuUiPath = AssetPath(OriginalContent.GameMenuUiCatalogAssetPath);
        using var gameMenuUiStream = File.OpenRead(gameMenuUiPath);
        var gameMenuUi = PackedUiCatalog.Read(
            gameMenuUiStream, OriginalContent.GameMenuUiCatalogAssetPath);
        foreach (var control in GameMenuInput.Resolve(gameMenuUi))
            _gameMenuControls.Add((control, LoadImageTexture(control.AssetPath)));
        var cursorAssets = OriginalContent.ExplorationCursorAssets.ToDictionary(
            asset => asset.Name, StringComparer.Ordinal);
        foreach (var visual in Enum.GetValues<ExplorationCursorVisual>())
        {
            var asset = cursorAssets[ExplorationCursorFeedback.AssetName(visual)];
            _cursorTextures.Add(visual, LoadImageTexture(asset.Path));
        }
        var interactionUiPath = AssetPath(OriginalContent.InteractionUiCatalogAssetPath);
        using (var interactionUiStream = File.OpenRead(interactionUiPath))
        {
            var interactionUi = PackedUiCatalog.Read(
                interactionUiStream, OriginalContent.InteractionUiCatalogAssetPath);
            _dialogueOverlay = DialogueInput.ResolveOverlay(interactionUi);
            _dialogueResponseControls = DialogueInput.ResolveResponses(interactionUi);
        }
        PackedIndexedImage dialoguePortraitImage;
        using (var portraitStream = File.OpenRead(AssetPath(
                   OriginalContent.FirstTyrDialoguePortraitAssetPath)))
        {
            dialoguePortraitImage = PackedIndexedImage.Read(
                portraitStream, OriginalContent.FirstTyrDialoguePortraitAssetPath);
            _dialogueTextPalette = dialoguePortraitImage.Palette;
            _dialoguePortrait = CreateTexture(
                dialoguePortraitImage, dialoguePortraitImage.Frames[0]);
        }
        var dialogueTextures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
        foreach (var placement in _dialogueOverlay.Images)
        {
            if (!dialogueTextures.TryGetValue(placement.AssetPath, out var texture))
            {
                texture = LoadImageTexture(placement.AssetPath);
                dialogueTextures.Add(placement.AssetPath, texture);
            }
            _dialogueOverlayImages.Add((placement, texture));
        }
        _solidPixel = new Texture2D(GraphicsDevice, 1, 1);
        _solidPixel.SetData([Color.White]);
        using (var fontStream = File.OpenRead(AssetPath(
                   OriginalContent.InterfaceFontAssetPath)))
        using (var scriptStream = File.OpenRead(AssetPath(
                   OriginalContent.FirstTyrDialogueScriptAssetPath)))
        using (var globalsStream = File.OpenRead(AssetPath(
                   OriginalContent.DialogueGlobalStringsScriptAssetPath)))
        {
            _dialogueFont = PackedIndexedBitmapFont.Read(
                fontStream, OriginalContent.InterfaceFontAssetPath);
            var script = PackedGplScript.Read(
                scriptStream, OriginalContent.FirstTyrDialogueScriptAssetPath);
            var globals = PackedGplScript.Read(globalsStream,
                OriginalContent.DialogueGlobalStringsScriptAssetPath);
            var globalString = GplGlobalStringProjectionReader.Read(globals);
            var thirdMenuExit = GplGlobalStringProjectionReader.ReadThirdMenuExit(globals);
            _dialogueProjection = FirstTyrDialogueProjectionReader.Read(script);
            AddDialogueBranch(DialogueSessionAdapter.ToCore(
                FirstTyrDialogueCompletionProjectionReader.Read(script)), []);
            AddDialogueBranch(FirstTyrDialogueResponseProjectionReader.ReadFirst(script));
            AddDialogueBranch(FirstTyrDialogueResponseProjectionReader.ReadSecond(script));
            AddDialogueBranch(FirstTyrDialogueResponseProjectionReader.ReadThird(script));
            AddDialogueBranch(FirstTyrDialogueResponseProjectionReader.ReadFourth(script));
            AddDialogueBranch(FirstTyrDialogueResponseProjectionReader.ReadFifth(script));
            AddDialogueBranch(FirstTyrDialogueResponseProjectionReader.ReadTrouble(script));
            AddDialogueBranch(FirstTyrDialogueResponseProjectionReader.ReadKing(script));
            AddDialogueBranch(
                FirstTyrDialogueResponseProjectionReader.ReadOpeningCaravan(script));
            AddDialogueBranch(FirstTyrDialogueResponseProjectionReader.ReadAcar(script));
            AddDialogueBranch(
                FirstTyrDialogueResponseProjectionReader.ReadCityIntroduction(script));
            AddDialogueBranch(
                FirstTyrDialogueResponseProjectionReader.ReadThirdMenuFirst(script));
            AddDialogueBranch(
                FirstTyrDialogueResponseProjectionReader.ReadThirdMenuSecond(script));
            AddDialogueBranch(
                FirstTyrDialogueResponseProjectionReader.ReadThirdMenuThird(script));
            _dialogueSession = DialogueSessionAdapter.Create(
                _dialogueProjection, FirstTyrDialogueObservedState.Create(),
                DialoguePreviewText.MaximumResponses);
            _dialogueVariableText = new Dictionary<GplDialogueVariable, string>
            {
                [globalString.Destination] = globalString.Text,
                [thirdMenuExit.Destination] = thirdMenuExit.Text
            };
            RebuildDialogueText();
        }
        var preferencesTextures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
        foreach (var control in PreferencesInput.Resolve(gameMenuUi))
        {
            if (!preferencesTextures.TryGetValue(control.AssetPath, out var texture))
            {
                texture = LoadImageTexture(control.AssetPath);
                preferencesTextures.Add(control.AssetPath, texture);
            }
            _preferencesControls.Add((control, texture));
        }
        _inventoryBase = LoadImageTexture(OriginalContent.InventoryLayer.Path);
        foreach (var layer in OriginalContent.ExplorationDestinationTitleLayers)
        {
            var view = layer.Name switch
            {
                "use-title" => ExplorationView.CastSpellsOrUsePsionics,
                "effects-title" => ExplorationView.CurrentSpellEffects,
                _ => throw new InvalidDataException(
                    $"Unknown exploration destination title '{layer.Name}'.")
            };
            _explorationDestinationTitles.Add(view, (layer, LoadTexture(layer)));
        }
        var destinationUiPath = AssetPath(
            OriginalContent.ExplorationDestinationUiCatalogAssetPath);
        using var destinationUiStream = File.OpenRead(destinationUiPath);
        var destinationUi = PackedUiCatalog.Read(destinationUiStream,
            OriginalContent.ExplorationDestinationUiCatalogAssetPath);
        var destinationTextures = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
        foreach (var view in new[]
                 {
                     ExplorationView.ViewCharacter,
                     ExplorationView.ViewInventory,
                     ExplorationView.CastSpellsOrUsePsionics,
                     ExplorationView.CurrentSpellEffects
                 })
        {
            var pageControls = new List<(ExplorationDestinationControl, Texture2D)>();
            foreach (var control in ExplorationDestinationInput.Resolve(destinationUi, view))
            {
                if (!destinationTextures.TryGetValue(control.AssetPath, out var texture))
                {
                    texture = LoadImageTexture(control.AssetPath);
                    destinationTextures.Add(control.AssetPath, texture);
                }
                pageControls.Add((control, texture));
            }
            _explorationDestinationControls.Add(view, pageControls);
        }
        var regionPath = AssetPath(OriginalContent.TyrRegionAssetPath);
        using var regionStream = File.OpenRead(regionPath);
        _tyrRegion = PackedRegion.Read(regionStream, OriginalContent.TyrRegionAssetPath);
        _tyrTerrain = new(_tyrRegion);
        var occupancy = new ExplorationOccupancySession(
            _tyrTerrain.Width, _tyrTerrain.Height,
            point => _tyrTerrain.IsTerrainOpen(point.X, point.Y));
        var leaderPlacement = occupancy.Execute(ExplorationOccupancyCommand.PlaceAt(
            1, new(OpeningTyrScene.LeaderAnchorCellX,
                OpeningTyrScene.LeaderAnchorCellY), GridFootprint.SingleCell));
        if (!leaderPlacement.Applied)
            throw new InvalidDataException(
                "The evidenced opening leader anchor is not passable in Tyr.");
        _leaderController = new(_tyrTerrain, occupancy, occupantId: 1);
        var objectPath = AssetPath(OriginalContent.TyrObjectCatalogAssetPath);
        using var objectStream = File.OpenRead(objectPath);
        _tyrObjects = PackedObjectFrameCatalog.Read(
            objectStream, OriginalContent.TyrObjectCatalogAssetPath);
        _tyrEntityHitTester = new(_tyrRegion, _tyrObjects);
        RefreshExplorationScene(_exploration.Snapshot());
        using (var leaderStream = File.OpenRead(AssetPath(
                   OriginalContent.OpeningLeaderImageAssetPath)))
        {
            var leaderImage = PackedIndexedImage.Read(
                leaderStream, OriginalContent.OpeningLeaderImageAssetPath);
            _openingLeaderFrame = leaderImage.Frames[0];
            _openingLeaderTexture = CreateTexture(leaderImage, _openingLeaderFrame);
        }
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        string AssetPath(string relativePath) => Path.Combine(_assetPack,
            relativePath.Replace('/', Path.DirectorySeparatorChar));

        Texture2D LoadImageTexture(string relativePath)
        {
            using var stream = File.OpenRead(AssetPath(relativePath));
            var image = PackedIndexedImage.Read(stream, relativePath);
            return CreateTexture(image, image.Frames[0]);
        }
    }

    private Texture2D CreateTexture(PackedIndexedImage image, IndexedImageFrame frame)
        => CreateTexture(image.Palette, frame.Pixels, frame.Alpha, frame.Width, frame.Height);

    private Texture2D CreateTexture(IReadOnlyList<Rgb24> palette, IndexedRegionViewport viewport)
        => CreateTexture(palette, viewport.Pixels, viewport.Alpha, viewport.Width, viewport.Height);

    private Texture2D CreateTexture(
        IReadOnlyList<Rgb24> palette,
        byte[] pixels,
        byte[] alpha,
        int width,
        int height)
    {
        var colors = CreateColors(palette, pixels, alpha);
        var texture = new Texture2D(GraphicsDevice, width, height);
        texture.SetData(colors);
        return texture;
    }

    private static Color[] CreateColors(
        IReadOnlyList<Rgb24> palette,
        byte[] pixels,
        byte[] alpha)
    {
        var colors = new Color[pixels.Length];
        for (var index = 0; index < colors.Length; index++)
        {
            var color = palette[pixels[index]];
            colors[index] = new Color(color.Red, color.Green, color.Blue, alpha[index]);
        }
        return colors;
    }

    protected override void Update(GameTime gameTime)
    {
        if (!_platformSmoke)
        {
            var mouse = Mouse.GetState();
            var screen = _startFlow.Snapshot().Screen;
            var explorationOverlayConsumedLeftClick = false;
            if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
            {
                var transform = new LogicalCanvasTransform(
                    GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                if (transform.TryToLogical(mouse.X, mouse.Y, out var x, out var y))
                {
                    if (screen == StartFlowScreen.Gameplay &&
                        _exploration.Snapshot().View == ExplorationView.World &&
                        _dialoguePreviewVisible &&
                        DialogueInput.HitTest(_dialogueResponseControls.Take(
                            _dialogueSession!.Snapshot().Choices.Count).ToArray(), x, y)
                            is { } response)
                    {
                        var choice = _dialogueSession!.Snapshot().Choices[response.Index];
                        if (_dialogueBranchesByTarget.TryGetValue(
                                choice.BranchTargetOffset, out var presentation))
                        {
                            var selection = _dialogueSession.Execute(
                                DialogueCommand.Select(response.Index));
                            var branch = presentation.Branch.ForSourceIndex(
                                selection.After.SelectedSourceIndex!.Value);
                            var applied = _dialogueSession.Execute(
                                DialogueCommand.Apply(branch));
                            if (applied.After.Phase == DialoguePhase.Completed)
                                _dialoguePreviewVisible = false;
                            else
                            {
                                _dialogueSpeechOutput = presentation.Output;
                                RebuildDialogueText();
                            }
                        }
                        explorationOverlayConsumedLeftClick = true;
                    }
                    else if (screen == StartFlowScreen.StartWindow &&
                        StartMenuInput.HitTest(
                            _startMenuControls.Select(item => item.Control).ToArray(), x, y) is { } choice)
                        _startFlow.Execute(StartFlowCommand.Choose(choice));
                    else if (screen == StartFlowScreen.AddExistingCharacter &&
                        AddExistingCharacterInput.HitTest(_addExistingControls, x, y) is { } control &&
                        AddExistingCharacterInput.CommandFor(control) is { } command)
                        _startFlow.Execute(command);
                    else if (screen == StartFlowScreen.Gameplay &&
                        _exploration.Snapshot().View == ExplorationView.GameMenu &&
                        GameMenuInput.HitTest(
                            _gameMenuControls.Select(item => item.Control).ToArray(), x, y) is { } menuControl &&
                        CommandForGameMenuControl(menuControl) is { } explorationCommand)
                    {
                        ExecuteExplorationCommand(explorationCommand);
                        explorationOverlayConsumedLeftClick = true;
                    }
                    else if (screen == StartFlowScreen.Gameplay &&
                        _exploration.Snapshot().View == ExplorationView.Preferences &&
                        PreferencesInput.HitTest(
                            _preferencesControls.Select(item => item.Control).ToArray(), x, y) is { } preferencesControl &&
                        PreferencesInput.CommandFor(preferencesControl) is { } preferencesCommand)
                    {
                        ExecuteExplorationCommand(preferencesCommand);
                        explorationOverlayConsumedLeftClick = true;
                    }
                    else if (screen == StartFlowScreen.Gameplay &&
                        _explorationDestinationControls.TryGetValue(
                            _exploration.Snapshot().View, out var pageControls) &&
                        ExplorationDestinationInput.HitTest(
                            pageControls.Select(item => item.Control).ToArray(), x, y) is { } destinationControl)
                        ExecuteExplorationCommand(
                            ExplorationDestinationInput.CommandFor(destinationControl));
                }
            }
            var keyboard = Keyboard.GetState();
            if (FullscreenInput.ShouldToggle(keyboard, _previousKeyboard))
                ToggleFullscreen();
            if (screen == StartFlowScreen.Gameplay &&
                _exploration.Snapshot().View == ExplorationView.World &&
                DialoguePreviewInput.ShouldToggle(keyboard, _previousKeyboard))
                _dialoguePreviewVisible = _dialogueSession?.Snapshot().Phase !=
                    DialoguePhase.Completed && !_dialoguePreviewVisible;
            else if (screen != StartFlowScreen.Gameplay ||
                _exploration.Snapshot().View != ExplorationView.World)
                _dialoguePreviewVisible = false;
            if (screen != StartFlowScreen.Gameplay &&
                keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
                _startFlow.Execute(StartFlowCommand.Cancel());
            if (screen == StartFlowScreen.Gameplay)
                UpdateExploration(mouse, keyboard, gameTime.ElapsedGameTime,
                    explorationOverlayConsumedLeftClick);
            _previousMouse = mouse;
            _previousKeyboard = keyboard;
            if (_startFlow.Snapshot().Screen == StartFlowScreen.ExitRequested) Exit();
        }
        base.Update(gameTime);
    }

    private void ToggleFullscreen()
    {
        var display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        var target = FullscreenInput.ResolveTarget(
            _graphics.IsFullScreen, display.Width, display.Height);
        _graphics.HardwareModeSwitch = false;
        _graphics.PreferredBackBufferWidth = target.Width;
        _graphics.PreferredBackBufferHeight = target.Height;
        _graphics.IsFullScreen = target.IsFullScreen;
        _graphics.ApplyChanges();
    }

    private void UpdateExploration(
        MouseState mouse,
        KeyboardState keyboard,
        TimeSpan elapsed,
        bool suppressWalkClick)
    {
        if (_tyrTerrain is null || _leaderController is null)
            throw new InvalidOperationException("The Tyr movement controller is not loaded.");
        if (ExplorationHotkeys.Resolve(keyboard, _previousKeyboard) is { } hotkey)
            ExecuteExplorationCommand(hotkey);
        var explorationSnapshot = _exploration.Snapshot();
        var explorationLayout = ResolveExplorationLayout(explorationSnapshot);
        var hasLogicalPointer = explorationLayout.TryToLogical(
            mouse.X, mouse.Y, out var pointerX, out var pointerY);
        var rightPressed = mouse.RightButton == ButtonState.Pressed;
        if (_rightMouseInput.Update(
                explorationSnapshot.View == ExplorationView.World,
                rightPressed,
                hasLogicalPointer ? new GridPoint(pointerX, pointerY) : null) is { } rightCommand)
            ExecuteExplorationCommand(rightCommand);
        if (_exploration.Snapshot().View == ExplorationView.World &&
            !suppressWalkClick &&
            mouse.LeftButton == ButtonState.Pressed &&
            _previousMouse.LeftButton == ButtonState.Released &&
            explorationLayout.TryToLogical(mouse.X, mouse.Y, out var walkX, out var walkY))
            _leaderController.PlanAt(explorationSnapshot with
                {
                    CameraX = explorationLayout.CameraX,
                    CameraY = explorationLayout.CameraY
                }, walkX, walkY);
        if (_exploration.Snapshot().View == ExplorationView.World && !rightPressed &&
            explorationLayout.TryToLogical(mouse.X, mouse.Y, out var x, out var y) &&
            ExplorationInput.ScrollAtEdge(
                x, y, explorationLayout.LogicalWidth, explorationLayout.LogicalHeight) is { } scroll)
            ExecuteExplorationCommand(scroll);
        if (_exploration.Snapshot().View == ExplorationView.World)
            _leaderController.Advance(elapsed);
        var after = _exploration.Snapshot();
        if (after.View == ExplorationView.ExitRequested)
            Exit();
    }

    private ExplorationTransition ExecuteExplorationCommand(ExplorationCommand command)
    {
        var transition = _exploration.Execute(command);
        if ((transition.Before.CameraX, transition.Before.CameraY, transition.Before.View) !=
            (transition.After.CameraX, transition.After.CameraY, transition.After.View))
            RefreshExplorationScene(transition.After);
        return transition;
    }

    private void RefreshExplorationScene(ExplorationSnapshot snapshot)
    {
        if (_tyrRegion is null || _tyrObjects is null)
            throw new InvalidOperationException("The Tyr exploration scene is not loaded.");
        var layout = ResolveExplorationLayout(snapshot);
        var viewport = RegionSceneRasterizer.Rasterize(_tyrRegion, _tyrObjects,
            layout.CameraX, layout.CameraY, layout.LogicalWidth, layout.LogicalHeight);
        var colors = CreateColors(_tyrRegion.Palette, viewport.Pixels, viewport.Alpha);
        if (_explorationSceneTexture is null ||
            _explorationSceneTexture.Width != layout.LogicalWidth ||
            _explorationSceneTexture.Height != layout.LogicalHeight)
        {
            _explorationSceneTexture?.Dispose();
            _explorationSceneTexture = new Texture2D(
                GraphicsDevice, layout.LogicalWidth, layout.LogicalHeight);
        }
        _explorationSceneTexture.SetData(colors);
        _renderedExplorationLayout = layout;
    }

    private ExplorationViewportLayout ResolveExplorationLayout(ExplorationSnapshot snapshot) =>
        ExplorationViewportLayout.Resolve(
            GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height,
            RegionSceneRasterizer.WorldWidth,
            RegionSceneRasterizer.WorldHeight,
            snapshot.CameraX,
            snapshot.CameraY,
            snapshot.View == ExplorationView.World);

    private (int X, int Y) LeaderWorldCenter()
    {
        if (_leaderController is null)
            throw new InvalidOperationException("The leader movement controller is not loaded.");
        return _leaderPresentation.WorldCenterAtMovement(
            _leaderController.VisualSnapshot(), GffRegion.TilePixelSize);
    }

    private ExplorationCommand? CommandForGameMenuControl(GameMenuControl control)
    {
        if (control.Action != GameMenuAction.CenterOnLeader)
            return GameMenuInput.CommandFor(control);
        var center = LeaderWorldCenter();
        return GameMenuInput.CommandFor(control, center.X, center.Y);
    }

    private ExplorationCursorVisual CursorVisualAt(
        ExplorationViewportLayout layout,
        int logicalX,
        int logicalY)
    {
        var snapshot = _exploration.Snapshot();
        if (snapshot.View != ExplorationView.World || _tyrTerrain is null ||
            _leaderController is null || _tyrEntityHitTester is null)
            return ExplorationCursorVisual.Walk;
        var worldX = layout.CameraX + logicalX;
        var worldY = layout.CameraY + logicalY;
        if (snapshot.CursorMode == ExplorationCursorMode.Walk)
        {
            var start = _leaderController.Snapshot().Position;
            var cell = _tyrTerrain.CellAtWorldPixel(worldX, worldY);
            var destination = new GridPoint(cell.X, cell.Y);
            var query = (start, destination);
            if (_walkCursorQuery != query)
            {
                _walkCursorReachable = ExplorationTerrainRoutePlanner.PlanAt(
                    _tyrTerrain, snapshot with
                    {
                        CameraX = layout.CameraX,
                        CameraY = layout.CameraY
                    }, start, logicalX, logicalY)?.Found == true;
                _walkCursorQuery = query;
            }
            return ExplorationCursorFeedback.Resolve(
                snapshot, _walkCursorReachable, false, false);
        }
        var entityHit = _tyrEntityHitTester.HitTest(worldX, worldY);
        var leaderHit = IsLeaderPixelAt(layout, logicalX, logicalY);
        var meleeTarget = entityHit?.Entity.ObjectResourceNumber ==
            OpeningTyrScene.ObservedMeleeTargetObjectResourceNumber;
        return ExplorationCursorFeedback.Resolve(snapshot, false, meleeTarget,
            leaderHit || entityHit is not null);
    }

    private bool IsLeaderPixelAt(
        ExplorationViewportLayout layout,
        int logicalX,
        int logicalY)
    {
        if (_leaderController is null || _openingLeaderFrame is null)
            return false;
        var bounds = _leaderPresentation.AtMovement(
            _leaderController.VisualSnapshot(), layout.CameraX, layout.CameraY,
            GffRegion.TilePixelSize);
        var sourceX = logicalX - bounds.X;
        var sourceY = logicalY - bounds.Y;
        return sourceX >= 0 && sourceY >= 0 &&
            sourceX < _openingLeaderFrame.Width && sourceY < _openingLeaderFrame.Height &&
            _openingLeaderFrame.Alpha[sourceY * _openingLeaderFrame.Width + sourceX] != 0;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        if (_spriteBatch is not null)
        {
            var screen = _startFlow.Snapshot().Screen;
            var explorationView = _exploration.Snapshot().View;
            if (screen == StartFlowScreen.Gameplay &&
                explorationView is ExplorationView.World or ExplorationView.GameMenu or
                    ExplorationView.Preferences)
            {
                var expectedLayout = ResolveExplorationLayout(_exploration.Snapshot());
                if (_renderedExplorationLayout != expectedLayout)
                    RefreshExplorationScene(_exploration.Snapshot());
            }
            var images = screen switch
            {
                StartFlowScreen.StartWindow => _startMenuLayers.Select(item =>
                    (item.Layer.X, item.Layer.Y, item.Texture)),
                StartFlowScreen.PartyOverview => _partyOverviewLayers.Select(item =>
                    (item.Layer.X, item.Layer.Y, item.Texture)),
                StartFlowScreen.AddExistingCharacter => _addExistingImages.Select(item =>
                    (item.Placement.X, item.Placement.Y, item.Texture)),
                StartFlowScreen.Gameplay when
                    explorationView == ExplorationView.ViewCharacter =>
                    _partyOverviewLayers.Select(item =>
                        (item.Layer.X, item.Layer.Y, item.Texture)),
                StartFlowScreen.Gameplay when
                    explorationView == ExplorationView.ViewInventory &&
                    _inventoryBase is not null =>
                    [(OriginalContent.InventoryLayer.X,
                        OriginalContent.InventoryLayer.Y, _inventoryBase)],
                StartFlowScreen.Gameplay when
                    _explorationDestinationTitles.TryGetValue(
                        explorationView, out var destinationTitle) =>
                    _partyOverviewLayers.Take(1).Select(item =>
                            (item.Layer.X, item.Layer.Y, item.Texture))
                        .Append((destinationTitle.Layer.X, destinationTitle.Layer.Y,
                            destinationTitle.Texture)),
                StartFlowScreen.Gameplay when _explorationSceneTexture is not null =>
                    [(0, 0, _explorationSceneTexture)],
                _ => []
            };
            var placedImages = images.ToArray();
            if (screen == StartFlowScreen.Gameplay &&
                _exploration.Snapshot().View is ExplorationView.World or ExplorationView.GameMenu or
                    ExplorationView.Preferences &&
                _openingLeaderTexture is not null &&
                _leaderController is not null)
            {
                var camera = _exploration.Snapshot();
                var layout = ResolveExplorationLayout(camera);
                var bounds = _leaderPresentation.AtMovement(
                    _leaderController.VisualSnapshot(),
                    layout.CameraX, layout.CameraY, GffRegion.TilePixelSize);
                if (bounds.Intersects(layout.LogicalWidth, layout.LogicalHeight))
                    placedImages = placedImages.Append(
                        (bounds.X, bounds.Y, _openingLeaderTexture)).ToArray();
            }
            if (screen == StartFlowScreen.Gameplay &&
                _exploration.Snapshot().View == ExplorationView.GameMenu &&
                _gameMenuBase is not null)
            {
                placedImages = placedImages
                    .Append((OriginalContent.GameMenuLayer.X,
                        OriginalContent.GameMenuLayer.Y, _gameMenuBase))
                    .Concat(_gameMenuControls.Select(item =>
                        (item.Control.X, item.Control.Y, item.Texture)))
                    .ToArray();
            }
            if (screen == StartFlowScreen.Gameplay &&
                _exploration.Snapshot().View == ExplorationView.Preferences &&
                _gameMenuBase is not null)
            {
                placedImages = placedImages
                    .Append((OriginalContent.GameMenuLayer.X,
                        OriginalContent.GameMenuLayer.Y, _gameMenuBase))
                    .Concat(_preferencesControls.Select(item =>
                        (item.Control.X, item.Control.Y, item.Texture)))
                    .ToArray();
            }
            if (screen == StartFlowScreen.Gameplay &&
                _explorationDestinationControls.TryGetValue(
                    _exploration.Snapshot().View, out var destinationControls))
                placedImages = placedImages.Concat(destinationControls.Select(item =>
                    (item.Control.X, item.Control.Y, item.Texture))).ToArray();
            if (placedImages.Length > 0)
            {
                var transform = new LogicalCanvasTransform(
                    GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                var worldExpanded = screen == StartFlowScreen.Gameplay &&
                    explorationView == ExplorationView.World;
                var worldLayout = worldExpanded
                    ? ResolveExplorationLayout(_exploration.Snapshot())
                    : (ExplorationViewportLayout?)null;
                var destination = worldExpanded
                    ? new Rectangle(0, 0, GraphicsDevice.Viewport.Width,
                        GraphicsDevice.Viewport.Height)
                    : new Rectangle(transform.X, transform.Y, transform.Width, transform.Height);
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                foreach (var (x, y, texture) in placedImages)
                    _spriteBatch.Draw(texture, worldLayout is { } expanded
                        ? ScaledRectangle(expanded, x, y, texture.Width, texture.Height)
                        : ScaledRectangle(destination, transform,
                            x, y, texture.Width, texture.Height), Color.White);
                if (screen == StartFlowScreen.StartWindow)
                    foreach (var (control, texture) in _startMenuControls)
                        _spriteBatch.Draw(texture, ScaledRectangle(destination, transform,
                            control.X, control.Y, texture.Width, texture.Height), Color.White);
                if (worldExpanded && _dialoguePreviewVisible &&
                    _dialogueOverlay is { } dialogue &&
                    _dialoguePortrait is not null && _solidPixel is not null)
                {
                    var fixedCanvas = new Rectangle(
                        transform.X, transform.Y, transform.Width, transform.Height);
                    DrawDialogueWindow(dialogue.SpeechWindow);
                    DrawDialogueWindow(dialogue.ResponseWindow);
                    _spriteBatch.Draw(_dialoguePortrait,
                        ScaledRectangle(fixedCanvas, transform,
                            dialogue.PortraitX, dialogue.PortraitY,
                            _dialoguePortrait.Width, _dialoguePortrait.Height), Color.White);
                    foreach (var (placement, texture) in _dialogueOverlayImages)
                        _spriteBatch.Draw(texture,
                            ScaledRectangle(fixedCanvas, transform,
                                placement.X, placement.Y, texture.Width, texture.Height),
                            Color.White);
                    if (_dialogueSpeechText is not null)
                        _spriteBatch.Draw(_dialogueSpeechText,
                            ScaledRectangle(fixedCanvas, transform,
                                77, 6, _dialogueSpeechText.Width,
                                _dialogueSpeechText.Height), Color.White);
                    foreach (var (x, y, texture) in _dialogueResponseText)
                        _spriteBatch.Draw(texture,
                            ScaledRectangle(fixedCanvas, transform,
                                x, y, texture.Width, texture.Height), Color.White);

                    void DrawDialogueWindow(DialogueOverlayRectangle window) =>
                        _spriteBatch.Draw(_solidPixel,
                            ScaledRectangle(fixedCanvas, transform,
                                window.X, window.Y, window.Width, window.Height), Color.Black);
                }
                var mouse = Mouse.GetState();
                var hasCursor = worldLayout is { } cursorLayout
                    ? cursorLayout.TryToLogical(mouse.X, mouse.Y, out var cursorX, out var cursorY)
                    : transform.TryToLogical(mouse.X, mouse.Y, out cursorX, out cursorY);
                if (hasCursor)
                {
                    var visual = screen == StartFlowScreen.Gameplay
                        ? CursorVisualAt(worldLayout ?? ResolveExplorationLayout(
                            _exploration.Snapshot()), cursorX, cursorY)
                        : ExplorationCursorVisual.Walk;
                    var cursor = _cursorTextures[visual];
                    _spriteBatch.Draw(cursor, worldLayout is { } expanded
                        ? ScaledRectangle(expanded,
                            cursorX, cursorY, cursor.Width, cursor.Height)
                        : ScaledRectangle(destination, transform,
                            cursorX, cursorY, cursor.Width, cursor.Height), Color.White);
                }
                _spriteBatch.End();
            }
        }
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        foreach (var (_, texture) in _startMenuLayers) texture.Dispose();
        foreach (var (_, texture) in _partyOverviewLayers) texture.Dispose();
        foreach (var texture in _addExistingImages.Select(item => item.Texture).Distinct())
            texture.Dispose();
        foreach (var (_, texture) in _startMenuControls) texture.Dispose();
        _gameMenuBase?.Dispose();
        foreach (var (_, texture) in _gameMenuControls) texture.Dispose();
        foreach (var texture in _preferencesControls.Select(item => item.Texture).Distinct())
            texture.Dispose();
        _inventoryBase?.Dispose();
        foreach (var (_, texture) in _explorationDestinationTitles.Values)
            texture.Dispose();
        foreach (var texture in _explorationDestinationControls.Values
                     .SelectMany(controls => controls.Select(item => item.Texture)).Distinct())
            texture.Dispose();
        _explorationSceneTexture?.Dispose();
        _openingLeaderTexture?.Dispose();
        foreach (var texture in _cursorTextures.Values) texture.Dispose();
        _dialoguePortrait?.Dispose();
        _dialogueSpeechText?.Dispose();
        _solidPixel?.Dispose();
        foreach (var texture in _dialogueOverlayImages.Select(item => item.Texture).Distinct())
            texture.Dispose();
        foreach (var (_, _, texture) in _dialogueResponseText) texture.Dispose();
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }

    private static Rectangle ScaledRectangle(
        Rectangle canvas, LogicalCanvasTransform transform, int x, int y, int width, int height) =>
        new(canvas.X + (int)MathF.Floor(x * transform.Scale),
            canvas.Y + (int)MathF.Floor(y * transform.Scale),
            Math.Max(1, (int)MathF.Floor(width * transform.Scale)),
            Math.Max(1, (int)MathF.Floor(height * transform.Scale)));

    private static Rectangle ScaledRectangle(
        ExplorationViewportLayout layout, int x, int y, int width, int height)
    {
        var left = (int)((long)x * layout.ViewportWidth / layout.LogicalWidth);
        var top = (int)((long)y * layout.ViewportHeight / layout.LogicalHeight);
        var right = (int)Math.Ceiling(
            (x + width) * (double)layout.ViewportWidth / layout.LogicalWidth);
        var bottom = (int)Math.Ceiling(
            (y + height) * (double)layout.ViewportHeight / layout.LogicalHeight);
        return new(left, top, Math.Max(1, right - left), Math.Max(1, bottom - top));
    }

    private Texture2D CreateTextTexture(
        IReadOnlyList<Rgb24> palette,
        byte[] pixels,
        int width,
        int height)
    {
        var alpha = pixels.Select(pixel => pixel == 0 ? (byte)0 : (byte)255).ToArray();
        return CreateTexture(palette, pixels, alpha, width, height);
    }

    private void AddDialogueBranch(FirstTyrDialogueResponseProjection projection) =>
        AddDialogueBranch(DialogueSessionAdapter.ToCore(projection), projection.Output);

    private void AddDialogueBranch(
        DialogueBranchResult branch,
        IReadOnlyList<GplDialogueOutput> output)
    {
        if (!_dialogueBranchesByTarget.TryAdd(branch.BranchTargetOffset,
                new(branch, output)))
            throw new InvalidDataException(
                $"Dialogue branch target {branch.BranchTargetOffset} is duplicated.");
    }

    private void RebuildDialogueText()
    {
        if (_dialogueFont is null || _dialogueProjection is null ||
            _dialogueSession is null || _dialogueOverlay is null)
            throw new InvalidOperationException("Dialogue preview content is not loaded.");
        var previewText = DialoguePreviewText.Create(
            _dialogueFont, _dialogueProjection, _dialogueSession.Snapshot(),
            _dialogueVariableText, _dialogueSpeechOutput);
        _dialogueSpeechText?.Dispose();
        _dialogueSpeechText = CreateTextTexture(
            _dialogueTextPalette, previewText.Speech.Pixels,
            previewText.Speech.Width, previewText.Speech.Height);
        foreach (var (_, _, texture) in _dialogueResponseText) texture.Dispose();
        _dialogueResponseText.Clear();
        var responsePlacements = _dialogueOverlay.Images
            .Where(image => image.ResourceNumber is >= 2076 and <= 2080)
            .OrderBy(image => image.ResourceNumber).ToArray();
        for (var index = 0; index < previewText.Responses.Count; index++)
        {
            var text = previewText.Responses[index].Text;
            _dialogueResponseText.Add((
                responsePlacements[index].X + 2,
                responsePlacements[index].Y + Math.Max(0, (10 - text.Height) / 2),
                CreateTextTexture(_dialogueTextPalette,
                    text.Pixels, text.Width, text.Height)));
        }
    }

    private sealed record DialogueBranchPresentation(
        DialogueBranchResult Branch,
        IReadOnlyList<GplDialogueOutput> Output);
}
