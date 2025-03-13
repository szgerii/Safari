using Microsoft.Xna.Framework;
using Safari.Objects.Entities.Animals;

namespace Safari.Objects.Entities;

public class Poacher : Entity {
	private Animal? caughtAnimal = null!;
	
	public Poacher(Vector2 pos) : base(pos) {
		displayName = "Poacher";
	}
}

