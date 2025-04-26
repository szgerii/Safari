using Engine.Components;
using Microsoft.Xna.Framework;
using Safari.Components;
using Safari.Model.Entities.Animals;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Model.Entities.Tourists;

public enum TouristState {
	Entering,
	Leaving,
	InQueue,
	InJeep
};

public class Tourist : Entity {
	private static double[] recentRatings = new double[360];
	private static int ratingCount = 0;
	public static EntitySpawner<Tourist> Spawner { get; set; }
	public static List<Tourist> Queue { get; private set; } = new List<Tourist>();

	private HashSet<Animal> seenAnimals = new();
	private HashSet<AnimalSpecies> seenAnimalSpecies = new();


	private int targetQueueIndex = -1;
	private int queueIndex = -1;
	private int reachedQueueIndex = -1;
	private bool shouldDraw = true;
	private int payedAmount = 0;
	private double rating = 2.5f;
	private int moneyThreshold;
	private AnimalSpecies favSpecies;
	private float xOffset;
	private bool preferStandingRight = true;
	private DateTime nextSwitch;

	/// <summary>
	/// The animated sprite component of the ranger
	/// </summary>
	public AnimatedSpriteCmp AnimatedSprite => Sprite as AnimatedSpriteCmp;

	/// <summary>
	/// The state machine used for transitioning between the different tourist behavior types
	/// </summary>
	public StateMachineCmp<TouristState> StateMachine { get; init; }

	/// <summary>
	/// The spot at which the tourists get picked up
	/// </summary>
	public static Point PickupSpot { get; set; }
	public static Level CurrentLevel => GameScene.Active.Model.Level;
	public static double QueueOffset => CurrentLevel.TileSize / 1.23;

	/// <summary>
	/// The average rating of the park based on the 30 newest ratings by tourist
	/// Ratings are always in [1.0, 5.0]
	/// </summary>
	public static double AvgRating {
		get {
			double sum = 0;
			for (int i = 0; i < ratingCount; i++) {
				sum += recentRatings[i];
			}
			int remaining = recentRatings.Length - ratingCount;
			sum += remaining * 2f;
			return sum / recentRatings.Length;
		}
	}

	public const int milestoneCount = 12;
	public static double[] milestones = new double[milestoneCount] {1.0, 2.0, 2.5, 3.0, 3.3, 3.7, 4.0, 4.2, 4.4, 4.6, 4.8, 5.0};
	public static double[] spawnRates = new double[milestoneCount] { 0.4, 0.7, 1.0, 1.5, 1.9, 2.5, 3.2, 4.2, 5.1, 6.0, 7.0, 8.0 };
	public static double SpawnRate {
		get {
			// base spawn rate -> lerp
			int i = 1;
			for (i = 1; i < milestoneCount - 1; i++) {
				if (AvgRating < milestones[i]) {
					break;
				}
			}
			double factor = (AvgRating - milestones[i - 1]) / (milestones[i] - milestones[i - 1]);
			return spawnRates[i - 1] + factor * (spawnRates[i] - spawnRates[i - 1]);
		}
	}

	/// <summary>
	/// The jeep this tourist is assigned to
	/// </summary>
	public Jeep Vehicle { get; set; }

	public static void Init() {
		Queue = new List<Tourist>();
		recentRatings = new double[30];
		ratingCount = 0;
	}

	public static void UpdateQueue() {
		for (int i = 0; i < Queue.Count; i++) {
			Queue[i].queueIndex = i;
		}
	}

	public static void AddReview(double review) {
		if (ratingCount < recentRatings.Length) {
			recentRatings[ratingCount] = review;
			ratingCount++;
		} else {
			for (int i = 1; i < recentRatings.Length; i++) {
				recentRatings[i - 1] = recentRatings[i];
			}
			recentRatings[recentRatings.Length - 1] = review;
		}
		UpdateSpawner();
	}

	public static void UpdateSpawner() {
		Spawner.Frequency = 1.0f / (SpawnRate);
	}

	public Tourist(Vector2 pos) : base(pos) {
		DisplayName = "Tourist";
		AnimatedSpriteCmp animSprite;
		if (Game.Random.Next(2) == 1) {
			animSprite = new(Game.LoadTexture("Assets/Tourist/Man/Walk"), 10, 2, 8);
			animSprite.Animations["walk-right"] = new Animation(0, 10, true);
			animSprite.Animations["walk-left"] = new Animation(1, 10, true);
			animSprite.Animations["idle-right"] = new Animation(0, 1, true, 8);
			animSprite.Animations["idle-left"] = new Animation(1, 1, true, 8);
		} else {
			animSprite = new(Game.LoadTexture("Assets/Tourist/Woman/Walk"), 8, 2, 10);
			animSprite.Animations["walk-right"] = new Animation(0, 8, true);
			animSprite.Animations["walk-left"] = new Animation(1, 8, true);
			animSprite.Animations["idle-right"] = new Animation(0, 1, true, 4);
			animSprite.Animations["idle-left"] = new Animation(1, 1, true, 4);
		}
		Sprite = animSprite;
		Sprite.LayerDepth = Animal.ANIMAL_LAYER;
		Sprite.YSortEnabled = true;
		Sprite.YSortOffset = 64;
		Sprite.Origin = new Vector2(16, 64); // just by the 'vibes'
		SightDistance = Game.Random.Next(4, 8);
		var values = Enum.GetValues(typeof(AnimalSpecies));
		favSpecies = (AnimalSpecies)values.GetValue(Game.Random.Next(values.Length));
		moneyThreshold = Game.Random.Next(5, 11) * 100;
		Attach(Sprite);
		animSprite.CurrentAnimation = "idle-right";
		xOffset = (float)Game.Random.NextDouble() * 24f - 12f;

		NavCmp.AccountForBounds = false;
		NavCmp.Speed = 60f;
		NavCmp.StopOnTargetReach = true;
		ReachDistance = 0;

		StateMachine = new StateMachineCmp<TouristState>(TouristState.Entering);
		Attach(StateMachine);
	}

	public override void Load() {
		GameScene.Active.Model.TouristCount++;

		base.Load();
	}

	public override void Unload() {
		GameScene.Active.Model.TouristCount--;

		base.Unload();
	}

	public override void Update(GameTime gameTime) {
		if (NavCmp.LastIntendedDelta != Vector2.Zero && NavCmp.Moving) {
			bool right = NavCmp.LastIntendedDelta.X > -0.075f;
			string anim = $"walk-{(right ? "right" : "left")}";

			if (!AnimatedSprite.IsPlaying || AnimatedSprite.CurrentAnimation != anim) {
				AnimatedSprite.CurrentAnimation = anim;
			}
		} else {
			string anim = $"idle-{(preferStandingRight ? "right" : "left")}";
			if (!AnimatedSprite.IsPlaying || AnimatedSprite.CurrentAnimation != anim) {
				AnimatedSprite.CurrentAnimation = anim;
			}
		}

		if (StateMachine.CurrentState == TouristState.Entering || StateMachine.CurrentState == TouristState.InQueue) {
			if (!GameScene.Active.Model.IsDaytime) {
				Queue.Remove(this);
				UpdateQueue();
				StateMachine.Transition(TouristState.Leaving);
			}
		}

		base.Update(gameTime);
	}

	public override void Draw(GameTime gameTime) {
		if (shouldDraw) {
			base.Draw(gameTime);
		}
	}

	[StateBegin(TouristState.Entering)]
	public void BeginEntering() {
		targetQueueIndex = Queue.Count;
		queueIndex = targetQueueIndex;
		Queue.Add(this);
		NavCmp.TargetPosition = GetQueueSpot(targetQueueIndex);
		NavCmp.StopOnTargetReach = true;
		NavCmp.Moving = true;
		NavCmp.ReachedTarget += ReachedQueueSpot;
	}

	private void ReachedQueueSpot(object sender, NavigationTargetEventArgs e) {
		reachedQueueIndex = targetQueueIndex;
		targetQueueIndex = -1;
		NavCmp.ReachedTarget -= ReachedQueueSpot;
		if (StateMachine.CurrentState != TouristState.InQueue) {
			StateMachine.Transition(TouristState.InQueue);
		}
		if (reachedQueueIndex == 0) {
			if (!TryEntering()) {
				Jeep.JeepReadyToFill += OnReadyToFill;
				Jeep.RequestNextJeep();
			}
		}
	}

	[StateBegin(TouristState.InQueue)]
	public void BeginInQueue() {
		nextSwitch = GameScene.Active.Model.IngameDate + TimeSpan.FromMinutes(Game.Random.NextDouble() * 30 + 10.0);
	}

	private void OnReadyToFill(object sender, EventArgs e) => TryEntering();

	private bool TryEntering() {
		Jeep jeep = Jeep.WaitingJeep;
		if (jeep != null && jeep.AddTourist(this)) {
			Queue.Remove(this);
			UpdateQueue();
			Vehicle = jeep;
			StateMachine.Transition(TouristState.InJeep);
			Jeep.JeepReadyToFill -= OnReadyToFill;
			return true;
		}
		return false;
	}

	[StateUpdate(TouristState.InQueue)]
	public void InQueueUpdate(GameTime gameTime) {
		if (queueIndex < reachedQueueIndex && targetQueueIndex == -1 && GameScene.Active.Model.IsDaytime) {
			targetQueueIndex = queueIndex;
			NavCmp.TargetPosition = GetQueueSpot(targetQueueIndex);
			NavCmp.StopOnTargetReach = true;
			NavCmp.Moving = true;
			NavCmp.ReachedTarget += ReachedQueueSpot;
		}
		
		DateTime now = GameScene.Active.Model.IngameDate;
		if (now >= nextSwitch) {
			preferStandingRight = !preferStandingRight;
			nextSwitch = now + TimeSpan.FromMinutes(Game.Random.NextDouble() * 11 + 6);
		}
	}

	private Vector2 GetQueueSpot(int index) {
		return CurrentLevel.GetTileCenter(PickupSpot) + (new Vector2(0, -1.0f * index * (float)QueueOffset)) + new Vector2(1.0f, 0) * xOffset;
	}

	[StateBegin(TouristState.InJeep)]
	public void BeginInJeep() {
		shouldDraw = false;
	}

	[StateUpdate(TouristState.InJeep)]
	public void InJeepUpdate(GameTime gameTime) {
		Position = Vehicle.Position;
		foreach (Entity entity in GetEntitiesInSight()) {
			if (entity is Animal animal && !seenAnimals.Contains(animal)) {
				seenAnimals.Add(animal);
				rating += 0.25f;
				if (animal.Species == favSpecies) {
					rating += 0.35f;
				}
				if (!seenAnimalSpecies.Contains(animal.Species)) {
					seenAnimalSpecies.Add(animal.Species);
					rating += 0.4f;
					if (animal.Species == favSpecies) {
						rating += 0.6f;
					}
				}
			}
		}
	}

	public void LeaveJeep() {
		Vehicle = null;
		shouldDraw = true;
		StateMachine.Transition(TouristState.Leaving);
	}

	public void TourFinished() {
		rating = Math.Clamp(rating, 1.0, 5.0);
		AddReview(rating);
	}

	public void TourFailed() {
		AddReview(1.0f);
	}

	private Vector2 GetNearestEdge() {
		Vector2[] options = [
			new Vector2(-10, Position.Y),
			new Vector2(CurrentLevel.MapWidth * CurrentLevel.TileSize + 10, Position.Y),
			new Vector2(Position.X, CurrentLevel.MapHeight * CurrentLevel.TileSize + 10),
			new Vector2(Position.X, -10)
		];

		Vector2 minVec = options[0];
		float minDist = Vector2.Distance(Position, minVec);
		for (int i = 1; i < options.Length; i++) {
			float dist = Vector2.Distance(Position, options[i]);

			if (dist < minDist) {
				minVec = options[i];
				minDist = dist;
			}
		}

		return minVec;
	}

	[StateBegin(TouristState.Leaving)]
	public void BeginLeaving() {
		NavCmp.TargetPosition = GetNearestEdge();
		NavCmp.StopOnTargetReach = true;
		NavCmp.Moving = true;
		NavCmp.ReachedTarget += LeftPark;
	}

	private void LeftPark(object sender, NavigationTargetEventArgs e) {
		NavCmp.ReachedTarget -= LeftPark;
		Die();
	}

	public void Pay() {
		GameScene.Active.Model.Funds += Jeep.RentFee;
		payedAmount = Jeep.RentFee;
		if (Jeep.RentFee > 600) {
			rating -= 0.05f * ((Jeep.RentFee - 600) / 50);
		}
	}
}
