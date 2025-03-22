using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class Tiger : Animal {
	public Tiger(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Tiger, gender) {
		DisplayName = "Tiger";

		sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/Tiger");
		sprite.SourceRectangle = new Rectangle(0, 0, 97, 64);
		sprite.YSortOffset = 64;
		sprite.Scale = 0.75f;
	}
}
