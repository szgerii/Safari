using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class Zebra : Animal {
	public Zebra(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Zebra, gender) {
		DisplayName = "Zebra";

		sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/Zebra");
		sprite.SourceRectangle = new Rectangle(0, 0, 32, 32);
		sprite.YSortOffset = 32;
	}
}
