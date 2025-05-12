using Engine.Collision;
using Engine.Graphics.Stubs.Texture;
using Engine.Helpers;
using Microsoft.Xna.Framework;
using System;

namespace Engine.Components;

public class CollisionCmp : Component, ISpatial {
	public event EventHandler<CollisionCmp>? CollisionEnter;
	public event EventHandler<CollisionCmp>? CollisionStay;
	public event EventHandler<CollisionCmp>? CollisionLeave;

	public bool HasActiveCollisionEvents => CollisionEnter != null || CollisionStay != null || CollisionLeave != null;

	public ITexture2D? ColliderTex { get; private set; }
	private Vectangle collider;
	public Vectangle Collider {
		get => collider;
		set {
			if (collider.Width != value.Width || collider.Height != value.Height) {
				int texWidth = Math.Max((int)value.Width, 1);
				int texHeight = Math.Max((int)value.Height, 1);
				ColliderTex = Utils.GenerateTexture(texWidth, texHeight, new Color(Color.Gold, 0.2f), true);
			}

			collider = value;
		}
	}
	public Vectangle AbsoluteCollider => Collider + (Owner?.Position ?? Vector2.Zero);
	public Vectangle Bounds => AbsoluteCollider;

	public Vectangle ScreenCollider => Collider + (Owner?.ScreenPosition ?? Vector2.Zero);

	public CollisionTags Tags { get; set; }
	public CollisionTags Targets { get; set; }
	
	public CollisionCmp(Vectangle collider) {
		Collider = collider;
	}

	public CollisionCmp(int x, int y, int width, int height) : this(new Vectangle(x, y, width, height)) { }

	public override void Load() {
		CollisionManager.Insert(this);

		base.Load();
	}

	public override void Unload() {
		CollisionManager.Remove(this);
		
		base.Unload();
	}

	public const int STEP_COUNT = 2;
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

		bool movedInPrevStep = true;
		for (int i = 0; i < STEP_COUNT && movedInPrevStep; i++) {
			movedInPrevStep = false;

			Owner!.Position += stepX;
			if (!CollisionManager.IsOutOfBounds(this) && !CollisionManager.Collides(this)) {
				sum += stepX;
				movedInPrevStep = true;
			} else {
				Owner.Position -= stepX;
			}

			Owner.Position += stepY;
			if (!CollisionManager.IsOutOfBounds(this) && !CollisionManager.Collides(this)) {
				sum += stepY;
				movedInPrevStep = true;
			} else {
				Owner.Position -= stepY;
			}
		}

		CollisionManager.Insert(this);

		return sum;
	}

	public bool Intersects(Vectangle collider) => AbsoluteCollider.Intersects(collider);

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
