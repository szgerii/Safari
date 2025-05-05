using Engine.Helpers;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Safari.Model.Entities.Animals;

[JsonObject(MemberSerialization.OptIn)]
public class Tiger : Animal {
	[JsonConstructor]
	public Tiger() : base() {
		InitSprite();
	}

	public Tiger(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Tiger, gender) {
		DisplayName = "Tiger";

		InitSprite();
	}

	private void InitSprite() {
		Sprite.Texture = Game.LoadTexture("Assets/Animals/Tiger");
		Sprite.YSortOffset = 64;
		Sprite.Scale = 0.75f;

		Vectangle baseColl = new(5, 43, 78, 20);
		collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
	}
}
