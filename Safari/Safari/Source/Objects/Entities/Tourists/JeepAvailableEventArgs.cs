using System;

namespace Safari.Objects.Entities.Tourists;

public class JeepAvailableEventArgs : EventArgs {
	private Jeep jeep;

	/// <summary>
	/// The jeep that becomes available
	/// </summary>
	public Jeep Jeep {
		get => jeep;
		set => jeep = value;
	}

	public JeepAvailableEventArgs(Jeep jeep) : base() {
		this.jeep = jeep;
	}
}
