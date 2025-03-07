using Engine.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Components;

public class CollisionCmp : Component {
	public event EventHandler<CollisionCmp> CollisionEnter;
	public event EventHandler<CollisionCmp> CollisionStay;
	public event EventHandler<CollisionCmp> CollisionLeave;

	public bool HasActiveCollisionEvents => CollisionEnter != null || CollisionStay != null || CollisionLeave != null;

	public Texture2D ColliderTex { get; private set; }
	private Collider collider;
	public Collider Collider {
		get => collider;
		set {
			collider = value;
			ColliderTex = Utils.GenerateTexture((int)collider.Width, (int)collider.Height, new Color(Color.Gold, 0.2f), true);
		}
	}
	public Collider AbsoluteCollider => Collider + Owner.Position;
	public Collider ScreenCollider => Collider + Owner.ScreenPosition;

	public CollisionTags Tags;
	public CollisionTags Targets;
	
	public CollisionCmp(Collider collider) {
		Collider = collider;
	}

	public CollisionCmp(int x, int y, int width, int height) : this(new Collider(x, y, width, height)) { }

	public override void Load() {
		CollisionManager.Insert(this);

		base.Load();
	}

	public override void Unload() {
		CollisionManager.Remove(this);
		
		base.Unload();
	}

	public const int STEP_COUNT = 10;
	/// <summary>
	/// Moves the owner object by the specified amount, taking into account collisions along the way and returns the actual distance traversed
	/// </summary>
	/// <param name="delta">The amount to move the object by</param>
	/// <returns>The actual movement vector that was traversed, equal to <paramref name="delta"/> if no collisions were detected</returns>
	public Vector2 MoveOwner(Vector2 delta) {
		CollisionManager.Remove(this);

		Vector2 stepX = Vector2.UnitX * delta / STEP_COUNT;
		Vector2 stepY = Vector2.UnitY * delta / STEP_COUNT;
		Vector2 sum = Vector2.Zero;

		Collider tempColl = AbsoluteCollider;
		bool movedInPrevStep = true;
		for (int i = 0; i < STEP_COUNT && movedInPrevStep; i++) {
			movedInPrevStep = false;

			tempColl += stepX;
			if (!CollisionManager.IsOutOfBounds(tempColl) && !CollisionManager.Collides(tempColl, this)) {
				Owner.Position += stepX;
				sum += stepX;
				movedInPrevStep = true;
			} else {
				tempColl -= stepX;
			}

			tempColl += stepY;
			if (!CollisionManager.IsOutOfBounds(tempColl) && !CollisionManager.Collides(tempColl, this)) {
				Owner.Position += stepY;
				sum += stepY;
				movedInPrevStep = true;
			} else {
				tempColl -= stepY;
			}
		}

		CollisionManager.Insert(this);
		return sum;
	}

	public bool Intersects(Collider collider) => AbsoluteCollider.Intersects(collider);

	public bool Intersects(CollisionCmp cmp) => AbsoluteCollider.Intersects(cmp.AbsoluteCollider);

	internal void OnCollisionEnter(CollisionCmp target) {
		CollisionEnter?.Invoke(this, target);
	}
	internal void OnCollisionStay(CollisionCmp target) {
		CollisionStay?.Invoke(this, target);
	}
	internal void OnCollisionLeave(CollisionCmp target) {
		CollisionLeave?.Invoke(this, target);
	}
}
