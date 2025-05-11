using System;
using Microsoft.Xna.Framework;

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
			AnimalSpecies.Zebra => 400,
			AnimalSpecies.Elephant => 500,
			AnimalSpecies.Giraffe => 600,
			AnimalSpecies.Lion => 400,
			AnimalSpecies.Tiger => 500,
			AnimalSpecies.TigerWhite => 700,
			_ => 0,
		};
	}

	/// <summary>
	/// Gets the size (in tiles) that the animal takes up on the grid
	/// </summary>
	/// <param name="species">The species to use</param>
	/// <returns>The area the species takes up on the tile grid</returns>
	public static Point GetSize(this AnimalSpecies species) {
		return species switch {
			AnimalSpecies.Zebra => new Point(2, 2),
			AnimalSpecies.Elephant => new Point(3, 3),
			AnimalSpecies.Giraffe => new Point(2, 3),
			AnimalSpecies.Lion => new Point(3, 2),
			AnimalSpecies.Tiger => new Point(3, 2),
			AnimalSpecies.TigerWhite => new Point(3, 2),
			_ => new Point(1, 1)
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