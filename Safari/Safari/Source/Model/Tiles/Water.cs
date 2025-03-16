using Microsoft.Xna.Framework.Graphics;

namespace Safari.Model.Tiles;

/// <summary>
/// Tile for representing a single water cell
/// </summary>
public class Water : AutoTile {
	public Water() : base(Game.ContentManager.Load<Texture2D>("Assets/Water/Water")) {
		IsWaterSource = true;

		HasDiagonalTiling = true;
		UseDefaultLayout();
	}
}
