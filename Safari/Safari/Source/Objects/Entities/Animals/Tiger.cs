using Microsoft.Xna.Framework;

namespace Safari.Objects.Entities.Animals;

public class Tiger : Animal {
	public Tiger(Vector2 pos) : base(pos) {
		displayName = "Tiger";
		species = AnimalSpecies.Tiger;
	}
}
