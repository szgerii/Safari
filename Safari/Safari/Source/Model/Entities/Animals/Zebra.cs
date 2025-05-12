using Engine.Helpers;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Safari.Model.Entities.Animals;

/// <summary>
/// A class representing zebras
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Zebra : Animal {
	[JsonConstructor]
	public Zebra() : base() {
		InitSprite();
	}

	public Zebra(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Zebra, gender) {
		DisplayName = "Zebra";

		InitSprite();
	}

	private void InitSprite() {
		Sprite!.Texture = Game.LoadTexture("Assets/Animals/Zebra");
		Sprite.YSortOffset = 96;
		Sprite.Scale = 2 / 3f;

		if (collisionCmp != null) {
			Vectangle baseColl = new(5, 83, 78, 15);
			collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
		}
	}
}
