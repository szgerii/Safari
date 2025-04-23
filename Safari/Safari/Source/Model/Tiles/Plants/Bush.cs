using Engine.Components;
using Engine.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Engine.Helpers;

namespace Safari.Model.Tiles;

/// <summary>
/// Tile for representing
/// </summary>
public class Bush : Tile {
	public Bush() : base(Game.ContentManager.Load<Texture2D>("Assets/Bush/Bush1")) {
		IsFoodSource = true;
		LightRange = 1;
		Sprite.LayerDepth = 0.4f;

		CollisionCmp collCmp = new CollisionCmp(new Vectangle(1, 16, 28, 14)) {
			Tags = CollisionTags.World
		};
		Attach(collCmp);
	}
}

public class WideBush : Tile {
	public WideBush() : base(Game.ContentManager.Load<Texture2D>("Assets/Bush/Bush2")) {
		IsFoodSource = true;
		LightRange = 2;
		Size = new Point(2, 1);
		Sprite.LayerDepth = 0.4f;

		CollisionCmp collCmp = new CollisionCmp(new Vectangle(3, 16, 58, 16)) {
			Tags = CollisionTags.World
		};
		Attach(collCmp);
	}
}
