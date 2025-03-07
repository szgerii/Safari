using Microsoft.Xna.Framework;
using System;

namespace Engine.Objects;

public class Camera : GameObject {
	public static Camera Active { get; set; }

	public Vector2 Offset { get; set; }

	public float Zoom { get; set; } = 1.0f;

	private float rotation = 0.0f;
	public float Rotation {
		get => rotation;
		set {
			rotation = value % (float)(2 * Math.PI);
		}
	}

	public Matrix TransformMatrix {
		get {
			return
				Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
				Matrix.CreateRotationZ(Rotation) *
				Matrix.CreateScale(Zoom) *
				Matrix.CreateTranslation(ScreenWidth * 0.5f, ScreenHeight * 0.5f, 0f);
		}
	}

	public Point ScreenSize => new Point(Game.RenderTarget.Width, Game.RenderTarget.Height);
	public int ScreenWidth => ScreenSize.X;
	public int ScreenHeight => ScreenSize.Y;

	public Camera(Vector2? position = null) : base(position ?? new Vector2(Game.RenderTarget.Width * 0.5f, Game.RenderTarget.Height * 0.5f)) { }
}
