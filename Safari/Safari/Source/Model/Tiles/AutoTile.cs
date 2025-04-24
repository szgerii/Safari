using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Model.Tiles;

/// <summary>
/// A tile that automatically adjusts its texture based on its neighbors
/// </summary>
public partial class AutoTile : Tile {
	/// <summary>
	/// A dictionary mapping the possible bitmask values to their offsets inside the atlas texture
	/// </summary>
	public Dictionary<AutoTileBitmask, Point> Layout { get; set; } = new();
	/// <summary>
	/// The size of the tiles inside the atlas texture
	/// <br/>
	/// If not set manually, will automatically take up the tile size of the current level
	/// </summary>
	public int TileSize { get; set; } = -1;
	/// <summary>
	/// Whether the AutoTile supports diagonal bitmasks
	/// </summary>
	public bool HasDiagonalTiling { get; set; } = false;
	/// <summary>
	/// Whether the current texture should be recalculated on the next frame
	/// </summary>
	public bool NeedsUpdate = true;

	public AutoTile(Texture2D atlasTex) : base() {
		Texture = atlasTex;
	}

	public override void Update(GameTime gameTime) {
		if (NeedsUpdate) {
			UpdateTexture();
		}

		base.Update(gameTime);
	}

	/// <summary>
	/// Updates the source rectangle to match the current neighbor state <br/>
	/// NOTE: this should only be used if you need the new texture immediately (e.g. construction preview).
	/// For most use cases, set the <see cref="NeedsUpdate"/> prop to true to defer updating to the next frame
	/// </summary>
	public void UpdateTexture() {
		NeedsUpdate = false;

		if (TileSize == -1) {
			TileSize = GameScene.Active.Model.Level.TileSize;
		}

		AutoTileBitmask result = 0;
		foreach (AutoTileBitmask dir in Enum.GetValues(typeof(AutoTileBitmask))) {
			if ((dir & AutoTileBitmaskHelper.Diagonals) != 0) {
				if (!HasDiagonalTiling)
					continue;

				bool hasTop = HasNeighborAtOffset(AutoTileBitmask.Top.ToOffset());
				bool hasBottom = HasNeighborAtOffset(AutoTileBitmask.Bottom.ToOffset());
				bool hasLeft = HasNeighborAtOffset(AutoTileBitmask.Left.ToOffset());
				bool hasRight = HasNeighborAtOffset(AutoTileBitmask.Right.ToOffset());

				if (dir == AutoTileBitmask.TopLeft && !(hasTop && hasLeft))
					continue;
				if (dir == AutoTileBitmask.TopRight && !(hasTop && hasRight))
					continue;
				if (dir == AutoTileBitmask.BottomLeft && !(hasBottom && hasLeft))
					continue;
				if (dir == AutoTileBitmask.BottomRight && !(hasBottom && hasRight))
					continue;
			}

			bool hasNeighbor = HasNeighborAtOffset(dir.ToOffset());

			if (hasNeighbor) {
				result |= dir;
			}
		}

		bool success = Layout.TryGetValue(result, out Point atlasPos)
					|| Layout.TryGetValue(0, out atlasPos);

		if (!success) {
			return;
		}

		atlasPos *= new Point(TileSize);
		Rectangle newSrcRect = new Rectangle(atlasPos, new Point(TileSize));
		SourceRectangle = newSrcRect;
	}

	/// <summary>
	/// Checks if the tile has a neighboring autotile of the same type at a given offset
	/// </summary>
	/// <param name="offset">The offset to check</param>
	/// <returns>Whether there's a neighbor at the offset</returns>
	private bool HasNeighborAtOffset(Point offset) {
		Point destPos = TilemapPosition + offset;
		Level currentLevel = GameScene.Active.Model.Level;

		if (currentLevel.IsOutOfBounds(destPos.X, destPos.Y)) {
			return false;
		}

		Tile target = currentLevel.GetTile(destPos.X, destPos.Y);

		return target != null && target.GetType() == GetType();
	}

	public override void UpdateYSortOffset() {
		Sprite.YSortOffset = Utils.GetYSortOffset(Texture, SourceRectangle) - 1;
	}
}
