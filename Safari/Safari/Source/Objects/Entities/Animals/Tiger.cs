using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class Tiger : Animal {
	public Tiger(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Tiger, gender) {
		DisplayName = "Tiger";

		sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/Tiger");
		sprite.SourceRectangle = new Rectangle(0, 0, 32, 32);
		sprite.YSortOffset = 32;
	}
}
