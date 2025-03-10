using Engine;
using Engine.Input;
using Engine.Objects;
using Microsoft.Xna.Framework;
using Safari.Debug;
using System;

namespace Safari.Components;

// TODO: separate offset
public class CameraControllerCmp : Component, IUpdatable {
	public float PanSpeed { get; set; } = 100f;
	public float FastPanModifier { get; set; } = 1.8f;
	public float SlowPanModifier { get; set; } = 0.5f;

	public float ZoomSpeed { get; set; } = 0.1f;
	public float MinZoom { get; set; } = 0.6f;
	public float MaxZoom { get; set; } = 1.75f;

	public Rectangle? Bounds { get; set; } = null;

	public bool ShowDebugInfo { get; set; } = false;

	private Camera Camera => Owner as Camera;

	public CameraControllerCmp() { }

	public CameraControllerCmp(Rectangle bounds) {
		Bounds = bounds;
	}

	public void Update(GameTime gameTime) {
		Vector2 prevPos = Owner.Position;

		Vector2 posDelta = GetInputPan(gameTime);
		Owner.Position += posDelta;
		Owner.Position = Utils.Round(Owner.Position).ToVector2();

		float zoomDelta = GetInputZoom(gameTime);
		Camera.Zoom += zoomDelta;
		Camera.Zoom = Math.Clamp(Camera.Zoom, MinZoom, MaxZoom);

		if (Bounds != null) {
			Rectangle realBounds = Bounds.Value;
			realBounds.Offset(Camera.ScreenWidth * 0.5f, Camera.ScreenHeight * 0.5f);

			//realBounds.Location = (realBounds.Location.ToVector2() * Camera.Zoom).ToPoint();
			//realBounds.Size -= (Camera.Zoom * Bounds.Value.Size.ToVector2()).ToPoint();

			Owner.Position = realBounds.Clamp(Owner.Position);
		}

		if (ShowDebugInfo) {
			DebugInfoManager.AddInfo("cam applied delta", posDelta.Format(), DebugInfoPosition.BottomRight);
			DebugInfoManager.AddInfo("cam real delta", (Owner.Position - prevPos).Format(), DebugInfoPosition.BottomRight);
		}
	}

	/// <summary>
	/// Calculates the camera movement delta vector based on the currently pressed inputs
	/// </summary>
	/// <param name="gameTime">The current game time</param>
	/// <returns>The result vector</returns>
	private Vector2 GetInputPan(GameTime gameTime) {
		Vector2 delta = Vector2.Zero;

		// delta unit is 3 for axis aligned movement, 2 for diagonal
		// this is because camera movement is the smoothest if the camera position only uses ints
		// 3-2 is a closer pairing than the standard 1-sqrt(2), resulting in less precision loss during rounding

		if (InputManager.Actions.IsDown("left")) {
			delta.X -= 3;
		}
		if (InputManager.Actions.IsDown("right")) {
			delta.X += 3;
		}
		if (InputManager.Actions.IsDown("up")) {
			delta.Y -= 3;
		}
		if (InputManager.Actions.IsDown("down")) {
			delta.Y += 3;
		}

		if (delta.X != 0 && delta.Y != 0) {
			delta.X = Math.Sign(delta.X) * 2;
			delta.Y = Math.Sign(delta.Y) * 2;
		}

		delta *= PanSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

		if (InputManager.Actions.IsDown("fast-pan")) {
			delta *= FastPanModifier;
		}
		if (InputManager.Actions.IsDown("slow-pan")) {
			delta *= SlowPanModifier;
		}

		return delta;
	}

	/// <summary>
	/// Calculates the zoom delta from inputs
	/// </summary>
	/// <param name="gameTime">The current game time</param>
	/// <returns>The calculated zoom delta</returns>
	private float GetInputZoom(GameTime gameTime) {
		if (!InputManager.Mouse.ScrollChanged) {
			return 0f;
		}

		float scaleDelta = InputManager.Mouse.ScrollMovement;
		scaleDelta *= ZoomSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

		return scaleDelta;
	}
}
