using Engine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Objects.Entities.Animals;

public class TigerWhite : Animal {
	public TigerWhite(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.TigerWhite, gender) {
		DisplayName = "White Tiger";

		Sprite.Texture = Game.ContentManager.Load<Texture2D>("Assets/Animals/TigerWhite");
		Sprite.YSortOffset = 64;
		Sprite.Scale = 0.75f;

		Vectangle baseColl = new(5, 43, 78, 20);
		collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
	}
}
