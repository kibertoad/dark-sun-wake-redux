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
    private readonly List<(StartMenuLayerAsset Layer, Texture2D Texture)> _startMenuLayers = [];
    private readonly List<(StartMenuControl Control, Texture2D Texture)> _startMenuControls = [];
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
        {
            var layerPath = Path.Combine(_assetPack,
                layer.Path.Replace('/', Path.DirectorySeparatorChar));
            using var layerStream = File.OpenRead(layerPath);
            var image = PackedIndexedImage.Read(layerStream, layer.Path);
            _startMenuLayers.Add((layer, CreateTexture(image, image.Frames.Single())));
        }
        var uiPath = Path.Combine(_assetPack,
            OriginalContent.StartFlowUiCatalogAssetPath.Replace('/', Path.DirectorySeparatorChar));
        using var uiStream = File.OpenRead(uiPath);
        var controls = StartMenuInput.Resolve(PackedUiCatalog.Read(
            uiStream, OriginalContent.StartFlowUiCatalogAssetPath));
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
            if (_startFlow.Snapshot().Screen == StartFlowScreen.StartWindow &&
                mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
            {
                var transform = new LogicalCanvasTransform(
                    GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                if (transform.TryToLogical(mouse.X, mouse.Y, out var x, out var y) &&
                    StartMenuInput.HitTest(_startMenuControls.Select(item => item.Control).ToArray(), x, y)
                        is { } choice)
                    _startFlow.Execute(StartFlowCommand.Choose(choice));
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
        if (_spriteBatch is not null && _startFlow.Snapshot().Screen == StartFlowScreen.StartWindow)
        {
            var transform = new LogicalCanvasTransform(
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            var destination = new Rectangle(transform.X, transform.Y, transform.Width, transform.Height);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            foreach (var (layer, texture) in _startMenuLayers)
                _spriteBatch.Draw(texture, ScaledRectangle(destination, transform,
                    layer.X, layer.Y, texture.Width, texture.Height), Color.White);
            foreach (var (control, texture) in _startMenuControls)
                _spriteBatch.Draw(texture, ScaledRectangle(destination, transform,
                    control.X, control.Y, texture.Width, texture.Height), Color.White);
            _spriteBatch.End();
        }
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        foreach (var (_, texture) in _startMenuLayers) texture.Dispose();
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
