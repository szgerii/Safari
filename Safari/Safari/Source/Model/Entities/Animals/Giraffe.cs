using Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Model.Entities.Animals;

public class Giraffe : Animal {
	public Giraffe(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Giraffe, gender) {
		DisplayName = "Giraffe";

		Sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/Giraffe");
		Sprite.Scale = 0.75f;
		Sprite.YSortOffset = 128;

		Vectangle baseColl = new(7, 107, 81, 20);
		collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);

		ReachDistance = 4;
		SightDistance = 7;
	}
}
