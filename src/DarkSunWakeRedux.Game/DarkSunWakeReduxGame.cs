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
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    private Texture2D CreateTexture(PackedIndexedImage image, IndexedImageFrame frame)
    {
        var colors = new Color[frame.Pixels.Length];
        for (var index = 0; index < colors.Length; index++)
        {
            var color = image.Palette[frame.Pixels[index]];
            colors[index] = new Color(color.Red, color.Green, color.Blue, frame.Alpha[index]);
        }
        var texture = new Texture2D(GraphicsDevice, frame.Width, frame.Height);
        texture.SetData(colors);
        return texture;
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
            if (keyboard.IsKeyDown(Keys.Escape) && _previousKeyboard.IsKeyUp(Keys.Escape))
                _startFlow.Execute(StartFlowCommand.Cancel());
            _previousMouse = mouse;
            _previousKeyboard = keyboard;
            if (_startFlow.Snapshot().Screen == StartFlowScreen.ExitRequested) Exit();
        }
        base.Update(gameTime);
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
