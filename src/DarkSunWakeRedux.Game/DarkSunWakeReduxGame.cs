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
    private Texture2D? _explorationSceneTexture;
    private PackedRegion? _tyrRegion;
    private PackedObjectFrameCatalog? _tyrObjects;
    private readonly ExplorationSession _exploration = new(
        RegionSceneRasterizer.WorldWidth,
        RegionSceneRasterizer.WorldHeight,
        OpeningTyrScene.Width,
        OpeningTyrScene.Height,
        OpeningTyrScene.OriginX,
        OpeningTyrScene.OriginY);
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
        var regionPath = AssetPath(OriginalContent.TyrRegionAssetPath);
        using var regionStream = File.OpenRead(regionPath);
        _tyrRegion = PackedRegion.Read(regionStream, OriginalContent.TyrRegionAssetPath);
        var objectPath = AssetPath(OriginalContent.TyrObjectCatalogAssetPath);
        using var objectStream = File.OpenRead(objectPath);
        _tyrObjects = PackedObjectFrameCatalog.Read(
            objectStream, OriginalContent.TyrObjectCatalogAssetPath);
        var explorationViewport = OpeningTyrScene.Rasterize(_tyrRegion, _tyrObjects);
        _explorationSceneTexture = CreateTexture(_tyrRegion.Palette, explorationViewport);
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        string AssetPath(string relativePath) => Path.Combine(_assetPack,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
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
                }
            }
            var keyboard = Keyboard.GetState();
            if (screen != StartFlowScreen.Gameplay &&
                keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
                _startFlow.Execute(StartFlowCommand.Cancel());
            if (screen == StartFlowScreen.Gameplay)
                UpdateExploration(mouse, keyboard);
            _previousMouse = mouse;
            _previousKeyboard = keyboard;
            if (_startFlow.Snapshot().Screen == StartFlowScreen.ExitRequested) Exit();
        }
        base.Update(gameTime);
    }

    private void UpdateExploration(MouseState mouse, KeyboardState keyboard)
    {
        var before = _exploration.Snapshot();
        if (ExplorationHotkeys.Resolve(keyboard, _previousKeyboard) is { } hotkey)
            _exploration.Execute(hotkey);
        if (_exploration.Snapshot().View == ExplorationView.World &&
            mouse.RightButton == ButtonState.Pressed &&
            _previousMouse.RightButton == ButtonState.Released)
            _exploration.Execute(new(ExplorationCommandKind.CycleCursorMode));

        var transform = new LogicalCanvasTransform(
            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        if (_exploration.Snapshot().View == ExplorationView.World &&
            transform.TryToLogical(mouse.X, mouse.Y, out var x, out var y) &&
            ExplorationInput.ScrollAtEdge(x, y) is { } scroll)
            _exploration.Execute(scroll);
        var after = _exploration.Snapshot();
        if (after.View == ExplorationView.ExitRequested)
            Exit();
        if ((before.CameraX, before.CameraY) != (after.CameraX, after.CameraY))
            RefreshExplorationScene(after);
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

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        if (_spriteBatch is not null)
        {
            var screen = _startFlow.Snapshot().Screen;
            var images = screen switch
            {
                StartFlowScreen.StartWindow => _startMenuLayers.Select(item =>
                    (item.Layer.X, item.Layer.Y, item.Texture)),
                StartFlowScreen.PartyOverview => _partyOverviewLayers.Select(item =>
                    (item.Layer.X, item.Layer.Y, item.Texture)),
                StartFlowScreen.AddExistingCharacter => _addExistingImages.Select(item =>
                    (item.Placement.X, item.Placement.Y, item.Texture)),
                StartFlowScreen.Gameplay when _explorationSceneTexture is not null =>
                    [(0, 0, _explorationSceneTexture)],
                _ => []
            };
            var placedImages = images.ToArray();
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
        _explorationSceneTexture?.Dispose();
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
