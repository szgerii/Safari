using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class TigerWhite : Animal {
	public TigerWhite(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.TigerWhite, gender) {
		DisplayName = "White Tiger";

		sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/TigerWhite");
		sprite.SourceRectangle = new Rectangle(0, 0, 32, 32);
		sprite.YSortOffset = 32;
	}
}
