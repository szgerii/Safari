using Engine.Debug;
using Engine.Scenes;
using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Model.Tiles;
using Safari.Objects.Entities;
using Safari.Objects.Entities.Animals;
using Safari.Scenes;
using System;
using System.Collections.Generic;
using Safari.Components;

namespace Safari.Model;

public enum GameDifficulty {
	Easy,
	Normal,
	Hard
}

public enum GameSpeed {
	Paused,
	Slow,
	Medium,
	Fast
}

public class GameModel {
	/// <summary>
	/// How much faster 'medium' speed is compared to 'slow'
	/// </summary>
	private const int mediumMultiplier = 4;
	/// <summary>
	/// How much faster 'fast' speed is compared to 'medium'
	/// </summary>
	private const int fastMultiplier = 7;
	/// <summary>
	/// Length of an in-game day (irl seconds), when the game speed
	/// is set to 'slow'
	/// </summary>
	private const double dayLength = 60.0;

	private string parkName;
	private int funds;
	private GameDifficulty difficulty;
	private GameSpeed gameSpeed = GameSpeed.Slow;
	private GameSpeed prevSpeed;
	private double currentTime = 0;
	private DateTime startDate;

	public string ParkName => parkName;
	public int Funds {
		get => funds;
		set {
			funds = value;
		}
	}
	public GameDifficulty Difficulty => difficulty;

	public GameSpeed GameSpeed {
		get => gameSpeed;
		set {
			gameSpeed = value;
		}
	}
	/// <summary>
	/// How fast the simulation should be updated at the current speed setting
	/// </summary>
	public int SpeedMultiplier {
		get {
			switch (gameSpeed) {
				case GameSpeed.Slow: return 1;
				case GameSpeed.Medium: return mediumMultiplier;
				case GameSpeed.Fast: return mediumMultiplier * fastMultiplier;
				default: return 0;
			}
		}
	}
	/// <summary>
	/// Time passed (in irl seconds) since the start of the game
	/// </summary>
	public double CurrentTime => currentTime;
	/// <summary>
	/// Returns a value between 0 and 1, corresponding to the time of day
	/// (where 0 means ~6 am, .5 means ~6 pm and so on)
	/// Values between 0 and .5 mean it's daytime,
	/// Values between .5 and 1 mean it's nighttime
	/// </summary>
	public double TimeOfDay {
		get {
			return (currentTime / dayLength) % 1.0;
		}
	}

	/// <summary>
	/// Indicates whether it is currently day time in-game
	/// (TimeOfDay < 0.54) ~ .5 plus extra time for sunset
	/// </summary>
	public bool IsDaytime => TimeOfDay < 0.54f;

	/// <summary>
	/// Time passed (in in-game days) since the start of the game
	/// </summary>
	public double IngameDays => currentTime / dayLength;
	/// <summary>
	/// The current in-game date
	/// </summary>
	public DateTime IngameDate => startDate.AddDays(currentTime / dayLength);

	/// <summary>
	/// The current game level's tilemap
	/// </summary>
	public Level Level { get; set; }

	public int EntityCount { get; set; }
	public int AnimalCount { get; set; }
	public int CarnivoreCount { get; set; }
	public int HerbivoreCount { get; set; }
	public int TouristCount { get; set; }
	public int JeepCount { get; set; }
	public int PoacherCount { get; set; }
	public int RangerCount { get; set; }

	static GameModel() {
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

        DebugMode.AddFeature(new ExecutedDebugFeature("gamespeed-slow", () => {
            if (SceneManager.Active is GameScene) {
                GameModel model = GameScene.Active.Model;
                model.GameSpeed = GameSpeed.Slow;
            }
        }));

        DebugMode.AddFeature(new ExecutedDebugFeature("gamespeed-medium", () => {
            if (SceneManager.Active is GameScene) {
                GameModel model = GameScene.Active.Model;
                model.GameSpeed = GameSpeed.Medium;
            }
        }));

        DebugMode.AddFeature(new ExecutedDebugFeature("gamespeed-fast", () => {
            if (SceneManager.Active is GameScene) {
                GameModel model = GameScene.Active.Model;
                model.GameSpeed = GameSpeed.Fast;
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
    }

	public GameModel(string parkName, int funds, GameDifficulty difficulty, DateTime startDate) {
		this.parkName = parkName;
		this.funds = funds;
		this.difficulty = difficulty;
		this.startDate = startDate;

		Texture2D staticBG = Game.ContentManager.Load<Texture2D>("Assets/Background/Background");
		Level = new Level(32, staticBG.Width / 32, staticBG.Height / 32, staticBG);

		Game.AddObject(Level);

		DebugMode.AddFeature(new LoopedDebugFeature("draw-grid", Level.PostDraw, GameLoopStage.POST_DRAW));
	}

	/// <summary>
	/// Should be called every 'simulation-update'
	/// </summary>
	/// <param name="gameTime">MG GameTime</param>
	public void Advance(GameTime gameTime) {
		this.currentTime += gameTime.ElapsedGameTime.TotalSeconds;
	}

	public void Pause() {
		if (gameSpeed != GameSpeed.Paused) {
			prevSpeed = gameSpeed;
			gameSpeed = GameSpeed.Paused;
		}
	}

	public void Resume() {
		if (gameSpeed == GameSpeed.Paused) {
			gameSpeed = prevSpeed;
		}
	}
}
