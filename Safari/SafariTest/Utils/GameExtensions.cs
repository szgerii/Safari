using Microsoft.Xna.Framework;

namespace SafariTest.Utils; 

public static class GameExtensions {
	/// <summary>
	/// Advances the game by N frames
	/// </summary>
	/// <param name="game">The game to advance</param>
	/// <param name="frameCount">The number of frames to advance the game by</param>
	public static void RunNFrames(this Game game, int frameCount) {
		for (int i = 0; i < frameCount; i++) {
			game.RunOneFrame();
		}
	}
}
