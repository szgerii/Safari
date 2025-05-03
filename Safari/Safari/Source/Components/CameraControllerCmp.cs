using Engine;
using Engine.Input;
using Engine.Objects;
using Microsoft.Xna.Framework;
using Safari.Debug;
using System;

namespace Safari.Components;

public class CameraControllerCmp : Component, IUpdatable {
	public static float DefaultScrollSpeed { get; set; } = 100f;
	public static float DefaultZoom { get; set; } = 2;

    public float ScrollSpeed { get; set; } = 100f;

	public float ZoomSpeed { get; set; } = 0.05f;
	public float MinZoom { get; set; } = 0.55f;
	public float MaxZoom { get; set; } = 4f;

	public float FastModifier { get; set; } = 1.8f;
	public float SlowModifier { get; set; } = 0.5f;

	public Rectangle? Bounds { get; set; } = null;

	public bool ShowDebugInfo { get; set; } = false;

	private Camera Camera => Owner as Camera;

	public CameraControllerCmp() { }

	public CameraControllerCmp(Rectangle bounds) {
		Bounds = bounds;
		ScrollSpeed = CameraControllerCmp.DefaultScrollSpeed;
	}

	public override void Load() {
		Camera.Zoom = DefaultZoom;

		base.Load();
	}

	public void Update(GameTime gameTime) {
		Vector2 prevPos = Owner.Position;

		// pos
		Vector2 posDelta = GetInputPan(gameTime);
		Owner.Position += posDelta;
		Owner.Position = Utils.Round(Owner.Position).ToVector2();

		// zoom
		float prevZoom = Camera.Zoom;
		Camera.Zoom += GetInputZoom(gameTime);
		Camera.Zoom = Math.Clamp(Camera.Zoom, MinZoom, MaxZoom);

		if (InputManager.Actions.JustPressed("reset-zoom")) {
			Camera.Zoom = DefaultZoom;
		}

		// clamp pos
		if (Bounds != null) {
			Rectangle bounds = Bounds.Value;

			float camScale = 1f / Camera.Zoom;
			int realWidth = Utils.Round(bounds.Width - Camera.ScreenWidth * camScale);
			int realHeight = Utils.Round(bounds.Height - Camera.ScreenHeight * camScale);

			Point realSize = new Point(realWidth, realHeight);
			Rectangle realBounds = new Rectangle(bounds.Location, realSize);

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

		delta *= ScrollSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

		if (InputManager.Actions.IsDown("fast-mod")) {
			delta *= FastModifier;
		}
		if (InputManager.Actions.IsDown("slow-mod")) {
			delta *= SlowModifier;
		}

		return delta;
	}

	/// <summary>
	/// Calculates the zoom delta from inputs
	/// </summary>
	/// <param name="gameTime">The current game time</param>
	/// <returns>The calculated zoom delta</returns>
	private float GetInputZoom(GameTime gameTime) {
		float scaleDelta = InputManager.Mouse.ScrollMovement;

		if (scaleDelta == 0f) {
			if (InputManager.Actions.IsDown("increase-zoom")) {
				scaleDelta += 10f;
			}
			if (InputManager.Actions.IsDown("decrease-zoom")) {
				scaleDelta -= 10f;
			}
		}

		scaleDelta *= ZoomSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

		if (InputManager.Actions.IsDown("fast-mod")) {
			scaleDelta *= FastModifier;
		}
		if (InputManager.Actions.IsDown("slow-mod")) {
			scaleDelta *= SlowModifier;
		}

		return scaleDelta;
	}

	public void CenterOnPosition(Vector2 position) {
		Vector2 camPos = position;

		camPos -= Camera.ScreenSize.ToVector2() * (1f / Camera.Zoom) / 2;

		if (Bounds != null) {
			camPos = Bounds.Value.Clamp(camPos);
		}

		Owner.Position = camPos;
	}
}
