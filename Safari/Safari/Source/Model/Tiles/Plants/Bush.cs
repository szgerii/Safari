using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Model.Tiles;

/// <summary>
/// Tile for representing
/// </summary>
public class Bush : Tile {
	public Bush() : base(Game.ContentManager.Load<Texture2D>("Assets/Bush/Bush1")) {
		IsFoodSource = true;
	}
}

public class WideBush : Tile {
	public WideBush() : base(Game.ContentManager.Load<Texture2D>("Assets/Bush/Bush2")) {
		IsFoodSource = true;

		Size = new Point(2, 1);
	}
}
