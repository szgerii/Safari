using Engine;
using Engine.Debug;
using Microsoft.Xna.Framework;
using Safari.Components;
using Safari.Model;
using Safari.Model.Tiles;
using Safari.Popups;
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
/// <br />
/// Every animal starts with a group of its own by default
/// </summary>
[SimulationActor]
public class AnimalGroup : GameObject {
	private const int MAX_SIZE = 10;
	private const float BASE_FORMATION_SPREAD = 30f;
	private const float FORMATION_SPREAD_STEP = 13f;

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
	public float FormationSpread => BASE_FORMATION_SPREAD + Size * FORMATION_SPREAD_STEP;

	/// <summary>
	/// The speed at which the animals inside the group move
	/// </summary>
	public float Speed { get; set; } = 50f;

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

	public NavigationCmp NavCmp { get; private set; }

	protected Vector2[] formationOffsets;

	public AnimalGroup(Animal creator) : base(creator.Position) {
		Species = creator.Species;
		AddMember(creator);
		OnSizeChange();

		NavCmp = new NavigationCmp(Speed);
		Attach(NavCmp);

		Game.AddObject(this);
	}

	static AnimalGroup() {
		DebugMode.AddFeature(new ExecutedDebugFeature("list-groups", () => {
			List<AnimalGroup> groups = [];

			foreach (GameObject obj in GameScene.Active.GameObjects) {
				if (obj is AnimalGroup group) groups.Add(group);
			}

			groups.Sort((a, b) => a.Species.ToString().CompareTo(b.Species.ToString()));

			foreach (AnimalGroup group in groups) {
				string name = group.Size > 0 ? group.Members[0].DisplayName : group.Species.ToString();
				DebugConsole.Instance.Write($"{name} group - {group.Size} members", false);
				DebugConsole.Instance.Info($"pos: {Utils.Format(group.Position, false, false)}");
				DebugConsole.Instance.Info($"current state: {group.State}");
			}
		}));
	}

	public override void Load() {
		OnSizeChange();

		Transition(AnimalGroupState.Wandering);

		base.Load();
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

		OnSizeChange();
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

		OnSizeChange();
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

	public bool CanAnybodyReach(Vector2 pos) {
		foreach (Animal anim in Members) {
			if (anim.CanReach(pos)) return true;
		}

		return false;
	}

	public bool CanEverybodyReach(Vector2 pos) {
		foreach (Animal anim in Members) {
			if (!anim.CanReach(pos)) return false;
		}

		return true;
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

	private void SyncToFormation() {
		for (int i = 0; i < Size; i++) {
			Vector2 targetPos = Position + formationOffsets[i];
			Members[i].NavCmp.TargetPosition = targetPos;
			Members[i].NavCmp.Moving = true;
			Members[i].NavCmp.StopOnTargetReach = false;
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
		switch (State) {
			case AnimalGroupState.Wandering:
				EndWandering();
				break;
		}

		State = newState;

		switch (newState) {
			case AnimalGroupState.Idle:
				BeginIdle();
				break;
			case AnimalGroupState.Wandering:
				BeginWandering();
				break;
			case AnimalGroupState.SeekingFood:
				if (Species.IsCarnivorous()) {
					Transition(AnimalGroupState.Wandering);
					return;
				}
				BeginSeekingFood();
				break;
			case AnimalGroupState.Feeding:
				NavCmp.Moving = false;
				break;
			case AnimalGroupState.SeekingWater:
				BeginSeekingWater();
				break;
			case AnimalGroupState.Drinking:
				NavCmp.Moving = false;
				break;
			default:
				break;
		}
	}

	private DateTime idleStart;
	private void BeginIdle() {
		idleStart = GameScene.Active.Model.IngameDate;

		NavCmp.Moving = false;

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
			OnSizeChange();
		}
	}

	private void IdleUpdate(GameTime gameTime) {
		if ((GameScene.Active.Model.IngameDate - idleStart).TotalHours > 2) {
			Transition(AnimalGroupState.Wandering);
		}
	}

	private void SetNewWanderingPos() {
		Level currLevel = GameScene.Active.Model.Level;
		NavCmp.TargetPosition = new Vector2(rand.Next(currLevel.MapWidth * currLevel.TileSize), rand.Next(currLevel.MapHeight * currLevel.TileSize));
		NavCmp.Moving = true;
		NavCmp.StopOnTargetReach = false;
	}

	private void OnWanderTargetReached(object sender, ReachedTargetEventArgs e) {
		SetNewWanderingPos();
	}

	private void BeginWandering() {
		SetNewWanderingPos();
		NavCmp.ReachedTarget += OnWanderTargetReached;
	}

	private void EndWandering() {
		NavCmp.ReachedTarget -= OnWanderTargetReached;
	}

	private readonly Random rand = new();
	private void WanderingUpdate(GameTime gameTime) {
		SyncToFormation();
	}

	private bool nearDestination = false;
	private void OnNearDestination(object sender, ReachedTargetEventArgs e) {
		nearDestination = true;
		NavCmp.ReachedTarget -= OnNearDestination;
	}

	private void BeginSeekingFood() {
		if (knownFoodSpots.Count == 0) {
			Transition(AnimalGroupState.Wandering);
			return;
		}

		nearDestination = false;
		NavCmp.Moving = true;
		NavCmp.StopOnTargetReach = true;
		NavCmp.ReachedTarget += OnNearDestination;
		NavCmp.TargetPosition = GetNearestFromList(knownFoodSpots);
	}

	private void SeekingFoodUpdate(GameTime gameTime) {
		// this shouldn't really happen, but let's be extra careful
		if (NavCmp.Target == null) {
			Transition(AnimalGroupState.SeekingFood);
			return;
		}

		if (!nearDestination) {
			SyncToFormation();
		} else {
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

					member.NavCmp.TargetPosition = NavCmp.TargetPosition;
					member.NavCmp.Moving = true;
					member.NavCmp.StopOnTargetReach = true;
				}
			}

			if (everyoneCanEat) {
				Transition(AnimalGroupState.Feeding);
				NavCmp.Moving = false;
				NavCmp.TargetPosition = null;
			}
		}
	}

	private void BeginSeekingWater() {
		if (knownWaterSpots.Count == 0) {
			Transition(AnimalGroupState.Wandering);
			return;
		}

		nearDestination = false;
		NavCmp.Moving = true;
		NavCmp.StopOnTargetReach = true;
		NavCmp.ReachedTarget += OnNearDestination;
		NavCmp.TargetPosition = GetNearestFromList(knownWaterSpots);
	}

	private void SeekingWaterUpdate(GameTime gameTime) {
		// this shouldn't really happen, but let's be extra careful
		if (NavCmp.Target == null) {
			Transition(AnimalGroupState.SeekingWater);
			return;
		}

		if (!nearDestination) {
			SyncToFormation();
		} else {
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

					member.NavCmp.TargetPosition = NavCmp.TargetPosition;
					member.NavCmp.Moving = true;
					member.NavCmp.StopOnTargetReach = true;
				}
			}

			if (everyoneCanDrink) {
				Transition(AnimalGroupState.Drinking);
				NavCmp.StopOnTargetReach = false;
				NavCmp.TargetPosition = null;
			}
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

	private Vector2? GetNearestFromList(List<Vector2> spots) {
		float minDist = -1;
		int minInd = -1;
		for (int i = 0; i < spots.Count; i++) {
			Vector2 spot = spots[i];
			float dist = Vector2.Distance(Position, spot);

			if (minInd == -1 || dist < minDist) {
				minDist = dist;
				minInd = i;
			}
		}

		return minInd > -1 ? spots[minInd] : null;
	}

	private void OnSizeChange() {
		if (Size <= 0) {
			formationOffsets = [];
		} else if (Size == 1) {
			formationOffsets = [Vector2.Zero];
		} else {
			Vector2[] result = new Vector2[Size];
			double step = 2 * (float)Math.PI / Size;

			for (int i = 0; i < Size; i++) {
				double rad = i * step;

				result[i] = new Vector2((float)Math.Cos(rad), (float)Math.Sin(rad)) * FormationSpread;
			}

			formationOffsets = result;
		}
	}
}
