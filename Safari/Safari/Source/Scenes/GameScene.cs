using Engine.Objects;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Safari.Model;
using Engine.Collision;
using Safari.Components;
using System;

namespace Safari.Scenes;

public class GameScene : Scene {
	private GameModel model;
	public static GameScene Active => SceneManager.Active as GameScene;
	public GameModel Model => model;


	public override void Unload() {
		base.Unload();

		PostUpdate -= CollisionManager.PostUpdate;
		Game.ContentManager.Unload();
	}

	public override void Load() {
		// init game model
		// The start of the game is always <date of creation> 6 am
		DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		startDate = startDate.AddHours(6);
		model = new GameModel("test park", 6000, GameDifficulty.Normal, startDate);

		CollisionManager.Init(model.Level.MapWidth, model.Level.MapHeight, model.Level.TileSize);
		PostUpdate += CollisionManager.PostUpdate;

		// init camera
		CreateCamera(
			new Rectangle(
				0, 0,
				model.Level.MapWidth * model.Level.TileSize,
				model.Level.MapHeight * model.Level.TileSize
			)
		);

		base.Load();
	}

	public override void Update(GameTime gameTime) {
		for (int i = 0; i < model.SpeedMultiplier; i++) {
			model.Advance(gameTime);
		}
		base.Update(gameTime);
	}

	private void CreateCamera(Rectangle bounds) {
		Camera.Active = new Camera();

		CameraControllerCmp controllerCmp = new(bounds);
		Camera.Active.Attach(controllerCmp);

		AddObject(Camera.Active);
	}
}
