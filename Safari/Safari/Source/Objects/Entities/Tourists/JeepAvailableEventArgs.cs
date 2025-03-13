using System;

namespace Safari.Objects.Entities.Tourists;

public class JeepAvailableEventArgs : EventArgs {
	private Jeep jeep;

	public Jeep Jeep {
		get => jeep;
		set => jeep = value;
	}

	public JeepAvailableEventArgs(Jeep jeep) : base() {
		this.jeep = jeep;
	}
}
