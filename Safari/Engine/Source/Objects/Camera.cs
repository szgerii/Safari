using Microsoft.Xna.Framework;
using Safari.Debug;
using System;

namespace Engine.Objects;

public class Camera : GameObject {
	public static Camera Active { get; set; }

	public float Zoom { get; set; } = 1.0f;

	private float rotation = 0.0f;
	public float Rotation {
		get => rotation;
		set {
			rotation = value % (float)(2 * Math.PI);
		}
	}

	public Matrix TransformMatrix { get; private set; }
	public Matrix ViewToWorld => Matrix.Invert(TransformMatrix);

	public Point ScreenSize => Game.RenderTarget.Bounds.Size;
	public int ScreenWidth => ScreenSize.X;
	public int ScreenHeight => ScreenSize.Y;

	public Camera(Vector2? position = null) : base(position ?? Vector2.Zero) { }

	public override void Update(GameTime gameTime) {
		base.Update(gameTime);

		TransformMatrix =
			Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
			Matrix.CreateRotationZ(Rotation) *
			Matrix.CreateScale(Zoom, Zoom, 1f);

		DebugInfoManager.AddInfo("cam", $"{Position.Format(false, false)}, x{Zoom:0.00}, {Rotation:0.00} rad", DebugInfoPosition.BottomRight);
	}
}
