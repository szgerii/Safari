using Engine.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class Elephant : Animal {
	public Elephant(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Elephant, gender) {
		DisplayName = "Elephant";

		sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/Elephant");
		sprite.YSortOffset = 96;
		sprite.Scale = 0.75f;

		Collider baseColl = new(5, 73, 85, 24);
		collisionCmp.Collider = baseColl.WithSpriteScale(sprite.Scale);

		ReachDistance = 2;
	}
}
