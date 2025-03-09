using Engine.Scenes;
using Microsoft.Xna.Framework;
using Safari.Model;
using System;

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

		// The start of the game is always <date of creation> 6 am
		DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		startDate = startDate.AddHours(6);
		model = new GameModel("test park", 6000, GameDifficulty.Normal, startDate);
		base.Load();
	}

	public override void Update(GameTime gameTime) {
		for (int i = 0; i < model.SpeedMultiplier; i++) {
			model.Advance(gameTime);
		}
		base.Update(gameTime);
	}
}
