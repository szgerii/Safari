using Engine.Scenes;
using Microsoft.Xna.Framework;
using Safari.Model;

namespace Safari.Scenes;

public class GameScene : Scene {
	private GameModel model;
	public static GameScene Active => SceneManager.Active as GameScene;
	public GameModel Model => model;


	public override void Unload() {
		// PostUpdate -= CollisionManager.PostUpdate;
		
		base.Unload();
	}

	public override void Load() {
		// CollisionManager.Init(numOfCellsInRow, numOfCellsInCol, cellSize);
		// PostUpdate += CollisionManager.PostUpdate;

		model = new GameModel("test park", 6000, GameDifficulty.Normal);
		base.Load();
	}

	public override void Update(GameTime gameTime) {
		for (int i = 0; i < model.SpeedMultiplier; i++) {
			model.Advance(gameTime);
		}
		base.Update(gameTime);
	}
}
