using Engine;
using Engine.Debug;
using Engine.Input;
using Engine.Objects;
using Microsoft.Xna.Framework;
using Safari.Debug;
using Safari.Scenes;
using System;

namespace Safari.Components;

public class CameraControllerCmp : Component, IUpdatable {
	public static float DefaultScrollSpeed { get; set; } = 200f;
	public static float DefaultAcceleration { get; set; } = 23f;
	public static float DefaultZoom { get; set; } = 2;

    public float ScrollSpeed { get; set; } = DefaultScrollSpeed;
	public float ScrollAcceleration { get; set; } = DefaultAcceleration;
	public float ScrollDeceleration { get; set; } = 100f;

	private Vector2 currentSpeed = Vector2.Zero;

	public float ZoomSpeed { get; set; } = 0.05f;
	public float MinZoom { get; set; } = 0.55f;
	public float MaxZoom { get; set; } = 4f;

	public float FastModifier { get; set; } = 1.8f;
	public float SlowModifier { get; set; } = 0.5f;

	public Rectangle? Bounds { get; set; } = null;

	private bool canEnterDragMode = false;
	private Camera Camera => Owner as Camera;

	public CameraControllerCmp() { }

	public CameraControllerCmp(Rectangle bounds) {
		Bounds = bounds;
	}

	public override void Load() {
		base.Load();
	}

	public void Update(GameTime gameTime) {
		Mouse mouse = InputManager.Mouse;
		if (mouse.JustPressed(MouseButtons.LeftButton) && !GameScene.Active.InMaskedArea(mouse.Location) && GameScene.Active.MouseMode == MouseMode.Inspect) {
			canEnterDragMode = true;
		} else if (mouse.IsUp(MouseButtons.LeftButton)) {
			canEnterDragMode = false;
			GameScene.Active.MouseDragLock = false;
		}

		Vector2 prevPos = Owner.Position;

		// pos
		CalcInputPan();
		Owner.Position += currentSpeed * (GameScene.Active.MouseDragLock ? 1f : (float)gameTime.ElapsedGameTime.TotalSeconds);
		if (prevPos != Utils.Round(Owner.Position).ToVector2()) {
			Owner.Position = Utils.Round(Owner.Position).ToVector2();
		}

		if (Owner.Position == prevPos) {
			currentSpeed = Vector2.Zero;
		}

		// zoom
		Camera.Zoom += GetInputZoom(gameTime);
		Camera.Zoom = Math.Clamp(Camera.Zoom, MinZoom, MaxZoom);

		if (InputManager.Actions.JustPressed("reset-zoom")) {
			Camera.Zoom = DefaultZoom;
		}

		// clamp pos
		ClampToBounds();

		if (DebugMode.IsFlagActive("cam-delta-stats")) {
			DebugInfoManager.AddInfo("cam applied delta", currentSpeed.Format(), DebugInfoPosition.BottomRight);
			DebugInfoManager.AddInfo("cam real delta", (Owner.Position - prevPos).Format(), DebugInfoPosition.BottomRight);
		}
	}

	/// <summary>
	/// Calculates the camera movement delta vector based on the currently pressed inputs
	/// </summary>
	/// <returns>The result vector</returns>
	private void CalcInputPan() {
		bool mouseMovementOverThreshold = InputManager.Mouse.Movement.LengthSquared() >= 1f;

		if (canEnterDragMode && mouseMovementOverThreshold) {
			GameScene.Active.MouseDragLock = true;
		}

		if (GameScene.Active.MouseDragLock) {
			if (mouseMovementOverThreshold) {
				float xDiff = Camera.RealViewportSize.X / (Game.RenderTarget.Width * Game.RenderTargetScale);
				float yDiff = Camera.RealViewportSize.Y / (Game.RenderTarget.Height * Game.RenderTargetScale);

				currentSpeed = -InputManager.Mouse.Movement * new Vector2(xDiff, yDiff);
			} else {
				currentSpeed = Vector2.Zero;
			}

			return;
		}

		GameScene.Active.MouseDragLock = false;

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

		bool noUserDelta = delta == Vector2.Zero;

		if (delta.X != 0 && delta.Y != 0) {
			delta.X = Math.Sign(delta.X) * 2;
			delta.Y = Math.Sign(delta.Y) * 2;
		}

		float speedMultiplier = InputManager.Actions.IsDown("fast-mod") ? FastModifier :
								InputManager.Actions.IsDown("slow-mod") ? SlowModifier : 1f;

		if (ScrollAcceleration > 0 && (currentSpeed != Vector2.Zero || delta != Vector2.Zero)) {
			// acceleration enabled

			// apply deceleration to stale components
			// same 3-2 unit system as above

			if (delta.X == 0 && Math.Abs(currentSpeed.X) < 0.5f) {
				currentSpeed.X = 0f;
			}

			if (delta.Y == 0 && Math.Abs(currentSpeed.Y) < 0.5f) {
				currentSpeed.Y = 0f;
			}

			int sx = Math.Sign(currentSpeed.X);
			int sy = Math.Sign(currentSpeed.Y);
			if (delta.X == 0 && delta.Y == 0) {
				delta.X = -Math.Sign(currentSpeed.X) * 2;
				delta.Y = -Math.Sign(currentSpeed.Y) * 2;
			} else {
				if (delta.X == 0) {
					delta.X = -Math.Sign(currentSpeed.X) * 3;
				}

				if (delta.Y == 0) {
					delta.Y = -Math.Sign(currentSpeed.Y) * 3;
				}
			}

			// apply acceleration/deceleration to delta
			currentSpeed += delta * ScrollAcceleration;

			if (noUserDelta) {
				if (sx != Math.Sign(currentSpeed.X)) {
					currentSpeed.X = 0;
					delta.X = 0;
				}

				if (sy != Math.Sign(currentSpeed.Y)) {
					currentSpeed.Y = 0;
					delta.Y = 0;
				}
			}

			if (currentSpeed.Length() > 5 * ScrollSpeed * speedMultiplier) {
				currentSpeed = Vector2.Normalize(currentSpeed) * 5 * ScrollSpeed * speedMultiplier;
			}
		} else {
			// acceleration disabled

			if (delta != Vector2.Zero) {
				currentSpeed = delta * ScrollSpeed * speedMultiplier;
			} else {
				currentSpeed = Vector2.Zero;
			}
		}
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

		scaleDelta *= ZoomSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds * Camera.Zoom;

		if (InputManager.Actions.IsDown("fast-mod")) {
			scaleDelta *= FastModifier;
		}
		if (InputManager.Actions.IsDown("slow-mod")) {
			scaleDelta *= SlowModifier;
		}

		return scaleDelta;
	}

	/// <summary>
	/// Centers the camera onto a given position
	/// </summary>
	/// <param name="position"></param>
	public void CenterOnPosition(Vector2 position) {
		Owner.Position = position;

		ClampToBounds();
	}

	/// <summary>
	/// Ensures that the camera is inside the bounds of the controller
	/// </summary>
	private void ClampToBounds() {
		if (Bounds == null) return;

		Rectangle bounds = Bounds.Value;

		float camScale = 1f / Camera.Zoom;
		int realWidth = Utils.Round(bounds.Width - Camera.ScreenWidth * camScale);
		int realHeight = Utils.Round(bounds.Height - Camera.ScreenHeight * camScale);

		Point realSize = new Point(realWidth, realHeight);
		Rectangle realBounds = new Rectangle(bounds.Location + (Camera.RealViewportSize / 2f).ToPoint(), realSize);

		Owner.Position = realBounds.Clamp(Owner.Position);
	}
}
