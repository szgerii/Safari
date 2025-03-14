using Microsoft.Xna.Framework;
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
	private GameSpeed gameSpeed;
	private GameSpeed prevSpeed;
	private double currentTime;
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

	public GameModel(string parkName, int funds, GameDifficulty difficulty, DateTime startDate) {
		this.parkName = parkName;
		this.funds = funds;
		this.difficulty = difficulty;
		this.startDate = startDate;
		this.gameSpeed = GameSpeed.Slow;
		this.currentTime = 0;
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
