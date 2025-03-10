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

	public Matrix TransformMatrix {
		get {
			return
				Matrix.CreateTranslation(-Position.X, -Position.Y, 0) *
				Matrix.CreateRotationZ(Rotation) *
				Matrix.CreateScale(Zoom) *
				Matrix.CreateTranslation(ScreenWidth * 0.5f, ScreenHeight * 0.5f, 0f);
		}
	}

	public Point ScreenSize => Game.RenderTarget.Bounds.Size;
	public int ScreenWidth => ScreenSize.X;
	public int ScreenHeight => ScreenSize.Y;

	public Camera(Vector2? position = null) : base(position ?? new Vector2(Game.RenderTarget.Width * 0.5f, Game.RenderTarget.Height * 0.5f)) { }

	public override void Update(GameTime gameTime) {
		base.Update(gameTime);

		DebugInfoManager.AddInfo("cam pos", Position.Format(false, false), DebugInfoPosition.BottomRight);
		DebugInfoManager.AddInfo("cam zoom", $"x{Zoom:0.00}", DebugInfoPosition.BottomRight);
	}
}
