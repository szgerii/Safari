using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class Lion : Animal {
	public Lion(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Lion, gender) {
		DisplayName = "Lion";

		sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/Lion");
		sprite.SourceRectangle = new Rectangle(0, 0, 32, 32);
		sprite.YSortOffset = 32;
	}
}
