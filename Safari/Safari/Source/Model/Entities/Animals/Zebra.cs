using Engine.Helpers;
using Microsoft.Xna.Framework;

namespace Safari.Model.Entities.Animals;

public class Zebra : Animal {
	public Zebra(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Zebra, gender) {
		DisplayName = "Zebra";

		Sprite.Texture = Game.LoadTexture("Assets/Animals/Zebra");
		Sprite.YSortOffset = 96;
		Sprite.Scale = 2 / 3f;

		Vectangle baseColl = new(5, 83, 78, 15);
		collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
	}
}
