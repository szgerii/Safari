using Engine;
using Engine.Components;
using Microsoft.Xna.Framework;
using Safari.Objects.Entities;
using Safari.Objects.Entities.Animals;
using System;

namespace Safari.Components;

/// <summary>
/// Event arguments for NavigationCmp's ReachedTarget event
/// </summary>
public class NavigationTargetEventArgs : EventArgs {
	/// <summary>
	/// The target position that has been reached (if the cmp was approaching a point)
	/// </summary>
	public Vector2? TargetPosition { get; init; } = null;
	/// <summary>
	/// The target object that has been reached (if the cmp was approaching an object)
	/// </summary>
	public GameObject TargetObject { get; init; } = null;

	public NavigationTargetEventArgs(GameObject obj) {
		TargetObject = obj;
	}

	public NavigationTargetEventArgs(Vector2 pos) {
		TargetPosition = pos;
	}
}

/// <summary>
/// Component for helping entities navigate through the game world
/// </summary>
[LimitCmpOwnerType(typeof(Entity), typeof(AnimalGroup))]
public class NavigationCmp : Component, IUpdatable {
	private const float FALLBACK_REACH_THRESHOLD = 0.1f;
	private const float FALLBACK_SIGHT_THRESHOLD = 0.2f;

	/// <summary>
	/// Fired when the cmp owner reaches their destination
	/// </summary>
	public event EventHandler<NavigationTargetEventArgs> ReachedTarget;
	public event EventHandler<NavigationTargetEventArgs> TargetInSight;

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
	public Vector2? Target {
		get {
			if (TargetObject != null) {
				return TargetObject is Entity entity ? entity.CenterPosition : TargetObject.Position;
			} else {
				return TargetPosition;
			}
		}
	}

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
	private Entity ownerEntity;
	private CollisionCmp collCmp;

	public bool AccountForBounds { get; set; } = true;

	public NavigationCmp(float speed = 50f) {
		Speed = speed;
	}

	public override void Load() {
		if (Owner is Entity entity) {
			ownerEntity = entity;
			Owner.GetComponent(out collCmp);
		}

		base.Load();
	}

	public override void Unload() {
		ownerEntity = null;
		collCmp = null;

		base.Unload();
	}

	public void Update(GameTime gameTime) {
		LastIntendedDelta = Vector2.Zero;

		if (Target == null) return;

		if (Moving) {
			if (ownerEntity != null) {
				EntityManager.RemoveEntity(ownerEntity);
			}

			Vector2 targetPos = Target.Value;
			if (AccountForBounds && ownerEntity != null) {
				targetPos -= ownerEntity.Bounds.Size / 2f;
				if (targetPos.X < 0) targetPos.X = 0;
				if (targetPos.Y < 0) targetPos.Y = 0;
			}

			Vector2 delta = Target.Value - Owner.Position;
			Vector2 deltaSaved = delta;
			Vector2 oldPos = Owner.Position;

			if (delta.Length() > 0.01f) {
				delta.Normalize();
				LastIntendedDelta = delta;
				delta *= Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

				if (collCmp != null) {
					if (delta.Length() > deltaSaved.Length()) {
						collCmp.MoveOwner(deltaSaved);
					} else {
						collCmp.MoveOwner(delta);
					}
				} else {
					if (delta.Length() > deltaSaved.Length()) {
						Owner.Position = Target.Value;
					} else {
						Owner.Position += delta;
					}
				}
			}

			if (ownerEntity != null) {
				if (oldPos != Owner.Position) {
					ownerEntity.UpdateBounds();
				}

				EntityManager.AddEntity(ownerEntity);
			}

			if (TargetInSight != null && CanSee(Target.Value)) {
				TargetInSight.Invoke(this, GetArgsForTarget());
			}

			if (CanReach(Target.Value)) {
				if (StopOnTargetReach) {
					Moving = false;
				}

				ReachedTarget?.Invoke(this, GetArgsForTarget());
			}
		}
	}

	private bool CanReach(Vector2 pos) {
		if (ownerEntity != null && ownerEntity.ReachDistance > 0) {
			return ownerEntity.CanReach(pos);
		}

		if (Owner is AnimalGroup ownerGroup) {
			return ownerGroup.CanAnybodyReach(pos);
		}

		return Vector2.DistanceSquared(Owner.Position, pos) < (FALLBACK_REACH_THRESHOLD * FALLBACK_REACH_THRESHOLD);
	}

	private bool CanSee(Vector2 pos) {
		if (ownerEntity != null && ownerEntity.SightDistance > 0) {
			return ownerEntity.CanSee(pos);
		}

		if (Owner is AnimalGroup ownerGroup) {
			return ownerGroup.CanAnybodySee(pos);
		}

		return Vector2.DistanceSquared(Owner.Position, pos) < (FALLBACK_SIGHT_THRESHOLD * FALLBACK_SIGHT_THRESHOLD);
	}

	private NavigationTargetEventArgs GetArgsForTarget() => TargetObject == null ? new(TargetPosition.Value) : new(TargetObject);
}
