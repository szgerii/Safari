using System;
using Engine;
using Engine.Components;
using Engine.Objects;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Model;
using Safari.Objects;
using Safari.Components;
using Engine.Input;
using Safari.Debug;

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

		// init game model
		// The start of the game is always <date of creation> 6 am
		DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		startDate = startDate.AddHours(6);
		model = new GameModel("test park", 6000, GameDifficulty.Normal, startDate);
	
		// test scene
		Texture2D brushTex = Game.ContentManager.Load<Texture2D>("assets/sprites/bush");

		int hCount = (Game.RenderTarget.Width / 32) * 2;
		int vCount = (Game.RenderTarget.Height / 32) * 2;
		for (int i = 0; i < hCount; i++) {
			for (int j = 0; j < vCount; j++) {
				int x = i * 32;
				int y = j * 32;

				if (x % 64 == y % 64) {
					continue;
				}

				Tile t = new Tile(new Vector2(x, y), brushTex);
				t.GetComponent<SpriteCmp>().Tint = new Color(i / (float)hCount, j / (float)vCount, 1);
				AddObject(t);
			}
		}

		// init camera
		CreateCamera(
			new Rectangle(
				0,
				0,
				hCount * 32 - Game.RenderTarget.Width,
				vCount * 32 - Game.RenderTarget.Height
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
		// TODO: delete test code
		Camera.Active = new Camera();

		CameraControllerCmp controllerCmp = new(bounds);
		Camera.Active.Attach(controllerCmp);

		AddObject(Camera.Active);
	}
}
