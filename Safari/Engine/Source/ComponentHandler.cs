using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Engine;

public class ComponentHandler {
	public GameObject Owner { get; set; }

	private readonly List<Component> components = new();
	private readonly List<IUpdatable> updatables = new();
	private readonly List<IDrawable> drawables = new();
	
	public void Attach(Component cmp) {
		components.Add(cmp);
		cmp.Owner = Owner;

		if (cmp is IUpdatable updatable) {
			updatables.Add(updatable);
		}

		if (cmp is IDrawable drawable) {
			drawables.Add(drawable);
		}

		if (IsLoaded) {
			cmp.Load();
		}
	}

	public void Detach<T>() where T : Component {
		for (int i = 0; i < components.Count; i++) {
			if (components[i].GetType().Equals(typeof(T))) {
				Component cmp = components[i];
				
				if (IsLoaded) {
					cmp.Unload();
				}

				if (cmp is IUpdatable updatable) {
					updatables.Remove(updatable);
				}

				if (cmp is IDrawable drawable) {
					drawables.Remove(drawable);
				}

				cmp.Owner = null;
				components.RemoveAt(i);
				break;
			}
		}
	}

	public T GetComponent<T>() where T : Component {
		foreach (Component cmp in components) {
			if (cmp.GetType().Equals(typeof(T))) {
				return (T)cmp;
			}
		}

		return null;
	}

	public bool GetComponent<T>(out T component) where T : Component {
		foreach (Component cmp in components) {
			if (cmp.GetType().Equals(typeof(T))) {
				component = (T)cmp;
				return true;
			}
		}

		component = null;
		return false;
	}

	public bool HasComponent<T>() where T : Component => GetComponent<T>() != null;

	public void LoadComponents() {
		foreach (Component cmp in components) {
			cmp.Load();
		}
	}

	public void UnloadComponents() {
		foreach (Component cmp in components) {
			cmp.Unload();
		}
	}

	public void UpdateComponents(GameTime gameTime) {
		foreach (IUpdatable cmp in updatables) {
			cmp.Update(gameTime);
		}
	}

	public void DrawComponents(GameTime gameTime) {
		foreach (IDrawable cmp in drawables) {
			cmp.Draw(gameTime);
		}
	}

	public void RegisterToObject(GameObject obj) {
		Owner = obj;

		if (obj.Loaded) {
			foreach (Component cmp in components) {
				cmp.Load();
			}
		}
	}

	public void Unregister() {
		if (Owner == null) {
			return;
		}
		
		if (Owner.Loaded) {
			foreach (Component cmp in components) {
				cmp.Unload();
			}
		}

		Owner = null;
	}

	private bool IsLoaded => Owner != null && Owner.Loaded;
}
