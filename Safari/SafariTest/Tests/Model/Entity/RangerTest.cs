using Engine.Graphics.Stubs.Texture;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using NSubstitute;
using Safari.Components;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Model.Entities.Animals;
using Safari.Scenes;
using SafariTest.Utils;
using System.Text;

namespace SafariTest.Tests.Model.Entity;

[TestClass]
public class RangerTest : SimulationTest {
	private Ranger? ranger;
	private PrivateObject? rangerPO;
	private readonly GameTime unitGT = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

	[TestInitialize]
	public void InitRanger() {
		Ranger.DefaultTarget = null;

		ranger = new Ranger(new Vector2(700, 700));
		rangerPO = new PrivateObject(ranger);
		ranger.Bounds = new Vectangle(0, 0, 50, 50);
		Safari.Game.AddObject(ranger);
		RunOneFrame();
	}

	[TestMethod("Properties Test")]
	public void TestProps() {
		Assert.IsTrue(ranger!.CanHunt);
		Assert.IsNull(ranger.ChaseTarget);
		Assert.IsNull(ranger.TargetSpecies);
		Assert.AreEqual(RangerState.Wandering, ranger.State);
		Assert.IsTrue(ranger.VisibleAtNight);

		ranger.TargetSpecies = AnimalSpecies.Giraffe;
		Assert.AreEqual(AnimalSpecies.Giraffe, ranger.TargetSpecies);
		
		Ranger.DefaultTarget = AnimalSpecies.Lion;
		Assert.AreEqual(AnimalSpecies.Giraffe, ranger.TargetSpecies);

		ranger.TargetSpecies = null;
		Assert.AreEqual(AnimalSpecies.Lion, ranger.TargetSpecies);

		ranger.Fire();
		Assert.IsTrue(ranger.IsDead);
		RunOneFrame();
		CollectionAssert.DoesNotContain(GameScene.Active.GameObjects, ranger);
	}

	[TestMethod("Salary Test")]
	public void TestSalary() {
		Safari.Game.RemoveObject(GameScene.Active.Model.Level!);
		PrivateObject gsPO = new(GameScene.Active);
		gsPO.SetField("model", Substitute.ForPartsOf<GameModel>("test park", 100_000, GameDifficulty.Normal, new DateTime(2003, 11, 04)));
		ITexture2D staticBG = new NoopTexture2D(null, 3584, 2048);
		Model.Level = new Level(32, staticBG.Width / 32, staticBG.Height / 32, staticBG);
		Safari.Game.AddObject(Model.Level);
		RunOneFrame();

		Model.IngameDate.Returns(new DateTime(2025, 02, 01));
		int prevFunds = Model.Funds;

		Ranger ranger2 = new(Vector2.Zero);
		Assert.AreEqual(Ranger.SALARY, prevFunds - Model.Funds);

		Model.IngameDate.Returns(new DateTime(2025, 02, 15));
		prevFunds = Model.Funds;

		Ranger ranger3 = new(Vector2.Zero);
		Assert.AreEqual(Ranger.SALARY / 2, prevFunds - Model.Funds);

		Model.IngameDate.Returns(new DateTime(2025, 03, 01));

		prevFunds = Model.Funds;
		ranger2.Update(unitGT);
		Assert.AreEqual(Ranger.SALARY, prevFunds - Model.Funds);

		prevFunds = Model.Funds;
		ranger3.Update(unitGT);
		Assert.AreEqual(Ranger.SALARY, prevFunds - Model.Funds);
	}

	[TestMethod("Hunting Test")]
	public void TestHunting() {
		ranger!.TargetSpecies = AnimalSpecies.Lion;

		bool targetReached = false;
		ranger.NavCmp.ReachedTarget += (object? _, NavigationTargetEventArgs _) => {
			targetReached = true;
		};

		Lion lion = new(Vector2.Zero, Gender.Male);
		lion.Bounds = new Vectangle(0, 0, 50, 50);
		Safari.Game.AddObject(lion);
		RunOneFrame();

		EntityBoundsManager.RemoveEntity(lion);
		lion.Position = ranger.CenterPosition;
		lion.UpdateBounds();
		EntityBoundsManager.AddEntity(lion);

		GameScene.Active.Update(unitGT);

		Assert.IsTrue(targetReached);
		Assert.AreEqual(Model.IngameDate, ranger.LastSuccessfulHunt);
		Assert.IsFalse(lion.IsDead); // doesn't kill the animal if it's the only one in the park

		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.FalseUntil(() => ranger.CanHunt, TimeSpan.FromHours(Ranger.HUNT_COOLDOWN_HRS));
		Assert.IsTrue(ranger.CanHunt);
		Model.GameSpeed = GameSpeed.Slow;

		EntityBoundsManager.RemoveEntity(lion);
		lion.Position = new Vector2(1700, 1700);
		lion.UpdateBounds();
		EntityBoundsManager.AddEntity(lion);

		targetReached = false;
		ranger.TargetSpecies = null;

		Lion lion2 = new(ranger.CenterPosition, Gender.Male);
		lion2.Bounds = new Vectangle(0, 0, 50, 50);
		Safari.Game.AddObject(lion2);
		RunOneFrame();

		ranger.TargetSpecies = AnimalSpecies.Lion;
		GameScene.Active.Update(unitGT);

		Assert.IsTrue(targetReached);
		Assert.AreEqual(Model.IngameDate, ranger.LastSuccessfulHunt);
		Assert.IsTrue(lion2.IsDead);
		Assert.IsFalse(ranger.CanHunt);

		ranger.SightDistance = 10;
		ranger.ReachDistance = 3;
		rangerPO!.SetProperty("LastSuccessfulHunt", DateTime.MinValue);
		Assert.IsTrue(ranger.CanHunt);

		EntityBoundsManager.RemoveEntity(lion);
		lion.Position = ranger.CenterPosition + new Vector2((ranger.SightDistance - 3) * Model.Level!.TileSize);
		lion.UpdateBounds();
		EntityBoundsManager.AddEntity(lion);

		Assert.IsTrue(ranger.CanSee(lion));
		Assert.IsFalse(ranger.CanReach(lion));

		GameAssert.AreEqualInNFrames(RangerState.Chasing, () => ranger.State, 10);
		Assert.AreEqual(lion, ranger.ChaseTarget);
		Assert.AreEqual(lion, ranger.NavCmp.TargetObject);
		Assert.IsTrue(ranger.NavCmp.Moving);

		Poacher poacher = new(ranger.CenterPosition + new Vector2(ranger.SightDistance - 1) * Model.Level.TileSize);
		Safari.Game.AddObject(poacher);
		RunOneFrame();

		Assert.IsTrue(ranger.CanSee(poacher));
		Assert.IsFalse(ranger.CanReach(poacher));
		Assert.AreEqual(RangerState.Chasing, ranger.State);
		Assert.AreEqual(poacher, ranger.ChaseTarget);
		Assert.AreEqual(poacher, ranger.NavCmp.TargetObject);
		Assert.IsTrue(ranger.NavCmp.Moving);
		
		// spawn closer poacher
		Poacher poacher2 = new(ranger.CenterPosition + new Vector2(ranger.SightDistance - 2) * Model.Level.TileSize);
		Safari.Game.AddObject(poacher2);
		RunOneFrame();

		Assert.IsTrue(ranger.CanSee(poacher2));
		Assert.IsFalse(ranger.CanReach(poacher2));
		Assert.IsTrue(Vector2.Distance(ranger.CenterPosition, poacher2.CenterPosition) < Vector2.Distance(ranger.CenterPosition, poacher.CenterPosition));
		RunOneFrame();
		Assert.AreEqual(RangerState.Chasing, ranger.State);
		Assert.AreEqual(poacher2, ranger.ChaseTarget);
		Assert.AreEqual(poacher2, ranger.NavCmp.TargetObject);
		Assert.IsTrue(ranger.NavCmp.Moving);
	}
}
