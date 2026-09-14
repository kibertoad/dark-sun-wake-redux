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
    private Texture2D? _openingLeaderTexture;
    private PackedRegion? _tyrRegion;
    private PackedObjectFrameCatalog? _tyrObjects;
    private RegionTerrainGrid? _tyrTerrain;
    private ExplorationActorController? _leaderController;
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
        IsMouseVisible = true;
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
        var explorationViewport = OpeningTyrScene.Rasterize(_tyrRegion, _tyrObjects);
        _explorationSceneTexture = CreateTexture(_tyrRegion.Palette, explorationViewport);
        _openingLeaderTexture = LoadImageTexture(OriginalContent.OpeningLeaderImageAssetPath);
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
                    if (screen == StartFlowScreen.StartWindow &&
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
        if (_exploration.Snapshot().View == ExplorationView.World &&
            mouse.RightButton == ButtonState.Pressed &&
            _previousMouse.RightButton == ButtonState.Released)
            ExecuteExplorationCommand(new(ExplorationCommandKind.CycleCursorMode));

        var transform = new LogicalCanvasTransform(
            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        if (_exploration.Snapshot().View == ExplorationView.World &&
            !suppressWalkClick &&
            mouse.LeftButton == ButtonState.Pressed &&
            _previousMouse.LeftButton == ButtonState.Released &&
            transform.TryToLogical(mouse.X, mouse.Y, out var walkX, out var walkY))
            _leaderController.PlanAt(_exploration.Snapshot(), walkX, walkY);
        if (_exploration.Snapshot().View == ExplorationView.World &&
            transform.TryToLogical(mouse.X, mouse.Y, out var x, out var y) &&
            ExplorationInput.ScrollAtEdge(x, y) is { } scroll)
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
        if ((transition.Before.CameraX, transition.Before.CameraY) !=
            (transition.After.CameraX, transition.After.CameraY))
            RefreshExplorationScene(transition.After);
        return transition;
    }

    private void RefreshExplorationScene(ExplorationSnapshot snapshot)
    {
        if (_tyrRegion is null || _tyrObjects is null || _explorationSceneTexture is null)
            throw new InvalidOperationException("The Tyr exploration scene is not loaded.");
        var viewport = RegionSceneRasterizer.Rasterize(_tyrRegion, _tyrObjects,
            snapshot.CameraX, snapshot.CameraY, OpeningTyrScene.Width, OpeningTyrScene.Height);
        _explorationSceneTexture.SetData(CreateColors(_tyrRegion.Palette,
            viewport.Pixels, viewport.Alpha));
    }

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

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        if (_spriteBatch is not null)
        {
            var screen = _startFlow.Snapshot().Screen;
            var explorationView = _exploration.Snapshot().View;
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
                var bounds = _leaderPresentation.AtMovement(
                    _leaderController.VisualSnapshot(),
                    camera.CameraX, camera.CameraY, GffRegion.TilePixelSize);
                if (bounds.Intersects(OpeningTyrScene.Width, OpeningTyrScene.Height))
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
                var destination = new Rectangle(transform.X, transform.Y, transform.Width, transform.Height);
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                foreach (var (x, y, texture) in placedImages)
                    _spriteBatch.Draw(texture, ScaledRectangle(destination, transform,
                        x, y, texture.Width, texture.Height), Color.White);
                if (screen == StartFlowScreen.StartWindow)
                    foreach (var (control, texture) in _startMenuControls)
                        _spriteBatch.Draw(texture, ScaledRectangle(destination, transform,
                            control.X, control.Y, texture.Width, texture.Height), Color.White);
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
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }

    private static Rectangle ScaledRectangle(
        Rectangle canvas, LogicalCanvasTransform transform, int x, int y, int width, int height) =>
        new(canvas.X + (int)MathF.Floor(x * transform.Scale),
            canvas.Y + (int)MathF.Floor(y * transform.Scale),
            Math.Max(1, (int)MathF.Floor(width * transform.Scale)),
            Math.Max(1, (int)MathF.Floor(height * transform.Scale)));
}
