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
using Engine.Helpers;
using Engine.Input;
using Safari.Objects.Entities.Animals;
using Safari.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Safari.Scenes;

public enum MouseMode {
	Build,
	Demolish,
	Inspect
}

public class GameScene : Scene {
	private GameModel model;
	public static GameScene Active => SceneManager.Active as GameScene;
	public GameModel Model => model;

	public MouseMode MouseMode { get; set; } = MouseMode.Inspect;
	public List<Rectangle> MaskedAreas { get; private set; } = new List<Rectangle>();

	private readonly List<GameObject> simulationActors = [];
	private readonly Queue<GameObject> simulationActorAddQueue = new();
	private readonly Queue<GameObject> simulationActorRemoveQueue = new();

	private Texture2D demolishHover;
	private Texture2D buildHover;

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

        Statusbar.Instance.Unload();
		EntityManager.Instance.Unload();

        base.Unload();

		PostUpdate -= CollisionManager.PostUpdate;
		EntityBoundsManager.CleanUp();
		CollisionManager.CleanUp();
		PostProcessPasses.Remove(model.Level.LightManager);

		EntityControllerMenu.Active?.Hide();

		Game.ContentManager.Unload();
	}

	public override void Load() {
		// init game model
		// The start of the game is always <date of creation> 6 am
		DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		startDate = startDate.AddHours(6);
		model = new GameModel("test park", 6000, GameDifficulty.Easy, startDate);
		PostProcessPasses.Add(model.Level.LightManager);

		int tileSize = model.Level.TileSize;
		Vectangle mapBounds = new(0, 0, model.Level.MapWidth * tileSize, model.Level.MapHeight * tileSize);

		EntityBoundsManager.Init(mapBounds);
		CollisionManager.Init(mapBounds);
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

		Texture2D outline = Utils.GenerateTexture(model.Level.TileSize, model.Level.TileSize, new Color(0.8f, 0.1f, 0.1f, 1f), true);
		Texture2D fill = Utils.GenerateTexture(model.Level.TileSize, model.Level.TileSize, new Color(0.7f, 0.1f, 0.1f, 0.3f));
		demolishHover = Utils.MergeTextures(fill, outline);

		buildHover = Utils.GenerateTexture(model.Level.TileSize, model.Level.TileSize, new Color(0.1f, 0.3f, 0.7f, 1f), true);

		base.Load();

		MapBuilder.BuildStartingMap(model.Level);

		Statusbar.Instance.Load();
		EntityManager.Instance.Load();
    }

    public override void Update(GameTime gameTime) {
		PerformPreUpdate(gameTime);

		if (InputManager.Keyboard.JustPressed(Microsoft.Xna.Framework.Input.Keys.D0)) {
			if (MouseMode == MouseMode.Inspect) {
				MouseMode = MouseMode.Build;
			} else {
				MouseMode = MouseMode.Inspect;
			}
		} else if (InputManager.Keyboard.JustPressed(Microsoft.Xna.Framework.Input.Keys.D1)) {
			if (MouseMode == MouseMode.Build) {
				MouseMode = MouseMode.Demolish;
			} else if (MouseMode == MouseMode.Demolish) {
				MouseMode = MouseMode.Build;
			}
		}

		UpdatePalette();

		Vector2 mouseTilePos = GetMouseTilePos();
		if (MousePlayable(mouseTilePos)) {
			UpdateBuild();
			if (MouseMode == MouseMode.Inspect) {
				UpdateInspect();
			}
		}

		if (model.GameSpeed != GameSpeed.Paused) {
			model.Advance(gameTime);
		}

        Statusbar.Instance.Update(gameTime);

		EntityManager.Instance.Update(gameTime);

        foreach (GameObject obj in GameObjects) {
			if (model.GameSpeed != GameSpeed.Paused || !Attribute.IsDefined(obj.GetType(), typeof(SimulationActorAttribute))) {
				if (obj is Entity e && e.IsDead) continue;

				obj.Update(gameTime);
			}
		}

		for (int i = 0; i < model.RealExtraFrames; i++) {
			GameTime customTime = new GameTime(gameTime.TotalGameTime, gameTime.ElapsedGameTime * model.FakeFrameMul);
			model.Advance(customTime);

			foreach (GameObject actor in simulationActors) {
				if (actor is Entity e && e.IsDead) continue;

				actor.Update(customTime);
			}
		}

		PerformPostUpdate(gameTime);
	}

	private void UpdateInspect() {
		if (InputManager.Mouse.JustPressed(MouseButtons.LeftButton)) {
			Entity entity = Entity.GetEntityOnMouse();
			if (entity != null && entity is Ranger ranger) {
				EntityControllerMenu controller = new RangerControllerMenu(ranger);
				controller.Show();
			} else if (entity != null && entity is Animal animal) {
				EntityControllerMenu controller = new AnimalControllerMenu(animal);
				controller.Show();
			}
		}
	}

	private void UpdatePalette() {
		if (MouseMode == MouseMode.Build) {
			if (InputManager.Keyboard.JustPressed(Microsoft.Xna.Framework.Input.Keys.M)) {
				Model.Level.ConstructionHelperCmp.SelectNext();
			}
			if (InputManager.Keyboard.JustPressed(Microsoft.Xna.Framework.Input.Keys.N)) {
				Model.Level.ConstructionHelperCmp.SelectPrev();
			}
			if (InputManager.Keyboard.JustPressed(Microsoft.Xna.Framework.Input.Keys.J)) {
				var cons = Model.Level.ConstructionHelperCmp;
				if (cons.SelectedIndex >= 0) {
					cons.Palette[cons.SelectedIndex].SelectNext();
				}
			}
			if (InputManager.Keyboard.JustPressed(Microsoft.Xna.Framework.Input.Keys.H)) {
				var cons = Model.Level.ConstructionHelperCmp;
				if (cons.SelectedIndex >= 0) {
					cons.Palette[cons.SelectedIndex].SelectNext();
				}
			}
		}
	}

	private void UpdateBuild() {
		if (InputManager.Mouse.IsDown(MouseButtons.LeftButton)) {
			Point p = (GetMouseTilePos() / Model.Level.TileSize).ToPoint();
			if (MouseMode == MouseMode.Build) {
				Model.Level.ConstructionHelperCmp.BuildCurrent(p);
			} else {
				Model.Level.ConstructionHelperCmp.Demolish(p);
			}

		}
	}

	private bool MousePlayable(Vector2 mouseTilePos) {
		return !Model.Level.IsOutOfPlayArea((int)mouseTilePos.X / Model.Level.TileSize, (int)mouseTilePos.Y / Model.Level.TileSize) && !InMaskedArea(InputManager.Mouse.Location);
	}

	private bool InMaskedArea(Point position) {
		foreach (Rectangle area in MaskedAreas) {
			if (area.Contains(position)) {
				return true;
			}
		}
		return false;
	}

	private Vector2 GetMouseTilePos() {
		Vector2 mouseTilePos = InputManager.Mouse.GetWorldPos();
		mouseTilePos.X -= mouseTilePos.X % Model.Level.TileSize;
		mouseTilePos.Y -= mouseTilePos.Y % Model.Level.TileSize;

		return mouseTilePos;
	}

	public override void Draw(GameTime gameTime) {
		if (MouseMode == MouseMode.Build) {
			var cons = Model.Level.ConstructionHelperCmp;
			if (cons.SelectedIndex >= 0) {
				var ins = cons.Palette[cons.SelectedIndex].Instance;
				Vector2 mousePos = GetMouseTilePos();
				Point tilePos = (mousePos / Model.Level.TileSize).ToPoint();
				ins.DrawPreviewAt(GetMouseTilePos(), cons.CanBuild(tilePos, ins));
				Game.SpriteBatch.Draw(buildHover, mousePos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			}
		} else if (MouseMode == MouseMode.Demolish) {
			Vector2 mousePosWorld = GetMouseTilePos();
			Game.SpriteBatch.Draw(demolishHover, mousePosWorld, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}

		base.Draw(gameTime);
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
