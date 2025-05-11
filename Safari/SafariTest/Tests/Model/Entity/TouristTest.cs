using Safari.Model.Entities.Tourists;
using Microsoft.Xna.Framework;
using Safari.Model;
using Safari.Scenes;
using NSubstitute;
using SafariTest.Utils;
using Safari.Model.Entities.Animals;

namespace SafariTest.Tests.Model.Entity;

[TestClass]
public class TouristTest : SimulationTest {
	private Tourist SpawnT(Vector2 pos) {
		Tourist t = new Tourist(pos);
		Safari.Game.AddObject(t);
		RunOneFrame();
		return t;
	}

	[TestMethod]
	public void TouristStaticState() {
		// Queue init, queue update
		Tourist.Init(60);
		Assert.IsNotNull(Tourist.Queue);
		Assert.AreEqual(0, Tourist.Queue.Count);
		Tourist t1 = SpawnT(new(0, 0));
		CollectionAssert.Contains(Tourist.Queue, t1);

		double acc = 0.001;

		// Review system
		Assert.AreEqual(2.0f, Tourist.AvgRating, acc);
		Assert.AreEqual(0.7f, Tourist.SpawnRate, acc);
		EntitySpawner<Tourist> tSpawner = new EntitySpawner<Tourist>(0.1f);
		Tourist.Spawner = tSpawner;
		Tourist.UpdateSpawner();
		Assert.AreEqual(1 / 0.7f, tSpawner.Frequency, acc);
		for (int i = 0; i < Tourist.RatingMemory / 2; i++) {
			Tourist.AddReview(4.0f);
		}
		Assert.AreEqual(3.0f, Tourist.AvgRating, acc);
		Assert.AreEqual(1.5f, Tourist.SpawnRate, acc);
		Assert.AreEqual(1 / 1.5f, tSpawner.Frequency, acc);
	}

	[TestMethod]
	public void TouristBasics() {
		Engine.Game.Random = NSubstitute.Substitute.For<Random>();
		Engine.Game.Random.Next(4, 8).Returns(4);
		Engine.Game.Random.NextDouble().Returns(0.5);

		Assert.AreEqual(0, Model.TouristCount);
		Tourist.Init(60);
		EntitySpawner<Tourist> tSpawner = new EntitySpawner<Tourist>(0.1f);
		Tourist.Spawner = tSpawner;
		Tourist.UpdateSpawner();
		Tourist t1 = SpawnT(new(0, 0));
		Assert.AreEqual("Tourist", t1.DisplayName);
		Assert.AreEqual(4, t1.SightDistance);
		Assert.IsNotNull(t1.NavCmp);
		Assert.IsNotNull(t1.StateMachine);
		Assert.AreEqual(TouristState.Entering, t1.StateMachine.CurrentState);
		Assert.AreEqual(1, Model.TouristCount);

		double oldRating = Tourist.AvgRating;
		Tourist t2 = SpawnT(new(0, 0));
		t2.TourFinished();
		Assert.IsTrue(Tourist.AvgRating > oldRating);

		oldRating = Tourist.AvgRating;
		Tourist t3 = SpawnT(new(0, 0));
		t3.TourFailed();
		Assert.IsTrue(Tourist.AvgRating < oldRating);

		Jeep.RentFee = 100;
		Tourist t4 = SpawnT(new(0, 0));
		oldRating = Tourist.AvgRating;
		int oldMoney = Model.Funds;
		t4.Pay();
		t4.TourFinished();
		Assert.IsTrue(Tourist.AvgRating > oldRating);
		Assert.AreEqual(oldMoney + 100, Model.Funds);

		Jeep.RentFee = 6000;
		oldRating = Tourist.AvgRating;
		oldMoney = Model.Funds;
		Tourist t5 = SpawnT(new(0, 0));
		t5.Pay();
		t5.TourFinished();
		Assert.IsTrue(Tourist.AvgRating < oldRating);
		Assert.AreEqual(oldMoney + 6000, Model.Funds);
	}

	[TestMethod]
	public void TouristQueue() {
		Engine.Game.Random = NSubstitute.Substitute.For<Random>();
		Engine.Game.Random.Next(4, 8).Returns(4);
		Engine.Game.Random.NextDouble().Returns(0.5);

		Tourist.PickupSpot = new Point(3, 3);
		Vector2 PickupCenter = Model.Level.GetTileCenter(new Point(3, 3));
		Tourist.Init(60);
		EntitySpawner<Tourist> tSpawner = new EntitySpawner<Tourist>(0.1f);
		Tourist.Spawner = tSpawner;
		Tourist.UpdateSpawner();

		Tourist t1 = SpawnT(new Vector2(64, 96));
		Assert.AreEqual(PickupCenter, t1.NavCmp.TargetPosition);
		float distance = Vector2.Distance(PickupCenter, t1.Position);
		RunOneFrame();
		RunOneFrame();
		Assert.IsTrue(Vector2.Distance(PickupCenter, t1.Position) < distance);
		Model.GameSpeed = GameSpeed.Medium;
		GameAssert.TrueBefore(() => t1.StateMachine.CurrentState == TouristState.InQueue, TimeSpan.FromMinutes(30));
		Assert.AreEqual(PickupCenter, t1.Position);
		Tourist t2 = SpawnT(new Vector2(32, 96));
		GameAssert.TrueBefore(() => t2.StateMachine.CurrentState == TouristState.InQueue, TimeSpan.FromMinutes(30));
		Assert.AreEqual(PickupCenter + new Vector2(0, -(float)Tourist.QueueOffset), t2.Position);

		Tourist t3 = SpawnT(new Vector2(32, 96));
		t3.NavCmp.Speed = 0;
		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.TrueBefore(() => t3.StateMachine.CurrentState == TouristState.Leaving, TimeSpan.FromHours(18));
		Assert.AreEqual(TouristState.Leaving, t1.StateMachine.CurrentState);
		Assert.AreEqual(TouristState.Leaving, t2.StateMachine.CurrentState);
	}

	[TestMethod]
	public void TouristTour() {
		Engine.Game.Random = NSubstitute.Substitute.For<Random>();
		Engine.Game.Random.Next(4, 8).Returns(20);
		Engine.Game.Random.Next(5, 11).Returns(8);
		Engine.Game.Random.NextDouble().Returns(0.5);

		Vector2 PickupCenter = Model.Level.GetTileCenter(Tourist.PickupSpot);
		Tourist.Init(60);
		EntitySpawner<Tourist> tSpawner = new EntitySpawner<Tourist>(0.1f);
		Tourist.Spawner = tSpawner;
		Tourist.UpdateSpawner();

		Tourist t1 = SpawnT(PickupCenter - new Vector2(100, 0));
		Assert.AreEqual(20, t1.SightDistance);
		Tourist t2 = SpawnT(PickupCenter - new Vector2(100, 0));
		GameAssert.TrueBefore(() => t1.StateMachine.CurrentState == TouristState.InQueue && t2.StateMachine.CurrentState == TouristState.InQueue, TimeSpan.FromMinutes(30));
		RunOneFrame();
		Assert.AreEqual(TouristState.InQueue, t1.StateMachine.CurrentState);
		Jeep.RentFee = 1300;
		Jeep j = new Jeep(PickupCenter, 0);
		Safari.Game.AddObject(j);
		RunOneFrame();
		j.Position = PickupCenter; // force spawn at pickup
		j.StateMachine.Transition(JeepState.WaitingForTourists);
		RunOneFrame();
		Assert.AreEqual(TouristState.InJeep, t1.StateMachine.CurrentState);
		Assert.AreEqual(TouristState.InQueue, t2.StateMachine.CurrentState);
		GameAssert.TrueBefore(() => t2.StateMachine.CurrentState == TouristState.InJeep && t2.Vehicle == j, TimeSpan.FromMinutes(30));
		j.Position = j.Position + new Vector2(2, 0);
		RunOneFrame();
		Assert.AreEqual(j.Position, t1.Position);
		Assert.AreEqual(j.Position, t2.Position);
		j.StateMachine.Transition(JeepState.WaitingForNormalRoute);
		RunOneFrame();
		RunOneFrame();
		for (int i = 0; i < 30; i++) {
			Zebra z = new Zebra(t2.Position, Gender.Female);
			z.Bounds = new Engine.Helpers.Vectangle(-20, -20, 40, 40);
			Safari.Game.AddObject(z);
			RunOneFrame();
		}
		RunNFrames(10);
		double oldRating = Tourist.AvgRating;
		t1.TourFinished();
		t1.LeaveJeep();
		t2.TourFinished();
		t2.LeaveJeep();
		Assert.IsTrue(Tourist.AvgRating > oldRating);
		RunOneFrame();
		Assert.AreEqual(TouristState.Leaving, t1.StateMachine.CurrentState);
		Assert.AreEqual(TouristState.Leaving, t2.StateMachine.CurrentState);
		GameAssert.FalseBefore(() => t1.Loaded, TimeSpan.FromHours(2));
	}
}
