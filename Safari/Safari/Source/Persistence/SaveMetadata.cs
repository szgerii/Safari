using Newtonsoft.Json;
using System;

namespace Safari.Persistence;

[JsonObject(MemberSerialization.OptIn)]
public class SaveMetadata {
	/// <summary>
	/// The date at which this save file was created
	/// </summary>
	[JsonProperty]
	public DateTime CreationDate { get; set; }

	/// <summary>
	/// The name of the park
	/// </summary>
	[JsonProperty]
	public string ParkName { get; set; }

	/// <summary>
	/// The playtime on this save (sped up gameplay counts as more playtime)
	/// </summary>
	[JsonProperty]
	public TimeSpan PlayTime { get; set; }

	/// <summary>
	/// The in game date at the time of saving
	/// </summary>
	[JsonProperty]
	public DateTime InGameDate { get; set; }

	/// <summary>
	/// Whether this save points to an already won state of the game
	/// </summary>
	[JsonProperty]
	public bool GameAlreadyWon { get; set; }

	[JsonConstructor]
	public SaveMetadata() { }

	public SaveMetadata(DateTime creationDate, string parkName, TimeSpan playTime, DateTime inGameDate, bool gameAlreadyWon) {
		CreationDate = creationDate;
		ParkName = parkName;
		PlayTime = playTime;
		InGameDate = inGameDate;
		GameAlreadyWon = gameAlreadyWon;
	}
}
