using System;
using System.Diagnostics.CodeAnalysis;

namespace Safari.Model.Entities.Animals;

/// <summary>
/// The different animal species found inside the park
/// </summary>
public enum AnimalSpecies {
	Zebra,
	Elephant,
	Giraffe,
	Lion,
	Tiger,
	TigerWhite
}

public static class AnimalSpeciesExtensions {
	/// <summary>
	/// Gets the proper Animal sub-class for a given species
	/// </summary>
	/// <param name="species">The species to use</param>
	/// <returns>The Animal sub-class Type belonging to the species</returns>
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

	/// <summary>
	/// Checks if a given species is carnivorous in their diet
	/// </summary>
	/// <param name="species">The species to check</param>
	/// <returns>Whether the species is carnivorous</returns>
	public static bool IsCarnivorous(this AnimalSpecies species) {
		return species switch {
			AnimalSpecies.Zebra or AnimalSpecies.Elephant or AnimalSpecies.Giraffe => false,
			AnimalSpecies.Lion or AnimalSpecies.Tiger or AnimalSpecies.TigerWhite => true,
			_ => false,
		};
	}

	/// <summary>
	/// Retrieves the base price for a given species
	/// (how much a new-born is worth from the species)
	/// </summary>
	/// <param name="species">The species to use</param>
	/// <returns>The base price of the species</returns>
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

	/// <summary>
	/// Returns the display name for the specified AnimalSpecies value
	/// </summary>
	/// <param name="species">The species to return the display name for</param>
	/// <returns>The display name of the species</returns>
	public static string GetDisplayName(this AnimalSpecies species) {
		return species switch {
			AnimalSpecies.TigerWhite => "White Tiger",
			_ => species.ToString()
		};
	}
}