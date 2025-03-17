using System;

namespace Safari.Objects.Entities.Animals;

public enum AnimalSpecies {
	Zebra,
	Elephant,
	Giraffe,
	Lion,
	Tiger,
	TigerWhite
}

public static class AnimalSpeciesExtensions {
	public static Type GetAnimalType(this AnimalSpecies species) {
		return species switch {
			AnimalSpecies.Zebra => typeof(Zebra),
			AnimalSpecies.Elephant => typeof(Elephant),
			AnimalSpecies.Giraffe => typeof(Giraffe),
			AnimalSpecies.Lion => typeof(Lion),
			AnimalSpecies.Tiger => typeof(Tiger),
			AnimalSpecies.TigerWhite => typeof(TigerWhite),
			_ => null
		};
	}

	public static bool IsCarnivorous(this AnimalSpecies species) {
		return species switch {
			AnimalSpecies.Zebra or AnimalSpecies.Elephant or AnimalSpecies.Giraffe => false,
			AnimalSpecies.Lion or AnimalSpecies.Tiger or AnimalSpecies.TigerWhite => true,
			_ => false,
		};
	}

	public static int GetPrice(this AnimalSpecies species) {
		return species switch {
			AnimalSpecies.Zebra => 100,
			AnimalSpecies.Elephant => 100,
			AnimalSpecies.Giraffe => 100,
			AnimalSpecies.Lion => 100,
			AnimalSpecies.Tiger => 100,
			AnimalSpecies.TigerWhite => 100,
			_ => 0,
		};
	}
}