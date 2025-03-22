using Engine;
using Microsoft.Xna.Framework;
using Safari.Model;
using Safari.Model.Tiles;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Objects.Entities.Animals;

/// <summary>
/// The possible states of an animal group
/// </summary>
public enum AnimalGroupState {
	Idle,
	Wandering,
	SeekingFood,
	SeekingWater,
	Feeding,
	Drinking
}

/// <summary>
/// A class for unifying several animal's behavior
/// Every animal starts with a group of its own by default
/// </summary>
[SimulationActor]
public class AnimalGroup : GameObject {
	private const int MAX_SIZE = 20;

	private readonly List<Vector2> knownFoodSpots = [];
	private readonly List<Vector2> knownWaterSpots = [];

	/// <summary>
	/// The species of the animals inside the group
	/// </summary>
	public AnimalSpecies Species { get; private init; }
	/// <summary>
	/// The state that determines which activity the group is currently performing
	/// </summary>
	public AnimalGroupState State { get; private set; }

	/// <summary>
	/// A list of the animals inside the group
	/// </summary>
	public List<Animal> Members { get; private init; } = [];
	/// <summary>
	/// The size of the group
	/// </summary>
	public int Size => Members.Count;

	/// <summary>
	/// Whether the group has a hungry animal inside it
	/// </summary>
	public bool HasHungryMember {
		get {
			foreach (Animal member in Members) {
				if (member.IsHungry) {
					return true;
				}
			}

			return false;
		}
	}

	/// <summary>
	/// Whether the group has a thirsty animals inside it
	/// </summary>
	public bool HasThirstyMember {
		get {
			foreach (Animal member in Members) {
				if (member.IsThirsty) {
					return true;
				}
			}

			return false;
		}
	}

	public AnimalGroup(Animal creator) : base(Vector2.Zero) {
		Species = creator.Species;
		Transition(AnimalGroupState.Wandering);
		AddMember(creator);
		Game.AddObject(this);
	}

	/// <summary>
	/// Checks if the group can be merged with another one
	/// </summary>
	/// <param name="other">The other group in the merge</param>
	/// <returns>Whether a merge is possible</returns>
	public bool CanMergeWith(AnimalGroup other) {
		return other.Species == Species && (Size + other.Size) <= MAX_SIZE;
	}

	/// <summary>
	/// Merges another group into this one
	/// </summary>
	/// <param name="other">The group to merge in</param>
	/// <exception cref="ArgumentException"></exception>
	public void MergeWith(AnimalGroup other) {
		if (!CanMergeWith(other)) {
			throw new ArgumentException("Cannot merge with the given animal group");
		}

		for (int i = other.Members.Count - 1; i >= 0; i--) {
			AddMember(other.Members[i]);
		}

		foreach (Vector2 foodSpot in other.knownFoodSpots) {
			AddFoodSpot(foodSpot);
		}
		foreach (Vector2 waterSpot in other.knownWaterSpots) {
			AddWaterSpot(waterSpot);
		}

		Game.RemoveObject(other);
	}

	/// <summary>
	/// Removes an animal from the group
	/// </summary>
	/// <param name="animal">The animal to remove</param>
	/// <exception cref="InvalidOperationException"></exception>
	public void Leave(Animal animal) {
		if (!Members.Contains(animal)) {
			throw new InvalidOperationException("The specified animal isn't inside the group it is being removed from");
		}

		animal.GotHungry -= OnHungryMember;
		animal.GotThirsty -= OnThirstyMember;

		Members.Remove(animal);
		animal.Group = null;

		if (Size <= 0) {
			Game.RemoveObject(this);
		}
	}

	/// <summary>
	/// Registers a food source position into the group's memory
	/// </summary>
	/// <param name="foodSpot">The food source position to add</param>
	public void AddFoodSpot(Vector2 foodSpot) {
		if (!knownFoodSpots.Contains(foodSpot)) {
			knownFoodSpots.Add(foodSpot);
		}

		bool validState = State == AnimalGroupState.Idle || State == AnimalGroupState.Wandering;
		if (validState && HasHungryMember) {
			Transition(AnimalGroupState.SeekingFood);
		}
	}

	/// <summary>
	/// Registers a water source position into the group's memory
	/// </summary>
	/// <param name="waterSpot">The water source position to add</param>
	public void AddWaterSpot(Vector2 waterSpot) {
		if (!knownWaterSpots.Contains(waterSpot)) {
			knownWaterSpots.Add(waterSpot);
		}

		bool validState = State == AnimalGroupState.Idle || State == AnimalGroupState.Wandering;
		if (validState && HasThirstyMember) {
			Transition(AnimalGroupState.SeekingWater);
		}
	}

	public override void Update(GameTime gameTime) {
		switch (State) {
			case AnimalGroupState.Idle:
				IdleUpdate(gameTime);
				break;
			case AnimalGroupState.Wandering:
				WanderingUpdate(gameTime);
				break;
			case AnimalGroupState.SeekingFood:
				SeekingFoodUpdate(gameTime);
				break;
			case AnimalGroupState.SeekingWater:
				SeekingWaterUpdate(gameTime);
				break;
			case AnimalGroupState.Feeding:
				FeedingUpdate(gameTime);
				break;
			case AnimalGroupState.Drinking:
				DrinkingUpdate(gameTime);
				break;
			default:
				break;
		}

		base.Update(gameTime);
	}

	private void AddMember(Animal animal) {
		if (animal.Species != Species) {
			throw new ArgumentException("Cannot add animal into a group with a different species attribute");
		}

		if (Size >= MAX_SIZE) {
			throw new InvalidOperationException("Cannot add an animal into a group that is already full");
		}

		if (Members.Contains(animal)) {
			throw new InvalidOperationException("The specified animal is already inside the group");
		}

		animal.Group?.Leave(animal);

		Members.Add(animal);
		animal.Group = this;
		animal.GotHungry += OnHungryMember;
		animal.GotThirsty += OnThirstyMember;
	}

	// TODO: replace with nav cmp
	private void MoveTowards(Vector2 target, GameTime gameTime) {
		foreach (Animal anim in Members) {
			Vector2 delta = target - anim.Position;
			delta.Normalize();

			anim.Move(ANIMAL_SPEED * delta * (float)gameTime.ElapsedGameTime.TotalSeconds);
		}
	}

	private void OnHungryMember(object sender, EventArgs e) {
		if (State != AnimalGroupState.Idle && State != AnimalGroupState.Wandering) return;

		Transition(AnimalGroupState.SeekingFood);
	}

	private void OnThirstyMember(object sender, EventArgs e) {
		if (State != AnimalGroupState.Idle && State != AnimalGroupState.Wandering) return;

		Transition(AnimalGroupState.SeekingWater);
	}

	private void Transition(AnimalGroupState newState) {
		State = newState;

		switch (newState) {
			case AnimalGroupState.Idle:
				BeginIdle();
				break;
			case AnimalGroupState.SeekingFood:
				if (Species.IsCarnivorous()) {
					Transition(AnimalGroupState.Wandering);
					return;
				}
				BeginSeekingFood();
				break;
			case AnimalGroupState.SeekingWater:
				if (Species.IsCarnivorous()) {
					Transition(AnimalGroupState.Wandering);
					return;
				}
				BeginSeekingWater();
				break;
			default:
				break;
		}
	}

	private DateTime idleStart;
	private void BeginIdle() {
		idleStart = GameScene.Active.Model.IngameDate;

		List<Animal> males = new();
		List<Animal> females = new();

		int maxAmt = MAX_SIZE - Size;
		foreach (Animal member in Members) {
			if (!member.CanMate) continue;

			if (member.Gender == Gender.Male) {
				males.Add(member);
			} else {
				females.Add(member);
			}

			if (males.Count >= maxAmt && females.Count >= maxAmt) {
				break;
			}
		}

		for (int i = Math.Min(males.Count, females.Count) - 1; i >= 0; i--) {
			males[i].Mate();
			females[i].Mate();

			Animal child = (Animal)Activator.CreateInstance(Species.GetAnimalType(), [females[i].Position, Utils.GetRandomEnumValue<Gender>()]);
			Game.AddObject(child);
			AddMember(child);
		}
	}

	private void IdleUpdate(GameTime gameTime) {
		if ((GameScene.Active.Model.IngameDate - idleStart).TotalHours > 2) {
			Transition(AnimalGroupState.Wandering);
		}
	}

	// TODO: replace with nav cmp
	private Vector2? wanderingTarget = null;
	private const float ANIMAL_SPEED = 50f;
	private readonly Random rand = new();
	private void WanderingUpdate(GameTime gameTime) {
		bool needsNewTarget = wanderingTarget == null;
		if (wanderingTarget != null) {
			foreach (Animal anim in Members) {
				if (anim.CanSee(wanderingTarget.Value)) {
					needsNewTarget = true;
					break;
				}
			}
		}

		if (needsNewTarget) {
			Level currLevel = GameScene.Active.Model.Level;
			wanderingTarget = new Vector2(rand.Next(currLevel.MapWidth), rand.Next(currLevel.MapHeight));
			wanderingTarget *= currLevel.TileSize;
		}

		MoveTowards(wanderingTarget.Value, gameTime);
	}

	Vector2? foodTarget = null;
	private void BeginSeekingFood() {
		if (knownFoodSpots.Count == 0) {
			Transition(AnimalGroupState.Wandering);
			return;
		}

		float minDist = -1;
		int minInd = -1;
		for (int i = 0; i < knownFoodSpots.Count; i++) {
			Vector2 spot = knownFoodSpots[i];
			float dist = Vector2.Distance(Members[0].Position, spot);

			if (minInd == -1 || dist < minDist) {
				minDist = dist;
				minInd = i;
			}
		}

		foodTarget = knownFoodSpots[minInd];
	}

	private void SeekingFoodUpdate(GameTime gameTime) {
		// this shouldn't really happen, but let's be extra careful
		if (foodTarget == null) {
			Transition(AnimalGroupState.SeekingFood);
			return;
		}

		MoveTowards(foodTarget.Value, gameTime);

		bool everyoneCanEat = true;
		foreach (Animal member in Members) {
			bool canEat = false;

			foreach (Tile tile in member.GetTilesInReach()) {
				if (tile.IsFoodSource) {
					canEat = true;
					break;
				}
			}

			if (!canEat) {
				everyoneCanEat = false;
				break;
			}
		}

		if (everyoneCanEat) {
			Transition(AnimalGroupState.Feeding);
			foodTarget = null;
		}
	}

	Vector2? waterTarget = null;
	private void BeginSeekingWater() {
		if (knownWaterSpots.Count == 0) {
			Transition(AnimalGroupState.Wandering);
			return;
		}

		float minDist = -1;
		int minInd = -1;
		for (int i = 0; i < knownWaterSpots.Count; i++) {
			Vector2 spot = knownWaterSpots[i];
			float dist = Vector2.Distance(Members[0].Position, spot);

			if (minInd == -1 || dist < minDist) {
				minDist = dist;
				minInd = i;
			}
		}

		waterTarget = knownWaterSpots[minInd];
	}

	private void SeekingWaterUpdate(GameTime gameTime) {
		// this shouldn't really happen, but let's be extra careful
		if (waterTarget == null) {
			Transition(AnimalGroupState.SeekingWater);
			return;
		}

		MoveTowards(waterTarget.Value, gameTime);

		bool everyoneCanDrink = true;
		foreach (Animal member in Members) {
			bool canDrink = false;

			foreach (Tile tile in member.GetTilesInReach()) {
				if (tile.IsWaterSource) {
					canDrink = true;
					break;
				}
			}

			if (!canDrink) {
				everyoneCanDrink = false;
				break;
			}
		}

		if (everyoneCanDrink) {
			Transition(AnimalGroupState.Drinking);
			waterTarget = null;
		}
	}

	private void FeedingUpdate(GameTime gameTime) {
		bool everybodyFull = true;
		bool hasThirsty = false;

		foreach (Animal member in Members) {
			member.Feed(gameTime);

			if (member.HungerLevel < 100f) {
				everybodyFull = false;
			}

			if (member.IsThirsty) {
				hasThirsty = true;
			}
		}

		if (everybodyFull) {
			Transition(hasThirsty ? AnimalGroupState.SeekingWater : AnimalGroupState.Idle);
		}
	}

	private void DrinkingUpdate(GameTime gameTime) {
		bool everybodyFull = true;
		bool hasHungry = false;

		foreach (Animal member in Members) {
			member.Drink(gameTime);

			if (member.ThirstLevel < 100f) {
				everybodyFull = false;
			}

			if (member.IsHungry) {
				hasHungry = true;
			}
		}

		if (everybodyFull) {
			Transition(hasHungry ? AnimalGroupState.SeekingFood : AnimalGroupState.Idle);
		}
	}
}
