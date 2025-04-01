using Engine.Debug;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Model.Tiles;
using Safari.Objects.Entities.Animals;
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

public enum LoseReason {
	Money,
	Animals
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
	
	// constants for money requirements for winning
	private const int WIN_FUNDS_EASY = 30000;
	private const int WIN_FUNDS_NORMAL = 50000;
	private const int WIN_FUNDS_HARD = 80000;

	// constants for herbivore count requirements for winning
	private const int WIN_HERB_EASY = 5;
	private const int WIN_HERB_NORMAL = 40;
	private const int WIN_HERB_HARD = 50;

	// constants for carnivore count requirements for winning
	private const int WIN_CARN_EASY = 5;
	private const int WIN_CARN_NORMAL = 40;
	private const int WIN_CARN_HARD = 50;

	// constants storing how long the player has to keep the winning conditions
	private const int WIN_DAYS_EASY = 3;
	private const int WIN_DAYS_NORMAL = 30;
	private const int WIN_DAYS_HARD = 60;
	
	private GameSpeed prevSpeed;
	private DateTime startDate;

	/// <summary>
	/// The name of the park (used when saving the park)
	/// </summary>
	public string ParkName { get; init; }
	private int funds;
	/// <summary>
	/// How much money ($?) the player currently has
	/// Reaching 0 will result in an instant game over
	/// </summary>
	public int Funds {
		get => funds;
		set {
			funds = value;
			if (CheckWinLose && funds <= 0) {
				TriggerLose(LoseReason.Money);
			} else if (CheckWinLose) {
				WinUpdate();
			}
		}
	}
	/// <summary>
	/// The selected difficulty for this park (easy, normal or hard)
	/// </summary>
	public GameDifficulty Difficulty { get; init; }

	/// <summary>
	/// The current speed of the simulation (paused, slow, medium, fast)
	/// </summary>
	public GameSpeed GameSpeed { get; set; } = GameSpeed.Slow;
	/// <summary>
	/// How fast the simulation should be updated at the current speed setting
	/// </summary>
	public int SpeedMultiplier => GameSpeed switch {
		GameSpeed.Slow => 1,
		GameSpeed.Medium => mediumMultiplier,
		_ => fastMultiplier * mediumMultiplier
	};
	/// <summary>
	/// Time passed (in irl seconds) since the start of the game
	/// </summary>
	public double CurrentTime { get; private set; } = 0.0f;
	/// <summary>
	/// Returns a value between 0 and 1, corresponding to the time of day
	/// (where 0 means ~6 am, .5 means ~6 pm and so on)
	/// Values between 0 and .5 mean it's daytime,
	/// Values between .5 and 1 mean it's nighttime
	/// </summary>
	public double TimeOfDay => (CurrentTime / dayLength) % 1.0;

	/// <summary>
	/// Indicates whether it is currently day time in-game
	/// (TimeOfDay < 0.54) ~ .5 plus extra time for sunset
	/// </summary>
	public bool IsDaytime => TimeOfDay < 0.54f;

	/// <summary>
	/// Time passed (in in-game days) since the start of the game
	/// </summary>
	public double IngameDays => CurrentTime / dayLength;
	/// <summary>
	/// The current in-game date
	/// </summary>
	public DateTime IngameDate => startDate.AddDays(CurrentTime / dayLength);

	/// <summary>
	/// The current game level's tilemap
	/// </summary>
	public Level Level { get; set; }

	/// <summary>
	/// The total number of entities in the game
	/// </summary>
	public int EntityCount { get; set; }
	private int animalCount;
	/// <summary>
	/// The total number of animals in the game
	/// This counter reaching zero will result in an instant game over
	/// </summary>
	public int AnimalCount {
		get => animalCount;
		set {
			animalCount = value;
			if (CheckWinLose && animalCount <= 0) {
				TriggerLose(LoseReason.Animals);
			}
		}
	}
	private int carnivoreCount;
	/// <summary>
	/// The number of carnivore animals in the park
	/// </summary>
	public int CarnivoreCount {
		get => carnivoreCount;
		set {
			carnivoreCount = value;
			if (CheckWinLose) {
				WinUpdate();
			}
		}
	}
	private int herbivoreCount;
	/// <summary>
	/// The number of herbivore animals in the park
	/// </summary>
	public int HerbivoreCount {
		get => herbivoreCount;
		set {
			herbivoreCount = value;
			if (CheckWinLose) {
				WinUpdate();
			}
		}
	}
	/// <summary>
	/// The number of tourists in the park (waiting or in a jeep)
	/// </summary>
	public int TouristCount { get; set; }
	/// <summary>
	/// The number of jeeps in the park
	/// </summary>
	public int JeepCount { get; set; }
	/// <summary>
	/// The number of poachers "attacking" the park
	/// </summary>
	public int PoacherCount { get; set; }
	/// <summary>
	/// The number of rangers defending the park
	/// </summary>
	public int RangerCount { get; set; }

	/// <summary>
	/// The ammount of money the player has to have in order to win
	/// </summary>
	public int WinCriteriaFunds => Difficulty switch {
		GameDifficulty.Easy => WIN_FUNDS_EASY,
		GameDifficulty.Normal => WIN_FUNDS_NORMAL,
		_ => WIN_FUNDS_HARD,
	};
	/// <summary>
	/// The ammount of herbivore animals the players has to have in order to win
	/// </summary>
	public int WinCriteriaHerb => Difficulty switch {
		GameDifficulty.Easy => WIN_HERB_EASY,
		GameDifficulty.Normal => WIN_HERB_NORMAL,
		_ => WIN_HERB_HARD,
	};
	/// <summary>
	/// The ammount of carnivore animals the players has to have in order to win
	/// </summary>
	public int WinCriteriaCarn => Difficulty switch {
		GameDifficulty.Easy => WIN_CARN_EASY,
		GameDifficulty.Normal => WIN_CARN_NORMAL,
		_ => WIN_CARN_HARD,
	};
	/// <summary>
	/// How long the player has to keep the winning conditions
	/// </summary>
	public int WinCriteriaDays => Difficulty switch {
		GameDifficulty.Easy => WIN_DAYS_EASY,
		GameDifficulty.Normal => WIN_DAYS_NORMAL,
		_ => WIN_DAYS_HARD,
	};

	private bool winTimerRunning = false;
	/// <summary>
	/// How many days are left until winning
	/// </summary>
	public double WinTimerDays { get; private set; } = 0.0;
	/// <summary>
	/// How much time is left until winning
	/// </summary>
	public TimeSpan WinTimerTime => winTimerRunning ? TimeSpan.FromDays(WinTimerDays) : TimeSpan.FromDays(WinCriteriaDays);
	/// <summary>
	/// Controls whether the model checks for winning / losing (turn off for testing)
	/// Automatically turned of once the players wins, meaning in "post-game" the player cant lose
	/// </summary>
	public bool CheckWinLose { get; set; } = true;
	/// <summary>
	/// Stores whether this save was won
	/// </summary>
	public bool PostWin { get; set; } = false;

	/// <summary>
	/// Invoked when the player has lost the game
	/// </summary>
	public event EventHandler<LoseReason> GameLost;
	/// <summary>
	/// Invoked when the player has won the game
	/// </summary>
	public event EventHandler GameWon;

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

		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-gameover-checks", () => {
			if (SceneManager.Active is GameScene gs) {
				gs.Model.CheckWinLose = !gs.Model.CheckWinLose;
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("add-money", () => {
			if (SceneManager.Active is GameScene gs) {
				gs.Model.Funds += 10000;
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("subtract-money", () => {
			if (SceneManager.Active is GameScene gs) {
				gs.Model.Funds -= 10000;
			}
		}));
	}

	public GameModel(string parkName, int funds, GameDifficulty difficulty, DateTime startDate) {
		ParkName = parkName;
		Funds = funds;
		Difficulty = difficulty;
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
		CurrentTime += gameTime.ElapsedGameTime.TotalSeconds;
		if (CheckWinLose && winTimerRunning) {
			WinTimerDays -= gameTime.ElapsedGameTime.TotalSeconds / dayLength;
			if (WinTimerDays <= 0.0) {
				TriggerWin();
				CheckWinLose = false;
				winTimerRunning = false;
			}
		}
	}

	public void Pause() {
		if (GameSpeed != GameSpeed.Paused) {
			prevSpeed = GameSpeed;
			GameSpeed = GameSpeed.Paused;
		}
	}

	public void Resume() {
		if (GameSpeed == GameSpeed.Paused) {
			GameSpeed = prevSpeed;
		}
	}

	private void TriggerLose(LoseReason reason) {
		GameLost?.Invoke(this, reason);
	}

	private void TriggerWin() {
		GameWon?.Invoke(this, EventArgs.Empty);
	}

	private void WinUpdate() {
		if (WinConCheck()) {
			if (!winTimerRunning) {
				WinTimerDays = WinCriteriaDays;
				winTimerRunning = true;
			}
		} else {
			if (winTimerRunning) {
				winTimerRunning = false;
			}
		}
	}

	private bool WinConCheck() {
		if (Difficulty == GameDifficulty.Easy) {
			return funds >= WIN_FUNDS_EASY && HerbivoreCount >= WIN_HERB_EASY && CarnivoreCount >= WIN_CARN_EASY;
		} else if (Difficulty == GameDifficulty.Normal) {
			return funds >= WIN_FUNDS_NORMAL && HerbivoreCount >= WIN_HERB_NORMAL && CarnivoreCount >= WIN_CARN_NORMAL;
		} else {
			return funds >= WIN_FUNDS_HARD && HerbivoreCount >= WIN_HERB_HARD && CarnivoreCount >= WIN_CARN_HARD;
		}
	}

	// TODO: remove once not needed
	public void GenerateTestLevel() {
		// tiles

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

		// animals

		y *= Level.TileSize;
		Random rand = new Random();
		Type[] animalTypes = [typeof(Zebra), typeof(Giraffe), typeof(Elephant), typeof(Lion), typeof(Tiger), typeof(TigerWhite)];
		//Type[] animalTypes = [ typeof(Zebra), typeof(Giraffe), typeof(Elephant) ];
		for (int i = 0; i < 20; i++) {
			//int randX = rand.Next(100, Level.MapWidth * Level.TileSize - 100);
			//int randY = rand.Next(100, Level.MapHeight * Level.TileSize - 100);
			Vector2 pos = new Vector2((i % 5) * 96, y + (i / 5 * 96));
			int randType = rand.Next(0, animalTypes.Length);

			Animal anim = (Animal)Activator.CreateInstance(animalTypes[randType], [pos, (rand.Next(2) == 0 ? Gender.Male : Gender.Female)]);
			Game.AddObject(anim);
		}
	}
}
