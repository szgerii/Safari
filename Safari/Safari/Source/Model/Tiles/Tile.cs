using Engine;
using Engine.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Scenes;

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
	/// The game world position of the tile's center point
	/// </summary>
	public Point CenterPosition => new Point((int)Position.X + Texture.Width / 2, (int)Position.Y + Texture.Height / 2);

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
	/// Determines how much light is emitted from the tile
	/// </summary>
	public float Light { get; set; }

	/// <summary>
	/// Determines how much light accumulates on the tile
	/// (from own and neighboring light sources)
	/// </summary>
	public float Visibility { get; private set; } = 1f;

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
}
