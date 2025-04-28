using Engine.Scenes;
using Engine.Debug;
using Engine.Input;
using Engine.Graphics;
using Safari.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using GeonBit.UI;
using Safari.Debug;
using Safari.Model;
using Safari.Popups;
using Safari.Scenes.Menus;
using System.Reflection;
using System.Linq;
using Safari.Helpers;

namespace Safari;

public enum GameStartupMode { Default, MainMenu, DemoScene, EmptyScene }

public class Game : Engine.Game {
	/// <summary>
	/// Whether to output the debug info collected through <see cref="DebugInfoManager"/> to the screen
 	/// </summary>
	public bool DisplayDebugInfos { get; private set; } = false;

	/// <summary>
	/// The state into which the game will be initialized
	/// </summary>
	public GameStartupMode StartupMode { get; init; } = GameStartupMode.Default;

	/// <param name="headless">Whether to start the game without any graphics (requires the SDL_VIDEODRIVER env var to be set to 'dummy' to function properly)</param>
	public Game(bool headless = false) : base(headless) { }

	protected override void Initialize() {
		BaseResolution = new Point(1920, 1080);

		base.Initialize();

		if (!IsHeadless) {
			// DisplayManager.SetTargetFPS(90, false);
			DisplayManager.SetVSync(true, true);
		}

		InputSetup();

		DebugMode.AddFeature(new ExecutedDebugFeature("scene-reload", () => {
			SceneManager.Load(new GameScene());
		}));
		InputManager.Keyboard.OnPressed(Keys.R, () => DebugMode.Execute("scene-reload"));

		if (!IsHeadless) {
			// GeonBit
			// create separate content manager for geonbit, so we can unload our assets in peace
			ContentManager uiContentManager = new ContentManager(Services, Content.RootDirectory);
			UserInterface.Initialize(uiContentManager, BuiltinThemes.hd);
			UserInterface.Active.ShowCursor = false;
			UserInterface.Active.GlobalScale = 1.25f;
			UserInterface.Active.UseRenderTarget = true;
		}

		// debug stuff

		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-fullscreen", () => {
			if (DisplayManager.WindowType == WindowType.FULL_SCREEN) {
				DisplayManager.SetWindowType(WindowType.WINDOWED, false);
				DisplayManager.SetResolution(1280, 720, false);
			} else {
				DisplayManager.SetWindowType(WindowType.FULL_SCREEN, false);
				DisplayMode nativeRes = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
				DisplayManager.SetResolution(nativeRes.Width, nativeRes.Height, false);
			}
			DisplayManager.ApplyChanges();
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-debug-infos", () => {
			DisplayDebugInfos = !DisplayDebugInfos;

			if (DisplayDebugInfos) {
				DebugInfoManager.ShowInfos();
			} else {
				DebugInfoManager.HideInfos();
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("dump-map", () => {
			if (SceneManager.Active is GameScene) {
				MapBuilder.DumpMap(GameScene.Active.Model.Level);
			}
		}));

        DebugMode.Enable();

		// startup

		GameStartupMode finalStartupMode = StartupMode;
		if (StartupMode == GameStartupMode.Default) {
			finalStartupMode = IsHeadless ? GameStartupMode.EmptyScene : GameStartupMode.MainMenu;
		}

		switch (finalStartupMode) {
			case GameStartupMode.MainMenu:
				SceneManager.Load(MainMenu.Instance);
				break;
			case GameStartupMode.DemoScene:
				SceneManager.Load(new GameScene());
				break;
			case GameStartupMode.EmptyScene:
				SceneManager.Load(new GameScene() { StrippedInit = true });
				break;
			default:
				throw new InvalidOperationException("Invalid game startup mode");
		}
    }

    protected override void LoadContent() {
		base.LoadContent();
	}

	protected override void Dispose(bool disposing) {
		base.Dispose(disposing);

		// reset singletons
		var singletons = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IResettableSingleton)));
		foreach (var singleton in singletons) {
			singleton.GetMethod("ResetSingleton", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
		}

		if (!IsHeadless) {
			UserInterface.Active.Dispose();
		}
	}

	private readonly PerformanceCalculator tickTime = new(50), drawTime = new(50);
	private readonly PerformanceCalculator updateFPS = new(10), drawFPS = new(10);
	protected override void Update(GameTime gameTime) {
        DateTime start = DateTime.Now;

		if (!IsHeadless) {
			DebugInfoManager.PreUpdate();
			DebugConsole.Instance.Update(gameTime);
			AlertMenu.Adjust();
			PauseMenu.Instance.Update(gameTime);
			MainMenu.Instance.Update(gameTime);
			NewGameMenu.Instance.Update(gameTime);
			EntityControllerMenu.Active?.Update(gameTime);

			UserInterface.Active.Update(gameTime);
		}

		base.Update(gameTime);

		tickTime.AddValue((DateTime.Now - start).TotalMilliseconds);
		updateFPS.AddValue(1f / gameTime.ElapsedGameTime.TotalSeconds);
		
		if (DisplayDebugInfos) {
			DebugInfoManager.AddInfo("avg", $"{tickTime.Average:0.00} ms / {drawTime.Average:0.00} ms (out of {tickTime.Capacity})");
			DebugInfoManager.AddInfo("max", $"{tickTime.Max:0.00} ms / {drawTime.Max:0.00} ms (out of {drawTime.Capacity})");

			DebugInfoManager.AddInfo("FPS (Update)", $"{updateFPS.Average:0}", DebugInfoPosition.TopRight);
		
			if (SceneManager.Active is GameScene) {
				GameScene.Active.Model.PrintModelDebugInfos();
			}
		}
    }

    protected override void Draw(GameTime gameTime) {
		// this is a safeguard, SuppressDraw should still be used instead for faster Draw skipping in headless environments
		if (IsHeadless) return;

		drawFPS.AddValue(1f / gameTime.ElapsedGameTime.TotalSeconds);
		DebugInfoManager.AddInfo("FPS (Draw)", $"  {drawFPS.Average:0}", DebugInfoPosition.TopRight);

		DebugInfoManager.PreDraw();

		DateTime start = DateTime.Now;

		UserInterface.Active.Draw(SpriteBatch);

		base.Draw(gameTime);
        
		UserInterface.Active.DrawMainRenderTarget(SpriteBatch);
        
		drawTime.AddValue((DateTime.Now - start).TotalMilliseconds);
	}

	private void InputSetup() {
		InputManager.Actions.Register("up", new InputAction(keys: [Keys.W, Keys.Up]));
		InputManager.Actions.Register("down", new InputAction(keys: [Keys.S, Keys.Down]));
		InputManager.Actions.Register("left", new InputAction(keys: [Keys.A, Keys.Left]));
		InputManager.Actions.Register("right", new InputAction(keys: [Keys.D, Keys.Right]));

		InputManager.Actions.Register("reset-zoom", new InputAction(keys: [Keys.F12]));
		InputManager.Actions.Register("increase-zoom", new InputAction(keys: [Keys.O]));
		InputManager.Actions.Register("decrease-zoom", new InputAction(keys: [Keys.I]));

		InputManager.Actions.Register("fast-mod", new InputAction(keys: [Keys.LeftShift, Keys.RightShift]));
		InputManager.Actions.Register("slow-mod", new InputAction(keys: [Keys.LeftControl, Keys.RightControl]));

		// construction (debug)
		InputManager.Actions.Register("prev-brush", new InputAction(keys: [Keys.N]));
		InputManager.Actions.Register("next-brush", new InputAction(keys: [Keys.M]));
		InputManager.Actions.Register("prev-brush-variant", new InputAction(keys: [Keys.V]));
		InputManager.Actions.Register("next-brush-variant", new InputAction(keys: [Keys.B]));
		InputManager.Actions.Register("cycle-interact-mode", new InputAction(keys: [Keys.D8]));
		InputManager.Actions.Register("cycle-build-mode", new InputAction(keys: [Keys.D9]));

        // debug
        InputManager.Keyboard.OnPressed(Keys.F1, () => DebugConsole.Instance.ToggleDebugConsole());
        InputManager.Keyboard.OnPressed(Keys.F2, () => Statusbar.Instance.Toggle());
        InputManager.Keyboard.OnPressed(Keys.Escape, () => PauseMenu.Instance.TogglePauseMenu());
        InputManager.Keyboard.OnPressed(Keys.Space, () => Statusbar.Instance.SetSpeed(GameSpeed.Paused));
        InputManager.Keyboard.OnPressed(Keys.D1, () => Statusbar.Instance.SetSpeed(GameSpeed.Slow));
        InputManager.Keyboard.OnPressed(Keys.D2, () => Statusbar.Instance.SetSpeed(GameSpeed.Medium));
        InputManager.Keyboard.OnPressed(Keys.D3, () => Statusbar.Instance.SetSpeed(GameSpeed.Fast));
        InputManager.Keyboard.OnPressed(Keys.Tab, () => EntityManager.Instance.Toggle());
		InputManager.Keyboard.OnPressed(Keys.C, () => DebugMode.ToggleFeature("draw-colliders"));
		InputManager.Keyboard.OnPressed(Keys.F, () => DebugMode.Execute("toggle-fullscreen"));
		InputManager.Keyboard.OnPressed(Keys.P, () => DebugMode.Execute("toggle-debug-infos"));
		InputManager.Keyboard.OnPressed(Keys.H, () => DebugMode.ToggleFeature("animal-indicators"));
		InputManager.Keyboard.OnPressed(Keys.X, () => DebugMode.ToggleFeature("entity-interact-bounds"));
	}
}
