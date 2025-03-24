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

public class ReachedTargetEventArgs : EventArgs {
	public Vector2? TargetPosition { get; init; } = null;
	public GameObject TargetObject { get; init; } = null;

	public ReachedTargetEventArgs(GameObject obj) {
		TargetObject = obj;
	}

	public ReachedTargetEventArgs(Vector2 pos) {
		TargetPosition = pos;
	}
}

[LimitCmpOwnerType(typeof(Entity), typeof(AnimalGroup))]
public class NavigationCmp : Component, IUpdatable {
	private const float FALLBACK_REACH_THRESHOLD = 0.1f;

	public event EventHandler<ReachedTargetEventArgs> ReachedTarget;

	private Vector2? targetPosition = null;
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
	public GameObject TargetObject {
		get => targetObject;
		set {
			targetObject = value;

			if (value != null) {
				TargetPosition = null;
			}
		}
	}

	public Vector2? Target => TargetObject?.Position ?? TargetPosition;

	public float Speed { get; set; }
	public bool StopOnTargetReach { get; set; } = true;
	public bool Moving { get; set; } = false;

	public Vector2 LastIntendedDelta { get; private set; } = Vector2.Zero;

	protected Entity OwnerEntity => Owner as Entity;

	public NavigationCmp(float speed = 50f) {
		Speed = speed;
	}

	public void Update(GameTime gameTime) {
		LastIntendedDelta = Vector2.Zero;

		if (Target == null) return;

		if (Moving) {
			Vector2 targetPos = Target.Value;
			if (OwnerEntity != null) {
				targetPos -= OwnerEntity.Bounds.Size.ToVector2() / 2f;
				if (targetPos.X < 0) targetPos.X = 0;
				if (targetPos.Y < 0) targetPos.Y = 0;
			}

			Vector2 delta = Target.Value - Owner.Position;


			if (delta.Length() > 0.1f) {
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
				ReachedTargetEventArgs args = TargetObject == null ? new(TargetPosition.Value) : new(TargetObject);
				ReachedTarget?.Invoke(this, args);

				if (StopOnTargetReach) {
					Moving = false;
				}
			}
		}
	}

	private bool CanReach(Vector2 pos) {
		if (Owner is Entity ownerEntity) {
			return ownerEntity.CanReach(pos);
		}

		if (Owner is AnimalGroup ownerGroup) {
			foreach (Animal a in ownerGroup.Members) {
				if (a.CanReach(pos)) return true;
			}

			return false;
		}

		return Vector2.Distance(Owner.Position, pos) < FALLBACK_REACH_THRESHOLD;
	}
}
