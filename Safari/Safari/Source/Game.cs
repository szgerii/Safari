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
using System.Collections.Generic;
using Safari.Debug;
using Safari.Model;

namespace Safari;

public class Game : Engine.Game {
	public bool DisplayDebugInfos { get; private set; } = false;

	private Dictionary<DebugInfoPosition, Paragraph> debugInfoParagraphs = new();

	protected override void Initialize() {
		base.Initialize();

		//DisplayManager.SetTargetFPS(60, false);
		DisplayManager.SetVSync(true, true);

		InputSetup();

		GameScene scn = new();
		SceneManager.Load(scn);

		DebugMode.AddFeature(new ExecutedDebugFeature("scene-reload", () => {
			SceneManager.Load(new GameScene());
		}));
		InputManager.Keyboard.OnPressed(Keys.R, () => DebugMode.Execute("scene-reload"));

		// GeonBit

		UserInterface.Initialize(Content, BuiltinThemes.editor);
		UserInterface.Active.ShowCursor = false;
		UserInterface.Active.GlobalScale = 1.25f;
		foreach (DebugInfoPosition pos in Enum.GetValues(typeof(DebugInfoPosition))) {
			debugInfoParagraphs[pos] = new Paragraph();
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

		DebugMode.AddFeature(new ExecutedDebugFeature("advance-gamespeed", () => {
			if (SceneManager.Active is GameScene) {
				GameModel model = GameScene.Active.Model;
				switch (model.GameSpeed) {
					case GameSpeed.Slow: model.GameSpeed = GameSpeed.Medium; break;
					case GameSpeed.Medium: model.GameSpeed = GameSpeed.Fast; break;
					case GameSpeed.Fast: model.GameSpeed = GameSpeed.Slow; break;
				}
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-simulation", () => {
			if (SceneManager.Active is GameScene) {
				GameModel model = GameScene.Active.Model;
				switch (model.GameSpeed) {
					case GameSpeed.Paused: model.Resume(); break;
					default: model.Pause(); break;
				}
			}
		}));

		DebugMode.Enable();
	}

	protected override void LoadContent() {
		base.LoadContent();
	}

	private readonly PerformanceCalculator tickTime = new(50), drawTime = new(50);
	protected override void Update(GameTime gameTime) {
		DebugInfoManager.PreUpdate();

		DateTime start = DateTime.Now;

		base.Update(gameTime);
		UserInterface.Active.Update(gameTime);

		tickTime.AddValue((DateTime.Now - start).TotalMilliseconds);
		
		if (DisplayDebugInfos) {
			DebugInfoManager.AddInfo("avg", $"{tickTime.Average:0.00} ms / {drawTime.Average:0.00} ms (out of {tickTime.Capacity})");
			DebugInfoManager.AddInfo("max", $"{tickTime.Max:0.00} ms / {drawTime.Max:0.00} ms (out of {drawTime.Capacity})");

			DebugInfoManager.AddInfo("FPS (Update)", $"{(1f / gameTime.ElapsedGameTime.TotalSeconds):0.00}", DebugInfoPosition.TopRight);
		
			if (SceneManager.Active is GameScene) {
				PrintModelDebugInfos();
			}
		}
	}

	protected override void Draw(GameTime gameTime) {
		DebugInfoManager.AddInfo("FPS (Draw)", $"{(1f / gameTime.ElapsedGameTime.TotalSeconds):0.00}", DebugInfoPosition.TopRight);
		DebugInfoManager.PreDraw();

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

		InputManager.Actions.Register("reset-zoom", new InputAction(keys: [Keys.NumPad0, Keys.D0]));
		InputManager.Actions.Register("increase-zoom", new InputAction(keys: [Keys.O]));
		InputManager.Actions.Register("decrease-zoom", new InputAction(keys: [Keys.I]));

		InputManager.Actions.Register("fast-mod", new InputAction(keys: [Keys.LeftShift, Keys.RightShift]));
		InputManager.Actions.Register("slow-mod", new InputAction(keys: [Keys.LeftControl, Keys.RightControl]));

		// debug
		InputManager.Keyboard.OnPressed(Keys.V, () => DebugMode.ToggleFeature("coll-check-areas"));
		InputManager.Keyboard.OnPressed(Keys.C, () => DebugMode.ToggleFeature("coll-draw"));		
		InputManager.Keyboard.OnPressed(Keys.F, () => DebugMode.Execute("toggle-fullscreen"));
		InputManager.Keyboard.OnPressed(Keys.P, () => DebugMode.Execute("toggle-debug-infos"));
		InputManager.Keyboard.OnPressed(Keys.K, () => DebugMode.Execute("advance-gamespeed"));
		InputManager.Keyboard.OnPressed(Keys.L, () => DebugMode.Execute("toggle-simulation"));
	}

	private void PrintModelDebugInfos() {
		GameModel model = GameScene.Active.Model;

		string speedName = "";
		switch (model.GameSpeed) {
			case GameSpeed.Slow: speedName = "Slow"; break;
			case GameSpeed.Medium: speedName = "Medium"; break;
			case GameSpeed.Fast: speedName = "Fast"; break;
			case GameSpeed.Paused: speedName = "Paused"; break;
		}
		DebugInfoManager.AddInfo("Current gamespeed", speedName, DebugInfoPosition.BottomLeft);
		DebugInfoManager.AddInfo("Day/Night cycle", model.IsDaytime ? "Day" : "Night", DebugInfoPosition.BottomLeft);
		DebugInfoManager.AddInfo("Irl time passed ", TimeSpan.FromSeconds(model.CurrentTime).ToString(@"hh\:mm\:ss"), DebugInfoPosition.BottomLeft);
		DebugInfoManager.AddInfo("In-game days passed", $"{model.IngameDays:0.00}", DebugInfoPosition.BottomLeft);
		DebugInfoManager.AddInfo("In-game date", $"{model.IngameDate}", DebugInfoPosition.BottomLeft);
	}
}
