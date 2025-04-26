#pragma warning disable CS8618

using SafariTest.Utils;

namespace SafariTest;

[TestClass]
public abstract class SimulationTest {
	public Safari.Game Game { get; set; }

	[TestInitialize]
	public void StartGame() {
		Game = new(true) { StartupMode = Safari.GameStartupMode.EmptyScene };
		GameAssert.GameInstance = Game;

		Game.SuppressDraw();
		Game.RunOneFrame();
	}

	[TestCleanup]
	public void CloseGame() {
		if (Game != null) {
			Game.Dispose();
			Game.Exit();
		}
	}
}
