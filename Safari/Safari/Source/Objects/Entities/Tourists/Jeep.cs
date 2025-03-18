using Microsoft.Xna.Framework;
using Safari.Scenes;
using System;
using System.Collections.Generic;

namespace Safari.Objects.Entities.Tourists;

public enum JeepState {
	Parking,
	Waiting,
	Transporting,
	Returning
}

public class Jeep : Entity {
	public const int CAPACITY = 4;

	private List<Tourist> occupants;
	private static Queue<Jeep> garage;
	private JeepState state;
	private static int rentFee;

	/// <summary>
	/// The current state of the jeep 
	/// </summary>
	public JeepState State => state;
	/// <summary>
	/// The amount a tourist has to pay in order to participate in the tour
	/// A high rent fee could make some tourists dissatisfied with the ride
	/// </summary>
	public static int RentFee {
		get => rentFee;
		set => rentFee = value;
	}

	/// <summary>
	/// Invoked when a jeep has returned to the waiting area
	/// </summary>
	public event EventHandler? Returned;

	public Jeep(Vector2 pos) : base(pos) {
		DisplayName = "Jeep";
	}

	public override void Load() {
		GameScene.Active.Model.JeepCount++;

		base.Load();
	}

	public override void Unload() {
		GameScene.Active.Model.JeepCount--;

		base.Unload();
	}
}
