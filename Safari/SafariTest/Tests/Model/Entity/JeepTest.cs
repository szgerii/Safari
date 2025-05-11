using Safari.Components;
using Safari.Model;
using Safari.Model.Entities.Tourists;
using SafariTest.Utils;
using Microsoft.Xna.Framework;
using Safari.Model.Tiles;

namespace SafariTest.Tests.Model.Entity;

[TestClass]
public class JeepTest : SimulationTest {
	[TestMethod]
	public void JeepBasics() {
		Jeep.Init(200);
		Assert.AreEqual(200, Jeep.RentFee);
		Assert.IsNull(Jeep.WaitingJeep);

		Assert.AreEqual(0, Model.JeepCount);
		Jeep j = new Jeep(new Microsoft.Xna.Framework.Vector2(0, 0), 0);
		Assert.AreEqual("Jeep", j.DisplayName);
		Safari.Game.AddObject(j);
		RunOneFrame();
		Assert.AreEqual(1, Model.JeepCount);
		Assert.IsTrue(j.HasComponent<LightEntityCmp>());
		Assert.IsTrue(j.HasComponent<NavigationCmp>());
		Assert.IsTrue(j.HasComponent<StateMachineCmp<JeepState>>());
		Assert.AreEqual(JeepState.Parking, j.StateMachine.CurrentState);
		Safari.Game.RemoveObject(j);
		RunOneFrame();
		Assert.AreEqual(0, Model.JeepCount);
		Jeep.SpawnJeep();
		RunOneFrame();
		Assert.AreEqual(1, Model.JeepCount);
	}

	[TestMethod]
	public void JeepWait() {
		Tourist.Init(60);
		EntitySpawner<Tourist> tSpawner = new EntitySpawner<Tourist>(0.1f) { Active = false };
		Tourist.Spawner = tSpawner;
		Tourist.UpdateSpawner();

		bool eventInvoked = false;

		Jeep.Init(200);
		Jeep.SpawnJeep();
		RunOneFrame();
		Assert.IsNull(Jeep.WaitingJeep);

		Jeep.JeepReadyToFill += (object? sender, EventArgs e) => { eventInvoked = true; };

		Jeep.RequestNextJeep();
		RunOneFrame();
		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.IsNotNullBefore(() => Jeep.WaitingJeep, TimeSpan.FromHours(1));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;
		Assert.IsTrue(eventInvoked);

		Game.RunOneFrame();
		Jeep j1 = Jeep.WaitingJeep!;
		Assert.AreEqual(JeepState.WaitingForTourists, j1.StateMachine.CurrentState);

		Point p2 = Model.Level!.Network.Start + new Microsoft.Xna.Framework.Point(2, 0);
		Model.Level.ClearTile(p2);
		Game.RunOneFrame();
		j1.AddTourist(new Tourist(new Vector2(40, 40)));
		j1.AddTourist(new Tourist(new Vector2(40, 40)));
		j1.AddTourist(new Tourist(new Vector2(40, 40)));
		j1.AddTourist(new Tourist(new Vector2(40, 40)));
		Game.RunOneFrame();
		Assert.AreEqual(JeepState.WaitingForNormalRoute, j1.StateMachine.CurrentState);
		
		Model.Level.SetTile(p2, new Road());
		RunOneFrame();
		Assert.AreEqual(JeepState.FollowingRoute, j1.StateMachine.CurrentState);

		// it works the other way around too
		Jeep.RequestNextJeep();
		RunOneFrame();
		Jeep.SpawnJeep();
		RunOneFrame();
		Assert.IsNull(Jeep.WaitingJeep);
		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.IsNotNullBefore(() => Jeep.WaitingJeep, TimeSpan.FromHours(1));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;
		Jeep j2 = Jeep.WaitingJeep!;

		j2.AddTourist(new Tourist(new Vector2(40, 40)));
		Game.RunOneFrame();
		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.AreEqualBefore(JeepState.FollowingRoute , () => j2.StateMachine.CurrentState, TimeSpan.FromHours(Jeep.MAX_WAITING_HOURS + 0.5));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;

		Jeep.RequestNextJeep();
		RunOneFrame();
		Jeep.SpawnJeep();
		RunOneFrame();
		Assert.IsNull(Jeep.WaitingJeep);
		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.IsNotNullBefore(() => Jeep.WaitingJeep, TimeSpan.FromHours(1));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;
		Jeep j3 = Jeep.WaitingJeep!;
		Assert.AreEqual(JeepState.WaitingForTourists, j3.StateMachine.CurrentState);

		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.AreEqualBefore(JeepState.Parking, () => j3.StateMachine.CurrentState, TimeSpan.FromHours(18));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;

		Assert.IsFalse(Model.IsDaytime);

		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.TrueBefore(() => Model.IsDaytime, TimeSpan.FromHours(18));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;

		Jeep.RequestNextJeep();
		RunOneFrame();
		Jeep.SpawnJeep();
		RunOneFrame();
		Assert.IsNull(Jeep.WaitingJeep);
		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.IsNotNullBefore(() => Jeep.WaitingJeep, TimeSpan.FromHours(1));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;
		Jeep j4 = Jeep.WaitingJeep!;
		Assert.AreEqual(JeepState.WaitingForTourists, j4.StateMachine.CurrentState);

		Model.Level.ClearTile(p2);
		Game.RunOneFrame();
		j4.AddTourist(new Tourist(new Vector2(40, 40)));
		j4.AddTourist(new Tourist(new Vector2(40, 40)));
		j4.AddTourist(new Tourist(new Vector2	(40, 40)));
		j4.AddTourist(new Tourist(new Vector2(40, 40)));
		Game.RunOneFrame();
		Assert.AreEqual(JeepState.WaitingForNormalRoute, j4.StateMachine.CurrentState);

		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.FalseBefore(() => Model.IsDaytime, TimeSpan.FromHours(18));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;

		Assert.AreEqual(JeepState.Canceling, j4.StateMachine.CurrentState);
		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.AreEqualBefore(JeepState.FollowingRoute, () => j4.StateMachine.CurrentState, TimeSpan.FromHours(1.5));
		GameAssert.AreEqualBefore(JeepState.Parking, () => j4.StateMachine.CurrentState, TimeSpan.FromHours(1));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;
	}

	[TestMethod]
	public void JeepFailTour() {
		Tourist.Init(60);
		EntitySpawner<Tourist> tSpawner = new EntitySpawner<Tourist>(0.1f);
		Tourist.Spawner = tSpawner;
		Tourist.UpdateSpawner();
		Jeep.Init(200);
		Point p1 = Model.Level!.Network.Start + new Microsoft.Xna.Framework.Point(1, 0);
		Point p2 = Model.Level.Network.Start + new Microsoft.Xna.Framework.Point(2, 0);
		Model.GameSpeed = GameSpeed.Slow;

		Jeep.RequestNextJeep();
		RunOneFrame();
		Jeep.SpawnJeep();
		RunOneFrame();
		Assert.IsNull(Jeep.WaitingJeep);
		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.IsNotNullBefore(() => Jeep.WaitingJeep, TimeSpan.FromHours(1));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;
		Jeep j1 = Jeep.WaitingJeep!;
		Assert.AreEqual(JeepState.WaitingForTourists, j1.StateMachine.CurrentState);

		Game.RunOneFrame();
		Tourist t1 = new Tourist(new Vector2(40, 40));
		Tourist t2 = new Tourist(new Vector2(40, 40));
		Tourist t3 = new Tourist(new Vector2(40, 40));
		Tourist t4 = new Tourist(new Vector2(40, 40));
		Engine.Game.AddObject(t1);
		Engine.Game.AddObject(t2);
		Engine.Game.AddObject(t3);
		Engine.Game.AddObject(t4);
		j1.AddTourist(t1);
		j1.AddTourist(t2);
		j1.AddTourist(t3);
		j1.AddTourist(t4);
		Game.RunOneFrame();
		Assert.AreEqual(4, Model.TouristCount);
		Assert.AreEqual(JeepState.FollowingRoute, j1.StateMachine.CurrentState);

		Model.Level.ClearTile(p2);
		GameAssert.AreEqualBefore(JeepState.WaitingForEscapeRoute, () => j1.StateMachine.CurrentState, TimeSpan.FromMinutes(20));

		Model.Level.SetTile(p2, new Road());
		Game.RunOneFrame();
		Assert.AreEqual(JeepState.FollowingRoute, j1.StateMachine.CurrentState);

		Model.Level.ClearTile(p1);
		Model.Level.ClearTile(p2);
		Game.RunOneFrame();
		Game.RunOneFrame();
		Assert.AreEqual(JeepState.Parking, j1.StateMachine.CurrentState);
		Assert.AreEqual(0, Model.TouristCount);
	}

	[TestMethod]
	public void JeepReturnFromTour() {
		Tourist.Init(60);
		EntitySpawner<Tourist> tSpawner = new EntitySpawner<Tourist>(0.1f);
		Tourist.Spawner = tSpawner;
		Tourist.UpdateSpawner();
		Jeep.Init(200);
		Point p2 = Model.Level!.Network.Start + new Microsoft.Xna.Framework.Point(2, 0);
		Model.GameSpeed = GameSpeed.Slow;

		Jeep.RequestNextJeep();
		RunOneFrame();
		Jeep.SpawnJeep();
		RunOneFrame();
		Assert.IsNull(Jeep.WaitingJeep);
		Model.GameSpeed = Safari.Model.GameSpeed.Fast;
		GameAssert.IsNotNullBefore(() => Jeep.WaitingJeep, TimeSpan.FromHours(1));
		Model.GameSpeed = Safari.Model.GameSpeed.Slow;
		Jeep j1 = Jeep.WaitingJeep!;
		Assert.AreEqual(JeepState.WaitingForTourists, j1.StateMachine.CurrentState);

		j1.AddTourist(new Tourist(new Vector2(40, 40)));
		j1.AddTourist(new Tourist(new Vector2(40, 40)));
		j1.AddTourist(new Tourist(new Vector2(40, 40)));
		j1.AddTourist(new Tourist(new Vector2(40, 40)));
		Game.RunOneFrame();
		Assert.AreEqual(JeepState.FollowingRoute, j1.StateMachine.CurrentState);

		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.AreEqualBefore(JeepState.Emptying, () => j1.StateMachine.CurrentState, TimeSpan.FromHours(5));
		Model.GameSpeed = GameSpeed.Slow;

		Model.Level.ClearTile(p2);
		RunOneFrame();

		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.AreEqualBefore(JeepState.WaitingForReturnRoute, () => j1.StateMachine.CurrentState, TimeSpan.FromHours(5));
		Model.GameSpeed = GameSpeed.Slow;

		Model.Level.SetTile(p2, new Road());
		RunOneFrame();
		Assert.AreEqual(JeepState.FollowingRoute, j1.StateMachine.CurrentState);

		RunOneFrame();
		Model.Level.ClearTile(p2);
		GameAssert.AreEqualBefore(JeepState.WaitingForEscapeRoute, () => j1.StateMachine.CurrentState, TimeSpan.FromMinutes(20));

		Model.Level.SetTile(p2, new Road());
		RunOneFrame();
		Assert.AreEqual(JeepState.FollowingRoute, j1.StateMachine.CurrentState);

		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.AreEqualBefore(JeepState.Parking, () => j1.StateMachine.CurrentState, TimeSpan.FromHours(5));
		Model.GameSpeed = GameSpeed.Slow;
	}
}
