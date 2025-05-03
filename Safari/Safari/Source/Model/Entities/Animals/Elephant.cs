using Engine.Helpers;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Safari.Model.Entities.Animals;

[JsonObject(MemberSerialization.OptIn)]
public class Elephant : Animal {
	[JsonConstructor]
	public Elephant() : base() {
		InitSprite();
	}
	
	public Elephant(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Elephant, gender) {
		DisplayName = "Elephant";

		InitSprite();
	}

	private void InitSprite() {
		Sprite.Texture = Game.LoadTexture("Assets/Animals/Elephant");
		Sprite.YSortOffset = 96;
		Sprite.Scale = 0.75f;

		Vectangle baseColl = new(5, 73, 85, 24);
		collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
	}
}
