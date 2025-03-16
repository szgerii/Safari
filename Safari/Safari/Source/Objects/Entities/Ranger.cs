using Microsoft.Xna.Framework;
using Safari.Objects.Entities.Animals;

namespace Safari.Objects.Entities;
public class Ranger : Entity {
	/// <summary>
	/// The monthly salary of all rangers, payed in advance
	/// </summary>
	public const int SALARY = 1500;

	private static AnimalSpecies? defaultTarget;
	private AnimalSpecies? targetSpecies;
	
	/// <summary>
	/// The species of animals rangers should target when they
	/// don't have a specific target (can be changed by the player)
	/// </summary>
	public static AnimalSpecies? DefaultTarget {
		get => defaultTarget;
		set => defaultTarget = value;
	}
	/// <summary>
	/// The species of animals this particular ranger should target
	/// </summary>
	public AnimalSpecies? TargetSpecies {
		get => targetSpecies;
		set => targetSpecies = value;
	}

	public Ranger(Vector2 pos) : base(pos) {
		displayName = "Ranger";
	}
}
