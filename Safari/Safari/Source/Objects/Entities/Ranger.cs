using Microsoft.Xna.Framework;
using Safari.Objects.Entities.Animals;

namespace Safari.Objects.Entities;
public class Ranger : Entity {
	public const int SALARY = 1500;

	private static AnimalSpecies? defaultTarget;
	private AnimalSpecies? targetSpecies;
	
	public static AnimalSpecies? DefaultTarget {
		get => defaultTarget;
		set => defaultTarget = value;
	}
	public AnimalSpecies? TargetSpecies {
		get => targetSpecies;
		set => targetSpecies = value;
	}

	public Ranger(Vector2 pos) : base(pos) {
		displayName = "Ranger";
	}
}
