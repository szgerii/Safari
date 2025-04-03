using Engine.Components;
using Engine.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Components;
using Safari.Model;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Objects.Entities.Tourists;

public enum JeepState {
	Parking,
	Entering,
	WaitingForTourists,
	WaitingForNormalRoute,
	WaitingForReturnRoute,
	FollowingRoute,
	Emptying
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
	public const int STARTING_JEEPS = 2;
	/// <summary>
	/// Once the hours specified here have passed, a jeep can start its journey even if its not full
	/// This also means that nighttime - MAX_WAITING_HOURS is the last time a jeep can start gathering tourists
	/// </summary>
	public const int MAX_WAITING_HOURS = 3;

	// TODO tourist list later
	private int occupants = 0;
	private static Queue<Jeep> garage = new Queue<Jeep>();
	private static bool jeepEntering = false;

	private int routeIndex = 0;
	private List<Point> route = new List<Point>();
	private Point goal;
	private Point postGoal;
	private JeepState postState;
	private JeepDirection dir = JeepDirection.Right;

	private static Level CurrentLevel => GameScene.Active.Model.Level;

	public StateMachineCmp<JeepState> StateMachine { get; private set; } = new(JeepState.Parking);

	/// <summary>
	/// The jeep that is currently being filled with tourists (or null)
	/// </summary>
	public static Jeep WaitingJeep { get; private set; }

	public static Point GarageSpot { get; set; }
	public static Point PickUpSpot { get; set; }
	public static Point DropOffSpot { get; set; }

	/// <summary>
	/// The amount a tourist has to pay in order to participate in the tour
	/// A high rent fee could make some tourists dissatisfied with the ride
	/// </summary>
	public static int RentFee {
		get; set;
	}

	/// <summary>
	/// Invoked when a jeep has returned to the waiting area
	/// </summary>
	public event EventHandler Returned;

	static Jeep() {
		DebugMode.AddFeature(new ExecutedDebugFeature("request-jeep", () => RequestNextJeep()));

		DebugMode.AddFeature(new ExecutedDebugFeature("fill-jeep", () => {
			for (int i = 0; i < Jeep.CAPACITY; i++) {
				if (WaitingJeep != null) {
					WaitingJeep.AddTourist();
				}
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("add-jeep", () => {
			Jeep.SpawnJeep();
		}));
	}

	public Jeep(Vector2 pos) : base(pos) {
		DisplayName = "Jeep";
		// buggy, because it does not account for sprite origin :(
		//LightEntityCmp lightCmp = new LightEntityCmp(CurrentLevel, 2);
		//Attach(lightCmp);
		Sprite = new SpriteCmp(Game.ContentManager.Load<Texture2D>("Assets/Jeep/JeepBrown"));
		Sprite.LayerDepth = 0.4f;
		Sprite.YSortEnabled = true;
		UpdateSrcRec();
		Sprite.Origin = new Vector2(36, 36); // just by the 'vibes'
		NavCmp.AccountForBounds = false;
		NavCmp.Speed = 150f;
		NavCmp.StopOnTargetReach = true;
		ReachDistance = 0;
		Attach(Sprite);
		Attach(StateMachine);
	}

	public static void SpawnJeep() {
		Game.AddObject(new Jeep(CurrentLevel.GetTileCenter(Jeep.GarageSpot)));
	}

	public static void RequestNextJeep() {
		// TODO obvi two way events with the tourist queue
		// Pop the queue and transition it to Entering
		if (!jeepEntering && WaitingJeep == null && garage.Count > 0) {
			Jeep next = garage.Dequeue();
			next.StateMachine.Transition(JeepState.Entering);
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

	public void AddTourist() {
		// TODO rewrite with real tourists 
		if (StateMachine.CurrentState == JeepState.WaitingForTourists) {
			occupants++;
			if (occupants >= CAPACITY) {
				StateMachine.Transition(JeepState.WaitingForNormalRoute);
				goal = CurrentLevel.Network.End;
				postGoal = DropOffSpot;
				postState = JeepState.Emptying;
			}
		}
	}

	public void RemoveTourist() {
		// TODO rewrite with real tourists
		if (StateMachine.CurrentState == JeepState.Emptying) {
			occupants--;
			if (occupants <= 0) {
				StateMachine.Transition(JeepState.WaitingForReturnRoute);
				goal = CurrentLevel.Network.Start;
				postGoal = GarageSpot;
				postState = JeepState.Parking;
			}
		}
	}

	[StateBegin(JeepState.Parking)]
	public void BeginParking() {
		garage.Enqueue(this);
	}

	[StateBegin(JeepState.Entering)]
	public void BeginEntering() {
		jeepEntering = true;
		NavCmp.TargetPosition = CurrentLevel.GetTileCenter(PickUpSpot);
		NavCmp.Moving = true;
		NavCmp.ReachedTarget += PickUpReached;
		dir = JeepDirection.Right;
	}

	public void PickUpReached(object sender, ReachedTargetEventArgs e) {
		StateMachine.Transition(JeepState.WaitingForTourists);
	}

	[StateEnd(JeepState.Entering)]
	public void EndEntering() {
		jeepEntering = false;
		NavCmp.ReachedTarget -= PickUpReached;
	}

	[StateBegin(JeepState.WaitingForTourists)]
	public void BeginWaitingForTourists() {
		WaitingJeep = this;
	}

	[StateBegin(JeepState.WaitingForNormalRoute)]
	public void BeginWaitingForNormalRoute() {
		RequestNormalRoute();
	}

	[StateBegin(JeepState.WaitingForReturnRoute)]
	public void BeginWaitingForReturnRoute() {
		RequestReturnRoute();
	}

	public void OnRoadChanged(object sender, EventArgs e) {
		if (StateMachine.CurrentState == JeepState.WaitingForNormalRoute) {
			RequestNormalRoute();
		} else if (StateMachine.CurrentState == JeepState.WaitingForReturnRoute) {
			RequestReturnRoute();
		}
	}

	public void RequestNormalRoute() {
		route = CurrentLevel.Network.RandomRoute;
		if (route.Count > 0) {
			WaitingJeep = null;
			StateMachine.Transition(JeepState.FollowingRoute);
		}
	}

	public void RequestReturnRoute() {
		route = CurrentLevel.Network.ReturnRoute;
		if (route.Count > 0) {
			StateMachine.Transition(JeepState.FollowingRoute);
		}
	}

	[StateBegin(JeepState.FollowingRoute)]
	public void BeginFollowingRoute() {
		routeIndex = 0;
		NavCmp.ReachedTarget += CheckPointReached;
		NavCmp.TargetPosition = CurrentLevel.GetTileCenter(route[0]);
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

	public void CheckPointReached(object sender, ReachedTargetEventArgs e) {
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

	public void PostGoalReached(object sender, ReachedTargetEventArgs e) {
		NavCmp.ReachedTarget -= PostGoalReached;
		NavCmp.StopOnTargetReach = true;
		StateMachine.Transition(postState);
	}

	[StateBegin(JeepState.Emptying)]
	public void BeginEmptying() {
		// TODO this is debug lolol
		for (int i = 0; i < 4; i++) {
			RemoveTourist();
		}
	}

	private void UpdateSrcRec() {
		Sprite.SourceRectangle = new Rectangle((int)dir * 64, 0, 64, 64);
	}
}
