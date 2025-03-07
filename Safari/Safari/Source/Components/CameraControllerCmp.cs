using Engine;
using Engine.Components;
using Engine.Input;
using Engine.Objects;
using Microsoft.Xna.Framework;
using Safari.Debug;
using System;

namespace Safari.Components;

// TODO: separate offset
public class CameraControllerCmp : Component, IUpdatable {
	public float PanSpeed { get; set; } = 100f;
	public float FastPanModifier { get; set; } = 2f;
	public float SlowPanModifier { get; set; } = 0.5f;

	public float ZoomSpeed { get; set; } = 0.1f;
	public float MinZoom { get; set; } = 0.6f;
	public float MaxZoom { get; set; } = 1.75f;

	public Rectangle? Bounds { get; set; }

	public Func<Vector2> TargetPosFn { get; set; }

	private Camera Camera => Owner as Camera;

	public CameraControllerCmp(Rectangle? bounds = null, Func<Vector2> targetPosFn = null) {
		Bounds = bounds;
		TargetPosFn = targetPosFn;
	}

	/// <summary>
	/// Sets the camera to follow a GameObject on screen
	/// </summary>
	/// <param name="obj">The game object to follow</param>
	public void SetTargetObject(GameObject obj) {
		if (obj.GetComponent(out SpriteCmp sprite)) {
			TargetPosFn = () => {
				Vector2 offset = new Vector2(
					sprite.Texture.Width / 2,
					sprite.Texture.Height /	2
				);

				return obj.Position + offset;
			};
		} else {
			TargetPosFn = () => obj.Position;
		}
	}

	public void Update(GameTime gameTime) {
		Vector2 prevPos = Owner.Position;
		Vector2 delta = Vector2.Zero;

		if (TargetPosFn == null) {
			delta = GetInputPan(gameTime);
			//Owner.Position += delta;

			Camera.Zoom += GetInputZoom(gameTime);
			Camera.Zoom = Math.Clamp(Camera.Zoom, MinZoom, MaxZoom);
		} else {
			Owner.Position = TargetPosFn();
		}

		/*Vector2 sFloor(Vector2 v) {
			float x = v.X;
			float y = v.Y;

			if (x < 0) {
				x = (float)Math.Ceiling(x);
			} else {
				x = (float)Math.Floor(x);
			}

			if (y < 0) {
				y = (float)Math.Ceiling(y);
			} else {
				y = (float)Math.Floor(y);
			}

			return new Vector2(x, y);
		}*/

		if (delta.X != 0 && delta.Y != 0) {
			Owner.Position = Owner.Position.ToPoint().ToVector2();
			//Owner.Position += sFloor(delta);
		} else {
			Owner.Position = Utils.Round(Owner.Position + delta).ToVector2();
		}

		Vector2 diff = delta - (Owner.Position - prevPos);

		if (Bounds != null) {
			Rectangle realBounds = Bounds.Value;
			realBounds.Offset(Camera.ScreenWidth * 0.5f, Camera.ScreenHeight * 0.5f);
			Owner.Position = realBounds.Clamp(Owner.Position);
		}

		DebugInfoManager.AddInfo("cam calc delta", delta.Format(), DebugInfoPosition.BottomRight);
		DebugInfoManager.AddInfo("cam real delta", (Owner.Position - prevPos).Format(), DebugInfoPosition.BottomRight);
		DebugInfoManager.AddInfo("cam - delta diff", diff.Format(), DebugInfoPosition.BottomRight);
		DebugInfoManager.AddInfo("cam pos", Owner.Position.Format(false), DebugInfoPosition.BottomRight);
		DebugInfoManager.AddInfo("delta time", $"{gameTime.ElapsedGameTime.TotalSeconds:0.000}", DebugInfoPosition.BottomRight);
	}

	/// <summary>
	/// Calculates the camera movement delta vector based on the currently pressed inputs
	/// </summary>
	/// <param name="gameTime">The current game time</param>
	/// <returns>The result vector</returns>
	private Vector2 GetInputPan(GameTime gameTime) {
		Vector2 delta = Vector2.Zero;

		int straightUnit = 3, diagUnit = 2;
		if (InputManager.Actions.IsDown("left")) {
			delta.X -= straightUnit;
		}
		if (InputManager.Actions.IsDown("right")) {
			delta.X += straightUnit;
		}
		if (InputManager.Actions.IsDown("up")) {
			delta.Y -= straightUnit;
		}
		if (InputManager.Actions.IsDown("down")) {
			delta.Y += straightUnit;
		}

		if (delta != Vector2.Zero) {
			if (delta.X != 0 && delta.Y != 0) {
				delta.X = Math.Sign(delta.X) * diagUnit;
				delta.Y = Math.Sign(delta.Y) * diagUnit;
			}
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
