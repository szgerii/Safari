using Microsoft.Xna.Framework;

namespace SafariTest.Utils;

public static class GameExtensions {
	/// <summary>
	/// Runs the game instance for a single frame, suppressing any drawing logic
	/// </summary>
	/// <param name="game">The game to advance</param>
	public static void RunOneFrameNoDraw(this Game game) {
		game.SuppressDraw();
		game.RunOneFrame();
	}

	/// <summary>
	/// Runs the game instance for N frames, suppressing any drawing logic
	/// </summary>
	/// <param name="game">The game to advance</param>
	/// <param name="n">The number of frames to run the game for</param>
	public static void RunNFrameNoDraw(this Game game, int n) {
		for (int i = 0; i < n; i++) {
			game.RunOneFrameNoDraw();
		}
	}
}
