using Engine;
using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Debug;
using Safari.Input;
using Safari.Model.Tiles;
using Safari.Scenes;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

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

	/// <summary>
	/// The network managing the roads and paths in this level
	/// </summary>
	public RoadNetwork Network { get; init; }
	public LightManager LightManager { get; init; }

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

		LightManager = new LightManager(width, height, tileSize);

		// start and end locations are not final
		Point start = new Point(0, height / 2);
		Point end = new Point(width / 2, 0);
		Network = new RoadNetwork(width, height, start, end);
		SetTile(start, new Road());
		SetTile(end, new Road());
		Point current = new Point(start.X, start.Y);
		while (current.X < end.X) {
			current.X++;
			SetTile(current, new Road());
		}
		while (current.Y > end.Y) {
			current.Y--;
			SetTile(current, new Road());
		}
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

	public List<Tile> GetTilesInArea(Rectangle tilemapArea) {
		List<Tile> tiles = new();

		for (int x = tilemapArea.X; x < tilemapArea.Right; x++) {
			for (int y = tilemapArea.Y; y < tilemapArea.Bottom; y++) {
				if (IsOutOfBounds(x, y)) continue;

				Tile tile = GetTile(x, y);

				if (tile == null) continue;

				tiles.Add(tile);
			}
		}

		return tiles;
	}

	public List<Tile> GetTilesInWorldArea(Rectangle worldArea) {
		Rectangle tilemapArea = new Rectangle(worldArea.Location / new Point(TileSize), worldArea.Size / new Point(TileSize));

		return GetTilesInArea(tilemapArea);
	}

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
		if (tile is Road) {
			Network.AddRoad(x, y);
		}
		tiles[x, y] = tile;
		if (tile.LightRange >= 0) {
			LightManager.AddLightSource(x, y, tile.LightRange);
		}

		if (!tile.Loaded) {
			Game.AddObject(tile);
		}

		UpdateAutoTilesAround(new Point(x, y));
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
		if (t is Road) {
			Network.ClearRoad(x, y);
		}
		if (t.LightRange >= 0) {
			LightManager.RemoveLightSource(x, y, t.LightRange);
		}

		tiles[x, y] = null;

		Game.RemoveObject(t);

		UpdateAutoTilesAround(new Point(x, y));
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

	// TODO: remove this whole tile placement part once not needed
	private readonly Type[] brushes = [typeof(Road), typeof(Grass), typeof(Water), typeof(Tree)];
	private int brushIndex = 0;
	public override void Update(GameTime gameTime) {
		if (InputManager.Keyboard.JustPressed(Microsoft.Xna.Framework.Input.Keys.N)) {
			brushIndex = Math.Max(0, brushIndex - 1);
		}
		if (InputManager.Keyboard.JustPressed(Microsoft.Xna.Framework.Input.Keys.M)) {
			brushIndex = Math.Min(brushes.Length - 1, brushIndex + 1);
		}
		DebugInfoManager.AddInfo("current brush", brushes[brushIndex].Name, DebugInfoPosition.BottomRight);

		Vector2 mousePosWorld = InputManager.Mouse.GetWorldPos();

		if (!IsOutOfBounds((int)mousePosWorld.X / TileSize, (int)mousePosWorld.Y / TileSize)) {
			Tile targetTile = GetTile((mousePosWorld / TileSize).ToPoint());

			bool alreadyPainted = targetTile != null && targetTile.GetType() == brushes[brushIndex];
			if (InputManager.Mouse.IsDown(MouseButtons.LeftButton) && !alreadyPainted) {
				Tile tile;

				if (brushes[brushIndex] == typeof(Tree)) {
					tile = (Tree)Activator.CreateInstance(brushes[brushIndex], [TreeType.Digitata]);
				} else {
					tile = (Tile)Activator.CreateInstance(brushes[brushIndex]);
				}

				SetTile((mousePosWorld / TileSize).ToPoint(), tile);
			}

			bool alreadyEmpty = targetTile == null;
			if (InputManager.Mouse.IsDown(MouseButtons.RightButton) && !alreadyEmpty) {
				ClearTile((int)mousePosWorld.X / TileSize, (int)mousePosWorld.Y / TileSize);
			}
		}

		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime) {
		if (Background != null) {
			Game.SpriteBatch.Draw(Background, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
		}

		// TODO: remove once not needed
		Vector2 mousePosWorld = InputManager.Mouse.GetWorldPos();
		Game.SpriteBatch.Draw(selectedTileTex, mousePosWorld, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

		base.Draw(gameTime);
	}

	public void PostDraw(object _, GameTime gameTime) {
		Game.SpriteBatch.Draw(debugGridTex, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0f);
	}

	/// <summary>
	/// Forces all of the autotiles around a given position to refresh
	/// </summary>
	/// <param name="pos">The tilemap position to refresh around</param>
	public void UpdateAutoTilesAround(Point pos) {
		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				if (i == 0 && j == 0) continue;

				Point destPos = pos + new Point(i, j);

				if (IsOutOfBounds(destPos.X, destPos.Y)) continue;

				if (GetTile(destPos.X, destPos.Y) is AutoTile at) {
					at.NeedsUpdate = true;
				}
			}
		}
	}
}
