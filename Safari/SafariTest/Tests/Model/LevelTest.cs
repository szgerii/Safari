using Safari.Scenes;
using Safari.Model;
using Engine.Graphics.Stubs.Texture;
using Microsoft.Xna.Framework;
using SafariTest.Utils;
using NSubstitute;
using Safari.Model.Tiles;

namespace SafariTest.Tests.Model;

[TestClass]
public class LevelTest : SimulationTest {
	private Level SetupLevel() {
		Level.PLAY_AREA_CUTOFF_X = 2;
		Level.PLAY_AREA_CUTOFF_Y = 2;
		NoopTexture2D tex = new NoopTexture2D(Engine.Game.Instance?.GraphicsDevice, 3200, 3200);
		Level l = new Level(32, 10, 10, tex);
		GameScene.Active.RemoveObject(GameScene.Active.Model.Level!);
		GameScene.Active.Model.Level = l;
		GameScene.Active.AddObject(l);
		return l;
	}

	[TestMethod]
	public void LevelInit() {
		Level l = SetupLevel();
		RunOneFrame();

		Assert.AreEqual(32, l.TileSize);
		Assert.AreEqual(10, l.MapWidth);
		Assert.AreEqual(10, l.MapHeight);
		Assert.AreEqual(new Rectangle(new Point(64, 64), new Point(192, 192)), l.PlayAreaBounds);

		Assert.IsNotNull(l.Background);
		Assert.IsNotNull(l.Network);
		Assert.IsNotNull(l.ConstructionHelperCmp);
		Assert.IsNotNull(l.LightManager);
	}

	[TestMethod]
	public void LevelFetchArea() {
		Level l = SetupLevel();
		RunOneFrame();

		Assert.IsTrue(l.IsOutOfBounds(-1, -1));
		Assert.IsTrue(l.IsOutOfBounds(20, 20));
		Assert.IsFalse(l.IsOutOfBounds(1, 1));
		Assert.IsFalse(l.IsOutOfBounds(8, 8));
		Assert.IsFalse(l.IsOutOfBounds(4, 4));

		Assert.IsTrue(l.IsOutOfPlayArea(-1, -1));
		Assert.IsTrue(l.IsOutOfPlayArea(20, 20));
		Assert.IsTrue(l.IsOutOfPlayArea(1, 1));
		Assert.IsTrue(l.IsOutOfPlayArea(8, 8));
		Assert.IsFalse(l.IsOutOfPlayArea(4, 4));

		Engine.Game.Random = NSubstitute.Substitute.For<Random>();
		Engine.Game.Random.Next(0, 288).Returns(100);
		Engine.Game.Random.Next(64, 224).Returns(200);

		Assert.AreEqual(new Vector2(100, 100), l.GetRandomPosition(false));
		Assert.AreEqual(new Vector2(200, 200), l.GetRandomPosition(true));

		Assert.AreEqual(new Vector2(80, 80), l.GetTileCenter(new(2, 2)));
	}

	[TestMethod]
	public void LevelTileManagement() {
		// check preload
		Level l = SetupLevel();
		Fence starterFence = new Fence();
		Assert.IsFalse(starterFence.Loaded);
		Assert.IsNull(l.GetTile(new(4, 4)));

		// check set
		l.SetTile(new(4, 4), starterFence);
		Assert.IsFalse(starterFence.Loaded);
		Assert.AreEqual(l.GetTile(4, 4), starterFence);

		RunOneFrame();
		Assert.IsTrue(starterFence.Loaded);

		// check clear
		l.ClearTile(new(4, 4));
		Assert.IsNull(l.GetTile(new(4, 4)));

		RunOneFrame();
		Assert.IsFalse(starterFence.Loaded);

		l.SetTile(new(4, 4), starterFence);
		RunOneFrame();

		// check other set features
		Road r = new Road();
		l.SetTile(new(3, 4), r);
		Assert.IsTrue(l.Network.GetRoad(new(3, 4)));
		Assert.IsTrue(l.LightManager.CheckLight(3, 4));
		Assert.IsTrue(starterFence.NeedsUpdate);
		RunOneFrame();
		CollectionAssert.Contains(GameScene.Active.GameObjects, r);

		// check area tiles
		List<Tile> tiles = l.GetTilesInArea(new Rectangle(new Point(3, 3), new Point(2, 2)));
		CollectionAssert.Contains(tiles, starterFence);
		CollectionAssert.Contains(tiles, r);

		tiles = l.GetTilesInWorldArea(new Rectangle(new Point(96, 96), new Point(64, 64)));
		CollectionAssert.Contains(tiles, starterFence);
		CollectionAssert.Contains(tiles, r);
	} 
}
