using Engine.Helpers;
using Microsoft.Xna.Framework;
using NSubstitute;
using Safari.Model;
using Safari.Model.Entities;
using Safari.Model.Entities.Animals;
using Safari.Model.Tiles;
using SafariTest.Utils;

namespace SafariTest.Tests.Model.Entity.Animals;

[TestClass]
public class AnimalGroupTest : SimulationTest {
	private AnimalGroup? group;
	private PrivateObject? groupPO;
	private Animal? animal;
	private PrivateObject? animalPO;
	private readonly PrivateType animalPT = new(typeof(Animal));
	private readonly GameTime unitGT = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

	[TestInitialize]
	public void InitGroup() {
		animal = Substitute.ForPartsOf<Animal>(new Vector2(200, 300), AnimalSpecies.Zebra, Gender.Female);
		animalPO = new(animal);
		animal.Bounds = new Vectangle(0, 0, 100, 100);
		animal.UpdateBounds();
		Safari.Game.AddObject(animal);

		RunOneFrame();

		group = animal.Group;
		groupPO = new(animal.Group);
	}

	[TestMethod("Initialization Test")]
	public void TestGroupInit() {
		Assert.AreEqual(animal!.Species, group!.Species);
		Assert.AreEqual(AnimalGroupState.Wandering, group.State);
		Assert.AreEqual(false, group.HasHungryMember);
		Assert.AreEqual(false, group.HasThirstyMember);
		Assert.AreEqual(1, group.Size);
		CollectionAssert.AreEqual(new Animal[] { animal }, group.Members);
	}

	[TestMethod("Memory Test")]
	public void TestMemory() {
		CollectionAssert.AreEqual(Array.Empty<Point>(), GetFoodSpots());
		CollectionAssert.AreEqual(Array.Empty<Point>(), GetWaterSpots());

		group!.AddFoodSpot(new Point(1, 2));
		CollectionAssert.AreEquivalent(new Point[] { new(1, 2) }, GetFoodSpots());
		group.AddFoodSpot(new Point(1, 2));
		CollectionAssert.AreEquivalent(new Point[] { new(1, 2) }, GetFoodSpots());
		group.AddFoodSpot(new Point(2, 2));
		CollectionAssert.AreEquivalent(new Point[] { new(1, 2), new(2, 2) }, GetFoodSpots());

		group.AddWaterSpot(new Point(1, 2));
		CollectionAssert.AreEquivalent(new Point[] { new(1, 2) }, GetWaterSpots());
		group.AddWaterSpot(new Point(1, 2));
		CollectionAssert.AreEquivalent(new Point[] { new(1, 2) }, GetWaterSpots());
		group.AddWaterSpot(new Point(2, 2));
		CollectionAssert.AreEquivalent(new Point[] { new(1, 2), new(2, 2) }, GetWaterSpots());
	}

	[TestMethod("Merging Test")]
	public void TestMerge() {
		group!.AddFoodSpot(new Point(1, 1));
		group!.AddWaterSpot(new Point(1, 1));

		Animal animal2 = Substitute.ForPartsOf<Animal>(new Vector2(200, 300), animal!.Species, Gender.Female);
		animal2.Group.AddFoodSpot(new Point(1, 1));
		animal2.Group.AddFoodSpot(new Point(2, 2));
		animal2.Group.AddWaterSpot(new Point(2, 2));
		Assert.IsTrue(group.CanMergeWith(animal2.Group));

		group.MergeWith(animal2.Group);
		Assert.ThrowsException<ArgumentException>(() => group.MergeWith(animal2.Group));

		Assert.AreEqual(2, group.Size);
		Assert.AreEqual(group, animal2.Group);
		CollectionAssert.Contains(group.Members, animal);
		CollectionAssert.Contains(group.Members, animal2);
		CollectionAssert.AreEqual(new Point[] { new(1, 1), new(2, 2) }, GetFoodSpots());
		CollectionAssert.AreEqual(new Point[] { new(1, 1), new(2, 2) }, GetWaterSpots());

		while (group.Size < AnimalGroup.MAX_SIZE) {
			Animal anim = Substitute.ForPartsOf<Animal>(new Vector2(200, 300), animal.Species, Gender.Female);
			Assert.IsTrue(group.CanMergeWith(anim.Group));
			group.MergeWith(anim.Group);
		}

		Animal animal3 = Substitute.ForPartsOf<Animal>(new Vector2(200, 300), animal.Species, Gender.Female);
		Assert.IsFalse(group.CanMergeWith(animal3.Group));

		group.Leave(group.Members[^1]);
		Assert.IsTrue(group.CanMergeWith(animal3.Group));

		Assert.ThrowsException<InvalidOperationException>(() => group.Leave(animal3));

		AnimalSpecies otherSpecies = animal.Species == 0 ? (animal.Species + 1) : (animal.Species - 1);
		Animal diffAnimal = Substitute.ForPartsOf<Animal>(new Vector2(200, 300), otherSpecies, Gender.Female);
		Assert.IsFalse(group.CanMergeWith(diffAnimal.Group));
	}

	[TestMethod("Detection Test")]
	public void TestDetection() {
		Vector2 target = new(1000, 1000);
		animal!.ReachDistance = 10;
		Animal animal2 = Substitute.ForPartsOf<Animal>(new Vector2(100, 100), animal.Species, Gender.Female);
		animal2.ReachDistance = 10;
		animal2.Bounds = new Vectangle(0, 0, 100, 100);
		animal2.UpdateBounds();
		group!.MergeWith(animal2.Group);

		Assert.IsFalse(group.CanAnybodyReach(target));
		Assert.IsFalse(group.CanAnybodySee(target));
		Assert.IsFalse(group.CanEverybodyReach(target));
		Assert.IsFalse(group.CanEverybodySee(target));

		animal.Position = target;
		animal.UpdateBounds();

		Assert.IsTrue(group.CanAnybodyReach(target));
		Assert.IsTrue(group.CanAnybodySee(target));
		Assert.IsFalse(group.CanEverybodyReach(target));
		Assert.IsFalse(group.CanEverybodySee(target));

		animal2.Position = target;
		animal2.UpdateBounds();

		Assert.IsTrue(group.CanAnybodyReach(target));
		Assert.IsTrue(group.CanAnybodySee(target));
		Assert.IsTrue(group.CanEverybodyReach(target));
		Assert.IsTrue(group.CanEverybodySee(target));
	}

	[TestMethod("Hungry/Thirsty Member Test")]
	public void HungerThirstTest() {
		Assert.IsFalse(group!.HasHungryMember);
		animalPO!.SetProperty("HungerLevel", (int)animalPT.GetField("HUNGER_THRESHOLD_HERB")! - 1);
		Assert.IsTrue(group.HasHungryMember);

		Assert.IsFalse(group!.HasThirstyMember);
		animalPO.SetProperty("ThirstLevel", (int)animalPT.GetField("THIRST_THRESHOLD")! - 1);
		Assert.IsTrue(group!.HasThirstyMember);

		Assert.AreNotEqual(AnimalGroupState.SeekingFood, group.State);

		group.AddFoodSpot(Point.Zero);

		Assert.AreEqual(AnimalGroupState.SeekingFood, group.State);
		Assert.IsTrue(group.NavCmp.Moving);
		Assert.AreEqual(Vector2.Zero, group.NavCmp.Target);

		group.StateMachine.Transition(AnimalGroupState.Wandering);

		Assert.AreNotEqual(AnimalGroupState.SeekingWater, group.State);

		group.AddWaterSpot(Point.Zero);

		Assert.AreEqual(AnimalGroupState.SeekingWater, group.State);
		Assert.IsTrue(group.NavCmp.Moving);
		Assert.AreEqual(Vector2.Zero, group.NavCmp.Target);
	}

	[TestMethod("Feeding/Drinking Test")]
	public void TestFeedingDrinking() {
		animalPO!.SetProperty("HungerLevel", 100f - (Animal.FEEDING_SPEED * 2));
		group!.StateMachine.Transition(AnimalGroupState.Feeding);

		group.Update(unitGT);
		Assert.AreEqual(AnimalGroupState.Feeding, group.State);

		group.Update(unitGT);
		Assert.AreEqual(AnimalGroupState.Idle, group.State);

		animalPO!.SetProperty("ThirstLevel", 100f - (Animal.DRINKING_SPEED * 2));
		group!.StateMachine.Transition(AnimalGroupState.Drinking);

		group.Update(unitGT);
		Assert.AreEqual(AnimalGroupState.Drinking, group.State);

		group.Update(unitGT);
		Assert.AreEqual(AnimalGroupState.Idle, group.State);
	}

	[TestMethod("Idling State Test")]
	public void TestIdling() {
		animalPO!.SetField("birthTime", Model.IngameDate - TimeSpan.FromDays(Animal.MATURE_AGE + 1));

		for (int i = 0; i <= 4; i++) {
			Gender gender = i % 2 == 0 ? Gender.Male : Gender.Female;
			Animal anim = Substitute.ForPartsOf<Animal>(Vector2.Zero, animal!.Species, gender);

			PrivateObject animPO = new(anim);
			animPO!.SetField("birthTime", Model.IngameDate - TimeSpan.FromDays(Animal.MATURE_AGE + 1));

			animal.Group.MergeWith(anim.Group);
		}

		group!.StateMachine.Transition(AnimalGroupState.Idle);
		// +3 gyerek
		Assert.AreEqual(8, group.Size);

		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.AreNotEqualBefore(AnimalGroupState.Idle, () => group.State, TimeSpan.FromDays(1));
		Model.GameSpeed = GameSpeed.Slow;
	}

	[TestMethod("SeekingFood State Test")]
	public void TestSeekingFood() {
		Vector2 target = new Vector2(1500, 1500);
		Point tilemapPos = (target / Model.Level.TileSize).ToPoint();
		Model.Level.SetTile(tilemapPos, new Grass());
		group!.AddFoodSpot(tilemapPos);

		animalPO!.SetProperty("HungerLevel", (int)animalPT!.GetField("HUNGER_THRESHOLD_HERB")! + 1);
		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.TrueInNFrames(() => animal!.IsHungry, 100);
		Model.GameSpeed = GameSpeed.Slow;
		RunOneFrame();

		Assert.AreEqual(AnimalGroupState.SeekingFood, group.State);
		Assert.AreEqual(tilemapPos.ToVector2() * Model.Level.TileSize, group.NavCmp.Target);
		Assert.AreEqual(true, group.NavCmp.Moving);

		EntityBoundsManager.RemoveEntity(animal);
		animal!.Position = tilemapPos.ToVector2() * Model.Level.TileSize;
		animal.UpdateBounds();
		EntityBoundsManager.AddEntity(animal);

		GameAssert.AreEqualInNFrames(AnimalGroupState.Feeding, () => group.State, 5);
	}

	[TestMethod("SeekingWater State Test")]
	public void TestSeekingWater() {
		Vector2 target = new Vector2(1500, 1500);
		Point tilemapPos = (target / Model.Level.TileSize).ToPoint();
		Model.Level.SetTile(tilemapPos, new Water());
		group!.AddWaterSpot(tilemapPos);

		animalPO!.SetProperty("ThirstLevel", (int)animalPT!.GetField("THIRST_THRESHOLD")! + 1);
		Model.GameSpeed = GameSpeed.Fast;
		GameAssert.TrueInNFrames(() => animal!.IsThirsty, 100);
		Model.GameSpeed = GameSpeed.Slow;
		RunOneFrame();

		Assert.AreEqual(AnimalGroupState.SeekingWater, group.State);
		Assert.AreEqual(tilemapPos.ToVector2() * Model.Level.TileSize, group.NavCmp.Target);
		Assert.AreEqual(true, group.NavCmp.Moving);

		EntityBoundsManager.RemoveEntity(animal);
		animal!.Position = tilemapPos.ToVector2() * Model.Level.TileSize;
		animal.UpdateBounds();
		EntityBoundsManager.AddEntity(animal);

		GameAssert.AreEqualInNFrames(AnimalGroupState.Drinking, () => group.State, 5);
	}

	private List<Point> GetFoodSpots() => ((HashSet<Point>)groupPO!.GetField("knownFoodSpots")!).ToList();
	private List<Point> GetWaterSpots() => ((HashSet<Point>)groupPO!.GetField("knownWaterSpots")!).ToList();
}
