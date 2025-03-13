using Microsoft.Xna.Framework;
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

	public JeepState State => state;
	public static int RentFee {
		get => rentFee;
		set => rentFee = value;
	}

	public event EventHandler? Returned;

	public Jeep(Vector2 pos) : base(pos) {
		displayName = "Jeep";
	}
}
