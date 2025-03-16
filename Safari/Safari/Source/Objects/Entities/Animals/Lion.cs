using Microsoft.Xna.Framework;

namespace Safari.Objects.Entities.Animals;

public class Lion : Animal {
	public Lion(Vector2 pos) : base(pos) {
		displayName = "Lion";
		species = AnimalSpecies.Lion;
	}
}
