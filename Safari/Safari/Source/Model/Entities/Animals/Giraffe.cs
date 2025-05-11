using Engine.Helpers;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
namespace Safari.Model.Entities.Animals;

/// <summary>
/// A class representing giraffes
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class Giraffe : Animal {
	[JsonConstructor]
	public Giraffe() : base() {
		InitSprite();
	}
	
	public Giraffe(Vector2 pos, Gender gender) : base(pos, AnimalSpecies.Giraffe, gender) {
		DisplayName = "Giraffe";

		InitSprite();

		ReachDistance = 4;
		SightDistance = 7;
	}

	private void InitSprite() {
		Sprite.Texture = Game.LoadTexture("Assets/Animals/Giraffe");
		Sprite.Scale = 0.75f;
		Sprite.YSortOffset = 128;

		Vectangle baseColl = new(7, 107, 81, 20);
		collisionCmp.Collider = baseColl.WithSpriteScale(Sprite.Scale);
	}
}
