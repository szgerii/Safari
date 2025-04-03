using Engine;
using Engine.Collision;
using Engine.Components;
using Microsoft.Xna.Framework;
using Safari.Model;
using Safari.Objects.Entities;
using Safari.Objects.Entities.Animals;
using Safari.Scenes;
using System;

namespace Safari.Components;

/// <summary>
/// Event arguments for NavigationCmp's ReachedTarget event
/// </summary>
public class ReachedTargetEventArgs : EventArgs {
	/// <summary>
	/// The target position that has been reached (if the cmp was approaching a point)
	/// </summary>
	public Vector2? TargetPosition { get; init; } = null;
	/// <summary>
	/// The target object that has been reached (if the cmp was approaching an object)
	/// </summary>
	public GameObject TargetObject { get; init; } = null;

	public ReachedTargetEventArgs(GameObject obj) {
		TargetObject = obj;
	}

	public ReachedTargetEventArgs(Vector2 pos) {
		TargetPosition = pos;
	}
}

/// <summary>
/// Component for helping entities navigate through the game world
/// </summary>
[LimitCmpOwnerType(typeof(Entity), typeof(AnimalGroup))]
public class NavigationCmp : Component, IUpdatable {
	private const float FALLBACK_REACH_THRESHOLD = 0.1f;

	/// <summary>
	/// Fired when the cmp owner reaches their destination
	/// </summary>
	public event EventHandler<ReachedTargetEventArgs> ReachedTarget;

	private Vector2? targetPosition = null;
	/// <summary>
	/// The position the component is approaching
	/// <br />
	/// Sets TargetObject to null (unless value is null)
	/// </summary>
	public Vector2? TargetPosition {
		get => targetPosition;
		set {
			targetPosition = value;

			if (value != null) {
				TargetObject = null;
			}
		}
	}

	private GameObject targetObject = null;
	/// <summary>
	/// The object the component is approaching
	/// <br />
	/// Sets TargetPosition to null (unless value is null)
	/// </summary>
	public GameObject TargetObject {
		get => targetObject;
		set {
			targetObject = value;

			if (value != null) {
				TargetPosition = null;
			}
		}
	}

	/// <summary>
	/// The current position the component is trying to move towards
	/// </summary>
	public Vector2? Target => TargetObject?.Position ?? TargetPosition;

	/// <summary>
	/// The speed at which the component's owner is moving
	/// </summary>
	public float Speed { get; set; }
	/// <summary>
	/// Whether to stop moving after the target has been reached
	/// </summary>
	public bool StopOnTargetReach { get; set; } = true;
	/// <summary>
	/// Whether the component's owner should currently be approaching their target
	/// </summary>
	public bool Moving { get; set; } = false;

	/// <summary>
	/// Unit vector of the last direction the component was trying to move its owner in
	/// </summary>
	public Vector2 LastIntendedDelta { get; private set; } = Vector2.Zero;

	public bool AccountForBounds { get; set; } = true;

	public NavigationCmp(float speed = 50f) {
		Speed = speed;
	}

	public void Update(GameTime gameTime) {
		LastIntendedDelta = Vector2.Zero;

		if (Target == null) return;

		if (Moving) {
			Vector2 targetPos = Target.Value;
			if (AccountForBounds && Owner is Entity ownerEntity) {
				targetPos -= ownerEntity.Bounds.Size.ToVector2() / 2f;
				if (targetPos.X < 0) targetPos.X = 0;
				if (targetPos.Y < 0) targetPos.Y = 0;
			}

			Vector2 delta = Target.Value - Owner.Position;
			Vector2 delta_saved = delta;

			if (delta.Length() > 0.01f) {
				delta.Normalize();
				LastIntendedDelta = delta;
				delta *= Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

				if (Owner.GetComponent(out CollisionCmp collCmp)) {
					if (delta.Length() > delta_saved.Length()) {
						collCmp.MoveOwner(delta_saved);
					} else {
						collCmp.MoveOwner(delta);
					}
				} else {
					if (delta.Length() > delta_saved.Length()) {
						Owner.Position = Target.Value;
					} else {
						Owner.Position += delta;
					}
				}
			}

			if (CanReach(Target.Value)) {
				ReachedTargetEventArgs args = TargetObject == null ? new(TargetPosition.Value) : new(TargetObject);
				ReachedTarget?.Invoke(this, args);

				if (StopOnTargetReach) {
					Moving = false;
				}
			}
		}
	}

	private bool CanReach(Vector2 pos) {
		if (Owner is Entity ownerEntity && ownerEntity.ReachDistance > 0) {
			return ownerEntity.CanReach(pos);
		}

		if (Owner is AnimalGroup ownerGroup) {
			return ownerGroup.CanAnybodyReach(pos);
		}

		return Vector2.Distance(Owner.Position, pos) < FALLBACK_REACH_THRESHOLD;
	}
}
