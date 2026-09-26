using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DarkSunWakeRedux.Game;

/// <summary>
/// The launch options screen (DEV-UI-001), which opens before anything of the original's and
/// hands its values to the rest of the game on Confirm.
/// </summary>
public sealed partial class DarkSunWakeReduxGame
{
    private LaunchOptionsSession? _launchOptions;
    private Texture2D? _launchOptionsTexture;
    private (LaunchSettings Settings, int Selected)? _renderedLaunchOptions;

    /// <summary>The Wide map view setting of DEV-EXPLORE-001, fixed once the player confirms.</summary>
    private bool _wideMapView = LaunchSettings.Default.WideMapView;

    private bool LaunchOptionsOpen => _launchOptions?.Outcome == LaunchOptionsOutcome.Open;

    private void OpenLaunchOptions()
    {
        var loaded = LaunchSettingsStore.ReadOrDefault(_settingsPath);
        _wideMapView = loaded.Settings.WideMapView;
        _launchOptions = new(loaded.Settings);
    }

    private void UpdateLaunchOptions(MouseState mouse)
    {
        if (_launchOptions is null) return;
        var keyboard = Keyboard.GetState();
        if (FullscreenInput.ShouldToggle(keyboard, _previousKeyboard))
            ToggleFullscreen();
        if (ScreenshotInput.ShouldCapture(keyboard, _previousKeyboard))
            _screenshotRequested = true;
        var transform = new LogicalCanvasTransform(
            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        var item = transform.TryToLogical(mouse.X, mouse.Y, out var x, out var y)
            ? LaunchOptionsLayout.HitTest(x, y)
            : null;
        var moved = mouse.X != _previousMouse.X || mouse.Y != _previousMouse.Y;
        var clicked = mouse.LeftButton == ButtonState.Pressed &&
            _previousMouse.LeftButton == ButtonState.Released;
        foreach (var command in LaunchOptionsInput.Resolve(
                     keyboard, _previousKeyboard, item, moved, clicked))
            _launchOptions.Execute(command);
        _previousMouse = mouse;
        _previousKeyboard = keyboard;
        switch (_launchOptions.Outcome)
        {
            case LaunchOptionsOutcome.Confirmed:
                _wideMapView = _launchOptions.Settings.WideMapView;
                SaveLaunchSettings(_launchOptions.Settings);
                break;
            case LaunchOptionsOutcome.ExitRequested:
                Exit();
                break;
        }
    }

    private void SaveLaunchSettings(LaunchSettings settings)
    {
        // A settings file that cannot be written costs the player the saved values, never the game.
        try
        {
            LaunchSettingsStore.WriteAtomic(_settingsPath, settings);
        }
        catch (Exception exception) when (exception is IOException or
            UnauthorizedAccessException or NotSupportedException or InvalidDataException or
            System.Security.SecurityException)
        {
            Console.Error.WriteLine(
                $"The launch options could not be saved to '{_settingsPath}': {exception.Message}");
        }
    }

    private void DrawLaunchOptions()
    {
        if (_spriteBatch is null || _launchOptions is null || _dialogueFont is null) return;
        var state = (_launchOptions.Settings, _launchOptions.Selected);
        if (_launchOptionsTexture is null || _renderedLaunchOptions != state)
        {
            var canvas = LaunchOptionsRasterizer.Rasterize(_dialogueFont, _launchOptions);
            var colors = CreateColors(LaunchOptionsRasterizer.Palette, canvas.Pixels,
                Enumerable.Repeat((byte)255, canvas.Pixels.Length).ToArray());
            _launchOptionsTexture ??= new Texture2D(GraphicsDevice, canvas.Width, canvas.Height);
            _launchOptionsTexture.SetData(colors);
            _renderedLaunchOptions = state;
        }
        var transform = new LogicalCanvasTransform(
            GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
        var destination = new Rectangle(transform.X, transform.Y, transform.Width, transform.Height);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_launchOptionsTexture, destination, Color.White);
        var mouse = Mouse.GetState();
        if (transform.TryToLogical(mouse.X, mouse.Y, out var x, out var y) &&
            _cursorTextures.TryGetValue(ExplorationCursorVisual.Walk, out var cursor))
            _spriteBatch.Draw(cursor, ScaledRectangle(destination, transform,
                x, y, cursor.Width, cursor.Height), Color.White);
        _spriteBatch.End();
    }
}
