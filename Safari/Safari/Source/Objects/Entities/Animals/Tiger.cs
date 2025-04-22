using Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class Tiger : Animal {
	public Tiger(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Tiger, gender) {
		DisplayName = "Tiger";

		Sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/Tiger");
		Sprite.YSortOffset = 64;
		Sprite.Scale = 0.75f;

		Vectangle baseColl = new(5, 43, 78, 20);
		collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
	}
}
