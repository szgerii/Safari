using Safari.Objects.Entities.Animals;
using System;

namespace Safari.Objects.Entities.Tourists;

public class SawAnimalEventArgs : EventArgs {
	private Animal animal;

	/// <summary>
	/// The animal that is seen
	/// </summary>
	public Animal Animal {
		get => animal;
		set => animal = value;
	}

	public SawAnimalEventArgs(Animal animal) : base() {
		this.animal = animal;
	}
}
