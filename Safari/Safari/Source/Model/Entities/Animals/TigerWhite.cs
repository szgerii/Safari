using Engine.Helpers;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Safari.Model.Entities.Animals;

/// <summary>
/// A class representing white tigers
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class TigerWhite : Animal {
	[JsonConstructor]
	public TigerWhite() : base() {
		InitSprite();
	}
	
	public TigerWhite(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.TigerWhite, gender) {
		DisplayName = "White Tiger";
		SightDistance = 8;

		InitSprite();
	}

	private void InitSprite() {
		Sprite!.Texture = Game.LoadTexture("Assets/Animals/TigerWhite");
		Sprite.YSortOffset = 64;
		Sprite.Scale = 0.75f;

		if (collisionCmp != null) {
			Vectangle baseColl = new(5, 43, 78, 20);
			collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
		}
	}
}
