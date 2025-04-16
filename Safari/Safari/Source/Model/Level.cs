using Engine;
using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Debug;
using Safari.Input;
using Safari.Model.Tiles;
using System;
using System.Collections.Generic;

namespace Safari.Model;

/// <summary>
/// Stores the static parts of the game world
/// </summary>
public class Level : GameObject {
	public const int PLAY_AREA_CUTOFF_X = 8;
	public const int PLAY_AREA_CUTOFF_Y = 8;

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

	public Rectangle PlayAreaBounds => new(new Point(PLAY_AREA_CUTOFF_X * TileSize, PLAY_AREA_CUTOFF_Y * TileSize), new Point((MapWidth - (2 * PLAY_AREA_CUTOFF_X)) * TileSize, (MapHeight - (2 * PLAY_AREA_CUTOFF_X)) * TileSize));

	/// <summary>
	/// The network managing the roads and paths in this level
	/// </summary>
	public RoadNetwork Network { get; init; }
	/// <summary>
	/// The object responsible for light sources and lighting in general
	/// </summary>
	public LightManager LightManager { get; init; }

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
		// lightmanager setup (before any tiles are placed)
		LightManager = new LightManager(width, height, tileSize);
		// Roadmanger setup
		Point start = new Point(PLAY_AREA_CUTOFF_X, height - PLAY_AREA_CUTOFF_Y - 8);
		Point end = new Point(width - PLAY_AREA_CUTOFF_X - 8, PLAY_AREA_CUTOFF_Y);
		Network = new RoadNetwork(width, height, start, end);
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
	/// Returns a list of tiles that are inside the given tilemap area
	/// </summary>
	/// <param name="worldArea">The bounds of the tilemap area to return from</param>
	/// <returns>The list of tile</returns>
	public List<Tile> GetTilesInArea(Rectangle tilemapArea) {
		List<Tile> tiles = new();

		for (int x = tilemapArea.X; x < tilemapArea.Right; x++) {
			for (int y = tilemapArea.Y; y < tilemapArea.Bottom; y++) {
				if (IsOutOfPlayArea(x, y)) continue;

				Tile tile = GetTile(x, y);

				if (tile == null) continue;

				tiles.Add(tile);
			}
		}

		return tiles;
	}

	/// <summary>
	/// Returns a list of tiles that are inside the given world area
	/// </summary>
	/// <param name="worldArea">The bounds of the world area to return from</param>
	/// <returns>The list of tile</returns>
	public List<Tile> GetTilesInWorldArea(Rectangle worldArea) {
		Rectangle tilemapArea = new Rectangle(worldArea.Location / new Point(TileSize), worldArea.Size / new Point(TileSize));

		return GetTilesInArea(tilemapArea);
	}

	public Vector2 GetTileCenter(Point p) => new Vector2(p.X * TileSize + TileSize / 2.0f, p.Y * TileSize + TileSize / 2.0f);

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
		if (tile is Road && !IsOutOfPlayArea(x, y)) {
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

	/// <summary>
	/// Checks if a given tilemap position falls outside of the playable area
	/// </summary>
	/// <param name="x">The x coordinate of the position</param>
	/// <param name="y">The y coordinate of the position</param>
	/// <returns>Whether the position is considered not playable</returns>
	public bool IsOutOfPlayArea(int x, int y) {
		return x < PLAY_AREA_CUTOFF_X || y < PLAY_AREA_CUTOFF_Y || x >= MapWidth - PLAY_AREA_CUTOFF_X || y >= MapHeight - PLAY_AREA_CUTOFF_Y;
	}

	private Random rand = new();
	/// <summary>
	/// Returns a random (tile) position from the level
	/// </summary>
	/// <param name="playAreaOnly">Exclude values from outside of the playing area</param>
	/// <returns>The random position</returns>
	public Vector2 GetRandomPosition(bool playAreaOnly = true) {
		int minX, maxX, minY, maxY;
		
		if (playAreaOnly) {
			minX = PLAY_AREA_CUTOFF_X * TileSize;
			maxX = (MapWidth - PLAY_AREA_CUTOFF_X - 1) * TileSize;
			minY = PLAY_AREA_CUTOFF_Y * TileSize;
			maxY = (MapHeight - PLAY_AREA_CUTOFF_Y - 1) * TileSize;
		} else {
			minX = 0; minY = 0;
			maxX = (MapWidth - 1) * TileSize;
			maxY = (MapHeight - 1) * TileSize;
		}

		return new Vector2(
			rand.Next(minX, maxX),
			rand.Next(minY, maxY)
		);
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
	private readonly Type[] brushes = [typeof(Road), typeof(Grass), typeof(Water), typeof(Tree), typeof(Fence), typeof(Bush), typeof(WideBush)];
	private int brushIndex = 0;
	public override void Update(GameTime gameTime) {
		if (InputManager.Keyboard.JustPressed(Microsoft.Xna.Framework.Input.Keys.N)) {
			brushIndex = Math.Max(0, brushIndex - 1);
		}
		if (InputManager.Keyboard.JustPressed(Microsoft.Xna.Framework.Input.Keys.M)) {
			brushIndex = Math.Min(brushes.Length - 1, brushIndex + 1);
		}
		DebugInfoManager.AddInfo("current brush", brushes[brushIndex].Name, DebugInfoPosition.BottomRight);

		Vector2 mouseWorldPos = InputManager.Mouse.GetWorldPos();
		DebugInfoManager.AddInfo("mouse pos", Utils.Format(mouseWorldPos, false, false), DebugInfoPosition.BottomRight);

		Vector2 mouseTilePos = GetMouseTilePos();
		
		if (!IsOutOfPlayArea((int)mouseTilePos.X / TileSize, (int)mouseTilePos.Y / TileSize)) {
			Tile targetTile = GetTile((mouseTilePos / TileSize).ToPoint());

			bool alreadyPainted = targetTile != null && targetTile.GetType() == brushes[brushIndex];
			if (InputManager.Mouse.IsDown(MouseButtons.LeftButton) && !alreadyPainted) {
				Tile tile;

				if (brushes[brushIndex] == typeof(Tree)) {
					tile = (Tree)Activator.CreateInstance(brushes[brushIndex], [TreeType.Digitata]);
				} else {
					tile = (Tile)Activator.CreateInstance(brushes[brushIndex]);
				}

				SetTile((mouseTilePos / TileSize).ToPoint(), tile);
			}

			bool alreadyEmpty = targetTile == null;
			if (InputManager.Mouse.IsDown(MouseButtons.RightButton) && !alreadyEmpty) {
				ClearTile((int)mouseTilePos.X / TileSize, (int)mouseTilePos.Y / TileSize);
			}
		}

		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime) {
		if (Background != null) {
			Game.SpriteBatch.Draw(Background, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
		}

		// TODO: remove once not needed
		Vector2 mousePosWorld = GetMouseTilePos();
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

	private Vector2 GetMouseTilePos() {
		Vector2 mouseTilePos = InputManager.Mouse.GetWorldPos();
		mouseTilePos.X -= mouseTilePos.X % TileSize;
		mouseTilePos.Y -= mouseTilePos.Y % TileSize;

		return mouseTilePos;
	}
}
