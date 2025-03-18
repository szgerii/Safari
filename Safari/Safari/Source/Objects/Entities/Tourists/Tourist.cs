using Microsoft.Xna.Framework;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Objects.Entities.Tourists;

public class Tourist : Entity {
	private float rating = 2.5f;
	private static Queue<Tourist> jeepQueue = new Queue<Tourist>();
	private static int[] recentRatings = new int[30];
	private bool inQueue = true;
	private Jeep? vehicle;

	/// <summary>
	/// The average rating of the park based on the 30 newest ratings by tourist
	/// </summary>
	public static int AvgRating {
		get {
			int count = 0;
			int sum = 0;
			for (int i = 0; i < recentRatings.Length && recentRatings[i] != 0; i++) {
				sum += recentRatings[i];
				count++;
			}
			return count == 0 ? 0 : (sum / count);
		}
	}
	/// <summary>
	/// The rating this tourist gives about the ride
	/// </summary>
	public float Rating => rating;
	/// <summary>
	/// How many tourists are waiting for a free jeep
	/// 0 means no queue
	/// </summary>
	public static int QueueLength => jeepQueue.Count;
	/// <summary>
	/// The jeep this tourist is assigned to
	/// </summary>
	public Jeep? Vehicle {
		get => vehicle;
		set => vehicle = value;
	}

	/// <summary>
	/// Invoked every time this tourist sees an animal during the tour
	/// </summary>
	public event EventHandler<SawAnimalEventArgs>? SawAnimal;
	/// <summary>
	/// Invoked when a jeep becomes available
	/// </summary>
	public event EventHandler<JeepAvailableEventArgs>? JeepAvailable;

	public Tourist(Vector2 pos) : base(pos) {
		DisplayName = "Tourist";
	}

	public override void Load() {
		GameScene.Active.Model.TouristCount++;

		base.Load();
	}

	public override void Unload() {
		GameScene.Active.Model.TouristCount--;

		base.Unload();
	}
}
