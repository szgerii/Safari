using Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class Lion : Animal {
	public Lion(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Lion, gender) {
		DisplayName = "Lion";

		Sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/Lion");
		Sprite.YSortOffset = 96;
		Sprite.Scale = 2 / 3f;

		Vectangle baseColl = new(12, 75, 80, 20);
		collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
	}
}
