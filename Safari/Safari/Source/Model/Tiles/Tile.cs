using Engine;
using Engine.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Scenes;
using System.Collections.Generic;

namespace Safari.Model.Tiles;

/// <summary>
/// Represents a single tile in the game world
/// </summary>
public abstract class Tile : GameObject {
	/// <summary>
	/// The position of the tile inside the tilemap grid
	/// </summary>
	public Point TilemapPosition
		=> (Position / GameScene.Active.Model.Level.TileSize).ToPoint();

	/// <summary>
	/// The Sprite component responsible for rendering this tile
	/// </summary>
	public SpriteCmp Sprite { get; init; }

	/// <summary>
	/// The display texture of the tile
	/// <br/>
	/// Set to null to make the tile invisible
	/// </summary>
	public Texture2D Texture {
		get => Sprite.Texture;
		set {
			Sprite.Texture = value;
			UpdateYSortOffset();
		}
	}

	/// <summary>
	/// The bounds of the area inside the texture that will be rendered
	/// <br />
	/// Set to null to render whole texture (default)
	/// </summary>
	public Rectangle? SourceRectangle {
		get => Sprite.SourceRectangle;
		set {
			Sprite.SourceRectangle = value;
			UpdateYSortOffset();
		}
	}

	/// <summary>
	/// Determines how far this tile gives vision at night
	/// 0 means only the tile itself is lit
	/// a number above 0 means a range
	/// a number below 0 means this tile does not give vision
	/// </summary>
	public int LightRange { get; set; } = -1;

	/// <summary>
	/// Indicates how many rows and columns the tile spans on the grid
	/// </summary>
	public Point Size { get; protected set; } = new Point(1, 1);

	/// <summary>
	/// The position of the tile used for placement and other calculations for tiles with textures that span multiple grid cells
	/// <br/>
	/// (0, 0) means top left tile inside the texture
	/// </summary>
	public Point AnchorTile { get; init; } = Point.Zero;

	/// <summary>
	/// Shorthand for getting the tilemap position of the anchor tile
	/// </summary>
	public Point AbsoluteAnchor => TilemapPosition + AnchorTile;

	/// <summary>
	/// Indicates if the tile can be used as a food source by herbivorous animals
	/// </summary>
	public bool IsFoodSource { get; init; } = false;
	/// <summary>
	/// Indicates if the tile can be used as a water source by animals
	/// </summary>
	public bool IsWaterSource { get; init; } = false;

	/// <summary>
	/// The offsets (in tiles) from the anchor tile that are considered "blocked" by this tile
	/// </summary>
	public Point[] ConstructionBlockOffsets { get; protected set; } = [];

	public Tile(Texture2D texture = null) : base(new Vector2(-1)) {
		Sprite = new SpriteCmp(texture);
		Sprite.YSortEnabled = true;
		Sprite.LayerDepth = 0.5f;

		Attach(Sprite);

		if (texture != null) {
			UpdateYSortOffset();
		}
	}

	/// <summary>
	/// Adjusts the Y-Sort offset to the current state of the tile
	/// </summary>
	public virtual void UpdateYSortOffset() {
		Rectangle? src = SourceRectangle;

		if (src != null) {
			src = new Rectangle(0, src.Value.Y, src.Value.Width, src.Value.Height);
		}

		Sprite.YSortOffset = Utils.GetYSortOffset(Texture, src);
	}

	public void DrawPreviewAt(Vector2 worldPos, bool canDraw) {
		Vector2 pos = new Vector2(Utils.Round(worldPos.X), Utils.Round(worldPos.Y));
		if (this is AutoTile auto) {
			auto.UpdateTexture();
		}
		Color tint = canDraw ? Color.CornflowerBlue * 0.4f : Color.Red * 0.4f;
		Game.SpriteBatch.Draw(
			Sprite.Texture,
			pos - AnchorTile.ToVector2() * GameScene.Active.Model.Level.TileSize,
			SourceRectangle,
			tint,
			Sprite.Rotation,
			Sprite.Origin,
			Sprite.Scale,
			Sprite.Flip,
			0.0f
		);
	}
}
