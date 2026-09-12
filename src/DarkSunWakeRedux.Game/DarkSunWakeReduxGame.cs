using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DarkSunWakeRedux.Resources;

namespace DarkSunWakeRedux.Game;

public sealed class DarkSunWakeReduxGame : Microsoft.Xna.Framework.Game
{
    private readonly string? _assetPack;
    private readonly bool _platformSmoke;
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private Texture2D? _titleTexture;
    private readonly List<(StartMenuButtonAsset Mapping, Texture2D Texture)> _startMenuButtons = [];

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
        foreach (var button in OriginalContent.StartMenuButtons)
        {
            var buttonPath = Path.Combine(_assetPack,
                button.Path.Replace('/', Path.DirectorySeparatorChar));
            using var buttonStream = File.OpenRead(buttonPath);
            var image = PackedIndexedImage.Read(buttonStream, button.Path);
            _startMenuButtons.Add((button, CreateTexture(image, image.Frames[0])));
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

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        if (_titleTexture is not null && _spriteBatch is not null)
        {
            var scale = Math.Min(
                GraphicsDevice.Viewport.Width / (float)_titleTexture.Width,
                GraphicsDevice.Viewport.Height / (float)_titleTexture.Height);
            var width = Math.Max(1, (int)MathF.Floor(_titleTexture.Width * scale));
            var height = Math.Max(1, (int)MathF.Floor(_titleTexture.Height * scale));
            var destination = new Rectangle(
                (GraphicsDevice.Viewport.Width - width) / 2,
                (GraphicsDevice.Viewport.Height - height) / 2,
                width,
                height);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_titleTexture, destination, Color.White);
            foreach (var (mapping, texture) in _startMenuButtons)
            {
                var buttonDestination = new Rectangle(
                    destination.X + (int)MathF.Floor(mapping.X * scale),
                    destination.Y + (int)MathF.Floor(mapping.Y * scale),
                    Math.Max(1, (int)MathF.Floor(texture.Width * scale)),
                    Math.Max(1, (int)MathF.Floor(texture.Height * scale)));
                _spriteBatch.Draw(texture, buttonDestination, Color.White);
            }
            _spriteBatch.End();
        }
        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        _titleTexture?.Dispose();
        foreach (var (_, texture) in _startMenuButtons) texture.Dispose();
        _spriteBatch?.Dispose();
        base.UnloadContent();
    }
}
