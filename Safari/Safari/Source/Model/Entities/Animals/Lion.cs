using Engine.Helpers;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Safari.Model.Entities.Animals;

/// <summary>
/// A class representing lions
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Lion : Animal {
	[JsonConstructor]
	public Lion() : base() {
		InitSprite();
	}

	public Lion(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Lion, gender) {
		DisplayName = "Lion";
		SightDistance = 8;

		InitSprite();
	}

	private void InitSprite() {
		Sprite!.Texture = Game.LoadTexture("Assets/Animals/Lion");
		Sprite.YSortOffset = 96;
		Sprite.Scale = 2 / 3f;

		if (collisionCmp != null) {
			Vectangle baseColl = new(12, 75, 80, 20);
			collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
		}
	}
}
