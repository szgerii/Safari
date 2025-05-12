using Engine.Components;
using System;

namespace Engine;

public abstract class Component {
	private GameObject? owner;
	/// <summary>
	/// The game object the component is currently attached to
	/// </summary>
	public GameObject? Owner {
		get => owner;
		set {
			if (value != null) {
				Attribute? limitAttr = Attribute.GetCustomAttribute(GetType(), typeof(LimitCmpOwnerTypeAttribute));

				if (limitAttr != null && !((LimitCmpOwnerTypeAttribute)limitAttr).IsAllowedType(value.GetType())) {
					throw new ArgumentException($"Component of type '{GetType().Name}' cannot be attached to game object of type '{value.GetType().Name}' due to manual type restrictions");
				}
			}

			owner = value;
		}
	}

	public bool Loaded { get; protected set; }

	/// <summary>
	/// Called right after the component has been added to the game
	/// (either by attaching it to an active game object or by the inactive owner object entering the game)
	/// </summary>
	public virtual void Load() {
		Loaded = true;
	}
	/// <summary>
	/// Called just before the component is removed from the game
	/// (either by detaching it from an active game object or by the active owner object leaving the game)
	/// </summary>
	public virtual void Unload() {
		Loaded = false;
	}
}
