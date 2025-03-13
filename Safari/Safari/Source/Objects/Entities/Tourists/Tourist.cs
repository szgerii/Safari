using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Safari.Objects.Entities.Tourists;

public class Tourist : Entity {
	private float rating = 2.5f;
	private static Queue<Tourist> jeepQueue = new Queue<Tourist>();
	private int[] recentRatings = new int[30];
	private bool inQueue = true;

	public int AvgRating {
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
	public float Rating => rating;
	public static int QueueLength => jeepQueue.Count;

	public event EventHandler<SawAnimalEventArgs>? SawAnimal;

	public Tourist(Vector2 pos) : base(pos) {
		displayName = "Tourist";
	}
}
