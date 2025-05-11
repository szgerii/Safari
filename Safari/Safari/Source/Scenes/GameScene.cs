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
using Safari.Model.Entities;
using Safari.Popups;
using Safari.Scenes.Menus;
using Safari.Model.Entities.Tourists;
using Engine.Helpers;
using Engine.Input;
using Safari.Input;
using Microsoft.Xna.Framework.Graphics;
using Engine.Graphics.Stubs.Texture;
using Safari.Model.Entities.Animals;
using Safari.Model.Tiles;

namespace Safari.Scenes;

public enum MouseMode {
	Build,
	Demolish,
	Inspect
}

public class GameScene : Scene {
	private GameModel model;
	public static GameScene Active => SceneManager.Active as GameScene;
	public virtual GameModel Model => model;

	public MouseMode MouseMode { get; set; } = MouseMode.Inspect;
	public List<Rectangle> MaskedAreas { get; private set; } = new List<Rectangle>();

	/// <summary>
	/// Whether to skip placing the demo animals/plants/etc on the map
	/// </summary>
	public bool StrippedInit { get; set; } = false;

	/// <summary>
	/// Whether the GS is being initialized from an existing save
	/// </summary>
	public bool LoadInit { get; set; } = false;

	private readonly List<GameObject> simulationActors = [];
	private readonly Queue<GameObject> simulationActorAddQueue = new();
	private readonly Queue<GameObject> simulationActorRemoveQueue = new();

	private ITexture2D demolishHover;
	private ITexture2D buildHover;

	static GameScene() {
		DebugMode.AddFeature(new ExecutedDebugFeature("list-objects", () => {
			if (Active == null) return;

			foreach (GameObject obj in Active.GameObjects) {
				string objStr = obj is Entity e ? e.ToString() : obj.ToString();

				DebugConsole.Instance.Write($"{obj} {Utils.Format(obj.Position, false, false)}", false);
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("scene-reload", () => {
			SceneManager.Load(new GameScene(Active.model.ParkName, Active.model.Difficulty));
		}));
	}

	public GameScene() : this("test park", GameDifficulty.Easy) { }

	public GameScene(string parkName, GameDifficulty difficulty) : base() {
		DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		startDate = startDate.AddHours(6);
		model = new GameModel(parkName, 6000, difficulty, startDate);
	}

	public GameScene(GameModel model) : base() {
		this.model = model;
		LoadInit = true;
	}

	public override void Unload() {
		model.GameLost -= OnGameLost;
		model.GameWon -= OnGameWon;

		Jeep.Cleanup();

		if (!Game.Instance.IsHeadless) {
			Statusbar.Instance.Unload();
			EntityControllerMenu.Active?.Hide();
		}

        base.Unload();

		PostUpdate -= CollisionManager.PostUpdate;
		EntityBoundsManager.CleanUp();
		CollisionManager.CleanUp();
		PostProcessPasses.Remove(model.Level.LightManager);

		Game.ContentManager.Unload();
	}

	public override void Load() {
		// init game model
		// The start of the game is always <date of creation> 6 am
		if (!LoadInit) {
			ITexture2D staticBG = Game.CanDraw ? Game.LoadTexture("Assets/Background/Background") : new NoopTexture2D(null, 3584, 2048);
			model.Level = new Level(32, staticBG.Width / 32, staticBG.Height / 32, staticBG);
			AddObject(model.Level);

			if (!StrippedInit) {
				// try to spawn poachers after 6 hours of previous spawn with a 0.5 base chance, which increase by 0.05 every attempt
				Jeep.Init(400);
				Tourist.Init();
				Ranger.Init();

				EntitySpawner<Poacher> poacherSpawner = new(4, 0.5f, 0.05f) {
					EntityLimit = 5, // don't spawn if there are >= 5 poachers on the map
					EntityCount = () => model.PoacherCount // use PoacherCount to determine number of poachers on the map
				};
				Game.AddObject(poacherSpawner);

				Tourist.Spawner = new(.2f, 0.6f, 0.05f) {
					EntityLimit = 30,
					EntityCount = () => Tourist.Queue.Count,
					SpawnArea = new Rectangle(-64, 512, 32, 320),
					ExtraCondition = () => model.IsDaytime
				};
				Game.AddObject(Tourist.Spawner);
				Tourist.UpdateSpawner();
			}
		}

		if (!Game.Instance.IsHeadless) {
			PostProcessPasses.Add(model.Level.LightManager);
		}

		

		int tileSize = model.Level.TileSize;
		Vectangle mapBounds = new(0, 0, model.Level.MapWidth * tileSize, model.Level.MapHeight * tileSize);

		EntityBoundsManager.Init(mapBounds);
		CollisionManager.Init(mapBounds);
		PostUpdate += CollisionManager.PostUpdate;

		model.GameLost += OnGameLost;

		model.GameWon += OnGameWon;

		// init camera
		if (!LoadInit) {
			CreateCamera(
				new Rectangle(
					0, 0,
					model.Level.MapWidth * model.Level.TileSize,
					model.Level.MapHeight * model.Level.TileSize
				)
			);
		}

		if (Game.CanDraw) {
			UserInterface.Active.MouseInputProvider.DoClick();
		}

		ITexture2D outline = Utils.GenerateTexture(model.Level.TileSize, model.Level.TileSize, new Color(0.8f, 0.1f, 0.1f, 1f), true);
		ITexture2D fill = Utils.GenerateTexture(model.Level.TileSize, model.Level.TileSize, new Color(0.7f, 0.1f, 0.1f, 0.3f));
		demolishHover = Utils.MergeTextures(fill, outline);

		buildHover = Utils.GenerateTexture(model.Level.TileSize, model.Level.TileSize, new Color(0.1f, 0.3f, 0.7f, 1f), true);

		base.Load();
		MapBuilder.BuildStartingMap(model.Level, StrippedInit, LoadInit);

		if (Game.CanDraw) {
			Statusbar.Instance.Load();
			EntityManager.Instance.Load();
		}

		model.CheckWinLose = true;
	}

    public override void Update(GameTime gameTime) {
		PerformPreUpdate(gameTime);

		if (InputManager.Actions.JustPressed("cycle-interact-mode")) {
			if (MouseMode == MouseMode.Inspect) {
				MouseMode = MouseMode.Build;
			} else {
				MouseMode = MouseMode.Inspect;
			}
		} else if (InputManager.Actions.JustPressed("cycle-build-mode")) {
			if (MouseMode == MouseMode.Build) {
				MouseMode = MouseMode.Demolish;
			} else if (MouseMode == MouseMode.Demolish) {
				MouseMode = MouseMode.Build;
			}
		}

		UpdatePalette();

		/*Vector2 mouseTilePos = GetMouseTilePos();
		if (MousePlayable(mouseTilePos)) {
			if (MouseMode == MouseMode.Build || MouseMode == MouseMode.Demolish) {
				UpdateBuild();
			} else if (MouseMode == MouseMode.Inspect) {
				UpdateInspect();
			}
		}*/

		if (model.GameSpeed != GameSpeed.Paused) {
			model.Advance(gameTime);
		}

		if (Game.CanDraw) {
			Statusbar.Instance.Update(gameTime);
			EntityManager.Instance.Update(gameTime);
		}

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

	public void UpdateInspect() {
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
			if (InputManager.Actions.JustPressed("next-brush")) {
				Model.Level.ConstructionHelperCmp.SelectNext();
			}
			if (InputManager.Actions.JustPressed("prev-brush")) {
				Model.Level.ConstructionHelperCmp.SelectPrev();
			}
			if (InputManager.Actions.JustPressed("next-brush-variant")) {
				var cons = Model.Level.ConstructionHelperCmp;
				if (cons.SelectedItem != null) {
					cons.SelectedItem.SelectNext();
				}
			}
			if (InputManager.Actions.JustPressed("prev-brush-variant")) {
				var cons = Model.Level.ConstructionHelperCmp;
				if (cons.SelectedItem != null) {
					cons.SelectedItem.SelectNext();
				}
			}
		}
	}

	public void UpdateBuild() {
		if (InputManager.Mouse.IsDown(MouseButtons.LeftButton)) {
			Point p = (GetMouseTilePos() / Model.Level.TileSize).ToPoint();
			if (MouseMode == MouseMode.Build) {
				Model.Level.ConstructionHelperCmp.BuildCurrent(p);
			} else if (MouseMode == MouseMode.Demolish) {
				Model.Level.ConstructionHelperCmp.Demolish(p);
			}
		}
	}

	public bool MousePlayable(Vector2 mouseTilePos) {
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

	public Vector2 GetMouseTilePos() {
		Vector2 mouseTilePos = InputManager.Mouse.GetWorldPos();
		mouseTilePos.X -= mouseTilePos.X % Model.Level.TileSize;
		mouseTilePos.Y -= mouseTilePos.Y % Model.Level.TileSize;

		return mouseTilePos;
	}

	public override void Draw(GameTime gameTime) {
		if (MouseMode == MouseMode.Build) {
			var cons = Model.Level.ConstructionHelperCmp;
			if (cons.SelectedInstance != null) {
				Tile t = cons.SelectedInstance;
				Vector2 mousePos = GetMouseTilePos();
				Point tilePos = (mousePos / Model.Level.TileSize).ToPoint();
				t.DrawPreviewAt(GetMouseTilePos(), cons.CanBuildCurrent(tilePos));
				Game.SpriteBatch.Draw(buildHover.ToTexture2D(), mousePos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
			}
		} else if (MouseMode == MouseMode.Demolish) {
			Vector2 mousePosWorld = GetMouseTilePos();
			Game.SpriteBatch.Draw(demolishHover.ToTexture2D(), mousePosWorld, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
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
		Camera.Active.Zoom = CameraControllerCmp.DefaultZoom;

		AddObject(Camera.Active);
	}

	private void OnGameLost(object sender, LoseReason reason) {
		string message = reason switch {
			LoseReason.Money => "You ran out of funds.",
			LoseReason.Animals => "All of your animals have died.",
			_ => ""
		};

		if (Game.CanDraw) {
			AlertMenu menu = new AlertMenu("You lose!", message, "Return to main menu");
			menu.Chosen += (object sender, bool e) => {
				SceneManager.Load(MainMenu.Instance);
			};
			model.Pause();
			menu.Show();
		}
	}

	private void OnGameWon(object sender, EventArgs e) {
		string difficulty = model.Difficulty switch {
			GameDifficulty.Easy => "easy",
			GameDifficulty.Normal => "normal",
			_ => "hard"
		};

		if (Game.CanDraw) {
			AlertMenu menu = new AlertMenu("You win!", $"Congratulations on beating the game on {difficulty} difficulty!", "Return to main menu", "Keep playing...");
			menu.Chosen += (object sender, bool e) => {
				if (e) {
					SceneManager.Load(MainMenu.Instance);
				} else {
					model.Resume();
				}
			};
			model.Pause();
			menu.Show();
		}
	}
}
