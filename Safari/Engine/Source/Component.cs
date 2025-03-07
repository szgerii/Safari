namespace Engine;

public abstract class Component {
	/// <summary>
	/// The game object the component is currently attached to
	/// </summary>
	public GameObject Owner { get; set; }

	/// <summary>
	/// Called right after the component has been added to the game
	/// (either by attaching it to an active game object or by the inactive owner object entering the game)
	/// </summary>
	public virtual void Load() { }
	/// <summary>
	/// Called just before the component is removed from the game
	/// (either by detaching it from an active game object or by the active owner object leaving the game)
	/// </summary>
	public virtual void Unload() { }
}
