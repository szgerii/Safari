namespace Safari.Helpers;

/// <summary>
/// Interface for singleton classes that are resetted at the end of a game instance's lifecycle
/// </summary>
public interface IResettableSingleton {
	/// <summary>
	/// Cleanup method called during <see cref="Game.Dispose(bool)"/>
	/// </summary>
	public static abstract void ResetSingleton();
}
