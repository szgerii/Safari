using Engine.Helpers;
using Microsoft.Xna.Framework;
using NSubstitute;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Model.Entities.Animals;
using Safari.Scenes;
using SafariTest.Utils;

namespace SafariTest.Tests.Model.Entity;

[TestClass]
public class PoacherTest : SimulationTest {
	private Poacher? poacher;
	private readonly GameTime gt = new();

	[TestInitialize]
	public void InitPoacher() {
		Safari.Game.Random = Substitute.For<Random>();

		poacher = Substitute.ForPartsOf<Poacher>(new Vector2(500, 500));
		poacher.Bounds = new Vectangle(0, 0, 50, 50);
		Safari.Game.AddObject(poacher);
		RunOneFrame();
	}

	[TestMethod("Properties Test")]
	public void TestProps() {
		Assert.IsNull(poacher!.CaughtAnimal);
		Assert.AreEqual(PoacherState.Wandering, poacher.State);
		Assert.IsFalse(poacher.Visible);
		Assert.AreEqual(typeof(PoacherState), poacher.StateMachine.BaseType);
		Assert.AreEqual("Poacher", poacher.DisplayName);

		poacher.Reveal();
		Assert.IsTrue(poacher.Visible);
	}

	[TestMethod("Chasing Test")]
	public void TestChasing() {
		Vector2 oldPos = poacher!.Position;
		GameAssert.TrueInNFrames(() => poacher.NavCmp.Moving, 5);
		Assert.IsNotNull(poacher.NavCmp.TargetPosition);
		RunOneFrame();
		Assert.AreNotEqual(oldPos, poacher.Position);

		Vector2 oldTarget = poacher.NavCmp.TargetPosition.Value;
		oldPos = poacher.Position;
		Model.GameSpeed = GameSpeed.Fast;
		Safari.Game.Random = new Random();
		GameAssert.AreNotEqualBefore(oldPos, () => poacher.Position, TimeSpan.FromDays(1));
		GameAssert.AreNotEqualBefore(oldTarget, () => poacher.NavCmp.TargetPosition, TimeSpan.FromDays(1));
		Model.GameSpeed = GameSpeed.Slow;
		Safari.Game.Random = Substitute.For<Random>();

		Zebra animal = Substitute.ForPartsOf<Zebra>(new Vector2(poacher.SightArea.Right - 1, poacher.SightArea.Bottom - 1), Gender.Male);
		animal.Bounds = new Vectangle(0, 0, 50, 50);
		animal.When((Zebra animal) => animal.Update(Arg.Any<GameTime>())).DoNotCallBase();
		Safari.Game.AddObject(animal);
        RunOneFrame();

		poacher.GetEntitiesInSight().Returns([animal]);
		
		RunOneFrame();
		Assert.AreEqual(PoacherState.Chasing, poacher.State);
		Assert.AreEqual(animal, poacher.ChaseTarget);
		Assert.AreEqual(animal, poacher.NavCmp.TargetObject);

		float oldDist = Vector2.DistanceSquared(poacher.CenterPosition, animal.CenterPosition);
		GameAssert.TrueInNFrames(() => Vector2.DistanceSquared(poacher.CenterPosition, animal.CenterPosition) < oldDist, 5);
		Assert.AreEqual(PoacherState.Chasing, poacher.State);
	}

	[TestMethod("Successful Smuggling Test")]
	public void TestSuccessfulSmuggle() {
		Zebra animal = Substitute.ForPartsOf<Zebra>(poacher!.CenterPosition, Gender.Male);
		animal.When((Zebra animal) => animal.Update(Arg.Any<GameTime>())).DoNotCallBase();
		animal.Bounds = new Vectangle(0, 0, 50, 50);
		Safari.Game.AddObject(animal);

		poacher.GetEntitiesInSight().Returns([animal]);

		// add another animal so the game doesnt enter the lose state
		Zebra animal2 = new(poacher.CenterPosition + new Vector2(poacher.ReachDistance + 1 * Model.Level!.TileSize), Gender.Male);
		Safari.Game.AddObject(animal2);

		// force smuggling
		Safari.Game.Random!.NextSingle().Returns(1f);
		
		RunOneFrame();

		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.AreEqualBefore(PoacherState.Smuggling, () => poacher.State, TimeSpan.FromDays(1));
		Model.GameSpeed = GameSpeed.Slow;
		RunOneFrame();

		Assert.IsNull(poacher.ChaseTarget);
		Assert.IsNull(poacher.NavCmp.TargetObject);
		Assert.IsNotNull(poacher.NavCmp.TargetPosition);
		Assert.AreEqual(animal, poacher.CaughtAnimal);
		Assert.IsTrue(animal.IsCaught);
		Assert.IsNull(animal.Group);

		// escaping

		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.TrueBefore(() => poacher.IsDead, TimeSpan.FromDays(1));
		Model.GameSpeed = GameSpeed.Slow;

		CollectionAssert.Contains(GameScene.Active.GameObjects, poacher);
		CollectionAssert.Contains(GameScene.Active.GameObjects, animal);
		RunOneFrame();
		CollectionAssert.DoesNotContain(GameScene.Active.GameObjects, poacher);
		CollectionAssert.DoesNotContain(GameScene.Active.GameObjects, animal);
	}

	[TestMethod("Failed Smuggling Test")]
	public void TestFailedSmuggle() {
		Zebra animal = Substitute.ForPartsOf<Zebra>(poacher!.CenterPosition, Gender.Male);
		animal.When((Zebra animal) => animal.Update(Arg.Any<GameTime>())).DoNotCallBase();
		animal.Bounds = new Vectangle(0, 0, 50, 50);
		Safari.Game.AddObject(animal);
		
		// force smuggling
		Safari.Game.Random!.NextSingle().Returns(1f);

		RunOneFrame();

		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.AreEqualBefore(PoacherState.Smuggling, () => poacher.State, TimeSpan.FromDays(1));
		Model.GameSpeed = GameSpeed.Slow;
		RunOneFrame();

		Assert.AreEqual(PoacherState.Smuggling, poacher.State);
		Assert.AreEqual(animal, poacher.CaughtAnimal);

		// escaping

		poacher.Die();

		Assert.IsFalse(animal.IsCaught);
		Assert.IsFalse(animal.IsDead);
		Assert.AreEqual(animal.Position, poacher.Position);
		Assert.IsNull(poacher.CaughtAnimal);

		RunOneFrame();

		CollectionAssert.Contains(GameScene.Active.GameObjects, animal);
		CollectionAssert.DoesNotContain(GameScene.Active.GameObjects, poacher);
	}

	[TestMethod("Hunting Test")]
	public void TestHunt() {
		Zebra animal = Substitute.ForPartsOf<Zebra>(poacher!.CenterPosition, Gender.Male);
		animal.When((Zebra animal) => animal.Update(Arg.Any<GameTime>())).DoNotCallBase();
		animal.Bounds = new Vectangle(0, 0, 50, 50);
		Safari.Game.AddObject(animal);

		// add another animal so the game doesnt enter the lose state
		Zebra animal2 = new(poacher.CenterPosition + new Vector2(poacher.ReachDistance + 1 * Model.Level!.TileSize), Gender.Male);
		Safari.Game.AddObject(animal2);

		RunOneFrame();

		// force shooting
		Safari.Game.Random!.NextSingle().Returns(0f);

		RunOneFrame();

		Assert.IsTrue(animal.IsDead);
		Assert.AreEqual(PoacherState.Wandering, poacher.State);
	}
}
