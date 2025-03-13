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
	public const int INITIAL_HUNGER_THRESHOLD = 30;
	public const int INITIAL_THIRST_THRESHOLD = 50;

	protected Gender gender;
	protected AnimalSpecies species;
	protected int hungerLevel = 100;
	protected int thirstLevel = 100;
	protected DateTime birthTime;
	protected DateTime lastMatingTime;

	public event EventHandler GotHungry;
	public event EventHandler GotThirsty;
	public event EventHandler Caught;
	public event EventHandler Released;
	public event EventHandler Mated;

	public Gender Gender {
		get => gender;
		set => gender = value;
	}
	public AnimalSpecies Species {
		get => species;
		set => species = value;
	}
	public int Age => (int)(GameScene.Active.Model.IngameDate - birthTime).TotalDays;

	public Animal(Vector2 pos) : base(pos) {

	}

	public override void Update(GameTime gameTime) {
		base.Update(gameTime);
	}
}
