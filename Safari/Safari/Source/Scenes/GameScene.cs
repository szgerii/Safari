using Engine.Objects;
using Engine.Components;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Model;
using Safari.Objects;
using Safari.Components;
using System;
using Safari.Objects.Entities.Animals;
using Safari.Objects.Entities.Tourists;
using Safari.Objects.Entities;

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
		Texture2D brushTex = Game.ContentManager.Load<Texture2D>("Assets/Bush/Bush1");

		int hCount = (Game.RenderTarget.Width * 2 / 32);
		int vCount = (Game.RenderTarget.Height * 2 / 32);
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
				hCount * 32,
				vCount * 32
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

	private void ChangeEntityCount(Engine.GameObject obj, int value) {
		if (obj is Entity) {
			model.EntityCount += value;
			if (obj is Animal) {
				model.AnimalCount += value;
				if (obj is Lion || obj is Tiger) {
					model.CarnivoreCount += value;
				}
				if (obj is Elephant || obj is Zebra) {
					model.HerbivoreCount += value;
				}
			}
			if (obj is Tourist) {
				model.TouristCount += value;
			}
			if (obj is Jeep) {
				model.JeepCount += value;
			}
			if (obj is Poacher) {
				model.PoacherCount += value;
			}
			if (obj is Ranger) {
				model.RangerCount += value;
			}
		}
	}

	public override void AddObject(Engine.GameObject obj) {
		ChangeEntityCount(obj, 1);
		base.AddObject(obj);
	}

	public override void RemoveObject(Engine.GameObject obj) {
		ChangeEntityCount(obj, -1);
		base.RemoveObject(obj);
	}

	private void CreateCamera(Rectangle bounds) {
		Camera.Active = new Camera();

		CameraControllerCmp controllerCmp = new(bounds);
		Camera.Active.Attach(controllerCmp);

		AddObject(Camera.Active);
	}
}
