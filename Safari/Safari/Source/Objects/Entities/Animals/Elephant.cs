using Microsoft.Xna.Framework;

namespace Safari.Objects.Entities.Animals;

public class Elephant : Animal {
	public Elephant(Vector2 pos) : base(pos) {
		displayName = "Elephant";
		species = AnimalSpecies.Elephant;
	}
}
