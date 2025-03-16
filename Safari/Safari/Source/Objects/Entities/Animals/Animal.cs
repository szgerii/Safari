using Microsoft.Xna.Framework;
using Safari.Objects.Entities;
using Safari.Scenes;
using System;

namespace Safari.Objects.Entities.Animals;

public enum Gender {
	Male,
	Female
}

public enum AnimalSpecies {
	Zebra,
	Elephant,
	Lion,
	Tiger
}

public abstract class Animal : Entity {
	/// <summary>
	/// The level of hunger at which an animal will become hungry
	/// (not accounting for aging)
	/// </summary>
	public const int INITIAL_HUNGER_THRESHOLD = 30;
	/// <summary>
	/// The level of hunger at which an animal will become thirsty
	/// (not accounting for aging)
	/// </summary>
	public const int INITIAL_THIRST_THRESHOLD = 50;

	protected Gender gender;
	protected AnimalSpecies species;
	protected int hungerLevel = 100;
	protected int thirstLevel = 100;
	protected DateTime birthTime;
	protected DateTime lastMatingTime;

	/// <summary>
	/// Invoked when this animal gets hungry
	/// </summary>
	public event EventHandler GotHungry;
	/// <summary>
	/// Invoked when this animal gets thirsty
	/// </summary>
	public event EventHandler GotThirsty;
	/// <summary>
	/// Invoked when this animal gets caught by a poacher
	/// </summary>
	public event EventHandler Caught;
	/// <summary>
	/// Invoked when this animal is released, usually when the poacher is killed by a ranger
	/// </summary>
	public event EventHandler Released;
	/// <summary>
	/// Invoked when this animal has finished mating
	/// </summary>
	public event EventHandler Mated;

	/// <summary>
	/// The gender of the animal
	/// </summary>
	public Gender Gender {
		get => gender;
		set => gender = value;
	}
	/// <summary>
	/// The species of this animal
	/// </summary>
	public AnimalSpecies Species => species;
	/// <summary>
	/// The age of this animal in in-game days!
	/// </summary>
	public int Age => (int)(GameScene.Active.Model.IngameDate - birthTime).TotalDays;

	public Animal(Vector2 pos) : base(pos) {

	}

	public override void Update(GameTime gameTime) {
		base.Update(gameTime);
	}
}
