using Engine.Components;
using Engine.Debug;
using Engine.Graphics.Stubs.Texture;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using Safari.Components;
using Safari.Model.Entities.Animals;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Model.Entities.Tourists;

public enum JeepState {
	Parking,
	Entering,
	WaitingForTourists,
	WaitingForNormalRoute,
	WaitingForReturnRoute,
	WaitingForEscapeRoute,
	FollowingRoute,
	Emptying,
	Canceling
}

public enum JeepDirection {
	Right = 0,
	Left = 1,
	Up = 2,
	Down = 3,
}

[SimulationActor]
public class Jeep : Entity {

	/// <summary>
	/// The maximum number of tourists a jeep can have
	/// </summary>
	public const int CAPACITY = 4;
	/// <summary>
	/// The number of jeeps the player starts out with
	/// </summary>
	public const int STARTING_JEEPS = 1;
	/// <summary>
	/// Once the hours specified here have passed, a jeep can start its journey even if its not full
	/// </summary>
	public const double MAX_WAITING_HOURS = 4;

	private List<Tourist> occupants = new List<Tourist>();
	private static Queue<Jeep> garage = new Queue<Jeep>();
	private static bool jeepEntering = false;

	private int routeIndex = 0;
	private List<Point> route = new List<Point>();
	private Point goal;
	private Point postGoal;
	private JeepState postState;
	private JeepDirection dir = JeepDirection.Right;
	private bool needEscape = false;
	private Point escapeStart;
	private DateTime lastDrop;
	private DateTime waitStart;

	private const int TEXTURE_COUNT = 4;
	private static string[] textureNames = new string[4] { "Red", "White", "Green", "Brown" };
	private static ITexture2D[] textures = new ITexture2D[4] {null, null, null, null};

	private static Level CurrentLevel => GameScene.Active.Model.Level;

	public StateMachineCmp<JeepState> StateMachine { get; private set; } = new(JeepState.Parking);

	/// <summary>
	/// The jeep that is currently being filled with tourists (or null)
	/// </summary>
	public static Jeep WaitingJeep { get; set; } = null;

	/// <summary>
	/// The spot where the jeeps park (offscreen)
	/// </summary>
	public static Point GarageSpot { get; set; }
	/// <summary>
	/// The spot where the jeeps pick up the tourists
	/// </summary>
	public static Point PickUpSpot { get; set; }
	/// <summary>
	/// The spot where the jeeps drop off the tourists
	/// </summary>
	public static Point DropOffSpot { get; set; }

	/// <summary>
	/// The amount a tourist has to pay in order to participate in the tour
	/// A high rent fee could make some tourists dissatisfied with the ride
	/// </summary>
	public static int RentFee {
		get; set;
	}

	/// <summary>
	/// Invoked when a jeep is ready to be entered by the tourists
	/// </summary>
	public static event EventHandler JeepReadyToFill;

	private static bool someoneWaitingForJeep = false;

	private static bool debugAutoFill = false;
	private static bool debugAutoRequest = false;

	static Jeep() {
		DebugMode.AddFeature(new ExecutedDebugFeature("request-jeep", () => RequestNextJeep()));

		DebugMode.AddFeature(new ExecutedDebugFeature("fill-jeep", () => {
			DebugFill();
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("add-jeep", () => {
			Jeep.SpawnJeep();
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("add-to-jeep", () => {
			DebugAdd();
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-jeep-autofill", () => {
			debugAutoFill = !debugAutoFill;
			if (debugAutoFill) {
				DebugFill();
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-jeep-autorequest", () => {
			debugAutoRequest = !debugAutoRequest;
			if (debugAutoRequest) {
				RequestNextJeep();
			}
		}));
	}

	/// <summary>
	/// Initializes the static state of the jeeps
	/// </summary>
	public static void Init(int baseRentFee) {
		someoneWaitingForJeep = false;
		debugAutoFill = false;
		debugAutoRequest = false;
		WaitingJeep = null;
		RentFee = baseRentFee;
		for (int i = 0; i < TEXTURE_COUNT; i++) {
			if (textures[i] == null) {
				textures[i] = Game.LoadTexture("Assets/Jeep/Jeep" + textureNames[i]);
			}
		}
		JeepReadyToFill += DebugReadyToFill;
		jeepEntering = false;
		garage = new();
	}

	public static void Cleanup() {
		if (JeepReadyToFill == null) return;

		foreach (Delegate d in JeepReadyToFill.GetInvocationList()) {
			JeepReadyToFill -= (EventHandler)d;
			for (int i = 0; i < TEXTURE_COUNT; i++) {
				textures[i] = null;
			}
		}
	}

	private static void DebugReadyToFill(object sender, EventArgs e) {
		if (debugAutoFill) {
			DebugFill();
		}
	}

	private static void DebugFill() {
		for (int i = 0; i < CAPACITY; i++) {
			DebugAdd();
		}
	}

	private static void DebugAdd() {
		if (WaitingJeep != null) {
			WaitingJeep.AddTourist(new Tourist(new(40, 40)));
		}
	}

	public Jeep(Vector2 pos, int color) : base(pos) {
		DisplayName = "Jeep";
		LightEntityCmp lightCmp = new LightEntityCmp(CurrentLevel, 6);
		Attach(lightCmp);
		Sprite = new SpriteCmp(textures[color]);
		Sprite.LayerDepth = Animal.ANIMAL_LAYER;
		Sprite.YSortEnabled = true;
		UpdateSrcRec();
		Sprite.Origin = new Vector2(32, 42); // just by the 'vibes'
		SightDistance = 6;
		NavCmp.AccountForBounds = false;
		NavCmp.Speed = 200f;
		NavCmp.StopOnTargetReach = true;
		ReachDistance = 0;
		Bounds = Vectangle.Empty;
		Attach(Sprite);
		Attach(StateMachine);
	}

	/// <summary>
	/// Spawns a jeep in the garage with a random color
	/// </summary>
	public static void SpawnJeep() {
		Game.AddObject(new Jeep(CurrentLevel.GetTileCenter(Jeep.GarageSpot), Game.Random.Next(TEXTURE_COUNT)));
	}

	/// <summary>
	/// Request a jeep
	/// </summary>
	public static void RequestNextJeep() {
		// Pop the queue and transition it to Entering
		if (!jeepEntering && WaitingJeep == null && garage.Count > 0 && GameScene.Active.Model.IsDaytime) {
			Jeep next = garage.Dequeue();
			next.StateMachine.Transition(JeepState.Entering);
			someoneWaitingForJeep = false;
		} else if (!jeepEntering && GameScene.Active.Model.IsDaytime) {
			someoneWaitingForJeep = true;
		}
	}

	public override void Load() {
		GameScene.Active.Model.JeepCount++;
		CurrentLevel.Network.RoadChanged += OnRoadChanged;

		base.Load();
	}

	public override void Unload() {
		GameScene.Active.Model.JeepCount--;

		base.Unload();
	}

	/// <summary>
	/// Add a tourist to this jeep
	/// </summary>
	public bool AddTourist(Tourist t) {
		if (StateMachine.CurrentState == JeepState.WaitingForTourists && occupants.Count < CAPACITY) {
			occupants.Add(t);
			if (occupants.Count >= CAPACITY) {
				// jeep is full here
				StateMachine.Transition(JeepState.WaitingForNormalRoute);
				goal = CurrentLevel.Network.End;
				postGoal = DropOffSpot;
				postState = JeepState.Emptying;
				if (debugAutoRequest) {
					RequestNextJeep();
				}
			}
			return true;
		}
		return false;
	}

	private Tourist RemoveFirstTourist() {
		Tourist result = null;
		if (occupants.Count > 0) {
			result = occupants[occupants.Count - 1];
			occupants.RemoveAt(occupants.Count - 1);
		}
		return result;
	}

	private void Kill() {
		for (int i = 0; i < 4; i++) {
			Tourist t = RemoveFirstTourist();
			t.TourFailed();
			t.Die();
		}
		NavCmp.Moving = false;
		if (StateMachine.CurrentState == JeepState.FollowingRoute) {
			NavCmp.ReachedTarget -= CheckPointReached;
		}
		Position = CurrentLevel.GetTileCenter(GarageSpot);
		StateMachine.Transition(JeepState.Parking);
	}

	[StateBegin(JeepState.Parking)]
	public void BeginParking() {
		if (someoneWaitingForJeep) {
			if (GameScene.Active.Model.IsDaytime) {
				StateMachine.Transition(JeepState.Entering);
				someoneWaitingForJeep = false;
			} else {
				someoneWaitingForJeep = false;
				garage.Enqueue(this);
			}
		} else {
			garage.Enqueue(this);
		}
	}

	[StateBegin(JeepState.Entering)]
	public void BeginEntering() {
		jeepEntering = true;
		NavCmp.TargetPosition = CurrentLevel.GetTileCenter(PickUpSpot);
		NavCmp.Moving = true;
		NavCmp.ReachedTarget += PickUpReached;
		NavCmp.StopOnTargetReach = false;
		dir = JeepDirection.Right;
		UpdateSrcRec();
	}

	private void PickUpReached(object sender, NavigationTargetEventArgs e) {
		StateMachine.Transition(JeepState.WaitingForTourists);
	}

	[StateEnd(JeepState.Entering)]
	public void EndEntering() {
		jeepEntering = false;
		NavCmp.StopOnTargetReach = true;
		NavCmp.ReachedTarget -= PickUpReached;
	}

	[StateBegin(JeepState.WaitingForTourists)]
	public void BeginWaitingForTourists() {
		WaitingJeep = this;
		JeepReadyToFill?.Invoke(this, EventArgs.Empty);
		waitStart = GameScene.Active.Model.IngameDate;
	}

	[StateUpdate(JeepState.WaitingForTourists)]
	public void WaitingForTouristsUpdate(GameTime gameTime) {
		DateTime now = GameScene.Active.Model.IngameDate;
		if (!GameScene.Active.Model.IsDaytime) {
			StateMachine.Transition(JeepState.Canceling);
	
			return;
		}
		if (now > waitStart + TimeSpan.FromHours(MAX_WAITING_HOURS) && occupants.Count > 0) {
			StateMachine.Transition(JeepState.WaitingForNormalRoute);
			goal = CurrentLevel.Network.End;
			postGoal = DropOffSpot;
			postState = JeepState.Emptying;
			if (debugAutoRequest) {
				RequestNextJeep();
			}
		}
	}

	[StateUpdate(JeepState.WaitingForNormalRoute)]
	public void WaitingForNormalRouteUpdate(GameTime gameTime) {
		if (!GameScene.Active.Model.IsDaytime) {
			StateMachine.Transition(JeepState.Canceling);
		}
	}

	[StateBegin(JeepState.WaitingForNormalRoute)]
	public void BeginWaitingForNormalRoute() {
		RequestNormalRoute();
	}

	[StateBegin(JeepState.WaitingForReturnRoute)]
	public void BeginWaitingForReturnRoute() {
		RequestReturnRoute();
	}

	[StateBegin(JeepState.WaitingForEscapeRoute)]
	public void BeginWaitingForEscapeRoute() {
		RequestEscapeRoute();
	}

	private void OnRoadChanged(object sender, RoadChangedEventArgs e) {
		if (StateMachine.CurrentState == JeepState.WaitingForNormalRoute && e.ChangeType) {
			RequestNormalRoute();
		} else if (StateMachine.CurrentState == JeepState.WaitingForReturnRoute && e.ChangeType) {
			RequestReturnRoute();
		} else if (StateMachine.CurrentState == JeepState.WaitingForEscapeRoute && e.ChangeType) {
			RequestEscapeRoute();
		} else if (StateMachine.CurrentState == JeepState.FollowingRoute && !e.ChangeType) {
			if (routeIndex < route.Count && route[routeIndex] == e.Location) {
				Kill();
				return;
			}
			bool found = false;
			for (int i = routeIndex + 1; i < route.Count && !found; i++) {
				if (route[i] == e.Location) {
					found = true;
				}
			}
			if (found) {
				needEscape = true;
			}
		} else if (StateMachine.CurrentState == JeepState.WaitingForEscapeRoute && !e.ChangeType) {
			if (escapeStart == e.Location) {
				Kill();
			}
		}
	}

	private void RequestNormalRoute() {
		route = CurrentLevel.Network.RandomRoute;
		if (route.Count > 0) {
			WaitingJeep = null;
			// jeep gets going
			StateMachine.Transition(JeepState.FollowingRoute);
			foreach (Tourist t in occupants) {
				t.Pay();
			}
			if (someoneWaitingForJeep) {
				RequestNextJeep();
			}
		}
	}

	private void RequestReturnRoute() {
		route = CurrentLevel.Network.ReturnRoute;
		if (route.Count > 0) {
			StateMachine.Transition(JeepState.FollowingRoute);
		}
	}

	private void RequestEscapeRoute() {
		route = CurrentLevel.Network.GetPath(escapeStart, goal);
		if (route.Count > 0) {
			StateMachine.Transition(JeepState.FollowingRoute);
		}
	}

	[StateBegin(JeepState.FollowingRoute)]
	public void BeginFollowingRoute() {
		needEscape = false;
		routeIndex = 0;
		if (route.Count > 0) {
			NavCmp.ReachedTarget += CheckPointReached;
			NavCmp.TargetPosition = CurrentLevel.GetTileCenter(route[0]);
		} else {
			NavCmp.ReachedTarget += PostGoalReached;
			NavCmp.TargetPosition = CurrentLevel.GetTileCenter(postGoal);
		}
		NavCmp.StopOnTargetReach = false;
		NavCmp.Moving = true;
	}

	[StateUpdate(JeepState.FollowingRoute)]
	public void FollowingRouteUpdate(GameTime gameTime) {
		if (!NavCmp.Moving)
			return;
		Vector2 delta = NavCmp.LastIntendedDelta;
		JeepDirection newDir = dir;
		bool horizontal = Math.Abs(delta.X) >= Math.Abs(delta.Y);
		if (horizontal) {
			newDir = delta.X >= 0 ? JeepDirection.Right : JeepDirection.Left;
		} else {
			newDir = delta.Y >= 0 ? JeepDirection.Down : JeepDirection.Up;
		}
		if (newDir != dir) {
			dir = newDir;
			UpdateSrcRec();
		}
	}

	private void CheckPointReached(object sender, NavigationTargetEventArgs e) {
		if (needEscape) {
			NavCmp.Moving = false;
			NavCmp.ReachedTarget -= CheckPointReached;
			escapeStart = route[routeIndex];
			StateMachine.Transition(JeepState.WaitingForEscapeRoute);
			return;
		}

		routeIndex++;
		
		if (routeIndex >= route.Count) {
			NavCmp.ReachedTarget -= CheckPointReached;
			NavCmp.TargetPosition = CurrentLevel.GetTileCenter(postGoal);
			NavCmp.Moving = true;
			NavCmp.ReachedTarget += PostGoalReached;
		} else {
			NavCmp.TargetPosition = CurrentLevel.GetTileCenter(route[routeIndex]);
			
			NavCmp.Moving = true;
		}
	}

	private void PostGoalReached(object sender, NavigationTargetEventArgs e) {
		NavCmp.ReachedTarget -= PostGoalReached;
		NavCmp.StopOnTargetReach = true;
		StateMachine.Transition(postState);
	}

	[StateBegin(JeepState.Emptying)]
	public void BeginEmptying() {
		lastDrop = GameScene.Active.Model.IngameDate;
	}

	[StateUpdate(JeepState.Emptying)]
	public void EmptyingUpdate(GameTime gameTime) {
		DateTime now = GameScene.Active.Model.IngameDate;
		if (now > lastDrop + TimeSpan.FromMinutes(2)) {
			Tourist t = RemoveFirstTourist();
			lastDrop = now;
			t.TourFinished();
			t.LeaveJeep();
			if (occupants.Count <= 0) {
				StateMachine.Transition(JeepState.WaitingForReturnRoute);
				goal = CurrentLevel.Network.Start;
				postGoal = GarageSpot;
				postState = JeepState.Parking;
			}
		}
	}

	[StateBegin(JeepState.Canceling)]
	public void BeginCanceling() {
		if (occupants.Count <= 0) {
			route = new();
			routeIndex = 0;
			postGoal = GarageSpot;
			postState = JeepState.Parking;
			WaitingJeep = null;
			StateMachine.Transition(JeepState.FollowingRoute);
			if (someoneWaitingForJeep) {
				RequestNextJeep();
			}
		}
	}

	[StateUpdate(JeepState.Canceling)]
	public void CancelingUpdate(GameTime gameTime) {
		DateTime now = GameScene.Active.Model.IngameDate;
		if (now > lastDrop + TimeSpan.FromMinutes(2)) {
			Tourist t = RemoveFirstTourist();
			lastDrop = now;
			t.LeaveJeep();
			if (occupants.Count <= 0) {
				route = new();
				routeIndex = 0;
				postGoal = GarageSpot;
				postState = JeepState.Parking;
				WaitingJeep = null;
				StateMachine.Transition(JeepState.FollowingRoute);
				if (someoneWaitingForJeep) {
					RequestNextJeep();
				}
			}
		}
	}

	private void UpdateSrcRec() {
		Sprite.SourceRectangle = new Rectangle((int)dir * 64, 0, 64, 64);
	}
}
