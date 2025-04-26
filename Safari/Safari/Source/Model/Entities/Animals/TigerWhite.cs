using Engine.Helpers;
using Microsoft.Xna.Framework;

namespace Safari.Model.Entities.Animals;

public class TigerWhite : Animal {
	public TigerWhite(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.TigerWhite, gender) {
		DisplayName = "White Tiger";

		Sprite.Texture = Game.LoadTexture("Assets/Animals/TigerWhite");
		Sprite.YSortOffset = 64;
		Sprite.Scale = 0.75f;

		Vectangle baseColl = new(5, 43, 78, 20);
		collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
	}
}
