#pragma warning disable CS8618

using Engine.Scenes;
using Safari.Model.Entities;
using Safari.Scenes;

namespace SafariTest;

[TestClass]
public class SimulationTest {
	public Safari.Game Game { get; set; }

	[TestInitialize]
	public void StartGame() {
		Game = new() { StartupMode = Safari.GameStartupMode.EmptyScene };
		Game.SuppressDraw();
		Game.RunOneFrame();
	}

	[TestCleanup]
	public void CloseGame() {
		Game.Dispose();
		Game.Exit();
	}

	[TestMethod]
	public void TestInit() {
		// TODO: remove (this is just for testing purposes only)
		Assert.IsInstanceOfType(SceneManager.Active, typeof(GameScene));
		Assert.AreEqual(0, GameScene.Active.Model.EntityCount);
		int cash = GameScene.Active.Model.Funds;
		Safari.Game.AddObject(new Ranger(new Microsoft.Xna.Framework.Vector2(300, 300)));
		Game.RunOneFrame();
		Assert.AreEqual(1, GameScene.Active.Model.EntityCount);
		Assert.IsTrue(cash > GameScene.Active.Model.Funds);
	}

	protected void RunFrames(int amount) {
		for (int i = 0; i < amount; i++) {
			Game.RunOneFrame();
		}
	}
}
