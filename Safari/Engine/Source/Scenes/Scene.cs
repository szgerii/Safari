using Engine.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Engine.Scenes;

public class Scene : IUpdatable, IDrawable {
	public event EventHandler<GameTime>? PreUpdate, PostUpdate, PreDraw, PostDraw;

	public bool Loaded { get; protected set; } = false;

	private bool loading = false;

	public List<GameObject> GameObjects { get; protected set; } = new List<GameObject>();

	private readonly Queue<GameObject> objAddBuffer = new();
	private readonly Queue<GameObject> objRemoveBuffer = new();

	/// <summary>
	/// List of the post process passes this scene should perform after drawing its objects
	/// </summary>
	public List<IPostProcessPass> PostProcessPasses { get; private set; } = new();

	public virtual void Load() {
		loading = true;
		foreach (GameObject obj in GameObjects) {
			obj.Load();
		}
		loading = false;

		Loaded = true;
	}

	public virtual void Unload() {
		foreach (GameObject obj in GameObjects) {
			obj.Unload();
		}

		Loaded = false;
	}

	public virtual void Update(GameTime gameTime) {
		PerformPreUpdate(gameTime);

		foreach (GameObject obj in GameObjects) {
			obj.Update(gameTime);
		}

		PerformPostUpdate(gameTime);
	}

	public virtual void Draw(GameTime gameTime) {
		PerformPreDraw(gameTime);

		foreach (GameObject obj in GameObjects) {
			obj.Draw(gameTime);
		}

		PerformPostDraw(gameTime);
	}

	public virtual void AddObject(GameObject obj) {
		if (Loaded || loading) {
			if (!objAddBuffer.Contains(obj)) {
				objAddBuffer.Enqueue(obj);
			}
		} else {
			GameObjects.Add(obj);
		}
	}

	public virtual void RemoveObject(GameObject obj) {
		if (Loaded || loading) {
			if (!objRemoveBuffer.Contains(obj)) {
				objRemoveBuffer.Enqueue(obj);
			}
		} else {
			GameObjects.Remove(obj);
		}
	}

	public virtual void PerformObjectAdditions() {
		while (objAddBuffer.Count > 0) {
			GameObject obj = objAddBuffer.Dequeue();

			GameObjects.Add(obj);
			obj.Load();
		}
	}

	public virtual void PerformObjectRemovals() {
		while (objRemoveBuffer.Count != 0) {
			GameObject obj = objRemoveBuffer.Dequeue();

			obj.Unload();
			GameObjects.Remove(obj);
		}
	}

	protected void PerformPreUpdate(GameTime gameTime) => PreUpdate?.Invoke(this, gameTime);
	protected void PerformPostUpdate(GameTime gameTime) => PostUpdate?.Invoke(this, gameTime);
	protected void PerformPreDraw(GameTime gameTime) => PreDraw?.Invoke(this, gameTime);
	protected void PerformPostDraw(GameTime gameTime) => PostDraw?.Invoke(this, gameTime);
}
