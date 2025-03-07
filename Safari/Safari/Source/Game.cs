using Engine.Scenes;
using Engine.Debug;
using Engine.Input;
using Engine.Graphics;
using Safari.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using GeonBit.UI;
using GeonBit.UI.Entities;

namespace Safari;

public class Game : Engine.Game {
	private bool displayStats = false;
	private Paragraph statsLine = null;

	protected override void Initialize() {
		base.Initialize();

		InputSetup();

		GameScene scn = new();
		SceneManager.Load(scn);

		// GeonBit
		UserInterface.Initialize(Content, BuiltinThemes.hd);
		UserInterface.Active.ShowCursor = false;
		statsLine = new Paragraph();

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

		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-stats", () => {
			displayStats = !displayStats;

			if (displayStats) {
				UserInterface.Active.AddEntity(statsLine);
			} else {
				UserInterface.Active.RemoveEntity(statsLine);
			}
		}));

		DebugMode.Enable();
	}

	protected override void LoadContent() {
		base.LoadContent();
	}

	private readonly PerformanceCalculator tickTime = new(50), drawTime = new(50);
	protected override void Update(GameTime gameTime) {
		DateTime start = DateTime.Now;
		base.Update(gameTime);
		UserInterface.Active.Update(gameTime);
		tickTime.AddValue((DateTime.Now - start).TotalMilliseconds);
		
		if (displayStats) {
			statsLine.Text = $"{tickTime.Average:0.00} ms / {drawTime.Average:0.00} ms\n{tickTime.Max:0.00} ms / {drawTime.Max:0.00} ms";
		}
	}

	protected override void Draw(GameTime gameTime) {
		DateTime start = DateTime.Now;
		base.Draw(gameTime);
		UserInterface.Active.Draw(SpriteBatch);
		drawTime.AddValue((DateTime.Now - start).TotalMilliseconds);
	}

	private void InputSetup() {
		InputManager.Actions.Register("up", new InputAction(keys: [Keys.W, Keys.Up]));
		InputManager.Actions.Register("down", new InputAction(keys: [Keys.S, Keys.Down]));
		InputManager.Actions.Register("left", new InputAction(keys: [Keys.A, Keys.Left]));
		InputManager.Actions.Register("right", new InputAction(keys: [Keys.D, Keys.Right]));

		// debug
		InputManager.Keyboard.OnPressed(Keys.V, () => DebugMode.ToggleFeature("coll-check-areas"));
		InputManager.Keyboard.OnPressed(Keys.C, () => DebugMode.ToggleFeature("coll-draw"));		
		InputManager.Keyboard.OnPressed(Keys.F, () => DebugMode.Execute("toggle-fullscreen"));
		InputManager.Keyboard.OnPressed(Keys.P, () => DebugMode.Execute("toggle-stats"));
	}
}
