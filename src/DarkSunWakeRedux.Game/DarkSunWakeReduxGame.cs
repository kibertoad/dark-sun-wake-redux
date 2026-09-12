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
    private Texture2D? _titleTexture;
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
        var path = Path.Combine(_assetPack,
            OriginalContent.TitleImageAssetPath.Replace('/', Path.DirectorySeparatorChar));
        using var stream = File.OpenRead(path);
        var title = PackedIndexedImage.Read(stream, OriginalContent.TitleImageAssetPath);
        _titleTexture = CreateTexture(title, title.Frames.Single());
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
        if (_titleTexture is not null && _spriteBatch is not null &&
            _startFlow.Snapshot().Screen == StartFlowScreen.StartWindow)
        {
            var transform = new LogicalCanvasTransform(
                GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            var destination = new Rectangle(transform.X, transform.Y, transform.Width, transform.Height);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_titleTexture, destination, Color.White);
            foreach (var (control, texture) in _startMenuControls)
            {
                var buttonDestination = new Rectangle(
                    destination.X + (int)MathF.Floor(control.X * transform.Scale),
                    destination.Y + (int)MathF.Floor(control.Y * transform.Scale),
                    Math.Max(1, (int)MathF.Floor(texture.Width * transform.Scale)),
                    Math.Max(1, (int)MathF.Floor(texture.Height * transform.Scale)));
                _spriteBatch.Draw(texture, buttonDestination, Color.White);
            }
            _spriteBatch.End();
        }
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        _titleTexture?.Dispose();
        foreach (var (_, texture) in _startMenuControls) texture.Dispose();
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }
}
