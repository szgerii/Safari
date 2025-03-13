using Microsoft.Xna.Framework;

namespace Safari.Objects.Entities.Animals;

public class Zebra : Animal {
	public Zebra(Vector2 pos) : base(pos) {
		displayName = "Zebra";
		species = AnimalSpecies.Zebra;
	}
}
