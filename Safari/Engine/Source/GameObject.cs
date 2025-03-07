using Engine.Objects;
using Microsoft.Xna.Framework;

namespace Engine;

public abstract class GameObject : IUpdatable, IDrawable {
	public Vector2 Position { get; set; }
	/// <summary>
	/// The position of the object relative to the screen
	/// </summary>
	public virtual Vector2 ScreenPosition {
		get => Position - Camera.Active.Position;
		set {
			Position = value + Camera.Active.Position;
		}
	}

	public bool Loaded { get; private set; } = false;
	public ComponentHandler CmpHandler { get; private set; } = new ComponentHandler();
	
	public GameObject(Vector2 pos) {
		Position = pos;

		CmpHandler.RegisterToObject(this);
	}

	/// <summary>
	/// Called right after the object has been added to the game
	/// </summary>
	public virtual void Load() {
		CmpHandler.LoadComponents();
		Loaded = true;
	}

	/// <summary>
	/// Called just before the object is removed from the game
	/// </summary>
	public virtual void Unload() {
		CmpHandler.UnloadComponents();
		Loaded = false;
	}

	public virtual void Update(GameTime gameTime) {
		CmpHandler.UpdateComponents(gameTime);
	}
	
	public virtual void Draw(GameTime gameTime) {
		CmpHandler.DrawComponents(gameTime);
	}

	public void Attach(Component cmp) => CmpHandler.Attach(cmp);
	public void Detach<T>() where T : Component => CmpHandler.Detach<T>();
	public T GetComponent<T>() where T : Component => CmpHandler.GetComponent<T>();
	public bool GetComponent<T>(out T component) where T : Component => CmpHandler.GetComponent(out component);
	public bool HasComponent<T>() where T : Component => CmpHandler.HasComponent<T>();
}