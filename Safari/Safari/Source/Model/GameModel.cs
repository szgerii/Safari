using Engine.Debug;
using Engine.Scenes;
using Microsoft.Xna.Framework;
using Safari.Debug;
using Safari.Model.Entities;
using Safari.Model.Entities.Tourists;
using Safari.Scenes;
using System;
using Engine.Graphics.Stubs.Texture;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Safari.Model;

/// <summary>
/// The possible difficulty levels the game can be played at
/// </summary>
public enum GameDifficulty {
	Easy,
	Normal,
	Hard
}

/// <summary>
/// The possible rate the game can be advanced at
/// </summary>
public enum GameSpeed {
	Paused,
	Slow,
	Medium,
	Fast
}

/// <summary>
/// The possible reasons the player has lost the game
/// </summary>
public enum LoseReason {
	Money,
	Animals
}

/// <summary>
/// The class responsible for the general state of the game (like funds, winning, losing, time management, etc.)
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class GameModel {
	/// <summary>
	/// The amount of money the player needs to hold in order to win on easy difficulty
	/// </summary>
	public const int WIN_FUNDS_EASY = 50000;
	/// <summary>
	/// The amount of money the player needs to hold in order to win on normal difficulty
	/// </summary>
	public const int WIN_FUNDS_NORMAL = 120000;
	/// <summary>
	/// The amount of money the player needs to hold in order to win on hard difficulty
	/// </summary>
	public const int WIN_FUNDS_HARD = 200000;

	/// <summary>
	/// The amount of herbivore animals the player needs to have in their park in order to win on easy difficulty
	/// </summary>
	public const int WIN_HERB_EASY = 20;
	/// <summary>
	/// The amount of herbivore animals the player needs to have in their park in order to win on normal difficulty
	/// </summary>
	public const int WIN_HERB_NORMAL = 40;
	/// <summary>
	/// The amount of herbivore animals the player needs to have in their park in order to win on hard difficulty
	/// </summary>
	public const int WIN_HERB_HARD = 80;

	/// <summary>
	/// The amount of carnivore animals the player needs to have in their park in order to win on easy difficulty
	/// </summary>
	public const int WIN_CARN_EASY = 20;
	/// <summary>
	/// The amount of carnivore animals the player needs to have in their park in order to win on normal difficulty
	/// </summary>
	public const int WIN_CARN_NORMAL = 40;
	/// <summary>
	/// The amount of carnivore animals the player needs to have in their park in order to win on hard difficulty
	/// </summary>
	public const int WIN_CARN_HARD = 80;

	/// <summary>
	/// The number of days, for which the player needs to meet all winning criteria in order to win on easy difficulty
	/// </summary>
	public const int WIN_DAYS_EASY = 5;
	/// <summary>
	/// The number of days, for which the player needs to meet all winning criteria in order to win on normal difficulty
	/// </summary>
	public const int WIN_DAYS_NORMAL = 30;
	/// <summary>
	/// The number of days, for which the player needs to meet all winning criteria in order to win on hard difficulty
	/// </summary>
	public const int WIN_DAYS_HARD = 60;

	/// <summary>
	/// How much faster 'medium' speed is compared to 'slow'
	/// </summary>
	public const int MEDIUM_FRAMES = 8;
	/// <summary>
	/// The component of medium speed which is faked, sacrificing sim accuracy for speed
	/// </summary>
	public const int MEDIUM_FAKE = 4;
	/// <summary>
	/// How much faster 'fast' speed is compared to 'slow'
	/// </summary>
	public const int FAST_FRAMES = 36;
	/// <summary>
	/// The component of fast speed which is faked, sacrificing sim accuracy for speed
	/// </summary>
	public const int FAST_FAKE = 12;
	/// <summary>
	/// Length of an in-game day (irl seconds), when the game speed
	/// is set to 'slow'
	/// </summary>
	public const double DAY_LENGTH = 210.0;

	/// <summary>
	/// The relative time in a day when sunrise should start
	/// </summary>
	public const double SUNRISE_START = 0.98;
	/// <summary>
	/// The relative time in a day when sunrise should end
	/// </summary>
	public const double SUNRISE_END = 0.02;
	/// <summary>
	/// The relative time in a day when sunset should start
	/// </summary>
	public const double SUNSET_START = 0.62;
	/// <summary>
	/// The relative time in a day when sunset should end
	/// </summary>
	public const double SUNSET_END = 0.66;

	private GameSpeed prevSpeed;
	[JsonProperty]
	private readonly DateTime startDate;

	/// <summary>
	/// The name of the park (used when saving the park)
	/// </summary>
	[JsonProperty]
	public string ParkName { get; init; }
	[JsonProperty]
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
	[JsonProperty]
	public GameDifficulty Difficulty { get; init; }

	/// <summary>
	/// The current speed of the simulation (paused, slow, medium, fast)
	/// </summary>
	public GameSpeed GameSpeed { get; set; } = GameSpeed.Slow;
	/// <summary>
	/// How fast the simulation should be updated at the current speed setting
	/// </summary>
	public int RealExtraFrames => GameSpeed switch {
		GameSpeed.Medium => MEDIUM_FRAMES / MEDIUM_FAKE,
		GameSpeed.Fast => FAST_FRAMES / FAST_FAKE,
		_ => 0
	};
	/// <summary>
	/// The number by which the gametime's delta should be multiplied at the current gamespeed
	/// </summary>
	public double FakeFrameMul => GameSpeed switch {
		GameSpeed.Medium => MEDIUM_FAKE,
		GameSpeed.Fast => FAST_FAKE,
		_ => 0
	};
	/// <summary>
	/// Time passed (in irl seconds) since the start of the game
	/// </summary>
	[JsonProperty]
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
	/// </summary>
	public bool IsDaytime => TimeOfDay < SUNSET_END || TimeOfDay > SUNRISE_START;

	/// <summary>
	/// Time passed (in in-game days) since the start of the game
	/// </summary>
	public double IngameDays => CurrentTime / DAY_LENGTH;
	/// <summary>
	/// The current in-game date
	/// </summary>
	public virtual DateTime IngameDate => startDate.AddDays(CurrentTime / DAY_LENGTH);

	/// <summary>
	/// The current game level's tilemap
	/// </summary>
	[JsonProperty]
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
	[JsonProperty]
	private bool winTimerRunning = false;
	/// <summary>
	/// Whether the win criteria is met therefore the win countdown is running
	/// </summary>
	public bool WinTimerRunning => winTimerRunning;
	/// <summary>
	/// How many days are left until winning
	/// </summary>
	[JsonProperty]
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
	[JsonProperty]
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
		DebugMode.AddFeature(new ExecutedDebugFeature("advance-gamespeed", [ExcludeFromCodeCoverage] () => {
			if (SceneManager.Active is GameScene) {
				GameModel model = GameScene.Active.Model;
				switch (model.GameSpeed) {
					case GameSpeed.Slow: model.GameSpeed = GameSpeed.Medium; break;
					case GameSpeed.Medium: model.GameSpeed = GameSpeed.Fast; break;
					case GameSpeed.Fast: model.GameSpeed = GameSpeed.Slow; break;
				}
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("gamespeed-slow", [ExcludeFromCodeCoverage] () => {
			if (SceneManager.Active is GameScene) {
				GameModel model = GameScene.Active.Model;
				model.GameSpeed = GameSpeed.Slow;
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("gamespeed-medium", [ExcludeFromCodeCoverage] () => {
			if (SceneManager.Active is GameScene) {
				GameModel model = GameScene.Active.Model;
				model.GameSpeed = GameSpeed.Medium;
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("gamespeed-fast", [ExcludeFromCodeCoverage] () => {
			if (SceneManager.Active is GameScene) {
				GameModel model = GameScene.Active.Model;
				model.GameSpeed = GameSpeed.Fast;
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-simulation", [ExcludeFromCodeCoverage] () => {
			if (SceneManager.Active is GameScene) {
				GameModel model = GameScene.Active.Model;
				switch (model.GameSpeed) {
					case GameSpeed.Paused: model.Resume(); break;
					default: model.Pause(); break;
				}
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("toggle-gameover-checks", [ExcludeFromCodeCoverage] () => {
			if (SceneManager.Active is GameScene gs) {
				gs.Model.CheckWinLose = !gs.Model.CheckWinLose;
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("add-money", [ExcludeFromCodeCoverage] () => {
			if (SceneManager.Active is GameScene gs) {
				gs.Model.Funds += 10000;
			}
		}));

		DebugMode.AddFeature(new ExecutedDebugFeature("subtract-money", [ExcludeFromCodeCoverage] () => {
			if (SceneManager.Active is GameScene gs) {
				gs.Model.Funds -= 10000;
			}
		}));
	}

	[JsonConstructor]
	public GameModel() {
		CheckWinLose = false;
	}

	public GameModel(string parkName, int funds, GameDifficulty difficulty, DateTime startDate) {
		ParkName = parkName;
		Funds = funds;
		Difficulty = difficulty;
		this.startDate = startDate;
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

	/// <summary>
	/// Pause the game (the simulation does not advance)
	/// </summary>
	public void Pause() {
		if (GameSpeed != GameSpeed.Paused) {
			prevSpeed = GameSpeed;
			GameSpeed = GameSpeed.Paused;
		}
	}

	/// <summary>
	/// Resume the game (the simulation runs normally)
	/// </summary>
	public void Resume() {
		if (GameSpeed == GameSpeed.Paused) {
			GameSpeed = prevSpeed;
		}
	}

	/// <summary>
	/// Print various information regarding the current state of the game model
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static void PrintModelDebugInfos() {
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
		//Level level = model.Level;
		//DebugInfoManager.AddInfo("Route count", level.Network.Routes.Count + "", DebugInfoPosition.BottomRight);
		//DebugInfoManager.AddInfo("Selected route length", level.Network.DebugRoute.Count + "", DebugInfoPosition.BottomRight);

		// Tourist debug stuff
		DebugInfoManager.AddInfo($"Average rating", $"{Tourist.AvgRating:0.00}", DebugInfoPosition.BottomRight);
		DebugInfoManager.AddInfo("Tourists spawn / hour", $"{Tourist.SpawnRate:0.00}", DebugInfoPosition.BottomRight);
	}

	private void TriggerLose(LoseReason reason) {
		CheckWinLose = false;
		GameLost?.Invoke(this, reason);
	}

	private void TriggerWin() {
		GameWon?.Invoke(this, EventArgs.Empty);
		PostWin = true;
	}

	private void WinUpdate() {
		if (WinConCheck() && !PostWin) {
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
