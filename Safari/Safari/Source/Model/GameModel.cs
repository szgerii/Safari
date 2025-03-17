using Engine.Debug;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Model.Tiles;
using Safari.Scenes;
using System;
using System.Collections.Generic;

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
	private int entityCount = 0;
	private int animalCount = 0;
	private int carnivoreCount = 0;
	private int herbivoreCount = 0;
	private int touristCount = 0;
	private int jeepCount = 0;
	private int poacherCount = 0;
	private int rangerCount = 0;

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
	/// (TimeOfDay < 0.5)
	/// </summary>
	public bool IsDaytime => TimeOfDay < 0.5f;

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

	public int EntityCount {
		get => entityCount;
		set => entityCount = value;
	}
	public int AnimalCount {
		get => animalCount;
		set => animalCount = value;
	}
	public int CarnivoreCount {
		get => carnivoreCount;
		set => carnivoreCount = value;
	}
	public int HerbivoreCount {
		get => herbivoreCount;
		set => herbivoreCount = value;
	}
	public int TouristCount {
		get => touristCount;
		set => touristCount = value;
	}
	public int JeepCount {
		get => jeepCount;
		set => jeepCount = value;
	}
	public int PoacherCount {
		get => poacherCount;
		set => poacherCount = value;
	}
	public int RangerCount {
		get => rangerCount;
		set => rangerCount = value;
	}

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

		GenerateTestLevel();

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

	// TODO: remove once not needed
	private void GenerateTestLevel() {
		List<List<Tile>> tiles = new();
		tiles.Add([
			new Bush(),
			new WideBush()
		]);
		tiles.Add([
			new Tree(TreeType.Digitata),
			new Tree(TreeType.Grandideri),
			new Tree(TreeType.ShortGrandideri),
			new Tree(TreeType.Gregorii),
		]);
		tiles.Add([
			new Tree(TreeType.Rubrostipa),
			new Tree(TreeType.Suarazensis),
			new Tree(TreeType.Za)
		]);

		int x = 0, y = 0;
		foreach (List<Tile> row in tiles) {
			x = 0;

			int maxY = -1;
			foreach (Tile tile in row) {
				Level.SetTile(x + tile.AnchorTile.X, y + tile.AnchorTile.Y, tile);

				x += tile.Size.X;
				if (tile.Size.Y > maxY) maxY = tile.Size.Y;
			}

			y += maxY;
		}
	}
}
