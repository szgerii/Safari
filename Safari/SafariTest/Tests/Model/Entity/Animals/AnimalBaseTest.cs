using Microsoft.Xna.Framework;
using NSubstitute;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Model.Entities.Animals;
using Safari.Model.Tiles;
using SafariTest.Utils;

namespace SafariTest.Tests.Model.Entity.Animals;

[TestClass]
public class AnimalBaseTest : SimulationTest {
	private Animal? animal;
	private PrivateObject? animalPO, groupPO;
	private readonly PrivateType animalPT = new(typeof(Animal));
	private readonly GameTime unitGT = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

	[TestInitialize]
	public void InitAnimal() {
		animal = AddAnimalToGame();
		animalPO = new(animal);
		groupPO = new(animal.Group);
	}

	[TestMethod("Properties Test")]
	public void TestProperties() {
		Assert.AreEqual(0, animal!.Age);
		Assert.IsFalse(animal.IsDead);
		Assert.IsFalse(animal.IsCaught);
		Assert.IsTrue(animal.HungerDecay > 0);
		Assert.IsTrue(animal.ThirstDecay > 0);
		Assert.AreEqual(Gender.Female, animal.Gender);
		Assert.IsNotNull(animal.Group);
		Assert.AreEqual(AnimalGroupState.Wandering, animal.State);
		Assert.IsFalse(animal.IsHungry);
		Assert.IsFalse(animal.IsThirsty);

		Assert.IsFalse(animal.VisibleAtNight);
		animal.HasChip = true;
		Assert.IsTrue(animal.HasChip);
		Assert.IsTrue(animal.VisibleAtNight);

		int prevBalance = Model.Funds;
		animal.Sell();
		Assert.AreEqual(prevBalance + animal.Price, Model.Funds);
	}

	[TestMethod("Aging Test")]
	public void TestAge() {
		DateTime birthTime = (DateTime)animalPO!.GetField("birthTime")!;

		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.TrueUntil(() => animal!.Age == 0, birthTime + TimeSpan.FromDays(1));
		Model.GameSpeed = GameSpeed.Slow;
		RunOneFrame();
		Assert.AreEqual(1, animal!.Age);

		DateTime newBirth = Model.IngameDate - TimeSpan.FromDays(Animal.MAX_AGE + 1);
		animalPO!.SetField("birthTime", newBirth);
		Assert.AreEqual(Animal.MAX_AGE + 1, animal.Age);
		RunOneFrame();
		Assert.IsTrue(animal.IsDead);
	}

	[TestMethod("Smuggling Test")]
	public void TestSmuggling() {
		Assert.IsFalse(animal!.IsCaught);

		animal.Catch();

		Assert.IsTrue(animal.IsCaught);
		Assert.IsNull(animal.Group);
		Assert.ThrowsException<InvalidOperationException>(animal.Catch);

		animal.Release(new Vector2(400, 100));

		Assert.IsFalse(animal.IsCaught);
		Assert.IsNotNull(animal.Group);
		Assert.AreEqual(1, animal.Group.Size);
		Assert.AreEqual(new Vector2(400, 100), animal.Position);
		Assert.ThrowsException<InvalidOperationException>(() => animal.Release(Vector2.Zero));
	}

	[TestMethod("Mating Test")]
	public void TestMating() {
		Assert.IsFalse(animal!.IsMature);
		Assert.IsFalse(animal.CanMate);
		Assert.ThrowsException<InvalidOperationException>(animal.Mate);

		DateTime newBirth = Model.IngameDate - TimeSpan.FromDays(Animal.MATURE_AGE);
		animalPO!.SetField("birthTime", newBirth);
		Assert.IsTrue(animal.IsMature);
		Assert.IsTrue(animal.CanMate);

		animal.Mate();
		Assert.IsFalse(animal.CanMate);
	}

	[TestMethod("Hunger and Thirst Test")]
	public void TestHungerAndThirst() {
		Animal animNotInGame = Substitute.ForPartsOf<Animal>(Vector2.Zero, AnimalSpecies.Zebra, Gender.Female);
		Assert.AreEqual(100, animNotInGame.HungerLevel);
		Assert.AreEqual(100, animNotInGame.ThirstLevel);

		animNotInGame.Feed(unitGT);
		animNotInGame.Drink(unitGT);
		Assert.AreEqual(100, animNotInGame.HungerLevel);
		Assert.AreEqual(100, animNotInGame.ThirstLevel);

		float prevHunger = animal!.HungerLevel, prevThirst = animal.ThirstLevel;

		RunOneFrame();

		Assert.IsTrue(animal.HungerLevel < prevHunger);
		Assert.IsTrue(animal.ThirstLevel < prevThirst);

		animalPO!.SetProperty("HungerLevel", 100f - (2f * Animal.FEEDING_SPEED));
		prevHunger = animal.HungerLevel;
		animal.Feed(unitGT);
		AssertFloatEquals(prevHunger + Animal.FEEDING_SPEED, animal.HungerLevel);

		animalPO!.SetProperty("ThirstLevel", 100f - (2f * Animal.DRINKING_SPEED));
		prevThirst = animal.ThirstLevel;
		animal.Drink(unitGT);
		AssertFloatEquals(prevThirst + Animal.DRINKING_SPEED, animal.ThirstLevel);

		Animal herb = Substitute.ForPartsOf<Animal>(new Vector2(300, 200), AnimalSpecies.Zebra, Gender.Female);
		Animal carn = Substitute.ForPartsOf<Animal>(new Vector2(800, 800), AnimalSpecies.Lion, Gender.Female);
		PrivateObject herbPO = new(herb);
		PrivateObject carnPO = new(carn);

		float herbHungerThreshold = (int)animalPT.GetField("HUNGER_THRESHOLD_HERB")!;
		float carnHungerThreshold = (int)animalPT.GetField("HUNGER_THRESHOLD_CARN")!;
		float thirstThreshold = (int)animalPT.GetField("THIRST_THRESHOLD")!;

		bool gotHungryFired = false, gotThirstyFired = false;
		herb.GotHungry += (object? _, EventArgs _) => gotHungryFired = true;
		herb.GotThirsty += (object? _, EventArgs _) => gotThirstyFired = true;

		herbPO.SetProperty("HungerLevel", herbHungerThreshold - 1);
		herbPO.SetProperty("ThirstLevel", thirstThreshold - 1);
		carnPO.SetProperty("HungerLevel", carnHungerThreshold - 1);

		Assert.IsTrue(herb.IsHungry);
		Assert.IsTrue(herb.IsThirsty);
		Assert.IsTrue(carn.IsHungry);

		herbPO.SetProperty("HungerLevel", herbHungerThreshold + (herb.HungerDecay / 2f));
		herbPO.SetProperty("ThirstLevel", thirstThreshold + (herb.HungerDecay / 2f));
		herb.Update(unitGT);
		Assert.IsTrue(gotHungryFired);
		Assert.IsTrue(gotThirstyFired);

		Safari.Game.AddObject(herb);
		Safari.Game.AddObject(carn);
		RunOneFrame();

		herbPO.SetProperty("HungerLevel", 0f);
		herb.Update(unitGT);
		Assert.IsTrue(herb.IsDead);

		carnPO.SetProperty("ThirstLevel", 0f);
		carn.Update(unitGT);
		Assert.IsTrue(carn.IsDead);
	}

	[TestMethod("Species Test")]
	public void TestSpecies() {
		List<Type> herbTypes = [typeof(Zebra), typeof(Elephant), typeof(Giraffe)];
		List<Type> carnTypes = [typeof(Lion), typeof(Tiger), typeof(TigerWhite)];
		List<Type> animTypes = [.. herbTypes, .. carnTypes];

		foreach (Type type in animTypes) {
			Animal anim = (Animal)Activator.CreateInstance(type, Vector2.Zero, Gender.Female)!;

			Assert.AreEqual(anim.GetType(), anim.Species.GetAnimalType());

			if (type == typeof(Zebra)) Assert.AreEqual(AnimalSpecies.Zebra, anim.Species);
			if (type == typeof(Elephant)) Assert.AreEqual(AnimalSpecies.Elephant, anim.Species);
			if (type == typeof(Giraffe)) Assert.AreEqual(AnimalSpecies.Giraffe, anim.Species);
			if (type == typeof(Lion)) Assert.AreEqual(AnimalSpecies.Lion, anim.Species);
			if (type == typeof(Tiger)) Assert.AreEqual(AnimalSpecies.Tiger, anim.Species);
			if (type == typeof(TigerWhite)) Assert.AreEqual(AnimalSpecies.TigerWhite, anim.Species);

			if (carnTypes.Contains(type)) Assert.IsTrue(anim.IsCarnivorous);
			if (herbTypes.Contains(type)) Assert.IsFalse(anim.IsCarnivorous);
		}

		foreach (AnimalSpecies species in Enum.GetValues<AnimalSpecies>()) {
			Assert.IsTrue(species.GetPrice() >= 0);
			Assert.IsNotNull(species.GetDisplayName());
			Assert.AreNotEqual("", species.GetDisplayName());
		}

		AnimalSpecies invalidSpecies = (AnimalSpecies)int.MaxValue;
		Assert.IsTrue(invalidSpecies.GetPrice() == 0);
		Assert.IsFalse(invalidSpecies.IsCarnivorous());
		Assert.IsNull(invalidSpecies.GetAnimalType());
	}

	[TestMethod("World Interaction Test")]
	public void TestInteraction() {
		Point tilemapPos = (animal!.Position / Model.Level.TileSize).ToPoint();

		Model.Level.SetTile(tilemapPos, new Grass());
		animal.Update(unitGT);
		CollectionAssert.Contains(((HashSet<Point>)groupPO!.GetField("knownFoodSpots")!).ToList(), tilemapPos);

		tilemapPos = (animal!.Position / Model.Level.TileSize).ToPoint();
		Model.Level.SetTile(tilemapPos, new Water());
		animal.Update(unitGT);
		CollectionAssert.Contains(((HashSet<Point>)groupPO!.GetField("knownWaterSpots")!).ToList(), tilemapPos);

		Poacher poacher = new(new Vector2(1800, 1800));
		Safari.Game.AddObject(poacher);

		Assert.IsFalse(poacher.Visible);
		animal.Update(unitGT);
		Assert.IsFalse(poacher.Visible);

		EntityBoundsManager.RemoveEntity(poacher);
		poacher.Position = animal.Position;
		poacher.UpdateBounds();
		EntityBoundsManager.AddEntity(poacher);
		animal.Update(unitGT);
		Assert.IsTrue(poacher.Visible);
	}

	private Animal AddAnimalToGame(bool performAdd = true, AnimalSpecies species = AnimalSpecies.Zebra) {
		Animal anim = Substitute.ForPartsOf<Animal>(new Vector2(700, 700), species, Gender.Female);
		Safari.Game.AddObject(anim);

		if (performAdd) {
			RunOneFrame();
		}

		return anim;
	}

	private const float ERROR_LIMIT = 0.01f;
	private static void AssertFloatEquals(float a, float b) {
		Assert.IsTrue(Math.Abs(a - b) < ERROR_LIMIT);
	}
}
