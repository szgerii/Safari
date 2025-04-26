using Engine.Collision;
using Engine.Components;
using Engine.Helpers;
using Microsoft.Xna.Framework;

namespace Safari.Model.Tiles;

public class Fence : AutoTile {
	public Fence() : base(Game.LoadTexture("Assets/Fence/Fence")) {
		Layout[0] = new Point(0, 0);

		Layout[AutoTileBitmask.Top | AutoTileBitmask.Bottom] = new Point(2, 1);
		Layout[AutoTileBitmask.Top] = new Point(2, 2);
		Layout[AutoTileBitmask.Bottom] = new Point(2, 0);
		Layout[AutoTileBitmask.Left | AutoTileBitmask.Right] = new Point(1, 3);
		Layout[AutoTileBitmask.Left] = new Point(2, 3);
		Layout[AutoTileBitmask.Right] = new Point(0, 3);

		Layout[AutoTileBitmask.Bottom | AutoTileBitmask.Right] = new Point(0, 4);
		Layout[AutoTileBitmask.Top | AutoTileBitmask.Right] = new Point(0, 6);
		Layout[AutoTileBitmask.Bottom | AutoTileBitmask.Left] = new Point(2, 4);
		Layout[AutoTileBitmask.Top | AutoTileBitmask.Left] = new Point(2, 6);

		Layout[AutoTileBitmask.Bottom | AutoTileBitmask.Left | AutoTileBitmask.Right] = new Point(1, 4);
		Layout[AutoTileBitmask.Top | AutoTileBitmask.Left | AutoTileBitmask.Right] = new Point(1, 6);
		Layout[AutoTileBitmask.Top | AutoTileBitmask.Bottom | AutoTileBitmask.Left] = new Point(2, 5);
		Layout[AutoTileBitmask.Top | AutoTileBitmask.Bottom | AutoTileBitmask.Right] = new Point(0, 5);

		Layout[AutoTileBitmaskHelper.Straights] = new Point(1, 5);
		LightRange = -1;

		CollisionCmp collCmp = new CollisionCmp(new Vectangle(0, 0, 32, 32)) {
			Tags = CollisionTags.World
		};
		Attach(collCmp);
	}
}