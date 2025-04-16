using Engine.Debug;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Safari.Debug;
using Safari.Objects.Entities;
using Safari.Model.Tiles;
using Safari.Objects.Entities.Animals;
using Safari.Objects.Entities.Tourists;
using Safari.Scenes;
using System;

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
	// constants for money requirements for winning
	public const int WIN_FUNDS_EASY = 30000;
	public const int WIN_FUNDS_NORMAL = 50000;
	public const int WIN_FUNDS_HARD = 80000;

	// constants for herbivore count requirements for winning
	public const int WIN_HERB_EASY = 5;
	public const int WIN_HERB_NORMAL = 40;
	public const int WIN_HERB_HARD = 50;

	// constants for carnivore count requirements for winning
	public const int WIN_CARN_EASY = 5;
	public const int WIN_CARN_NORMAL = 40;
	public const int WIN_CARN_HARD = 50;

	// constants storing how long the player has to keep the winning conditions
	public const int WIN_DAYS_EASY = 3;
	public const int WIN_DAYS_NORMAL = 30;
	public const int WIN_DAYS_HARD = 60;

	/// <summary>
	/// How much faster 'medium' speed is compared to 'slow'
	/// </summary>
	private const int MEDIUM_MULTIPLIER = 4;
	/// <summary>
	/// How much faster 'fast' speed is compared to 'medium'
	/// </summary>
	private const int FAST_MULTIPLIER = 7;
	/// <summary>
	/// Length of an in-game day (irl seconds), when the game speed
	/// is set to 'slow'
	/// </summary>
	private const double DAY_LENGTH = 60.0;
	
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
		GameSpeed.Paused => 0,
		GameSpeed.Slow => 1,
		GameSpeed.Medium => MEDIUM_MULTIPLIER,
		_ => FAST_MULTIPLIER * MEDIUM_MULTIPLIER
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
	public double TimeOfDay => (CurrentTime / DAY_LENGTH) % 1.0;

	/// <summary>
	/// Indicates whether it is currently day time in-game
	/// (TimeOfDay < 0.54) ~ .5 plus extra time for sunset
	/// </summary>
	public bool IsDaytime => TimeOfDay < 0.54f || TimeOfDay > .96;

	/// <summary>
	/// Time passed (in in-game days) since the start of the game
	/// </summary>
	public double IngameDays => CurrentTime / DAY_LENGTH;
	/// <summary>
	/// The current in-game date
	/// </summary>
	public DateTime IngameDate => startDate.AddDays(CurrentTime / DAY_LENGTH);

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
	/// Automatically turned off once the players wins, meaning in "post-game" the player cant lose
	/// </summary>
	public bool CheckWinLose { get; set; } = true;
	/// <summary>
	/// Stores whether this save has been won
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

		Jeep.Init(250);

		Game.AddObject(Level);

		// try to spawn poachers after 6 hours of previous spawn with a 0.5 base chance, which increase by 0.05 every attempt
		EntitySpawner<Poacher> poacherSpawner = new(6, 0.5f, 0.05f) {
			EntityLimit = 5, // don't spawn if there are >= 5 poachers on the map
			EntityCount = () => PoacherCount // use PoacherCount to determine number of poachers on the map
		};
		Game.AddObject(poacherSpawner);

		DebugMode.AddFeature(new LoopedDebugFeature("draw-grid", Level.PostDraw, GameLoopStage.POST_DRAW));
	}

	/// <summary>
	/// Should be called every 'simulation-update'
	/// </summary>
	/// <param name="gameTime">MG GameTime</param>
	public void Advance(GameTime gameTime) {
		CurrentTime += gameTime.ElapsedGameTime.TotalSeconds;
		if (CheckWinLose && winTimerRunning) {
			WinTimerDays -= gameTime.ElapsedGameTime.TotalSeconds / DAY_LENGTH;
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

	public void PrintModelDebugInfos() {
		GameModel model = GameScene.Active.Model;

		string speedName = "";
		switch (model.GameSpeed) {
			case GameSpeed.Slow: speedName = "Slow"; break;
			case GameSpeed.Medium: speedName = "Medium"; break;
			case GameSpeed.Fast: speedName = "Fast"; break;
			case GameSpeed.Paused: speedName = "Paused"; break;
		}
		DebugInfoManager.AddInfo("Current gamespeed", speedName, DebugInfoPosition.BottomLeft);
		DebugInfoManager.AddInfo("In-game date", $"{model.IngameDate}", DebugInfoPosition.BottomLeft);
		DebugInfoManager.AddInfo("Entity count", model.EntityCount + "", DebugInfoPosition.BottomRight);
		DebugInfoManager.AddInfo("Win timer", $"{model.WinTimerTime}", DebugInfoPosition.BottomLeft);
		DebugInfoManager.AddInfo("Funds", $"{model.Funds}$ / {model.WinCriteriaFunds}$", DebugInfoPosition.BottomLeft);
		DebugInfoManager.AddInfo("Herbivores", $"{model.HerbivoreCount} / {model.WinCriteriaHerb}", DebugInfoPosition.BottomLeft);
		DebugInfoManager.AddInfo("Carnivores", $"{model.CarnivoreCount} / {model.WinCriteriaCarn}", DebugInfoPosition.BottomLeft);

		// network debug stuff
		Level level = model.Level;
		DebugInfoManager.AddInfo("Route count", level.Network.Routes.Count + "", DebugInfoPosition.BottomRight);
		DebugInfoManager.AddInfo("Selected route length", level.Network.DebugRoute.Count + "", DebugInfoPosition.BottomRight);
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
}
