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

namespace Safari;

public class Game : Engine.Game {
	public bool DisplayDebugInfos { get; private set; } = false;

	protected override void Initialize() {
		base.Initialize();

		// DisplayManager.SetTargetFPS(90, false);
		DisplayManager.SetVSync(true, true);

		InputSetup();

		DebugMode.AddFeature(new ExecutedDebugFeature("scene-reload", () => {
			SceneManager.Load(new GameScene());
		}));
		InputManager.Keyboard.OnPressed(Keys.R, () => DebugMode.Execute("scene-reload"));

		// GeonBit

		// create separate content manager for geonbit, so we can unload our assets in peace
		ContentManager uiContentManager = new ContentManager(Services, Content.RootDirectory);

		UserInterface.Initialize(uiContentManager, BuiltinThemes.hd);
		UserInterface.Active.ShowCursor = false;
		UserInterface.Active.GlobalScale = 1.25f;
        UserInterface.Active.UseRenderTarget = true;

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
        SceneManager.Load(MainMenu.Instance);
    }

    protected override void LoadContent() {
		base.LoadContent();
	}

	private readonly PerformanceCalculator tickTime = new(50), drawTime = new(50);
	protected override void Update(GameTime gameTime) {
		DebugInfoManager.PreUpdate();
		DebugConsole.Instance?.Update(gameTime);
		AlertMenu.Adjust();
        PauseMenu.Instance?.Update(gameTime);
		MainMenu.Instance.Update(gameTime);
		NewGameMenu.Instance.Update(gameTime);
		EntityControllerMenu.Active?.Update(gameTime);

        DateTime start = DateTime.Now;

		UserInterface.Active.Update(gameTime);
		base.Update(gameTime);

		tickTime.AddValue((DateTime.Now - start).TotalMilliseconds);
		
		if (DisplayDebugInfos) {
			DebugInfoManager.AddInfo("avg", $"{tickTime.Average:0.00} ms / {drawTime.Average:0.00} ms (out of {tickTime.Capacity})");
			DebugInfoManager.AddInfo("max", $"{tickTime.Max:0.00} ms / {drawTime.Max:0.00} ms (out of {drawTime.Capacity})");

			DebugInfoManager.AddInfo("FPS (Update)", $"{(1f / gameTime.ElapsedGameTime.TotalSeconds):0.00}", DebugInfoPosition.TopRight);
		
			if (SceneManager.Active is GameScene) {
				GameScene.Active.Model.PrintModelDebugInfos();
			}
		}
    }

    protected override void Draw(GameTime gameTime) {
		DebugInfoManager.AddInfo("FPS (Draw)", $"{(1f / gameTime.ElapsedGameTime.TotalSeconds):0.00}", DebugInfoPosition.TopRight);

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

		InputManager.Actions.Register("reset-zoom", new InputAction(keys: [Keys.NumPad0, Keys.D0]));
		InputManager.Actions.Register("increase-zoom", new InputAction(keys: [Keys.O]));
		InputManager.Actions.Register("decrease-zoom", new InputAction(keys: [Keys.I]));

		InputManager.Actions.Register("fast-mod", new InputAction(keys: [Keys.LeftShift, Keys.RightShift]));
		InputManager.Actions.Register("slow-mod", new InputAction(keys: [Keys.LeftControl, Keys.RightControl]));

        // debug
        InputManager.Keyboard.OnPressed(Keys.F1, () => DebugConsole.Instance.ToggleDebugConsole());
        InputManager.Keyboard.OnPressed(Keys.F2, () => Statusbar.Instance.Toggle());
        InputManager.Keyboard.OnPressed(Keys.Escape, () => PauseMenu.Instance.TogglePauseMenu());
        InputManager.Keyboard.OnPressed(Keys.Space, () => Statusbar.Instance.SetSpeed(GameSpeed.Paused));
        InputManager.Keyboard.OnPressed(Keys.D1, () => Statusbar.Instance.SetSpeed(GameSpeed.Slow));
        InputManager.Keyboard.OnPressed(Keys.D2, () => Statusbar.Instance.SetSpeed(GameSpeed.Medium));
        InputManager.Keyboard.OnPressed(Keys.D3, () => Statusbar.Instance.SetSpeed(GameSpeed.Fast));
        InputManager.Keyboard.OnPressed(Keys.Tab, () => EntityManager.Instance.Toggle());
        InputManager.Keyboard.OnPressed(Keys.V, () => DebugMode.ToggleFeature("coll-check-areas"));
		InputManager.Keyboard.OnPressed(Keys.C, () => DebugMode.ToggleFeature("coll-draw"));
		InputManager.Keyboard.OnPressed(Keys.F, () => DebugMode.Execute("toggle-fullscreen"));
		InputManager.Keyboard.OnPressed(Keys.P, () => DebugMode.Execute("toggle-debug-infos"));
		InputManager.Keyboard.OnPressed(Keys.K, () => DebugMode.Execute("advance-gamespeed"));
		InputManager.Keyboard.OnPressed(Keys.L, () => DebugMode.Execute("toggle-simulation"));
		InputManager.Keyboard.OnPressed(Keys.G, () => DebugMode.ToggleFeature("draw-grid"));
		InputManager.Keyboard.OnPressed(Keys.H, () => DebugMode.ToggleFeature("animal-indicators"));
		InputManager.Keyboard.OnPressed(Keys.X, () => DebugMode.ToggleFeature("entity-interact-bounds"));
		InputManager.Keyboard.OnPressed(Keys.Z, () => DebugMode.Execute("request-route"));
		InputManager.Keyboard.OnPressed(Keys.U, () => DebugMode.ToggleFeature("draw-route"));
	}
}
