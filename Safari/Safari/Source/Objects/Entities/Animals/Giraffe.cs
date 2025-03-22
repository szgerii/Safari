using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class Giraffe : Animal {
	public Giraffe(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Giraffe, gender) {
		DisplayName = "Giraffe";

		sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/Giraffe");
		sprite.SourceRectangle = new Rectangle(0, 0, 82, 128);
		sprite.YSortOffset = 128;
		sprite.Scale = 0.75f;

		ReachDistance = 2;
		SightDistance = 6;
	}
}
