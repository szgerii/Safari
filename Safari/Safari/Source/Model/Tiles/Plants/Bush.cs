using Engine.Components;
using Engine.Collision;
using Microsoft.Xna.Framework;
using Engine.Helpers;
using Newtonsoft.Json;

namespace Safari.Model.Tiles;

/// <summary>
/// Tile for representing bushes, which are food sources
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Bush : Tile {
	[JsonConstructor]
	public Bush() : base(Game.LoadTexture("Assets/Bush/Bush1")) {
		IsFoodSource = true;
		LightRange = 1;
		Sprite.LayerDepth = 0.4f;

		CollisionCmp collCmp = new CollisionCmp(new Vectangle(1, 16, 28, 14)) {
			Tags = CollisionTags.World
		};
		Attach(collCmp);
	}
}

/// <summary>
/// Tile for representing wide bushes (2x1), which are food sources
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class WideBush : Tile {
	private static readonly Point[] BushOffsets = [
		new(1, 0)
	];

	[JsonConstructor]
	public WideBush() : base(Game.LoadTexture("Assets/Bush/Bush2")) {
		ConstructionBlockOffsets = BushOffsets;
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
