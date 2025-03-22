using Engine.Objects;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Safari.Model;
using Engine.Collision;
using Safari.Components;
using System;
using Engine;
using System.Collections.Generic;

namespace Safari.Scenes;

public class GameScene : Scene {
	private GameModel model;
	public static GameScene Active => SceneManager.Active as GameScene;
	public GameModel Model => model;

	private readonly List<GameObject> simulationActors = [];
	private readonly Queue<GameObject> simulationActorAddQueue = new();
	private readonly Queue<GameObject> simulationActorRemoveQueue = new();

	public override void Unload() {
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
		model = new GameModel("test park", 6000, GameDifficulty.Normal, startDate);
		PostProcessPasses.Add(model.Level.LightManager);

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
		PerformPreUpdate(gameTime);

		if (model.GameSpeed != GameSpeed.Paused) {
			model.Advance(gameTime);
		}

		foreach (GameObject obj in GameObjects) {
			if (model.GameSpeed != GameSpeed.Paused || !Attribute.IsDefined(obj.GetType(), typeof(SimulationActorAttribute))) {
				obj.Update(gameTime);
			}
		}

		for (int i = 0; i < model.SpeedMultiplier - 1; i++) {
			model.Advance(gameTime);

			foreach (GameObject actor in simulationActors) {
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
}
