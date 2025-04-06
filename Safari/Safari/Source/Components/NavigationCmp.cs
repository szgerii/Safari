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

	public NavigationCmp(float speed = 50f) {
		Speed = speed;
	}

	public void Update(GameTime gameTime) {
		LastIntendedDelta = Vector2.Zero;

		if (Target == null) return;

		if (Moving) {
			Vector2 targetPos = Target.Value;
			if (Owner is Entity ownerEntity) {
				targetPos -= ownerEntity.Bounds.Size.ToVector2() / 2f;
				if (targetPos.X < 0) targetPos.X = 0;
				if (targetPos.Y < 0) targetPos.Y = 0;
			}

			Vector2 delta = Target.Value - Owner.Position;

			if (delta.Length() > 0.01f) {
				delta.Normalize();
				LastIntendedDelta = delta;
				delta *= Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

				Level currLevel = GameScene.Active.Model.Level;
				Vector2 minPos = Vector2.Zero;
				Vector2 maxPos = new Vector2(currLevel.MapWidth, currLevel.MapHeight) * currLevel.TileSize + new Vector2(currLevel.TileSize);
				if (Owner.GetComponent(out CollisionCmp collCmp)) {
					collCmp.MoveOwner(delta);
				} else {
					Owner.Position += delta;
				}

				Vector2 clampPos = Vector2.Clamp(Owner.Position, minPos, maxPos);
				if (clampPos != Owner.Position) {
					if (collCmp != null) {
						CollisionManager.Remove(collCmp);
					}

					Owner.Position = clampPos;

					if (collCmp != null) {
						CollisionManager.Insert(collCmp);
					}
				}
			}

			if (CanReach(Target.Value)) {
				if (StopOnTargetReach) {
					Moving = false;
				}

				ReachedTargetEventArgs args = TargetObject == null ? new(TargetPosition.Value) : new(TargetObject);
				ReachedTarget?.Invoke(this, args);
			}
		}
	}

	private bool CanReach(Vector2 pos) {
		if (Owner is Entity ownerEntity) {
			return ownerEntity.CanReach(pos);
		}

		if (Owner is AnimalGroup ownerGroup) {
			return ownerGroup.CanAnybodyReach(pos);
		}

		return Vector2.Distance(Owner.Position, pos) < FALLBACK_REACH_THRESHOLD;
	}
}
