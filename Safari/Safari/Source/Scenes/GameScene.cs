using Engine.Objects;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Safari.Model;
using Engine.Collision;
using Safari.Components;
using System;
using Engine;
using System.Collections.Generic;
using GeonBit.UI;
using Engine.Debug;
using Safari.Objects.Entities;
using Safari.Popups;
using Safari.Scenes.Menus;
using Safari.Objects.Entities.Tourists;

namespace Safari.Scenes;

public class GameScene : Scene {
	private GameModel model;
	public static GameScene Active => SceneManager.Active as GameScene;
	public GameModel Model => model;

	private readonly List<GameObject> simulationActors = [];
	private readonly Queue<GameObject> simulationActorAddQueue = new();
	private readonly Queue<GameObject> simulationActorRemoveQueue = new();

	static GameScene() {
		DebugMode.AddFeature(new ExecutedDebugFeature("list-objects", () => {
			if (Active == null) return;

			foreach (GameObject obj in Active.GameObjects) {
				string objStr = obj is Entity e ? e.ToString() : obj.ToString();

				DebugConsole.Instance.Write($"{obj} {Utils.Format(obj.Position, false, false)}", false);
			}
		}));
	}

	public override void Unload() {
		model.GameLost -= OnGameLost;
		model.GameWon -= OnGameWon;

		Jeep.Cleanup();

        base.Unload();

		PostUpdate -= CollisionManager.PostUpdate;
		PostProcessPasses.Remove(model.Level.LightManager);
		Game.ContentManager.Unload();
	}

	public override void Load() {
		// init game model
		// The start of the game is always <date of creation> 6 am
		DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		startDate = startDate.AddHours(6);
		model = new GameModel("test park", 6000, GameDifficulty.Easy, startDate);
		PostProcessPasses.Add(model.Level.LightManager);

		CollisionManager.Init(model.Level.MapWidth, model.Level.MapHeight, model.Level.TileSize);
		PostUpdate += CollisionManager.PostUpdate;

		model.GameLost += OnGameLost;

		model.GameWon += OnGameWon;

		// init camera
		CreateCamera(
			new Rectangle(
				0, 0,
				model.Level.MapWidth * model.Level.TileSize,
				model.Level.MapHeight * model.Level.TileSize
			)
		);

		UserInterface.Active.MouseInputProvider.DoClick();

        base.Load();

		MapBuilder.BuildStartingMap(model.Level);
	}

	public override void Update(GameTime gameTime) {
		PerformPreUpdate(gameTime);

		if (model.GameSpeed != GameSpeed.Paused) {
			model.Advance(gameTime);
		}

		foreach (GameObject obj in GameObjects) {
			if (model.GameSpeed != GameSpeed.Paused || !Attribute.IsDefined(obj.GetType(), typeof(SimulationActorAttribute))) {
				if (obj is Entity e && e.IsDead) continue;

				obj.Update(gameTime);
			}
		}

		for (int i = 0; i < model.SpeedMultiplier - 1; i++) {
			model.Advance(gameTime);

			foreach (GameObject actor in simulationActors) {
				if (actor is Entity e && e.IsDead) continue;

				actor.Update(gameTime);
			}
		}

		PerformPostUpdate(gameTime);
	}

	public override void AddObject(GameObject obj) {
		if (Attribute.IsDefined(obj.GetType(), typeof(SimulationActorAttribute))) {
			simulationActorAddQueue.Enqueue(obj);
		}

		base.AddObject(obj);
	}

	public override void RemoveObject(GameObject obj) {
		if (Attribute.IsDefined(obj.GetType(), typeof(SimulationActorAttribute))) {
			simulationActorRemoveQueue.Enqueue(obj);
		}

		base.RemoveObject(obj);
	}

	public override void PerformObjectAdditions() {
		while (simulationActorAddQueue.Count > 0) {
			simulationActors.Add(simulationActorAddQueue.Dequeue());
		}

		base.PerformObjectAdditions();
	}

	public override void PerformObjectRemovals() {
		while (simulationActorRemoveQueue.Count > 0) {
			simulationActors.Remove(simulationActorRemoveQueue.Dequeue());
		}

		base.PerformObjectRemovals();
	}

	private void CreateCamera(Rectangle bounds) {
		Camera.Active = new Camera();

		CameraControllerCmp controllerCmp = new(bounds);
		Camera.Active.Attach(controllerCmp);

		AddObject(Camera.Active);
	}

	private void OnGameLost(object sender, LoseReason reason) {
		string message = reason switch {
			LoseReason.Money => "You ran out of funds.",
			LoseReason.Animals => "All of your animals have died.",
			_ => ""
		};
		AlertMenu menu = new AlertMenu("You lose!", message, "Return to main menu");
		menu.Chosen += (object sender, bool e) => {
			SceneManager.Load(MainMenu.Instance);
		};
		model.Pause();
		menu.Show();
	}

	private void OnGameWon(object sender, EventArgs e) {
		string difficulty = model.Difficulty switch {
			GameDifficulty.Easy => "easy",
			GameDifficulty.Normal => "normal",
			_ => "hard"
		};
		AlertMenu menu = new AlertMenu("You win!", $"Congratulations on beating the game on {difficulty} difficulty!", "Return to main menu", "Keep playing...");
		menu.Chosen += (object sender, bool e) => {
			if (e) {
				SceneManager.Load(MainMenu.Instance);
			} else {
				model.PostWin = true;
				model.CheckWinLose = false;
				model.Resume();
			}
		};
		model.Pause();
		menu.Show();
	}
}
