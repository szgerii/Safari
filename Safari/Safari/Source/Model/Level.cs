using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Model.Tiles;
using System;

namespace Safari.Model;

/// <summary>
/// Stores the static parts of the game world
/// </summary>
public class Level : GameObject {
	/// <summary>
	/// The image to draw as a background to the tiles
	/// </summary>
	public Texture2D Background { get; set; }
	/// <summary>
	/// The dimension of a single cell inside the tilemap grid
	/// </summary>
	public int TileSize { get; init; }
	/// <summary>
	/// The number of cells that make up a row in the tilemap grid
	/// </summary>
	public int MapWidth { get; init; }
	/// <summary>
	/// The number of cells that make up a column in the tilemap grid
	/// </summary>
	public int MapHeight { get; init; }

	private readonly Tile[,] tiles;

	private readonly Texture2D debugGridTex;
	private readonly Texture2D selectedTileTex;

	public Level(int tileSize, int width, int height, Texture2D background) : base(Vector2.Zero) {
		TileSize = tileSize;
		MapWidth = width;
		MapHeight = height;
		tiles = new Tile[width, height];
		Background = background;

		Texture2D gridCellTex = Utils.GenerateTexture(TileSize, TileSize, Color.Black, true);
		Texture2D[] mergeArray = new Texture2D[MapWidth * MapHeight];
		for (int i = 0; i < mergeArray.Length; i++) {
			mergeArray[i] = gridCellTex;
		}
		debugGridTex = Utils.CreateAtlas(mergeArray, MapWidth);

		Texture2D outline = Utils.GenerateTexture(TileSize, TileSize, new Color(0f, 0f, 1f, 1f), true);
		Texture2D fill = Utils.GenerateTexture(TileSize, TileSize, new Color(0.3f, 0.3f, 1f, 0.3f));
		selectedTileTex = Utils.MergeTextures(fill, outline);
	}

	/// <summary>
	/// Retrieves a tile based on its tilemap position
	/// </summary>
	/// <param name="x">The tilemap column the tile is in inside the grid</param>
	/// <param name="y">The tilemap row the tile is in inside the grid</param>
	/// <returns>The tile at the given position, or null if the cell's empty</returns>
	/// <exception cref="ArgumentException"></exception>
	public Tile GetTile(int x, int y) {
		if (IsOutOfBounds(x, y)) {
			throw new ArgumentException("Given tilemap position is outside the bounds of the level");
		}

		return tiles[x, y];
	}

	/// <summary>
	/// Retrieves a tile based on its tilemap position
	/// </summary>
	/// <param name="pos">The tilemap position the tile is in inside the grid</param>
	/// <returns>The tile at the given position, or null if the cell's empty</returns>
	/// <exception cref="ArgumentException"></exception>
	public Tile GetTile(Point pos) => GetTile(pos.X, pos.Y);

	/// <summary>
	/// Places or modifies a tile at a tilemap position
	/// </summary>
	/// <param name="x">The tilemap column to modify</param>
	/// <param name="y">The tilemap row to modify</param>
	/// <param name="tile">The tile to place at the given position</param>
	/// <exception cref="ArgumentException"></exception>
	public void SetTile(int x, int y, Tile tile) {
		if (IsOutOfBounds(x, y)) {
			throw new ArgumentException("Given tilemap position is outside the bounds of the level");
		}

		if (tiles[x, y] != null) {
			ClearTile(x, y);
		}

		tile.Position = new Vector2(x * TileSize, y * TileSize) - tile.AnchorTile.ToVector2() * TileSize;
		tiles[x, y] = tile;

		if (!tile.Loaded) {
			Game.AddObject(tile);
		}
	}

	/// <summary>
	/// Places or modifies a tile at a tilemap position
	/// </summary>
	/// <param name="pos">The tilemap position to modify at</param>
	/// <param name="tile">The tile to place at the given position</param>
	public void SetTile(Point pos, Tile tile) => SetTile(pos.X, pos.Y, tile);

	/// <summary>
	/// Clears out a given cell of the tilemap
	/// </summary>
	/// <param name="x">The x coordinate to clear at</param>
	/// <param name="y">The y coordinate to clear at</param>
	/// <exception cref="ArgumentException"></exception>
	public void ClearTile(int x, int y) {
		if (IsOutOfBounds(x, y)) {
			throw new ArgumentException("Given tilemap position is outside the bounds of the level");
		}

		if (tiles[x, y] == null) return;

		Tile t = tiles[x, y];
		tiles[x, y] = null;

		Game.RemoveObject(t);
	}

	/// <summary>
	/// Clears out a given cell of the tilemap
	/// </summary>
	/// <param name="pos">The position to clear at</param>
	public void ClearTile(Point pos) => ClearTile(pos.X, pos.Y);

	/// <summary>
	/// Checks if a given tilemap position falls outside of the tilemap grid
	/// </summary>
	/// <param name="x">The x coordinate of the position</param>
	/// <param name="y">The y coordinate of the position</param>
	/// <returns>Whether the position is considered out of bounds</returns>
	public bool IsOutOfBounds(int x, int y) {
		return x < 0 || y < 0 || x >= MapWidth || y >= MapHeight;
	}

	public override void Load() {
		foreach (Tile tile in tiles) {
			if (tile == null) continue;

			if (!tile.Loaded) {
				Game.AddObject(tile);
			}
		}

		base.Load();
	}

	public override void Unload() {
		foreach (Tile tile in tiles) {
			if (tile == null) continue;
			Game.RemoveObject(tile);
		}

		base.Unload();
	}

	public override void Draw(GameTime gameTime) {
		if (Background != null) {
			Game.SpriteBatch.Draw(Background, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
		}

		base.Draw(gameTime);
	}

	public void PostDraw(object _, GameTime gameTime) {
		Game.SpriteBatch.Draw(debugGridTex, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0f);
	}
}
