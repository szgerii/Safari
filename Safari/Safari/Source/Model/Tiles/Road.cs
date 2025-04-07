using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Model.Tiles;

/// <summary>
/// Tile for representing a single road cell
/// </summary>
public class Road : AutoTile {
	public Road() : base(Game.ContentManager.Load<Texture2D>("Assets/Road/Road")) {
		// set up custom bitmask to offset layout
		Layout[0] = new Point(5, 0);

		Layout[AutoTileBitmask.Top | AutoTileBitmask.Bottom] = new Point(0, 0);
		Layout[AutoTileBitmask.Top] = new Point(0, 0);
		Layout[AutoTileBitmask.Bottom] = new Point(0, 0);
		Layout[AutoTileBitmask.Left | AutoTileBitmask.Right] = new Point(0, 1);
		Layout[AutoTileBitmask.Left] = new Point(0, 1);
		Layout[AutoTileBitmask.Right] = new Point(0, 1);

		Layout[AutoTileBitmask.Bottom | AutoTileBitmask.Right] = new Point(1, 0);
		Layout[AutoTileBitmask.Top | AutoTileBitmask.Right] = new Point(1, 1);
		Layout[AutoTileBitmask.Bottom | AutoTileBitmask.Left] = new Point(2, 0);
		Layout[AutoTileBitmask.Top | AutoTileBitmask.Left] = new Point(2, 1);

		Layout[AutoTileBitmask.Bottom | AutoTileBitmask.Left | AutoTileBitmask.Right] = new Point(3, 0);
		Layout[AutoTileBitmask.Top | AutoTileBitmask.Left | AutoTileBitmask.Right] = new Point(3, 1);
		Layout[AutoTileBitmask.Top | AutoTileBitmask.Bottom | AutoTileBitmask.Left] = new Point(4, 0);
		Layout[AutoTileBitmask.Top | AutoTileBitmask.Bottom | AutoTileBitmask.Right] = new Point(4, 1);

		Layout[AutoTileBitmaskHelper.Straights] = new Point(5, 0);
		LightRange = 0;
	}
}
