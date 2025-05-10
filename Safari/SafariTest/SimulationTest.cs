#pragma warning disable CS8618

using Safari.Model;
using Safari.Scenes;
using SafariTest.Utils;

namespace SafariTest;

[TestClass]
public abstract class SimulationTest {
	protected Safari.Game Game { get; set; }
	protected GameModel Model => GameScene.Active.Model;


	[TestInitialize]
	public void StartGame() {
		Game = new(headless: true) { StartupMode = Safari.GameStartupMode.EmptyScene };
		GameAssert.GameInstance = Game;

		RunOneFrame();
	}

	[TestCleanup]
	public void CloseGame() {
		if (Game != null) {
			Game.Dispose();
			Game.Exit();
		}
	}

	/// <summary>
	/// Advances the game instance of the test by one frame
	/// </summary>
	protected virtual void RunOneFrame() {
		Game.RunOneFrameNoDraw();
	}

	/// <summary>
	/// Advances the game instance of the test by N frames
	/// </summary>
	/// <param name="n">The number of frames to advance the game by</param>
	protected void RunNFrames(int n) {
		Game.RunNFrameNoDraw(n);
	}
}
