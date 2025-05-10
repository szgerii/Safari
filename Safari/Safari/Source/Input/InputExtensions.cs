using Engine.Input;
using Engine.Objects;
using Microsoft.Xna.Framework;
using Safari.Model;
using Safari.Scenes;
using System;

namespace Safari.Input;

public static class InputExtensions {
    /// <summary>
    /// Calculates game world position of the mouse pointer
    /// </summary>
    /// <param name="mouse">The base mouse object</param>
    /// <param name="currLevel">The current level object (leave as null to use the active level)</param>
    /// <returns>The game world position of the mouse pointer as a vector</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Vector2 GetWorldPos(this Mouse mouse, Level currLevel = null) {        
        try {
            currLevel ??= GameScene.Active.Model.Level;
        } catch (NullReferenceException) {
            throw new InvalidOperationException("Cannot get mouse's game world position when the current scene isn't a GameScene or no level is active");
		}

        if (Game.Instance.IsHeadless) return Vector2.Zero;

        float rtScale = Game.RenderTargetScale;
		float rtDiffX = Game.Graphics.PreferredBackBufferWidth - rtScale * Game.RenderTarget.Width;
		float rtDiffY = Game.Graphics.PreferredBackBufferHeight - rtScale * Game.RenderTarget.Height;
		Vector2 rtOffset = Vector2.Zero;
		if (rtDiffX != 0) {
			rtOffset.X = rtDiffX / 2;
		}
		if (rtDiffY != 0) {
			rtOffset.Y = rtDiffY / 2;
		}

		Vector2 result = mouse.Location.ToVector2();

        result -= rtOffset;
        result /= rtScale * Camera.Active.Zoom;
        result += Camera.Active.Position - (Camera.Active.RealViewportSize / 2f);

        return result;
    }
}
