using Engine;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.Extensions;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Scenes;
using SafariTest.Utils;

namespace SafariTest.Tests.Helpers;

internal class DummyEntity(Vector2 pos) : Entity(pos) { }

[TestClass]
public class EntitySpawnerTest {
	private DateTime start;
	private DateTime current;
	private int spawnedCount = 0;
	private int goAddCount = 0;
	private GameModel model = null!;
	private event EventHandler<GameObject>? EntityCreated;

	[TestInitialize]
	public void InitSpawner() {
		start = DateTime.Now;
		spawnedCount = 0;

		Safari.Game.Random = new Random();

		// nullify game object additions
		GameScene gs = Substitute.ForPartsOf<GameScene>();
		gs.Configure()
			.When((GameScene gs) => gs.AddObject(Arg.Any<GameObject>()))
			.DoNotCallBase();

		gs.Configure()
			.When((GameScene gs) => gs.AddObject(Arg.Any<GameObject>()))
			.Do((CallInfo ci) => {
				spawnedCount++;
				goAddCount++;
				EntityCreated?.Invoke(this, ci.Arg<GameObject>());
			});

		// scene & game stuff
		PrivateType sm = new(typeof(SceneManager));
		sm.SetProperty("Active", gs);

		// model
		current = start;
		model = Substitute.ForPartsOf<GameModel>("test park", 10000, GameDifficulty.Normal, start);
		model.Configure().IngameDate.Returns((CallInfo _) => current);
		gs.Configure().Model.Returns((CallInfo _) => model);
	}
	
	[TestMethod("Entity Spawning Test")]
	public void TestSpawning() {
		GameTime gt = new();

		spawnedCount = 0;
		EntitySpawner<DummyEntity> spawner = new(3, 1, 0.1f);
		Assert.AreEqual(3, spawner.Frequency);
		Assert.AreEqual(1, spawner.BaseChance);
		Assert.AreEqual(0.1f, spawner.ChanceIncrease);
		Assert.AreEqual(1, spawner.CurrentChance);
		Assert.IsNull(spawner.SpawnArea);

		spawner.SpawnArea = new Rectangle(0, 0, 400, 400);

		spawner.Update(gt);
		Assert.AreEqual(1, spawnedCount);
		Assert.AreEqual(model.IngameDate, spawner.LastSpawnAttempt);
		Assert.AreEqual(model.IngameDate, spawner.LastSuccessfulSpawn);
		Assert.AreEqual(0, spawner.CurrentChance);

		spawnedCount = 0;
		spawner.Update(gt);
		Assert.AreEqual(0, spawnedCount);
		current += TimeSpan.FromHours(2);
		spawner.Update(gt);
		Assert.AreEqual(0, spawnedCount);
		current += TimeSpan.FromHours(1);
		spawner.Update(gt);
		Assert.AreEqual(1, spawnedCount);
		Assert.AreEqual(model.IngameDate, spawner.LastSpawnAttempt);

		spawnedCount = 0;
		spawner.BaseChance = 0;
		current += TimeSpan.FromHours(3);
		spawner.Update(gt);
		Assert.AreEqual(0, spawnedCount);
		Assert.AreEqual(0.1f, spawner.CurrentChance);
		Assert.AreEqual(model.IngameDate, spawner.LastSpawnAttempt);

		spawner.Frequency = 2;
		current += TimeSpan.FromHours(2);
		spawner.Update(gt);
		Assert.AreEqual(model.IngameDate, spawner.LastSpawnAttempt);
	}

	[TestMethod("Spawning Conditions Test")]
	public void TestConditionals() {
		GameTime gt = new();
		goAddCount = 0;
		EntitySpawner<DummyEntity> spawner = new(1, 1, 0.1f);
		spawner.SpawnArea = new Rectangle(0, 0, 400, 400);

		spawner.EntityLimit = 2;
		spawner.EntityCount = () => goAddCount;
		spawner.BaseChance = 1;

		spawnedCount = 0;
		current += TimeSpan.FromHours(1);
		spawner.Update(gt);
		Assert.AreEqual(1, spawnedCount);
		current += TimeSpan.FromHours(1);
		spawner.Update(gt);
		Assert.AreEqual(2, spawnedCount);
		current += TimeSpan.FromHours(1);
		spawner.Update(gt);
		Assert.AreEqual(2, spawnedCount);

		spawner.EntityLimit = 10;
		current += TimeSpan.FromHours(1);
		spawner.Update(gt);
		Assert.AreEqual(3, spawnedCount);

		spawner.ExtraCondition = () => false;
		current += TimeSpan.FromHours(1);
		spawner.Update(gt);
		Assert.AreEqual(3, spawnedCount);

		spawner.ExtraCondition = () => true;
		current += TimeSpan.FromHours(1);
		spawner.Update(gt);
		Assert.AreEqual(4, spawnedCount);

		spawner.Active = false;
		current += TimeSpan.FromHours(1);
		spawner.Update(gt);
		Assert.AreEqual(4, spawnedCount);
		spawner.Active = true;

		Vector2? placement = null;
		EntityCreated += (object? sender, GameObject go) => {
			placement = go.Position;
		};
		spawner.SpawnArea = new Rectangle(50, 50, 0, 0);
		current += TimeSpan.FromHours(1);
		spawner.Update(gt);
		Assert.AreEqual(5, spawnedCount);
		Assert.IsNotNull(placement);
		Assert.AreEqual(new Vector2(50, 50), placement);
	}
}
