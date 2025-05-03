namespace Safari.Debug;

public enum DebugInfoPosition {
	TopLeft,
	TopRight,
	BottomLeft,
	BottomRight
}

public struct DebugInfo {

	public string Name { get; set; }
	public string Info { get; set; }
	public DebugInfoPosition ScreenPos { get; set; }

	public readonly override string ToString() {
		if (Name != null) {
			return $"{Name}: {Info}";
		} else {
			return Info;
		}
	}
}
