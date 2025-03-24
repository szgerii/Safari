using Engine.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class Elephant : Animal {
	public Elephant(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Elephant, gender) {
		DisplayName = "Elephant";

		Sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/Elephant");
		Sprite.YSortOffset = 96;
		Sprite.Scale = 0.75f;

		Collider baseColl = new(5, 73, 85, 24);
		collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);

		ReachDistance = 2;
	}
}
