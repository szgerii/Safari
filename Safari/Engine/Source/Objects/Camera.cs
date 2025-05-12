using Engine.Debug;
using Engine.Graphics.Stubs.Texture;
using Engine.Helpers;
using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Safari.Debug;
using Safari.Input;
using Safari.Scenes;
using System;

namespace Engine.Objects;

[JsonObject(MemberSerialization.OptIn)]
public class Camera : GameObject {
	public static Camera? Active { get; set; }

	[JsonProperty]
	public float Zoom { get; set; } = 1.0f;

	[JsonProperty]
	private float rotation = 0.0f;
	public float Rotation {
		get => rotation;
		set {
			rotation = value % (float)(2 * Math.PI);
		}
	}

	public Matrix TransformMatrix { get; private set; }

	public Point ScreenSize => Game.RenderTarget!.Bounds.Size;
	public int ScreenWidth => ScreenSize.X;
	public int ScreenHeight => ScreenSize.Y;
	public Vector2 RealViewportSize => ScreenSize.ToVector2() * (1f / Zoom);
	public Vectangle RealViewportBounds => new Vectangle(Position - (RealViewportSize / 2f), RealViewportSize);

	[JsonConstructor]
	public Camera(Vector2? position = null) : base(position ?? Vector2.Zero) { }

	public override void Update(GameTime gameTime) {
		base.Update(gameTime);

		TransformMatrix =
			Matrix.CreateTranslation(new Vector3(-Position, 0)) *
			Matrix.CreateRotationZ(Rotation) *
			Matrix.CreateScale(Zoom, Zoom, 1f) *
			Matrix.CreateTranslation(ScreenWidth / 2f, ScreenHeight / 2f, 0);

		DebugInfoManager.AddInfo("cam", $"{Position.Format(false, false)}, x{Zoom:0.00}, {Rotation:0.00} rad", DebugInfoPosition.BottomRight);
	}

	private ITexture2D? tex;
	public override void Draw(GameTime gameTime) {
		if (DebugMode.IsFlagActive("cam-indicators")) {
			int tileSize = GameScene.Active.Model.Level!.TileSize;
			tex ??= Utils.GenerateTexture(tileSize, tileSize, Color.Red, true);

			int halfTileSize = Utils.Round(tileSize / 2f);
			Vectangle destRect = new Vectangle(RealViewportBounds.Center, new Vector2(tileSize));
			destRect.Offset(-halfTileSize, -halfTileSize);
			Game.SpriteBatch!.Draw(tex.ToTexture2D(), (Rectangle)destRect, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			destRect = new Vectangle(InputManager.Mouse.GetWorldPos(), new Vector2(tileSize));
			destRect.Offset(-halfTileSize, -halfTileSize);
			Game.SpriteBatch.Draw(tex.ToTexture2D(), (Rectangle)destRect, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		}

		base.Draw(gameTime);
	}
}
